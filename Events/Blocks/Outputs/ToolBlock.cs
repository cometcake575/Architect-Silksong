using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Architect.Events.Blocks.Outputs;

public class ToolBlock : ScriptBlock
{
    protected override IEnumerable<string> Inputs => ["ActReset", "AddUses", "TakeUses", "AddLiquid", "TakeLiquid"];
    protected override IEnumerable<string> Outputs => ["OnUse"];
    protected override IEnumerable<(string, string)> OutputVars => [
        ("Equipped", "Boolean")
    ];
    
    protected override string Name => "Tool Control";

    public override void Reset()
    {
        ToolName = "";
    }

    public string ToolName;
    public int Amount = 1;

    public override object GetValue(string id)
    {
        var tool = ToolItemManager.Instance.toolItems.GetByName(ToolName);
        return tool && tool.IsEquipped;
    }

    public override void SetupReference()
    {
        var te = new GameObject("[Architect] Receive Block").AddComponent<ToolEvent>();
        te.Block = this;
    }

    public class ToolEvent : MonoBehaviour
    {
        public ToolBlock Block;

        public static readonly List<ToolEvent> Events = [];  

        private void OnEnable()
        {
            Events.Add(this);
        }
        
        private void OnDisable()
        {
            Events.Remove(this);
        }
    }

    protected override void Trigger(string trigger)
    {
        var tool = ToolItemManager.Instance.toolItems.GetByName(ToolName);
        
        switch (trigger)
        {
            case "ActReset":
                ToolItemManager.AutoEquip(
                    ToolItemManager.GetCrestByName(PlayerData.instance.CurrentCrestID),
                    false,
                    true
                );
                break;
            case "AddUses":
                if (!tool) return;
                var sd = tool.SavedData;
                sd.AmountLeft += Amount;
                tool.SavedData = sd;
                
                ToolItemManager.ReportAllBoundAttackToolsUpdated();
                break;
            case "TakeUses":
                if (!tool) return;
                var sd2 = tool.SavedData;
                sd2.AmountLeft += Amount;
                tool.SavedData = sd2;
                
                ToolItemManager.ReportAllBoundAttackToolsUpdated();
                break;
            case "AddLiquid":
                if (!tool) return;

                if (tool is ToolItemStatesLiquid liquid)
                {
                    var lsd = liquid.LiquidSavedData;
                    var amount = Amount;
                    if (lsd.RefillsLeft + amount > liquid.refillsMax) amount = liquid.refillsMax - lsd.RefillsLeft;
                    
                    var currencyCounter = LiquidReserveCounter.GetCurrencyCounter(liquid, true);
                    if (!currencyCounter) return;
                    currencyCounter.IconOverride = null;
                    if (currencyCounter.infiniteIcon) currencyCounter.infiniteIcon.gameObject.SetActive(false);
                    currencyCounter.QueueAdd(amount);
                    
                    lsd.RefillsLeft += amount;
                    liquid.LiquidSavedData = lsd;
                }
                
                break;
            case "TakeLiquid":
                if (!tool) return;

                if (tool is ToolItemStatesLiquid liquid2)
                {
                    var lsd = liquid2.LiquidSavedData;
                    var amount = Amount;
                    if (lsd.RefillsLeft - amount < 0) amount = lsd.RefillsLeft;
                    
                    var currencyCounter = LiquidReserveCounter.GetCurrencyCounter(liquid2, true);
                    if (!currencyCounter) return;
                    currencyCounter.IconOverride = null;
                    if (currencyCounter.infiniteIcon) currencyCounter.infiniteIcon.gameObject.SetActive(false);
                    currencyCounter.QueueTake(amount);
                    
                    lsd.RefillsLeft -= amount;
                    liquid2.LiquidSavedData = lsd;
                }
                
                break;
        }
    }

    public static void DoBroadcast(string tool)
    {
        foreach (var e in ToolEvent.Events
                     .Where(e => e.Block.ToolName == tool))
        {
            e.Block.Event("OnUse");
        }
    } 
}
