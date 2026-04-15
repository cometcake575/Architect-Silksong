using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Architect.Events.Blocks.Operators;

public class RandomBoolBlock : ScriptBlock
{
    protected override IEnumerable<(string, string)> OutputVars => [("Value", "Boolean")];
    
    protected override string Name => "Random (Bool)";

    public override object GetValue(string id)
    {
        return Random.value > 0.5f;
    }
}

public class RandomNumBlock : ScriptBlock
{
    protected override IEnumerable<(string, string)> OutputVars => [("Value", "Number")];
    
    protected override string Name => "Random (Number)";

    public float LowerBound;
    public float UpperBound;
    public bool WholeNumber;

    public override object GetValue(string id)
    {
        if (UpperBound <= LowerBound) return LowerBound;
        return WholeNumber ?
            Random.RandomRangeInt(Mathf.CeilToInt(LowerBound), Mathf.FloorToInt(UpperBound) + 1)
            : Random.Range(LowerBound, UpperBound);
    }
}

public class RandomTextBlock : ScriptBlock
{
    protected override IEnumerable<(string, string)> OutputVars => [("Value", "Text")];

    
    
    protected override string Name => "Random (Text)";

    public string Pool = string.Empty;
    public string Delimiter = ",";

    public override object GetValue(string id)
    {
        try
        {
            return Pool.Split(Delimiter).GetRandomElement();
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }
}
