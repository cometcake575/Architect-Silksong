using System.Collections.Generic;
using UnityEngine;

namespace Architect.Events.Blocks.Operators;

public class CounterBlock : ScriptBlock
{
    protected override IEnumerable<string> Inputs => ["In", "Reset"];
    protected override IEnumerable<string> Outputs => ["Out"];

    private static readonly Color DefaultColor = new(0.9f, 0.7f, 0.3f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Counter";

    public int Count;
    private int _count;

    public override void SetupReference()
    {
        _count = 0;
    }

    protected override void Trigger(string trigger)
    {
        if (trigger == "Reset")
        {
            _count = 0;
        }
        else
        {
            _count++;
            if (_count >= Count)
            {
                _count = 0;
                Event("Out");
            }
        }
    }
}