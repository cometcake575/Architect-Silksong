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
               "Right click a placed object to add its block to the Script Editor.";
    }

    private static int _lastNum;

    public override void Click(Vector3 mousePosition, bool first)
    {
        var obj = PlacementManager.FindObject(mousePosition);
        if (obj == null) return;
        var id = obj.GetId();
        EditorUI.ObjectIdLabel.textComponent.text = $"{obj.GetPlacementType().GetName()} ID: {id}";
        ArchitectPlugin.Instance.StartCoroutine(ClearCursorInfoLabel());
    }

    public override void RightClick(Vector3 mousePosition)
    {
        var obj = PlacementManager.FindObject(mousePosition);
        if (obj == null) return;
        EditorUI.ObjectIdLabel.textComponent.text = $"{obj.GetPlacementType().GetName()} added";
        ArchitectPlugin.Instance.StartCoroutine(ClearCursorInfoLabel());

        var block = new ObjectBlock(obj.GetId());
        block.Setup();
        PlacementManager.GetLevelData().ScriptBlocks.Add(block);
    }

    private static IEnumerator ClearCursorInfoLabel()
    {
        _lastNum++;
        var n = _lastNum;
        yield return new WaitForSeconds(10);
        if (_lastNum == n) EditorUI.ObjectIdLabel.textComponent.text = "";
    }
}