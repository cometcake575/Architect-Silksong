using System.Collections.Generic;
using UnityEngine;

namespace Architect.Events.Blocks.Operators;

public class PlayerDataBoolBlock : ScriptBlock
{
    protected override IEnumerable<string> Inputs => ["Set"];
    protected override IEnumerable<(string, string)> OutputVars => [("Value", "Boolean")];

    private static readonly Color DefaultColor = new(0.9f, 0.5f, 0.2f);
    protected override Color Color => DefaultColor;
    protected override string Name => "PlayerData Control (Bool)";

    public string Data;
    public bool Value;

    protected override void Trigger(string trigger)
    {
        PlayerData.instance.SetBool(Data, Value);
    }

    protected override object GetValue(string id)
    {
        return PlayerData.instance.GetBool(id);
    }
}

public class PlayerDataIntBlock : ScriptBlock
{
    protected override IEnumerable<string> Inputs => ["Set"];
    protected override IEnumerable<(string, string)> OutputVars => [("Value", "Number")];

    private static readonly Color DefaultColor = new(0.9f, 0.5f, 0.2f);
    protected override Color Color => DefaultColor;
    protected override string Name => "PlayerData Control (Int)";

    public string Data;
    public int Value;

    protected override void Trigger(string trigger)
    {
        PlayerData.instance.SetInt(Data, Value);
    }

    protected override object GetValue(string id)
    {
        return PlayerData.instance.GetInt(id);
    }
}

public class PlayerDataFloatBlock : ScriptBlock
{
    protected override IEnumerable<string> Inputs => ["Set"];
    protected override IEnumerable<(string, string)> OutputVars => [("Value", "Number")];

    private static readonly Color DefaultColor = new(0.9f, 0.5f, 0.2f);
    protected override Color Color => DefaultColor;
    protected override string Name => "PlayerData Control (Float)";

    public string Data;
    public float Value;

    protected override void Trigger(string trigger)
    {
        PlayerData.instance.SetFloat(Data, Value);
    }

    protected override object GetValue(string id)
    {
        return PlayerData.instance.GetFloat(id);
    }
}
