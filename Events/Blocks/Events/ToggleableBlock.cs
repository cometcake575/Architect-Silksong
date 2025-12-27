using System.Collections.Generic;

namespace Architect.Events.Blocks.Events;

public abstract class ToggleableBlock : ScriptBlock
{
    protected override IEnumerable<string> Inputs => ["Disable", "Enable"];

    private bool _enabled = true;
    
    public override void Event(string name)
    {
        if (!_enabled) return;
        base.Event(name);
    }

    protected override void Trigger(string trigger)
    {
        _enabled = trigger == "Enable";
    }
}