using System.Collections;
using Architect.Editor;
using Architect.Events.Blocks;
using Architect.Events.Blocks.Objects;
using Architect.Placements;
using UnityEngine;

namespace Architect.Objects.Tools;

public class CursorObject() : ToolObject("cursor", Storage.Settings.Cursor, -1)
{
    public static readonly CursorObject Instance = new();
    
    public override string GetName()
    {
        return "Cursor";
    }

    public override string GetDescription()
    {
        return "Click a placed object to see its ID.\n\n" +
               "Right click a placed object to add its block to the Script Editor.\n\n" +
               "Click a spot to see its position.";
    }

    private static int _lastNum;

    public override void Click(Vector3 mousePosition, bool first)
    {
        var obj = PlacementManager.FindObject(mousePosition);
        string info;
        if (obj == null)
        {
            var pos = EditManager.GetWorldPos(mousePosition);
            info = $"X: {pos.x}, Y: {pos.y}";
        }
        else info = $"{obj.GetPlacementType().GetName()} ID: {obj.GetId()}";
        EditorUI.ObjectIdLabel.textComponent.text = info;
        ArchitectPlugin.Instance.StartCoroutine(ClearCursorInfoLabel());
    }

    public override void RightClick(Vector3 mousePosition)
    {
        var obj = PlacementManager.FindObject(mousePosition);
        if (obj == null) return;
        var wasLocal = ScriptManager.IsLocal;
        if (!wasLocal) ScriptManager.IsLocal = true;
        
        EditorUI.ObjectIdLabel.textComponent.text = $"{obj.GetPlacementType().GetName()} added";
        ArchitectPlugin.Instance.StartCoroutine(ClearCursorInfoLabel());

        var block = new ObjectBlock
        {
            TypeId = obj.GetPlacementType().GetId(),
            TargetId = obj.GetId(),
            Type = "object"
        };
        block.Setup(true);
        PlacementManager.GetLevelData().ScriptBlocks.Add(block);

        if (!wasLocal) ScriptManager.IsLocal = false;
    }

    private static IEnumerator ClearCursorInfoLabel()
    {
        _lastNum++;
        var n = _lastNum;
        yield return new WaitForSeconds(10);
        if (_lastNum == n) EditorUI.ObjectIdLabel.textComponent.text = "";
    }
}