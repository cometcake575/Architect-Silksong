using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Architect.Placements;
using Architect.Storage;
using JetBrains.Annotations;
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

    [CanBeNull]
    public static LevelData GetModData(string scene)
    {
        if (!ModMaps.TryGetValue(scene, out var map)) return null;

        var ld = new LevelData([], [], []);
        foreach (var (asm, mapPath) in map)
        {
            using var s = asm.GetManifestResourceStream(mapPath);
            if (s == null) continue;
            var buffer = new byte[s.Length];
            _ = s.Read(buffer, 0, buffer.Length);

            var json = Encoding.UTF8.GetString(buffer);
            var level = StorageManager.DeserializeLevel(json);
            
            ld.Placements.AddRange(level.Placements);
            ld.TilemapChanges.AddRange(level.TilemapChanges.Where(t => !ld.TilemapChanges.Contains(t)));
            ld.ScriptBlocks.AddRange(level.ScriptBlocks);
        }

        return ld;
    }
}