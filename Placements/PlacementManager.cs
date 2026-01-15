using System;
using System.Collections.Generic;
using System.Linq;
using Architect.Api;
using Architect.Behaviour.Utility;
using Architect.Content.Custom;
using Architect.Content.Preloads;
using Architect.Editor;
using Architect.Events;
using Architect.Events.Blocks;
using Architect.Prefabs;
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
    private static LevelData _globalData;

    private static tk2dTileMap _tileMap;

    public static readonly Dictionary<string, GameObject> Objects = [];
    
    public static readonly Dictionary<string, ObjectPlacement> PrefabPlacements = [];
    
    // ReSharper disable once UnassignedField.Global
    public static Action<string, string, GameObject> OnPlace;

    public static LevelData GetLevelData()
    {
        VerifyLevelData();
        return _levelData;
    }

    public static LevelData GetGlobalData()
    {
        VerifyLevelData();
        return _globalData;
    }

    private static void VerifyLevelData()
    {
        var sceneName = GameManager.instance.sceneName;
        if (_sceneName == sceneName) return;

        if (EditManager.IsEditing)
        {
            StorageManager.SaveScene(_sceneName, _levelData);
            StorageManager.SaveScene(StorageManager.GLOBAL, _globalData);
        }
        _sceneName = sceneName;
        _levelData = StorageManager.LoadScene(sceneName);
        _globalData = StorageManager.LoadScene(StorageManager.GLOBAL);
    }

    public static tk2dTileMap GetTilemap()
    {
        if (!_tileMap) _tileMap = Object.FindAnyObjectByType<tk2dTileMap>();
        return _tileMap;
    }

    public static ObjectPlacement GetPlacement(string id)
    {
        var placement =  _levelData.Placements.FirstOrDefault(o => o.GetId() == id);
        if (placement == null)
        {
            if (PrefabPlacements.TryGetValue(id, out var b)) return b;
        }
        return placement;
    }

    public static void InvalidateScene()
    {
        _sceneName = "Invalid";
    }

    private static void LoadLevel(string sceneName)
    {
        VerifyLevelData();
        
        PrefabManager.Prefabs.Clear();
        PrefabPlacements.Clear();
        
        var extGlobal = MapLoader.GetModData(StorageManager.GLOBAL);
        var ext = MapLoader.GetModData(sceneName);
        
        EventManager.ResetReceivers();
        PlayerHook.PlayerListeners.Clear();
        AbilityObjects.ActiveCrystals.Clear();
        AbilityObjects.RefreshCrystalUI();
        
        Objects.Clear();

        foreach (var block in ScriptManager.Blocks.Values)
        {
            block.DestroyObject();
        }
        foreach (var link in ScriptManager.Links.Values)
        {
            Object.Destroy(link);
        }
        ScriptManager.Blocks.Clear();

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

        foreach (var placement in _levelData.Placements)
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
        if (map)
        {
            if (ext != null && !ext.TilemapChanges.IsNullOrEmpty())
            {
                foreach (var (x, y) in ext.TilemapChanges)
                {
                    if (map.GetTile(x, y, 0) == -1) map.SetTile(x, y, 0, 0);
                    else map.ClearTile(x, y, 0);
                }
            }

            if (!_levelData.TilemapChanges.IsNullOrEmpty())
            {
                foreach (var (x, y) in _levelData.TilemapChanges)
                {
                    if (map.GetTile(x, y, 0) == -1) map.SetTile(x, y, 0, 0);
                    else map.ClearTile(x, y, 0);
                }
            }

            map.Build();
        }

        var wasLocal = ScriptManager.IsLocal;

        ScriptManager.IsLocal = true;
        if (ext != null) foreach (var block in ext.ScriptBlocks) block.Setup(false);
        
        foreach (var block in _levelData.ScriptBlocks) block.Setup(EditManager.IsEditing);
        foreach (var block in _levelData.ScriptBlocks) block.LateSetup();

        ScriptManager.IsLocal = false;
        if (extGlobal != null) foreach (var block in extGlobal.ScriptBlocks) block.Setup(false);
        
        foreach (var block in _globalData.ScriptBlocks) block.Setup(EditManager.IsEditing);
        foreach (var block in _globalData.ScriptBlocks) block.LateSetup();

        ScriptManager.IsLocal = wasLocal;
    }

    public static void Init()
    {
        typeof(HeroController).Hook(nameof(HeroController.SceneInit),
            (Action<HeroController> orig, HeroController self) =>
            {
                orig(self);
                
                if (!PreloadManager.HasPreloaded) return;
                LoadLevel(GameManager.instance.sceneName);
            });
    }

    [CanBeNull]
    public static ObjectPlacement FindObject(Vector3 mousePos, int includeLocked = 0)
    {
        return GetLevelData().Placements.FirstOrDefault(placement => (includeLocked == 1 || 
                                                                      placement.Locked == (includeLocked == 2)) 
                                                                     && placement.Touching(mousePos));
    }
}