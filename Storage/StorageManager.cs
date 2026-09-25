using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Architect.Api;
using Architect.Config.Types;
using Architect.Content.Preloads;
using Architect.Editor;
using Architect.Events.Blocks.Config.Types;
using Architect.Events.Blocks.Outputs;
using Architect.Objects.Categories;
using Architect.Objects.Placeable;
using Architect.Objects.Tools;
using Architect.Placements;
using Architect.Prefabs;
using Architect.Utils;
using Architect.Workshop;
using BepInEx;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace Architect.Storage;

public static class StorageManager
{
    public const string GLOBAL = "Global";
    
    internal static readonly LevelData.LevelDataConverter Ldc = new();
    internal static readonly ObjectPlacement.ObjectPlacementConverter Opc = new();
    
    public static string DataPath;
    
    private static readonly Dictionary<string, List<IScheduledEdit>> ScheduledEdits = [];
    
    
    public static void Init()
    {
        DataPath = Path.GetFullPath(Path.Combine(Application.persistentDataPath, "Architect"));
        Directory.CreateDirectory(Path.Combine(DataPath, "Scenes"));
        Directory.CreateDirectory(Path.Combine(DataPath, "Prefabs"));
        Directory.CreateDirectory(Path.Combine(DataPath, "Assets"));
        Directory.CreateDirectory(Path.Combine(DataPath, "Backups"));
        Directory.CreateDirectory(Path.Combine(DataPath, "ModAssets"));
        Directory.CreateDirectory(Path.Combine(DataPath, "Hotbars"));
        
        typeof(GameManager).Hook(nameof(GameManager.SaveGame), 
            (Action<GameManager, Action<bool>> orig, GameManager self, Action<bool> callback) => 
            { 
                SaveFavourites(FavouritesCategory.Favourites);
                SaveSavedObjects(SavedCategory.Objects);

                if (EditManager.IsEditing)
                {
                    SaveScene(GameManager.instance.sceneName, PlacementManager.GetLevelData());
                    SaveScene(GLOBAL, PlacementManager.GetGlobalData());
                    SaveWorkshopData();
                }

                foreach (var (scene, edits) in ScheduledEdits)
                {
                    ApplyScheduledEdits(scene, edits);
                }
                ScheduledEdits.Clear();

                orig(self, callback);
            }, typeof(Action<bool>));
    }

    public static void SaveHotbar(int slot)
    {
        for (var i = 0; i < 9; i++)
        {
            var path = Path.Combine(DataPath, "Hotbars", $"{slot}_{i}.json");
            var obj = EditManager.HotbarCurrentObject[i];
            switch (obj)
            {
                case PlaceableObject p:
                {
                    var placement = new ObjectPlacement(
                        p,
                        new Vector3(0, 0, EditManager.HotbarCurrentZ[i]),
                        string.Empty,
                        EditManager.HotbarCurrentlyFlipped[i],
                        EditManager.HotbarCurrentRotation[i],
                        EditManager.HotbarCurrentScale[i],
                        false,
                        0,
                        EditManager.HotbarBroadcasters[i].ToArray(),
                        EditManager.HotbarReceivers[i].ToArray(),
                        EditManager.HotbarConfig[i].Values.ToArray()
                    );
                    File.WriteAllText(path, JsonConvert.SerializeObject(placement, Opc));
                    break;
                }
                case ToolObject t:
                    File.WriteAllText(path, t.Id);
                    break;
            }
        }
    }

    public static IEnumerator LoadHotbar(int slot)
    {
        for (var i = 0; i < 9; i++)
        {
            var path = Path.Combine(DataPath, "Hotbars", $"{slot}_{i}.json");
            if (!File.Exists(path))
            {
                EditManager.HotbarCurrentObject[i] = BlankObject.Instance;
            }
            else
            {
                var data = File.ReadAllText(path);

                if (ToolObject.Tools.TryGetValue(data, out var tool))
                {
                    EditManager.HotbarCurrentObject[i] = tool;
                }
                else
                {
                    ObjectPlacement placement = null;
                    try
                    {
                        placement = JsonConvert.DeserializeObject<ObjectPlacement>(data, Opc);
                    }
                    catch
                    {
                        EditManager.HotbarCurrentObject[i] = BlankObject.Instance;
                    }

                    if (placement != null)
                    {
                        yield return placement.GetPlacementType().EnsureLoaded();

                        EditManager.HotbarCurrentObject[i] = placement.GetPlacementType();
                        EditManager.HotbarCurrentlyFlipped[i] = placement.IsFlipped();
                        EditManager.HotbarCurrentRotation[i] = placement.GetRotation();
                        EditManager.HotbarCurrentScale[i] = placement.GetScale();
                        EditManager.HotbarCurrentZ[i] = placement.GetPos().z;
                        EditManager.HotbarBroadcasters[i] = placement.Broadcasters.ToList();
                        EditManager.HotbarReceivers[i] = placement.Receivers.ToList();
                        EditManager.HotbarConfig[i] = placement.Config.ToDictionary(k => k.GetTypeId(), k => k);
                    }
                }
            }

            EditorUI.RefreshItem(i);
        }
        
        EditorUI.RefreshItem();
        EditManager.HotbarIndex = EditManager.HotbarIndex;
    }

    private static string GetScenePath(string scene)
    {
        if (scene.StartsWith("Prefab_"))
        {
            return MapLoader.GetPrefab(scene) ?? Path.Combine(DataPath, "Prefabs", $"{scene}.architect.json");
        }
        return Path.Combine(DataPath, "Scenes", $"{scene}.architect.json");
    }
    
    public static void SaveScene(string scene, LevelData level)
    {
        var path = GetScenePath(scene);
        if (File.Exists(path)) File.Delete(path);

        if (level.Placements.IsNullOrEmpty() &&
            level.TilemapChanges.IsNullOrEmpty() &&
            level.ScriptBlocks.IsNullOrEmpty() &&
            level.Comments.IsNullOrEmpty())
        {
            if (scene.StartsWith("Prefab_")) PrefabsCategory.Remove(scene.Replace("Prefab_", ""));
            return;
        }
        if (scene.StartsWith("Prefab_")) PrefabsCategory.Add(scene.Replace("Prefab_", ""), level);
        
        var data = SerializeLevel(level, Formatting.Indented);
        
        using var stream = File.Create(path);
        using var writer = new StreamWriter(stream);

        writer.Write(data);
    }

    public static LevelData LoadScene(string scene)
    {
        if (ScheduledEdits.Remove(scene, out var edits)) return ApplyScheduledEdits(scene, edits);
        
        var path = GetScenePath(scene);

        return File.Exists(path) ? DeserializeLevel(File.ReadAllText(path)) : 
            new LevelData([], [], [], []);
    }

    public static void LoadWorkshopData()
    {
        var path = Path.Join(DataPath, "workshop.json");
        if (!File.Exists(path))
        {
            WorkshopManager.LoadWorkshop(new WorkshopData());
            return;
        }
        
        var data = File.ReadAllText(path);
        
        WorkshopManager.LoadWorkshop(JsonConvert.DeserializeObject<WorkshopData>(data));
    }

    public static void SaveWorkshopData()
    {
        var path = Path.Join(DataPath, "workshop.json");
        if (File.Exists(path)) File.Delete(path);
        
        File.WriteAllText(path, JsonConvert.SerializeObject(WorkshopManager.WorkshopData, Formatting.Indented));
    }

    public static LevelData DeserializeLevel(string data)
    {
        try
        {
            return JsonConvert.DeserializeObject<LevelData>(data);
        }
        catch
        {
            return new LevelData([], [], [], []);
        }
    }

    public static List<ObjectPlacement> DeserializePlacements(string data)
    {
        try
        {
            return JsonConvert.DeserializeObject<List<ObjectPlacement>>(data);
        }
        catch (Exception)
        {
            ArchitectPlugin.Logger.LogError("Invalid saved objects file (prefabs.json), clearing saved objects");
            return [];
        }
    }

    public static string SerializeLevel(LevelData level, Formatting formatting)
    {
        return JsonConvert.SerializeObject(level, formatting, Ldc, Opc);
    }

    public static string SerializePlacements(List<ObjectPlacement> placements)
    {
        return JsonConvert.SerializeObject(placements, Formatting.Indented, Opc);
    }

    public static void SaveFavourites(List<string> favourites)
    {
        var path = Path.Join(DataPath, "favourites.json");
        if (File.Exists(path)) File.Delete(path);

        var data = JsonConvert.SerializeObject(favourites);

        using var stream = File.Create(path);
        using var writer = new StreamWriter(stream);

        writer.Write(data);
    }

    public static List<string> LoadFavourites()
    {
        var path = Path.Join(DataPath, "favourites.json");
        if (File.Exists(path))
        {
            var deserialized = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(path));
            if (deserialized != null) return deserialized;
        }

        return [];
    }

    private static void SaveSavedObjects(List<SavedObject> prefabs)
    {
        var path = Path.Join(DataPath, "prefabs.json");
        if (File.Exists(path)) File.Delete(path);
        
        var data = SerializePlacements(prefabs.Where(p => p != null)
            .Select(obj => obj.Placement).ToList());

        using var stream = File.Create(path);
        using var writer = new StreamWriter(stream);

        writer.Write(data);
    }

    public static List<SavedObject> LoadSavedObjects()
    {
        var path = Path.Join(DataPath, "prefabs.json");
        if (File.Exists(path))
        {
            var deserialized = DeserializePlacements(File.ReadAllText(path));
            if (deserialized != null) return deserialized.Select(obj => new SavedObject(obj)).ToList();
        }

        return [];
    }

    public static List<PrefabObject> LoadPrefabs(string dp)
    {
        var path = dp + "/Prefabs";
        List<PrefabObject> prefabs = [];
        if (Directory.Exists(path)) foreach (var file in Directory.GetFiles(path))
        {
            if (!file.EndsWith(".architect.json")) continue;
            prefabs.Add(new PrefabObject(Path.GetFileNameWithoutExtension(file)
                .Replace("Prefab_", "").Replace(".architect", "")));
        }
        
        return prefabs;
    }

    public static readonly List<string> Directories = [];

    public static void FindLoadRequirements()
    {
        FindLoadRequirements(DataPath);
        foreach (var dir in Directories) FindLoadRequirements(dir);
    }

    public static void FindLoadRequirements(string dp)
    {
        foreach (var s in new[] { "Scenes/", "Prefabs/" })
        {
            var path = Path.Combine(dp, s);
            if (!Directory.Exists(path)) continue;
            foreach (var file in Directory.GetFiles(path))
            {
                if (!file.EndsWith(".architect.json") || file.StartsWith(".")) continue;
                var scene = DeserializeLevel(File.ReadAllText(file));
                if (scene == null)
                {
                    ArchitectPlugin.Logger.LogInfo($"Invalid scene found at {file}");
                    continue;
                }
                foreach (var o in scene.Placements.Where(o => o != null))
                {
                    if (o.GetPlacementType() is PreloadObject preload) 
                        preload.ShouldLoad = true;
                }
            }
        }
    }

    public static string SerializeAllScenes()
    {
        Dictionary<string, LevelData> data = [];
        foreach (var s in new[] { "Scenes/", "Prefabs/" })
        {
            foreach (var file in Directory.GetFiles(Path.Join(DataPath, s)))
            {
                var name = Path.GetFileName(file);
                if (!name.EndsWith(".architect.json")) continue;

                var n = name.Replace(".architect.json", "");
                data[n] = LoadScene(n);
            }
        }

        return JsonConvert.SerializeObject(data, Formatting.None, Ldc, Opc);
    }

    public static void WipeLevelData()
    {
        foreach (var file in Directory.GetFiles(Path.Join(DataPath, "Scenes"))) File.Delete(file);
        foreach (var file in Directory.GetFiles(Path.Join(DataPath, "Prefabs"))) File.Delete(file);
        foreach (var file in Directory.GetFiles(Path.Join(DataPath, "Assets"))) File.Delete(file);

        GlobalArchitectData.Instance.CurrentMap = "";
        GlobalArchitectData.Instance.CurrentMapId = "";
        CustomAssetManager.WipeAssets();
        WorkshopManager.LoadWorkshop(new WorkshopData());
    }

    public static IEnumerator LoadLevelData(Dictionary<string, LevelData> levels, WorkshopData workshop, Text status)
    {
        WipeLevelData();
        var startTime = Time.realtimeSinceStartup;

        Dictionary<string, StringConfigValue> downloads = [];
        Dictionary<string, StringConfigValue<PngBlock>> blockDownloads = [];

        HashSet<PlaceableObject> placeable = [];

        var cherries = 0;
        var goldCherries = 0;
        foreach (var (scene, data) in levels)
        {
            foreach (var pt in data.Placements
                         .Select(placement => placement.GetPlacementType()).OfType<PreloadObject>())
            {
                placeable.Add(pt);
            }
            
            foreach (var config in data.Placements.SelectMany(obj => obj.Config)
                         .OfType<StringConfigValue>())
            {
                if (!config.GetTypeId().Contains("_url")) continue;
                var cfg = config.GetValue();
                downloads.TryAdd(cfg, config);
            }

            foreach (var config in data.ScriptBlocks.SelectMany(obj => obj.CurrentConfig.Values)
                         .OfType<StringConfigValue<PngBlock>>())
            {
                if (!config.GetTypeId().Contains("_url")) continue;
                var cfg = config.GetValue();
                blockDownloads.TryAdd(cfg, config);
            }

            cherries += data.Placements.Count(p => p.ID.StartsWith("cherry_false"));
            goldCherries += data.Placements.Count(p => p.ID.StartsWith("cherry_true"));

            SaveScene(scene, data);
        }

        var gad = GlobalArchitectData.Instance;
        var levelId = gad.CurrentMapId;

        if (!gad.CherryScores.TryGetValue(levelId, out var score))
        {
            score = gad.CherryScores[levelId] = ([], cherries);
        }
        else score.Item2 = cherries;

        gad.CherryScores[levelId] = score;

        if (!gad.GoldCherryScores.TryGetValue(levelId, out var gscore))
        {
            gscore = gad.GoldCherryScores[levelId] = ([], goldCherries);
        }
        else score.Item2 = goldCherries;

        gad.GoldCherryScores[levelId] = gscore;

        Dictionary<string, string> workshopDownloads = [];
        foreach (var item in workshop.Items)
        {
            foreach (var cfg in item.CurrentConfig.Values) cfg.Setup(item);
            if (item.FilesToDownload == null) continue;
            foreach (var file in item.FilesToDownload)
            {
                if (file.Item1.IsNullOrWhiteSpace()) continue;
                workshopDownloads.TryAdd(file.Item1, file.Item2);
            }
        }

        CustomAssetManager.DownloadingAssets = 0;
        CustomAssetManager.Downloaded = 0;
        CustomAssetManager.Failed = 0;
        var downloadCount = downloads.Count + blockDownloads.Count + workshopDownloads.Count;
        status.text = "Downloading Assets...\n" +
                      $"0/{downloadCount}";

        foreach (var config in downloads.Values)
        {
            while (CustomAssetManager.DownloadingAssets > 4) yield return null;
            CustomAssetManager.DownloadingAssets++;
            Task.Run(() => CustomAssetManager.TryDownloadAssets(config, status, downloadCount));
        }

        foreach (var config in blockDownloads.Values)
        {
            while (CustomAssetManager.DownloadingAssets > 4) yield return null;
            CustomAssetManager.DownloadingAssets++;
            Task.Run(() => CustomAssetManager.TryDownloadAssets(config, status, downloadCount));
        }

        foreach (var (url, type) in workshopDownloads)
        {
            while (CustomAssetManager.DownloadingAssets > 4) yield return null;
            CustomAssetManager.DownloadingAssets++;
            Task.Run(() => CustomAssetManager.TryDownloadAssets(url, type, status, downloadCount));
        }

        while (CustomAssetManager.Downloaded < downloadCount) yield return null;

        var preloadCount = placeable.Count;
        var doneCount = 0;
        status.text = "Loading Objects...\n" + 
                      $"0/{preloadCount}";
        foreach (var preload in placeable)
        {
            yield return preload.EnsureLoaded();
            doneCount++;
            status.text = "Loading Objects...\n" + 
                          $"{doneCount}/{preloadCount}";
        }

        var elapsed = Time.realtimeSinceStartup - startTime;
        if (elapsed < 1) yield return new WaitForSeconds(1 - elapsed);

        PrefabsCategory.Prefabs = LoadPrefabs(DataPath);
        WorkshopManager.LoadWorkshop(workshop);
        SaveWorkshopData();

        var plural = CustomAssetManager.Failed == 1 ? "" : "s";
        status.text = "Download Complete" + (CustomAssetManager.Failed == 0
            ? ""
            : $"\n{CustomAssetManager.Failed} asset{plural} could not be downloaded");
    }

    public static void MakeBackup(string backupId)
    {
        try
        {
            var path = Path.Join(DataPath, "Backups", backupId);
            
            if (Directory.Exists(path)) Directory.Delete(path, true);
            
            Directory.CreateDirectory(Path.Join(path, "Scenes"));
            Directory.CreateDirectory(Path.Join(path, "Prefabs"));

            foreach (var file in Directory.GetFiles(Path.Join(DataPath, "Scenes")))
            {
                if (!file.EndsWith(".architect.json")) continue;
                File.Copy(file, Path.Join(path, "Scenes", Path.GetFileName(file)));
            }

            foreach (var file in Directory.GetFiles(Path.Join(DataPath, "Prefabs")))
            {
                if (!file.EndsWith(".architect.json")) continue;
                File.Copy(file, Path.Join(path, "Prefabs", Path.GetFileName(file)));
            }

            if (File.Exists(Path.Join(DataPath, "workshop.json")))
            {
                File.Copy(Path.Join(DataPath, "workshop.json"), Path.Join(path, "workshop.json"));
            }
        }
        catch
        {
            //
        }
    }

    public static void DeleteBackup(string backupId)
    {
        try
        {
            
            var path = Path.Join(DataPath, "Backups", backupId);
            Directory.Delete(path, true);
        }
        catch
        {
            //
        }
    }

    public static void LoadBackup(string backupId)
    {
        try
        {
            var path = Path.Join(DataPath, "Backups", backupId);
            
            WipeLevelData();

            foreach (var file in Directory.GetFiles(Path.Join(path, "Scenes")))
            {
                if (!file.EndsWith(".architect.json")) continue;
                File.Copy(file, Path.Join(DataPath, "Scenes", Path.GetFileName(file)));
            }

            foreach (var file in Directory.GetFiles(Path.Join(path, "Prefabs")))
            {
                if (!file.EndsWith(".architect.json")) continue;
                File.Copy(file, Path.Join(DataPath, "Prefabs", Path.GetFileName(file)));
            }

            if (File.Exists(Path.Join(path, "workshop.json")))
            {
                File.Copy(Path.Join(path, "workshop.json"), Path.Join(DataPath, "workshop.json"), true);
            }

            PrefabsCategory.Prefabs = LoadPrefabs(DataPath);
            LoadWorkshopData();

            if (!Settings.LoadAllAssets.Value)
            {
                FindLoadRequirements();
                PreloadManager.HasPreloaded = false;
                PreloadManager.FinishPreloading();
            }
        }
        catch
        {
            //
        }
    }

    public static void SaveApiKey([CanBeNull] string key)
    {
        var path = Path.Join(DataPath, "key.txt");
        if (File.Exists(path)) File.Delete(path);
        
        if (key == null) return;
        
        using var stream = File.Create(path);
        using var writer = new StreamWriter(stream);

        writer.Write(key);
    }

    [CanBeNull]
    public static string LoadSharerKey()
    {
        var path = Path.Combine(DataPath, "key.txt");
        return File.Exists(path) ? File.ReadAllText(path) : null;
    }
    
    public static void ScheduleEdit(string scene, IScheduledEdit edit)
    {
        if (!ScheduledEdits.TryGetValue(scene, out var edits)) edits = ScheduledEdits[scene] = [];
        edits.Add(edit);
    }
    
    public static LevelData ApplyScheduledEdits(string scene, List<IScheduledEdit> edits)
    {
        var data = LoadScene(scene);
        foreach (var edit in edits) edit.ExecuteScheduled(data);
        SaveScene(scene, data);

        return data;
    }
    
    public static void WipeScheduledEdits(string scene)
    {
        ScheduledEdits.Remove(scene);
    }
}