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

    protected override void Reset() => Delay = 0;

    protected override void Trigger(string trigger)
    {
        ArchitectPlugin.Instance.StartCoroutine(DelayedEvent());
    }

    private IEnumerator DelayedEvent()
    {
        var delay = Delay;
        delay += GetVariable<float>("Extra Delay");
        yield return new WaitForSeconds(delay);
        Event("Out");
    }
}

public class LoopBlock : ScriptBlock
{
    protected override IEnumerable<(string, string)> InputVars => [("Times", "Number")];

    protected override IEnumerable<(string, string)> OutputVars => [("Loop Value", "Number")];

    protected override IEnumerable<string> Inputs => ["In"];
    protected override IEnumerable<string> Outputs => ["Out"];

    private static readonly Color DefaultColor = Color.yellow;
    protected override Color Color => DefaultColor;
    protected override string Name => "Loop";

    public float Delay;
    
    private int _currentTime;

    protected override void Reset() => Delay = 0;

    protected override void Trigger(string trigger)
    {
        ArchitectPlugin.Instance.StartCoroutine(DelayedEvent());
    }

    protected override object GetValue(string id)
    {
        return _currentTime;
    }

    private IEnumerator DelayedEvent()
    {
        var times = Mathf.RoundToInt(GetVariable<float>("Times"));
        for (_currentTime = 0; _currentTime < times; _currentTime++)
        {
            if (Delay > 0) yield return new WaitForSeconds(Delay);
            Event("Out");
        }
    }
}