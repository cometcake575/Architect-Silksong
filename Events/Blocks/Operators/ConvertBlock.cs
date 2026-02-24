using System.Collections.Generic;
using UnityEngine;

namespace Architect.Events.Blocks.Operators;

public class ConvertBlock : ScriptBlock
{
    protected override IEnumerable<(string, string)> InputVars => [("Value", "Any")];
    protected override IEnumerable<(string, string)> OutputVars => [
        ("ToText", "Text"),
        ("ToNum", "Number"),
        ("ToBool", "Boolean")
    ];

    private static readonly Color DefaultColor = new(0.9f, 0.5f, 0.2f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Convert";

    protected override object GetValue(string id)
    {
        var inp = GetVariable<object>("Value").ToString();
        return id switch
        {
            "ToText" => inp,
            "ToBool" => bool.TryParse(inp, out var b) && b,
            "ToNum" => float.TryParse(inp, out var f) ? f : 0f,
            _ => null
        };
    }
}