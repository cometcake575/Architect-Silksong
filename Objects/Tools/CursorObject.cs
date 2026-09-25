using System.Collections;
using System.Globalization;
using Architect.Editor;
using Architect.Events.Blocks;
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
    
    public override bool Highlight => true;

    public override string GetDescription()
    {
        return "Click a placed object to see its ID, shift + click to edit it.\n\n" +
               "Right click a placed object to add its block to the Script Editor.\n\n" +
               "Click a spot to see its position.";
    }

    private static int _lastNum;
    private Vector3 _startPos;
    
    public override void Click(Vector3 mousePosition, bool first)
    {
        if (!first || EditManager.HoveredObject == null)
        {
            var pos = EditManager.GetWorldPos(mousePosition);
            if (first) _startPos = pos;
            
            EditorUI.DisplayHotbarText($"X: {pos.x}, Y: {pos.y}\n" +
                                       $"Distance: {(pos - _startPos).magnitude}");
        }
        else
        {
            var obj = EditManager.HoveredObject;
            
            var info = $"{obj.GetPlacementType().GetName()} ID: {obj.GetId()}";
            if (Input.GetKey(KeyCode.LeftShift))
            {
                EditManager.HoveredObject = null;
                EditManager.EditingObject = obj;

                EditorUI.PosXText.text = obj.GetPos().x.ToString(CultureInfo.InvariantCulture);
                EditorUI.PosYText.text = obj.GetPos().y.ToString(CultureInfo.InvariantCulture);
                EditorUI.PositionOptions.SetActive(true);

                obj.LoadToSlot();

                EditorUI.ObjectIdLabel.textComponent.text = info;

                CursorManager.NeedsRefresh = false;
            } else EditorUI.DisplayHotbarText(info);
        }
    }

    public override void RightClick(Vector3 mousePosition)
    {
        var obj = PlacementManager.FindObject(mousePosition);
        if (obj == null) return;
        ActionManager.ScriptActionManager.PerformAction(ScriptManager.AddToScript(obj));
    }

    public static IEnumerator ClearCursorInfoLabel()
    {
        _lastNum++;
        var n = _lastNum;
        yield return new WaitForSeconds(10);
        if (_lastNum == n) EditorUI.ObjectIdLabel.textComponent.text = "";
    }
}