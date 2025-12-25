using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architect.Events.Blocks.Operators;

public class DelayBlock : ScriptBlock
{
    protected override IEnumerable<(string, string)> InputVars => [("Extra Delay", "Number")];

    protected override IEnumerable<string> Inputs => ["In"];
    protected override IEnumerable<string> Outputs => ["Out"];

    private static readonly Color DefaultColor = Color.yellow;
    protected override Color Color => DefaultColor;
    protected override string Name => "Delay";

    public float Delay;

    protected override void Trigger(string trigger)
    {
        ArchitectPlugin.Instance.StartCoroutine(DelayedEvent());
    }

    private IEnumerator DelayedEvent()
    {
        yield return new WaitForSeconds(Delay + GetVariable<float>("Extra Delay"));
        Event("Out");
    }
}