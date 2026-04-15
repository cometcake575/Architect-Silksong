using System.Collections.Generic;
using PrepatcherPlugin;
using UnityEngine;

namespace Architect.Events.Blocks.Operators;

public class PlayerDataBoolBlock : ScriptBlock
{
    private static readonly Dictionary<string, List<BoolBlockRef>> BlockRefs = [];
    
    public static void Init()
    {
        PlayerDataVariableEvents.OnSetBool += (_, name, current) =>
        {
            if (!BlockRefs.TryGetValue(name, out var refs)) return current;
            foreach (var block in refs) block.Block.Event(current ? "OnTrue" : "OnFalse");
            return current;
        };
    }

    protected override IEnumerable<string> Inputs => ["Set"];
    protected override IEnumerable<string> Outputs => ["OnTrue", "OnFalse"];
    protected override IEnumerable<(string, string)> OutputVars => [("Value", "Boolean")];

    
    
    protected override string Name => "PlayerData Control (Bool)";

    public string Data = string.Empty;
    public bool Value;
    
    public override void SetupReference()
    {
        var blockRef = new GameObject("[Architect] Bool Block");
        var blockRefInst = blockRef.AddComponent<BoolBlockRef>();
        blockRefInst.Block = this;
        if (!BlockRefs.ContainsKey(Data)) BlockRefs[Data] = [];
        BlockRefs[Data].Add(blockRefInst);
    }

    public class BoolBlockRef : MonoBehaviour
    {
        public PlayerDataBoolBlock Block;

        private void OnDisable()
        {
            BlockRefs[Block.Data].Remove(this);
        }
    }

    protected override void Trigger(string trigger)
    {
        PlayerData.instance.SetBool(Data, Value);
    }

    public override object GetValue(string id)
    {
        return PlayerData.instance.GetBool(Data);
    }
}

public class PersistentBoolBlock : ScriptBlock
{
    protected override IEnumerable<string> Inputs => ["Set", "ClearScene"];
    protected override IEnumerable<(string, string)> OutputVars => [("Value", "Boolean")];

    
    
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

    public override object GetValue(string id)
    {
        return SceneData.instance.persistentBools
            .TryGetValue(Group, Data, out var val) && val.Value;
    }
}

public class PlayerDataIntBlock : ScriptBlock
{
    protected override IEnumerable<string> Inputs => ["Set", "Add", "Subtract"];
    protected override IEnumerable<(string, string)> OutputVars => [("Value", "Number")];

    
    
    protected override string Name => "PlayerData Control (Int)";

    public string Data;
    public int Value;

    protected override void Trigger(string trigger)
    {
        switch (trigger)
        {
            case "Set":
                PlayerData.instance.SetInt(Data, Value);
                break;
            case "Add":
                PlayerData.instance.SetInt(Data, PlayerData.instance.GetInt(Data) + Value);
                break;
            case "Subtract":
                PlayerData.instance.SetInt(Data, PlayerData.instance.GetInt(Data) - Value);
                break;
        }
    }

    public override object GetValue(string id)
    {
        return PlayerData.instance.GetInt(Data);
    }
}

public class PlayerDataFloatBlock : ScriptBlock
{
    protected override IEnumerable<string> Inputs => ["Set", "Add", "Subtract"];
    protected override IEnumerable<(string, string)> OutputVars => [("Value", "Number")];

    
    
    protected override string Name => "PlayerData Control (Float)";

    public string Data;
    public float Value;

    protected override void Trigger(string trigger)
    {
        switch (trigger)
        {
            case "Set":
                PlayerData.instance.SetFloat(Data, Value);
                break;
            case "Add":
                PlayerData.instance.SetFloat(Data, PlayerData.instance.GetFloat(Data) + Value);
                break;
            case "Subtract":
                PlayerData.instance.SetFloat(Data, PlayerData.instance.GetFloat(Data) - Value);
                break;
        }
    }

    public override object GetValue(string id)
    {
        return PlayerData.instance.GetFloat(Data);
    }
}
