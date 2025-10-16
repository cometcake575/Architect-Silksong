using Architect.Editor;
using Architect.Placements;
using Architect.Storage;
using UnityEngine;

namespace Architect.Objects.Tools;

public class LockObject() : ToolObject("lock", Settings.Lock, -7)
{
    public static readonly LockObject Instance = new();
    
    public override string GetName()
    {
        return "Lock Tool";
    }

    public override string GetDescription()
    {
        return "Locks an object in place so it cannot be edited or selected in any way until unlocked.\n" +
               "This has no effect on actual gameplay, only edit mode.\n\n" +
               "Useful for things like large trigger zones that may get in the way of editing.";
    }

    public override void Click(Vector3 mousePosition, bool first)
    {
        if (!first) return;

        var obj = PlacementManager.FindObject(mousePosition, true);
        if (obj != null) ActionManager.PerformAction(new ToggleLock(obj));
    }
}