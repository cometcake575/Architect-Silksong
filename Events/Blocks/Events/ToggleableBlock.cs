using System.Collections.Generic;

namespace Architect.Events.Blocks.Events;

public abstract class ToggleableBlock : ScriptBlock
{
    protected override IEnumerable<string> Inputs => ["Disable", "Enable"];

    protected bool Enabled = true;

    protected override void Reset()
    {
        Enabled = true;
    }
    
    public override void Event(string name)
    {
        if (!Enabled) return;
        base.Event(name);
    }

    protected override void Trigger(string trigger)
    {
        Enabled = trigger == "Enable";
    }
}