using System;
using System.Collections.Generic;
using System.Linq;
using Architect.Events.Blocks;
using Architect.Multiplayer;
using Architect.Placements;
using Architect.Storage;
using Architect.Utils;
using UnityEngine;

namespace Architect.Editor;

public static class ActionManager
{
    private static readonly List<IEdit> Before = [];
    private static readonly List<IEdit> After = [];

    private static string _lastScene;

    public static void Init()
    {
        typeof(HeroController).Hook(nameof(HeroController.SceneInit),
            (Action<HeroController> orig, HeroController self) =>
            {
                orig(self);
                
                if (_lastScene != GameManager.instance.sceneName)
                {
                    _lastScene = GameManager.instance.sceneName;
                    Before.Clear();
                    After.Clear();
                }
            });
    }

    public static void UndoLast()
    {
        if (Before.Count == 0) return;

        var result = Before[^1].Undo();
        if (result != null)
        {
            result.Execute();
            MultiplayerShare(result);
            After.Add(result);
            Before.RemoveAt(Before.Count - 1);
        }
        else Before.Clear();
    }

    public static void RedoLast()
    {
        if (After.Count == 0) return;

        var result = After[^1].Undo();
        if (result != null)
        {
            result.Execute();
            MultiplayerShare(result);
            Before.Add(result);
        }
        else After.Clear();

        After.RemoveAt(After.Count - 1);
    }

    public static void PerformAction(IEdit edit)
    {
        _lastScene = GameManager.instance.sceneName;
        
        edit.Execute();
        MultiplayerShare(edit);
        
        After.Clear();
        Before.Add(edit);
    }

    public static void ReceiveAction(IEdit edit)
    {
        edit.Execute();
    }

    public static void MultiplayerShare(IEdit edit)
    {
        if (!CoopManager.Instance.IsActive()) return;
        edit.MultiplayerShare();
    }
}

public interface IEdit
{
    void Execute();
    
    IEdit Undo();

    void MultiplayerShare();
}

public interface IScheduledEdit
{
    void ExecuteScheduled(LevelData levelData);
}

public class PlaceObjects(List<ObjectPlacement> placements) : IEdit, IScheduledEdit
{
    public void Execute()
    {
        foreach (var obj in placements)
        {
            PlacementManager.GetLevelData().Placements.Add(obj);
            if (EditManager.IsEditing) obj.PlaceGhost();
        }
    }
    
    public void Execute(string scene)
    {
        StorageManager.ScheduleEdit(scene, this);
    }
    
    public IEdit Undo() => new EraseObject(placements);

    public void MultiplayerShare()
    {
        CoopManager.Instance.PlaceObjects(GameManager.instance.sceneName, placements);
    }

    public void ExecuteScheduled(LevelData levelData)
    {
        levelData.Placements.AddRange(placements);
    }
}

public class ToggleLock(ObjectPlacement placement) : IEdit
{
    public void Execute()
    {
        placement.ToggleLocked();
    }
    
    public static void Execute(string scene, string id)
    {
        StorageManager.ScheduleEdit(scene, new ScheduledToggleLock(id));
    }
    
    public IEdit Undo() => new ToggleLock(placement);

    public void MultiplayerShare()
    {
        CoopManager.Instance.ToggleLock(GameManager.instance.sceneName, placement.GetId());
    }

    public class ScheduledToggleLock(string id) : IScheduledEdit
    {
        public void ExecuteScheduled(LevelData levelData)
        {
            levelData.Placements.FirstOrDefault(o => o.GetId() == id)?.ToggleLocked();
        }
    }
}

public class EraseObject(List<ObjectPlacement> placements) : IEdit
{
    public void Execute()
    {
        foreach (var o in placements) o.Destroy();
    }
    
    public static void Execute(string scene, List<string> removals)
    {
        StorageManager.ScheduleEdit(scene, new ScheduledErase(removals));
    }
    
    public IEdit Undo() => new PlaceObjects(placements);

    public void MultiplayerShare()
    {
        CoopManager.Instance.EraseObjects(GameManager.instance.sceneName, 
            placements.Select(o => o.GetId()).ToList());
    }

    public class ScheduledErase(List<string> ids) : IScheduledEdit
    {
        public void ExecuteScheduled(LevelData levelData)
        {
            levelData.Placements.RemoveAll(o => ids.Remove(o.GetId()));
        }
    }
}

public class ToggleTile(List<(int, int)> tiles, bool empty) : IEdit, IScheduledEdit
{
    public void Execute()
    {
        var map = PlacementManager.GetTilemap();
        if (!map) return;
        foreach (var (x, y) in tiles)
        {
            if (empty) map.ClearTile(x, y, 0);
            else map.SetTile(x, y, 0, 0);
            
            PlacementManager.GetLevelData().ToggleTile((x, y));
        }
        map.Build();
    }
    
    public void Execute(string scene)
    {
        StorageManager.ScheduleEdit(scene, this);
    }
    
    public IEdit Undo() => new ToggleTile(tiles, !empty);

    public void MultiplayerShare()
    {
        CoopManager.Instance.ToggleTiles(GameManager.instance.sceneName, tiles, empty);
    }

    public void ExecuteScheduled(LevelData levelData)
    {
        foreach (var pos in tiles) levelData.ToggleTile(pos);
    }
}

public class MoveObjects(List<(ObjectPlacement, Vector3, Vector3)> data) : IEdit
{
    public void Execute()
    {
        // Dragging preview will already have moved to new position, this is used for undo/redo and multiplayer
        foreach (var (obj, pos, _) in data) obj.Move(pos);
    }
    
    public static void Execute(string scene, List<(string, Vector3)> movements)
    {
        StorageManager.ScheduleEdit(scene, new ScheduledMove(movements));
    }
    
    public IEdit Undo()
    {
        List<(ObjectPlacement, Vector3, Vector3)> reversed = [];
        foreach (var (obj, pos, oldPos) in data) reversed.Add((obj, oldPos, pos));
        
        return new MoveObjects(reversed);
    }

    public void MultiplayerShare()
    {
        CoopManager.Instance.MoveObjects(GameManager.instance.sceneName, data
            .Select(o => (o.Item1.GetId(), o.Item2)).ToList());
    }

    public class ScheduledMove(List<(string, Vector3)> data) : IScheduledEdit
    {
        public void ExecuteScheduled(LevelData levelData)
        {
            foreach (var (id, pos) in data) 
                levelData.Placements.FirstOrDefault(o => o.GetId() == id)?.Move(pos);
        }
    }
}

public class ResetRoom : IEdit
{
    public void Execute()
    {
        var data = PlacementManager.GetLevelData();
        while (data.Placements.Count > 0) data.Placements[0].Destroy();

        foreach (var scriptBlock in data.ScriptBlocks.ToArray())
        {
            scriptBlock.Delete();
        }
        data.ScriptBlocks.Clear();

        var map = PlacementManager.GetTilemap();
        
        if (!map) return;
        foreach (var (x, y) in data.TilemapChanges)
        {
            if (map.GetTile(x, y, 0) == -1) map.SetTile(x, y, 0, 0);
            else map.ClearTile(x, y, 0);
        }
        map.Build();
        
        data.TilemapChanges.Clear();
    }

    public static void Execute(string scene)
    {
        StorageManager.SaveScene(scene, new LevelData([], [], []));
    }

    public IEdit Undo() => null;

    public void MultiplayerShare()
    {
        CoopManager.Instance.ResetRoom(GameManager.instance.sceneName);
    }
}