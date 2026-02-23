using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Architect.Config.Types;
using Architect.Editor;
using Architect.Events.Blocks.Config.Types;
using Architect.Events.Blocks.Outputs;
using Architect.Objects.Categories;
using Architect.Objects.Placeable;
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
        DataPath = Path.GetFullPath(Application.persistentDataPath + "/Architect/");
        Directory.CreateDirectory(DataPath + "Scenes/");
        Directory.CreateDirectory(DataPath + "Prefabs/");
        Directory.CreateDirectory(DataPath + "Assets/");
        Directory.CreateDirectory(DataPath + "Backups/");
        Directory.CreateDirectory(DataPath + "ModAssets/");
        
        typeof(GameManager).Hook(nameof(GameManager.SaveGame), 
            (Action<GameManager, Action<bool>> orig, GameManager self, Action<bool> callback) => 
            { 
                SaveFavourites(FavouritesCategory.Favourites);
                SavePrefabs(SavedCategory.Objects);

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

    private static string GetScenePath(string scene)
    {
        return DataPath + (scene.StartsWith("Prefab_") ? "Prefabs/" : "Scenes/") + scene + ".architect.json";
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
        if (scene.StartsWith("Prefab_")) PrefabsCategory.Add(scene.Replace("Prefab_", ""));
        
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
        var path = DataPath + "workshop.json";
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
        var path = DataPath + "workshop.json";
        if (File.Exists(path)) File.Delete(path);
        
        File.WriteAllText(path, JsonConvert.SerializeObject(WorkshopManager.WorkshopData, Formatting.Indented));
    }

    public static LevelData DeserializeLevel(string data)
    {
        return JsonConvert.DeserializeObject<LevelData>(data);
    }

    public static List<ObjectPlacement> DeserializePlacements(string data)
    {
        return JsonConvert.DeserializeObject<List<ObjectPlacement>>(data);
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
        var path = DataPath + "favourites.json";
        if (File.Exists(path)) File.Delete(path);

        var data = JsonConvert.SerializeObject(favourites);

        using var stream = File.Create(path);
        using var writer = new StreamWriter(stream);

        writer.Write(data);
    }

    public static List<string> LoadFavourites()
    {
        var path = DataPath + "favourites.json";
        if (File.Exists(path))
        {
            var deserialized = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(path));
            if (deserialized != null) return deserialized;
        }

        return [];
    }

    public static void SavePrefabs(List<SavedObject> prefabs)
    {
        var path = DataPath + "prefabs.json";
        if (File.Exists(path)) File.Delete(path);

        var data = SerializePlacements(prefabs.Select(obj => obj.Placement).ToList());

        using var stream = File.Create(path);
        using var writer = new StreamWriter(stream);

        writer.Write(data);
    }

    public static List<SavedObject> LoadSavedObjects()
    {
        var path = DataPath + "prefabs.json";
        if (File.Exists(path))
        {
            var deserialized = DeserializePlacements(File.ReadAllText(path));
            if (deserialized != null) return deserialized.Select(obj => new SavedObject(obj)).ToList();
        }

        return [];
    }

    public static List<PrefabObject> LoadPrefabs()
    {
        var path = DataPath + "/Prefabs";
        List<PrefabObject> prefabs = [];
        foreach (var file in Directory.GetFiles(path))
        {
            if (!file.EndsWith(".architect.json")) continue;
            prefabs.Add(new PrefabObject(Path.GetFileNameWithoutExtension(file)
                .Replace("Prefab_", "").Replace(".architect", "")));
        }
        
        return prefabs;
    }

    public static string SerializeAllScenes()
    {
        Dictionary<string, LevelData> data = [];
        foreach (var s in new[] { "Scenes/", "Prefabs/" })
        {
            foreach (var file in Directory.GetFiles(DataPath + s))
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
        foreach (var file in Directory.GetFiles(DataPath + "Scenes/")) File.Delete(file);
        foreach (var file in Directory.GetFiles(DataPath + "Prefabs/")) File.Delete(file);
        foreach (var file in Directory.GetFiles(DataPath + "Assets/")) File.Delete(file);

        CustomAssetManager.WipeAssets();
    }

    public static IEnumerator LoadLevelData(Dictionary<string, LevelData> levels, WorkshopData workshop, Text status)
    {
        WipeLevelData();
        var startTime = Time.realtimeSinceStartup;

        Dictionary<string, StringConfigValue> downloads = [];
        Dictionary<string, StringConfigValue<PngBlock>> blockDownloads = [];
        
        foreach (var (scene, data) in levels)
        {
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
            
            SaveScene(scene, data);
        }

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
            Task.Run(() => CustomAssetManager.TryDownloadAssets(config, status, downloadCount));
        }
        foreach (var config in blockDownloads.Values)
        {
            while (CustomAssetManager.DownloadingAssets > 4) yield return null;
            Task.Run(() => CustomAssetManager.TryDownloadAssets(config, status, downloadCount));
        }
        foreach (var (url, type) in workshopDownloads)
        {
            while (CustomAssetManager.DownloadingAssets > 4) yield return null;
            Task.Run(() => CustomAssetManager.TryDownloadAssets(url, type, status, downloadCount));
        }
        while (CustomAssetManager.Downloaded < downloadCount) yield return null;
        
        var elapsed = Time.realtimeSinceStartup - startTime;
        if (elapsed < 1) yield return new WaitForSeconds(1 - elapsed);

        PrefabsCategory.Prefabs = LoadPrefabs();
        WorkshopManager.LoadWorkshop(workshop);
        SaveWorkshopData();

        var plural = CustomAssetManager.Failed == 1 ? "" : "s";
        status.text = "Download Complete" + (CustomAssetManager.Failed == 0 ? "" : 
            $"\n{CustomAssetManager.Failed} asset{plural} could not be downloaded");
    }

    public static void MakeBackup()
    {
        try
        {
            var backupId = DateTime.Now.ToString("yy-MM-dd-HH-mm-ss");
            var path = DataPath + $"Backups/{backupId}";
            Directory.CreateDirectory($"{path}/Scenes");
            Directory.CreateDirectory($"{path}/Prefabs");

            foreach (var file in Directory.GetFiles(DataPath + "Scenes/"))
            {
                if (!file.EndsWith(".architect.json")) continue;
                File.Copy(file, path + "/Scenes/" + Path.GetFileName(file));
            }

            foreach (var file in Directory.GetFiles(DataPath + "Prefabs/"))
            {
                if (!file.EndsWith(".architect.json")) continue;
                File.Copy(file, path + "/Prefabs/" + Path.GetFileName(file));
            }

            if (File.Exists(DataPath + "workshop.json"))
            {
                File.Copy(DataPath + "workshop.json", path + "/workshop.json");
            }
        }
        catch
        {
            //
        }
    }

    public static void SaveApiKey([CanBeNull] string key)
    {
        var path = DataPath + "key.txt";
        if (File.Exists(path)) File.Delete(path);
        
        if (key == null) return;
        
        using var stream = File.Create(path);
        using var writer = new StreamWriter(stream);

        writer.Write(key);
    }

    [CanBeNull]
    public static string LoadSharerKey()
    {
        var path = DataPath + "key.txt";
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