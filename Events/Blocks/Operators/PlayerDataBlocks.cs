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
        return PlayerData.instance.GetBool(Data);
    }
}

public class PersistentBoolBlock : ScriptBlock
{
    protected override IEnumerable<string> Inputs => ["Set", "ClearScene"];
    protected override IEnumerable<(string, string)> OutputVars => [("Value", "Boolean")];

    private static readonly Color DefaultColor = new(0.9f, 0.5f, 0.2f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Persistent Data Control";

    public string Group;
    public string Data;
    public bool Value;

    protected override void Trigger(string trigger)
    {
        if (trigger == "Set")
        {
            if (SceneData.instance.persistentBools.TryGetValue(Group, Data, out var val))
                val.Value = Value;
        }
        else SceneData.instance.persistentBools.scenes.Remove(Group);
    }

    protected override object GetValue(string id)
    {
        return SceneData.instance.persistentBools
            .TryGetValue(Group, Data, out var val) && val.Value;
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
        return PlayerData.instance.GetInt(Data);
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
        return PlayerData.instance.GetFloat(Data);
    }
}
