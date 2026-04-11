using System.Collections.Generic;
using UnityEngine;

namespace Architect.Events.Blocks.Events;

public class EveryFrameBlock : ToggleableBlock
{
    protected override IEnumerable<string> Inputs => ["Disable", "Enable"];
    protected override IEnumerable<string> Outputs => ["OnCall"];
    protected override string Name => "Every Frame";

    private TimerEvent _te;

    protected override void Trigger(string trigger)
    {
        if (trigger != "Reset")
        {
            base.Trigger(trigger);
            return;
        }
        if (!_te) return;
        _te.gameObject.SetActive(true);
    }

    public override void SetupReference()
    {
        _te = new GameObject("[Architect] Every Frame Block").AddComponent<TimerEvent>();
        _te.Block = this;
    }

    public class TimerEvent : MonoBehaviour
    {
        public EveryFrameBlock Block;
        
        private void Update()
        {
            if (!Block.Enabled) return;
            Block.Event("OnCall");
        }
    }
}