using System.Collections.Generic;
using UnityEngine;

namespace Architect.Events.Blocks.Operators;

public class ConstantNumBlock : ScriptBlock
{
    public float Value;

    protected override IEnumerable<(string, string)> OutputVars => [("Value", "Number")];

    private static readonly Color DefaultColor = new(0.9f, 0.5f, 0.2f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Constant (Number)";

    protected override object GetValue(string id)
    {
        return Value;
    }
}

public class ConstantBoolBlock : ScriptBlock
{
    public bool Value;

    protected override IEnumerable<(string, string)> OutputVars => [("Value", "Boolean")];

    private static readonly Color DefaultColor = new(0.9f, 0.5f, 0.2f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Constant (Bool)";

    protected override object GetValue(string id)
    {
        return Value;
    }
}