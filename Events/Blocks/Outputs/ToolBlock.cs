using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Architect.Events.Blocks.Outputs;

public class ToolBlock : ScriptBlock
{
    protected override IEnumerable<string> Outputs => ["OnUse"];
    protected override IEnumerable<(string, string)> OutputVars => [
        ("Equipped", "Boolean")
    ];

    private static readonly Color DefaultColor = new(0.6f, 0.2f, 0.9f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Custom Tool Control";

    protected override void Reset()
    {
        ToolName = "";
    }

    public string ToolName;

    protected override object GetValue(string id)
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

    public static void DoBroadcast(string tool)
    {
        foreach (var e in ToolEvent.Events
                     .Where(e => e.Block.ToolName == tool))
        {
            e.Block.Event("OnUse");
        }
    } 
}
