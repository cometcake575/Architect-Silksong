using System.Collections.Generic;
using UnityEngine;

namespace Architect.Events.Blocks.Operators;

public class MathsBlock : ScriptBlock
{
    protected override IEnumerable<(string, string)> OutputVars => [("Value", "Number")];
    protected override IEnumerable<(string, string)> InputVars => [("1", "Number"), ("2", "Number")];
    
    private static readonly Color DefaultColor = new(0.9f, 0.7f, 0.3f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Operation";

    public int Mode;

    protected override object GetValue(string id)
    {
        var v1 = GetVariable<float>("1");
        var v2 = GetVariable<float>("2");
        return Mode switch
        {
            0 => v1 + v2,
            1 => v1 - v2,
            2 => v1 * v2,
            3 => v1 / v2,
            4 => Mathf.Floor(v1 / v2),
            5 => v1 % v2,
            6 => Mathf.Pow(v1, v2),
            _ => Mathf.Pow(v1, 1f/v2)
        };
    }
}

public class TrigBlock : ScriptBlock
{
    protected override IEnumerable<(string, string)> OutputVars => [("Value", "Number")];
    protected override IEnumerable<(string, string)> InputVars => [("Value", "Number")];
    
    private static readonly Color DefaultColor = new(0.9f, 0.7f, 0.3f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Trig Operation";

    public int Mode;
    public bool IsDegrees;

    protected override object GetValue(string id)
    {
        var v1 = GetVariable<float>("Value");
        return Mode switch
        {
            0 => Mathf.Sin(v1 * (IsDegrees ? Mathf.Deg2Rad : 1)),
            1 => Mathf.Cos(v1 * (IsDegrees ? Mathf.Deg2Rad : 1)),
            2 => Mathf.Tan(v1 * (IsDegrees ? Mathf.Deg2Rad : 1)),
            3 => Mathf.Asin(v1) * (IsDegrees ? Mathf.Rad2Deg : 1),
            4 => Mathf.Acos(v1) * (IsDegrees ? Mathf.Rad2Deg : 1),
            _ => Mathf.Atan(v1) * (IsDegrees ? Mathf.Rad2Deg : 1)
        };
    }
}

public class NormaliseBlock : ScriptBlock
{
    protected override IEnumerable<(string, string)> OutputVars => [("X", "Number"), ("Y", "Number")];
    protected override IEnumerable<(string, string)> InputVars => [("X", "Number"), ("Y", "Number")];
    
    private static readonly Color DefaultColor = new(0.9f, 0.7f, 0.3f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Normalise";

    protected override void Reset()
    {
        Angle = 0;
    }
    
    public float Angle;

    protected override object GetValue(string id)
    {
        var v1 = GetVariable<float>("X");
        var v2 = GetVariable<float>("Y");

        var normal = Quaternion.Euler(0, 0, Angle) * new Vector2(v1, v2).normalized;
        return id == "X" ? normal.x : normal.y;
    }
}