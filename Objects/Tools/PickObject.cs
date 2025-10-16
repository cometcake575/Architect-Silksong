using Architect.Editor;
using Architect.Placements;
using UnityEngine;

namespace Architect.Objects.Tools;

public class PickObject() : ToolObject("pick", Storage.Settings.Pick, -3)
{
    public static readonly PickObject Instance = new();
    
    public override string GetName()
    {
        return "Pick Tool";
    }

    public override string GetDescription()
    {
        return "Click a placed object to copy it to the cursor.\n\n" +
               "To edit a placed object without changing its ID or position select it with the Pick tool,\n" +
               "make changes and then hold 'O' and click the object to overwrite it.";
    }

    public override void Click(Vector3 mousePosition, bool first)
    {
        if (!first) return;
        var obj = PlacementManager.FindObject(mousePosition);
        if (obj == null) return;
        
        EditManager.TryFindEmptySlot();

        EditManager.ClearAttributes();
        
        EditManager.Broadcasters.AddRange(obj.Broadcasters);
        EditManager.Receivers.AddRange(obj.Receivers);
        foreach (var conf in obj.Config) EditManager.Config[conf.GetTypeId()] = conf;

        EditManager.SetRotation(obj.GetRotation());
        EditManager.SetScale(obj.GetScale());
        EditManager.CurrentlyFlipped = obj.IsFlipped();

        EditManager.CurrentObject = obj.GetPlacementType();
        
        EditorUI.RefreshAttributeControls(false);
        EditorUI.RefreshItem();
    }
}