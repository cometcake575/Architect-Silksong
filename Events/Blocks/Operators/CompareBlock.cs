using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;

namespace Architect.Events.Blocks.Operators;

public class CompareBlock : ScriptBlock
{
    protected override IEnumerable<(string, string)> OutputVars => [("Value", "Boolean")];
    protected override IEnumerable<(string, string)> InputVars => [("1", "Number"), ("2", "Number")];
    
    private static readonly Color DefaultColor = new(0.9f, 0.7f, 0.3f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Compare";

    public int Mode;

    protected override object GetValue(string id)
    {
        var v1 = GetVariable<float>("1");
        var v2 = GetVariable<float>("2");
        
        return Mode switch
        {
            0 => Mathf.Approximately(v1, v2),
            1 => v1 > v2,
            2 => v1 < v2,
            3 => v1 >= v2,
            _ => v1 <= v2
        };
    }
}