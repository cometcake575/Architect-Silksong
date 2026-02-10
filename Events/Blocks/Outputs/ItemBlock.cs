using System.Collections.Generic;
using Architect.Utils;
using UnityEngine;

namespace Architect.Events.Blocks.Outputs;

public class ItemBlock : ScriptBlock
{
    protected override IEnumerable<string> Inputs => ["Give", "GiveSilent", "Take", "Clear"];
    protected override IEnumerable<(string, string)> OutputVars => [
        ("Obtained", "Boolean")
    ];

    private static readonly Color DefaultColor = new(0.6f, 0.2f, 0.9f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Item Control";

    protected override void Reset()
    {
        ItemName = "";
    }

    public string ItemName;

    protected override object GetValue(string id)
    {
        var item = MiscUtils.GetSavedItem(ItemName);
        return item switch
        {
            MateriumItem i => i.IsCollected,
            CollectableItem i => i.CollectedAmount > 0,
            ToolItem i => i.IsUnlocked,
            _ => null
        };
    }

    protected override void Trigger(string trigger)
    {
        var item = MiscUtils.GetSavedItem(ItemName);
        if (!item) return;
        switch (trigger)
        {
            case "Give":
                item.Get();
                break;
            case "GiveSilent":
                item.Get(false);
                break;
            case "Take":
            case "Clear":
                switch (item)
                {
                    case MateriumItem i:
                        var data = i.SavedData;
                        data.IsCollected = false;
                        i.SavedData = data;
                        break;
                    case CollectableItem i:
                        i.Take(trigger == "Take" ? 1 : 999);
                        break;
                    case ToolItem i:
                        var tdata = i.SavedData;
                        tdata.IsUnlocked = false;
                        i.SavedData = tdata;
                        break;
                }
                break;
        }
    }
}