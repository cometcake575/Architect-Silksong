using System;
using System.Collections.Generic;
using System.Linq;
using Architect.Api;
using Architect.Behaviour.Utility;
using Architect.Content.Custom;
using Architect.Content.Preloads;
using Architect.Editor;
using Architect.Events;
using Architect.Storage;
using Architect.Utils;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Architect.Placements;

public static class PlacementManager
{
    private static string _sceneName;
    private static LevelData _levelData;

    private static tk2dTileMap _tileMap;

    public static readonly Dictionary<string, GameObject> Objects = [];
    
    // ReSharper disable once UnassignedField.Global
    public static Action<string, string, GameObject> OnPlace;

    public static LevelData GetLevelData()
    {
        var sceneName = GameManager.instance.sceneName;
        if (_sceneName == sceneName) return _levelData;

        if (EditManager.IsEditing) StorageManager.SaveScene(_sceneName, _levelData);
        _sceneName = sceneName;
        _levelData = StorageManager.LoadScene(sceneName);

        return _levelData;
    }

    public static tk2dTileMap GetTilemap()
    {
        if (!_tileMap) _tileMap = Object.FindAnyObjectByType<tk2dTileMap>();
        return _tileMap;
    }

    public static ObjectPlacement GetPlacement(string id)
    {
        return _levelData.Placements.FirstOrDefault(o => o.GetId() == id);
    }

    public static void InvalidateScene()
    {
        _sceneName = "Invalid";
    }

    private static void LoadLevel(LevelData data, string sceneName)
    {
        var ext = MapLoader.GetModData(sceneName);
        
        EventManager.ResetReceivers();
        PlayerHook.PlayerListeners.Clear();
        AbilityObjects.ActiveCrystals.Clear();
        AbilityObjects.RefreshCrystalUI();
        
        Objects.Clear();

        if (ext != null)
        {
            foreach (var placement in ext.Placements)
            {
                var obj = placement.SpawnObject();
                if (obj)
                {
                    Objects[placement.GetId()] = obj;
                    OnPlace?.Invoke(placement.GetPlacementType().GetId(), placement.GetId(), obj);
                }
            }
        }

        foreach (var placement in data.Placements)
        {
            if (EditManager.IsEditing) placement.PlaceGhost();
            else
            {
                var obj = placement.SpawnObject();
                if (obj)
                {
                    Objects[placement.GetId()] = obj;
                    OnPlace?.Invoke(placement.GetPlacementType().GetId(), placement.GetId(), obj);
                }
            }
        }

        var map = GetTilemap();
        if (!map) return;

        if (ext != null && !ext.TilemapChanges.IsNullOrEmpty())
        {
            foreach (var (x, y) in ext.TilemapChanges)
            {
                if (map.GetTile(x, y, 0) == -1) map.SetTile(x, y, 0, 0);
                else map.ClearTile(x, y, 0);
            }
        }

        if (!data.TilemapChanges.IsNullOrEmpty())
        {
            foreach (var (x, y) in data.TilemapChanges)
            {
                if (map.GetTile(x, y, 0) == -1) map.SetTile(x, y, 0, 0);
                else map.ClearTile(x, y, 0);
            }
        }

        map.Build();
    }

    public static void Init()
    {
        typeof(HeroController).Hook(nameof(HeroController.SceneInit),
            (Action<HeroController> orig, HeroController self) =>
            {
                orig(self);
                
                if (!PreloadManager.HasPreloaded) return;
                LoadLevel(GetLevelData(), GameManager.instance.sceneName);
            });
    }

    [CanBeNull]
    public static ObjectPlacement FindObject(Vector3 mousePos, bool includeLocked = false)
    {
        return GetLevelData().Placements.FirstOrDefault(placement => (includeLocked || !placement.Locked) 
                                                                     && placement.Touching(mousePos));
    }
}