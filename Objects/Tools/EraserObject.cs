using System.Collections.Generic;
using Architect.Editor;
using Architect.Placements;
using UnityEngine;

namespace Architect.Objects.Tools;

public class EraserObject() : ToolObject("eraser", Storage.Settings.Eraser, -2)
{
    public static readonly EraserObject Instance = new();
    
    public override string GetName()
    {
        return "Eraser";
    }

    public override string GetDescription()
    {
        return "Click or drag over a placed object to delete it.\n\n" +
               "Make a selection with the Drag tool and click\nwith the Eraser to delete the selection.";
    }

    public override void Click(Vector3 mousePosition, bool first)
    {
        List<ObjectPlacement> placements = [];
        placements.AddRange(EditManager.SelectedObjects);
        EditManager.SelectedObjects.Clear();

        var placement = PlacementManager.FindObject(mousePosition);
        if (placement != null && !placements.Contains(placement)) placements.Add(placement);

        if (placements.Count == 0) return;

        ActionManager.PerformAction(new EraseObject(placements));
    }
}