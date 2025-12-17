using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Architect.Config.Types;
using Architect.Editor;
using Architect.Objects.Categories;
using Architect.Objects.Placeable;
using Architect.Placements;
using Architect.Storage.Sharer;
using Architect.Utils;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace Architect.Storage;

public static class StorageManager
{
    internal static readonly LevelData.LevelDataConverter Ldc = new();
    internal static readonly ObjectPlacement.ObjectPlacementConverter Opc = new();
    
    public static string DataPath;
    
    private static readonly Dictionary<string, List<IScheduledEdit>> ScheduledEdits = [];
    
    
    public static void Init()
    {
        DataPath = Path.GetFullPath(Application.persistentDataPath + "/Architect/");
        Directory.CreateDirectory(DataPath + "Scenes/");
        Directory.CreateDirectory(DataPath + "Assets/");
        Directory.CreateDirectory(DataPath + "ModAssets/");
        
        typeof(GameManager).Hook(nameof(GameManager.SaveGame), 
            (Action<GameManager, Action<bool>> orig, GameManager self, Action<bool> callback) => 
            { 
                SaveFavourites(FavouritesCategory.Favourites);
                SavePrefabs(PrefabsCategory.Prefabs);

                if (EditManager.IsEditing)
                {
                    SaveScene(GameManager.instance.sceneName, PlacementManager.GetLevelData());
                }

                foreach (var (scene, edits) in ScheduledEdits)
                {
                    ApplyScheduledEdits(scene, edits);
                }
                ScheduledEdits.Clear();

                orig(self, callback);
            }, typeof(Action<bool>));
    }
    
    public static void SaveScene(string scene, LevelData level)
    {
        var path = DataPath + "Scenes/" + scene + ".architect.json";
        if (File.Exists(path)) File.Delete(path);

        if (level.Placements.IsNullOrEmpty() && level.TilemapChanges.IsNullOrEmpty()) return;
        
        var data = SerializeLevel(level, Formatting.Indented);
        
        using var stream = File.Create(path);
        using var writer = new StreamWriter(stream);

        writer.Write(data);
    }

    public static LevelData LoadScene(string scene)
    {
        if (ScheduledEdits.Remove(scene, out var edits)) return ApplyScheduledEdits(scene, edits);
        
        var path = DataPath + "Scenes/" + scene + ".architect.json";

        return File.Exists(path) ? DeserializeLevel(File.ReadAllText(path)) : 
            new LevelData([], []);
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

    public static void SavePrefabs(List<PrefabObject> prefabs)
    {
        var path = DataPath + "prefabs.json";
        if (File.Exists(path)) File.Delete(path);

        var data = SerializePlacements(prefabs.Select(obj => obj.Placement).ToList());

        using var stream = File.Create(path);
        using var writer = new StreamWriter(stream);

        writer.Write(data);
    }

    public static List<PrefabObject> LoadPrefabs()
    {
        var path = DataPath + "prefabs.json";
        if (File.Exists(path))
        {
            var deserialized = DeserializePlacements(File.ReadAllText(path));
            if (deserialized != null) return deserialized.Select(obj => new PrefabObject(obj)).ToList();
        }

        return [];
    }

    public static string SerializeAllScenes()
    {
        Dictionary<string, LevelData> data = [];
        foreach (var file in Directory.GetFiles(DataPath + "Scenes/"))
        {
            var name = Path.GetFileName(file);
            if (!name.EndsWith(".architect.json")) continue;

            var n = name.Replace(".architect.json", "");
            data[n] = LoadScene(n);
        }

        return JsonConvert.SerializeObject(data, Formatting.None, Ldc, Opc);
    }

    private static void WipeLevelData()
    {
        foreach (var file in Directory.GetFiles(DataPath + "Scenes/")) File.Delete(file);
        foreach (var file in Directory.GetFiles(DataPath + "Assets/")) File.Delete(file);

        CustomAssetManager.WipeAssets();
    }

    public static IEnumerator LoadLevelData(Dictionary<string, LevelData> levels, string levelId, Text status)
    {
        WipeLevelData();
        var startTime = Time.realtimeSinceStartup;

        Dictionary<string, StringConfigValue> downloads = [];
        
        foreach (var (scene, data) in levels)
        {
            foreach (var config in data.Placements.SelectMany(obj => obj.Config)
                         .OfType<StringConfigValue>())
            {
                if (!config.GetTypeId().Contains("_url")) continue;
                var cfg = config.GetValue();
                downloads.TryAdd(cfg, config);
            }
            
            SaveScene(scene, data);
        }

        CustomAssetManager.DownloadingAssets = 0;
        CustomAssetManager.Downloaded = 0;
        CustomAssetManager.Failed = 0;
        var downloadCount = downloads.Count;
        
        foreach (var config in downloads.Values)
        {
            while (CustomAssetManager.DownloadingAssets > 4) yield return null;
            Task.Run(() => CustomAssetManager.TryDownloadAssets(config, status, downloadCount));
        }
        while (CustomAssetManager.Downloaded < downloadCount) yield return null;
        
        var elapsed = Time.realtimeSinceStartup - startTime;
        if (elapsed < 1) yield return new WaitForSeconds(1 - elapsed);

        LevelSharerUI.CurrentlyDownloading = false;
        LevelSharerUI.RefreshActiveOptions();

        var plural = CustomAssetManager.Failed == 1 ? "" : "s";
        status.text = "Download Complete" + (CustomAssetManager.Failed == 0 ? "" : 
            $"\n{CustomAssetManager.Failed} asset{plural} could not be downloaded");
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
    public static string LoadApiKey()
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