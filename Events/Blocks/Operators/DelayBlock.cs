using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architect.Events.Blocks.Operators;

public class DelayBlock : ScriptBlock
{
    protected override IEnumerable<(string, string)> InputVars => [("Extra Delay", "Number")];

    protected override IEnumerable<string> Inputs => ["In", "Cancel"];
    protected override IEnumerable<string> Outputs => ["Out"];

    private static readonly Color DefaultColor = Color.yellow;
    protected override Color Color => DefaultColor;
    protected override string Name => "Delay";

    public float Delay;

    protected override void Reset() => Delay = 0;

    private DelayObj _delay;

    public override void SetupReference()
    {
        var obj = new GameObject("[Architect] Delay Block");
        _delay = obj.AddComponent<DelayObj>();
    }

    protected override void Trigger(string trigger)
    {
        if (!_delay) return;
        if (trigger == "In") _delay.StartCoroutine(DelayedEvent());
        else _delay.StopAllCoroutines();
    }

    private IEnumerator DelayedEvent()
    {
        var delay = Delay;
        delay += GetVariable<float>("Extra Delay");
        yield return new WaitForSeconds(delay);
        Event("Out");
    }
}

public class WaitUntilBlock : ScriptBlock
{
    protected override IEnumerable<(string, string)> InputVars => [("Check", "Boolean")];

    protected override IEnumerable<string> Inputs => ["In"];
    protected override IEnumerable<string> Outputs => ["Out"];

    private static readonly Color DefaultColor = Color.yellow;
    protected override Color Color => DefaultColor;
    protected override string Name => "Wait Until";

    private DelayObj _delay;

    public override void SetupReference()
    {
        var obj = new GameObject("[Architect] Wait Until Block");
        _delay = obj.AddComponent<DelayObj>();
    }

    protected override void Trigger(string trigger)
    {
        if (!_delay) return;
        _delay.StartCoroutine(DelayedEvent());
    }

    private IEnumerator DelayedEvent()
    {
        yield return new WaitUntil(() => GetVariable<bool>("Check"));
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

    private DelayObj _delay;

    public override void SetupReference()
    {
        var obj = new GameObject("[Architect] Delay Block");
        _delay = obj.AddComponent<DelayObj>();
    }

    protected override void Trigger(string trigger)
    {
        if (!_delay) return;
        _delay.StartCoroutine(DelayedEvent());
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

public class DelayObj : MonoBehaviour;
