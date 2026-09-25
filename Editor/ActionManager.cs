using System;
using System.Collections.Generic;
using System.Linq;
using Architect.Events.Blocks;
using Architect.Multiplayer;
using Architect.Placements;
using Architect.Storage;
using Architect.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Architect.Editor;

public class ActionManager
{
    private static readonly List<ActionManager> Managers = [];
    
    public static readonly ActionManager SceneActionManager = new(true);
    public static readonly ActionManager ScriptActionManager = new(false);
    
    private readonly List<IEdit> _before = [];
    private readonly List<IEdit> _after = [];

    private static string _lastScene;
    private readonly bool _mpShare;

    private ActionManager(bool mpShare)
    {
        Managers.Add(this);
        _mpShare = mpShare;
    }

    public static void Init()
    {
        typeof(HeroController).Hook(nameof(HeroController.SceneInit),
            (Action<HeroController> orig, HeroController self) =>
            {
                orig(self);
                if (GameManager.instance.sceneName == "Temp" || _lastScene == "Temp") return;
                
                if (_lastScene != GameManager.instance.sceneName)
                {
                    _lastScene = GameManager.instance.sceneName;
                    foreach (var manager in Managers)
                    {
                        manager._before.Clear();
                        manager._after.Clear();
                    }
                }
            });
    }

    public static void UndoLast()
    {
        switch (EditorUI.CurrentType)
        {
            case EditorUI.EditorType.Map:
                SceneActionManager.Undo();
                break;
            case EditorUI.EditorType.Script:
                ScriptActionManager.Undo();
                break;
        }
    }

    public static void RedoLast()
    {
        switch (EditorUI.CurrentType)
        {
            case EditorUI.EditorType.Map:
                SceneActionManager.Redo();
                break;
            case EditorUI.EditorType.Script:
                ScriptActionManager.Redo();
                break;
        }
    }

    private void Undo()
    {
        if (_before.Count == 0) return;

        var result = _before[^1].Undo();
        if (result != null)
        {
            result.Execute();
            MultiplayerShare(result);
            _after.Add(result);
            _before.RemoveAt(_before.Count - 1);
        }
        else _before.Clear();
    }

    private void Redo()
    {
        if (_after.Count == 0) return;

        var result = _after[^1].Undo();
        if (result != null)
        {
            result.Execute();
            MultiplayerShare(result);
            _before.Add(result);
        }
        else _after.Clear();

        _after.RemoveAt(_after.Count - 1);
    }

    public void PerformAction(IEdit edit)
    {
        if (edit == null) return;
        _lastScene = GameManager.instance.sceneName;
        
        edit.Execute();
        MultiplayerShare(edit);
        
        _after.Clear();
        _before.Add(edit);
    }

    public static void ReceiveAction(IEdit edit)
    {
        edit.Execute();
    }

    private void MultiplayerShare(IEdit edit)
    {
        if (!_mpShare || !CoopManager.Instance.IsActive()) return;
        edit.MultiplayerShare();
    }
}

public interface IEdit
{
    void Execute();
    
    IEdit Undo();

    void MultiplayerShare() { }
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
            obj.ClearDraggedColour();
            obj.ClearHoverColour();
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

    private class ScheduledToggleLock(string id) : IScheduledEdit
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
        foreach (var o in placements)
        {
            PlacementManager.GetPlacement(o.GetId())?.Destroy();
        }
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

    private class ScheduledErase(List<string> ids) : IScheduledEdit
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
            try
            {
                if (empty) map.ClearTile(x, y, 0);
                else map.SetTile(x, y, 0, 0);
            }
            catch (Exception)
            {
                // Out of bounds
            }

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
        foreach (var (obj, pos, _) in data)
        {
            PlacementManager.GetPlacement(obj.GetId())?.Move(pos);
        }
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

    private class ScheduledMove(List<(string, Vector3)> data) : IScheduledEdit
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
            try
            {
                if (map.GetTile(x, y, 0) == -1) map.SetTile(x, y, 0, 0);
                else map.ClearTile(x, y, 0);
            }
            catch (Exception)
            {
                // Out of bounds
            }
        }

        map.Build();
        
        data.TilemapChanges.Clear();
    }

    public static void Execute(string scene)
    {
        StorageManager.SaveScene(scene, new LevelData([], [], [], []));
    }

    public IEdit Undo() => null;

    public void MultiplayerShare()
    {
        CoopManager.Instance.ResetRoom(GameManager.instance.sceneName);
    }
}

public class MultiEdit(IEnumerable<IEdit> edits) : IEdit
{
    public void Execute()
    {
        foreach (var edit in edits) edit?.Execute();
    }

    public IEdit Undo()
    {
        return new MultiEdit(edits.Select(e => e.Undo()).Reverse());
    }

    public void MultiplayerShare()
    {
        foreach (var edit in edits) edit.MultiplayerShare();
    }
}

public class PlaceScriptBlock(ScriptBlock block, bool local, bool needsSetup = false) : IEdit
{
    public void Execute()
    {
        ScriptManager.IsLocal = local;
        (local ? PlacementManager.GetLevelData() : PlacementManager.GetGlobalData()).ScriptBlocks.Add(block);
        if (needsSetup) block.Setup(true);
    }

    public IEdit Undo()
    {
        return new RemoveScriptBlock(block, local);
    }
}

public class RemoveScriptBlock(ScriptBlock block, bool local) : IEdit
{
    public void Execute()
    {
        ScriptManager.IsLocal = local;
        ScriptManager.Blocks.Remove(block.BlockId);
        (local ? PlacementManager.GetLevelData() : PlacementManager.GetGlobalData()).ScriptBlocks.Remove(block);
        if (block.BlockObject) Object.Destroy(block.BlockObject);
    }

    public IEdit Undo()
    {
        return new PlaceScriptBlock(block, local, true);
    }
}

public class ConnectScriptBlock(ScriptBlock from, string fromPoint, ScriptBlock to, string toPoint, ScriptManager.Connection.LinkType linkType, bool local) : IEdit
{
    public void Execute()
    {
        ScriptManager.IsLocal = local;
        if (linkType == ScriptManager.Connection.LinkType.Var)
        {
            var vMap = from.VarMap;
            if (!vMap.ContainsKey(fromPoint)) vMap[fromPoint] = (to.BlockId, toPoint);
        }
        else
        {
            var eMap = from.EventMap;
            if (!eMap.ContainsKey(fromPoint)) eMap[fromPoint] = [];
            eMap[fromPoint].AddIfNotPresent((to.BlockId, toPoint));
        }
        
        ScriptManager.MakeLink(from, fromPoint, to, toPoint, linkType);
    }
    
    public IEdit Undo()
    {
        return new DisconnectScriptBlock(from, fromPoint, to, toPoint, linkType, local);
    }
}

public class DisconnectScriptBlock(ScriptBlock from, string fromPoint, ScriptBlock to, string toPoint, ScriptManager.Connection.LinkType linkType, bool local) : IEdit
{
    public void Execute()
    {
        ScriptManager.IsLocal = local;
        ScriptManager.DestroyLink(from.BlockId, fromPoint, to.BlockId, toPoint, linkType);
    }

    public IEdit Undo()
    {
        return new ConnectScriptBlock(from, fromPoint, to, toPoint, linkType, local);
    }
}
