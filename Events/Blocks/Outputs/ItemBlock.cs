using System.Collections.Generic;
using Architect.Utils;

namespace Architect.Events.Blocks.Outputs;

public class ItemBlock : ScriptBlock
{
    protected override IEnumerable<string> Inputs => ["Give", "GiveSilent", "Take", "Clear", "ShowCounter", "HideCounter"];
    protected override IEnumerable<(string, string)> OutputVars => [
        ("Obtained", "Boolean")
    ];
    
    protected override string Name => "Item Control";

    public override void Reset()
    {
        ItemName = "";
        Amount = 1;
    }

    public string ItemName;
    public int Amount = 1;

    public override object GetValue(string id)
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
                item.Get(Amount);
                break;
            case "GiveSilent":
                item.Get(Amount, false);
                break;
            case "ShowCounter":
                if (item is not CollectableItem cis) return;
                ItemCurrencyCounter.Show(cis);
                break;
            case "HideCounter":
                if (item is not CollectableItem cih) return;
                ItemCurrencyCounter.Hide(cih);
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
                        i.Take(trigger == "Take" ? Amount : 999);
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