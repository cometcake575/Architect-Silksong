using Architect.Editor;
using Architect.Placements;
using Architect.Storage;
using UnityEngine;

namespace Architect.Objects.Tools;

public class DragObject() : ToolObject("drag", Settings.Drag, -4)
{
    public static readonly DragObject Instance = new();
    
    public override string GetName()
    {
        return "Drag Tool";
    }

    public override string GetDescription()
    {
        return "Click and drag a placed object to move it.\n\n" +
               "Hold Left Control to select multiple, or drag a box over an area.\n\n" +
               "Press C to copy and V to paste the current selection.";
    }

    public override void Click(Vector3 mousePosition, bool first)
    {
        if (!first) return;

        var obj = PlacementManager.FindObject(mousePosition);

        var ms = Settings.MultiSelect.IsPressed;
        if (obj != null && (!EditManager.SelectedObjects.Contains(obj) || ms)) EditManager.ToggleSelectedObject(
            obj,
            !ms);
        else if (EditManager.SelectedObjects.Count == 0) EditManager.StartGroupSelect();
        else EditManager.BeginDragging();
    }
}