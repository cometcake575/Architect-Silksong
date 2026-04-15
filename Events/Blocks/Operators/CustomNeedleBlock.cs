using System.Collections.Generic;
using UnityEngine;

namespace Architect.Events.Blocks.Operators;

public class CustomNeedleBlock : ScriptBlock
{
    protected override IEnumerable<string> Inputs => ["Set", "Clear"];
    protected override IEnumerable<(string, string)> OutputVars => [("Value", "Text")];

    protected override string Name => "Custom Needle Control";

    public string Value;
    
    protected override void Trigger(string trigger)
    {
        ArchitectData.Instance.CustomNeedle = trigger == "Set" ? Value : string.Empty;
    }

    public override object GetValue(string id)
    {
        return ArchitectData.Instance.CustomNeedle;
    }
}
