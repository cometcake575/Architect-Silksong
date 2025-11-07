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
               "Make a selection with the Drag tool and click\nwith the Eraser to delete the selection.\n\n" +
               "Hold the Left Alt key to only delete the clicked object,\n" +
               "rather than all objects the cursor is dragged over.";
    }

    private List<ObjectPlacement> _erasedPlacements = [];
    
    public override void Click(Vector3 mousePosition, bool first)
    {
        if (!first && Input.GetKey(KeyCode.LeftAlt)) return;
        
        List<ObjectPlacement> placements = [];
        placements.AddRange(EditManager.SelectedObjects);
        EditManager.SelectedObjects.Clear();

        var placement = PlacementManager.FindObject(mousePosition);
        if (placement != null && !placements.Contains(placement)) placements.Add(placement);

        if (placements.Count == 0) return;
        
        _erasedPlacements.AddRange(placements);
        foreach (var o in placements) o.Destroy();
    }

    public override void Release()
    {
        ActionManager.PerformAction(new EraseObject(_erasedPlacements));
        _erasedPlacements = [];
    }
}