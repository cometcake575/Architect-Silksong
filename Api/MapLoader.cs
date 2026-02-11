using System;
using System.Collections.Generic;
using System.Reflection;
using Architect.Placements;
using Architect.Storage;
using Encoding = System.Text.Encoding;

namespace Architect.Api;

public static class MapLoader
{
    private static readonly Dictionary<string, Dictionary<Assembly, string>> ModMaps = [];
    
    public static void LoadMap(Assembly asm, string sceneName, string mapPath)
    {
        if (!ModMaps.ContainsKey(sceneName)) ModMaps[sceneName] = [];
        ModMaps[sceneName][asm] = mapPath;
    }

    private static readonly Dictionary<string, HashSet<Func<LevelData>>> ExtMapLoadersByScene = [];
    private static readonly HashSet<Func<string, LevelData>> ExtMapLoaders = [];

    public static void AddMapLoader(string scene, Func<LevelData> mapLoader)
    {
        if (ExtMapLoadersByScene.TryGetValue(scene, out var loaders)) loaders.Add(mapLoader);
        else ExtMapLoadersByScene.Add(scene, [mapLoader]);
    }
    public static void RemoveMapLoader(string scene, Func<LevelData> mapLoader)
    {
        if (ExtMapLoadersByScene.TryGetValue(scene, out var loaders) && loaders.Remove(mapLoader) && loaders.Count == 0) ExtMapLoadersByScene.Remove(scene);
    }

    public static void AddMapLoader(Func<string, LevelData> mapLoader) => ExtMapLoaders.Add(mapLoader);
    public static void RemoveMapLoader(Func<string, LevelData> mapLoader) => ExtMapLoaders.Remove(mapLoader);
    
    public static LevelData GetModData(string scene)
    {
        LevelData levelData = new([], [], [], []);

        void TryLoadMap(Func<LevelData> loader)
        {
            try
            {
                var level = loader();
                if (level != null) levelData.Merge(level);
            }
            catch (Exception ex)
            {
                ArchitectPlugin.Logger.LogError($"External map loader failed on '{scene}': {ex}");
            }
        }

        foreach (var mapLoader in ExtMapLoaders)
        {
            TryLoadMap(() => mapLoader(scene));
        }

        if (ExtMapLoadersByScene.TryGetValue(scene, out var mapLoaders))
        {
            foreach (var mapLoader in mapLoaders)
            {
                TryLoadMap(mapLoader);
            }
        }

        if (ModMaps.TryGetValue(scene, out var map))
        {
            foreach (var (asm, mapPath) in map)
            {
                TryLoadMap(() =>
                {
                    using var s = asm.GetManifestResourceStream(mapPath);
                    if (s == null) return null;
                    var buffer = new byte[s.Length];
                    _ = s.Read(buffer, 0, buffer.Length);

                    var json = Encoding.UTF8.GetString(buffer);
                    return StorageManager.DeserializeLevel(json);
                });
            }
        }

        return levelData;
    }
}