using System.Collections.Generic;
using UnityEngine;

namespace Architect.Events.Blocks.Operators;

public class RandomBoolBlock : ScriptBlock
{
    protected override IEnumerable<(string, string)> OutputVars => [("Value", "Boolean")];

    private static readonly Color DefaultColor = new(0.9f, 0.5f, 0.2f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Random (Bool)";

    protected override object GetValue(string id)
    {
        return Random.value > 0.5f;
    }
}


public class RandomNumBlock : ScriptBlock
{
    protected override IEnumerable<(string, string)> OutputVars => [("Value", "Number")];

    private static readonly Color DefaultColor = new(0.9f, 0.5f, 0.2f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Random (Number)";

    public float LowerBound;
    public float UpperBound;
    public bool WholeNumber;

    protected override object GetValue(string id)
    {
        if (UpperBound <= LowerBound) return LowerBound;
        return WholeNumber ?
            Random.RandomRangeInt(Mathf.CeilToInt(LowerBound), Mathf.FloorToInt(UpperBound) + 1)
            : Random.Range(LowerBound, UpperBound);
    }
}
