using System.Collections.Generic;
using UnityEngine;

namespace Architect.Events.Blocks.Operators;

public class IfBlock : ScriptBlock
{
    protected override IEnumerable<string> Inputs => ["In"];
    protected override IEnumerable<string> Outputs => ["True", "False"];

    protected override IEnumerable<(string, string)> InputVars => [("Check", "Boolean")];

    private static readonly Color DefaultColor = new(0.9f, 0.7f, 0.3f);
    protected override Color Color => DefaultColor;
    protected override string Name => "If";

    protected override void Trigger(string trigger)
    {
        Event(GetVariable<bool>("Check") ? "True" : "False");
    }
}