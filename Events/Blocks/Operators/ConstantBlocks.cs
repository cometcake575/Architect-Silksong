using System.Collections.Generic;
using Architect.Config;
using Architect.Config.Types;
using Architect.Prefabs;
using UnityEngine;

namespace Architect.Events.Blocks.Operators;

public abstract class ConstantBlock : ScriptBlock
{
    public abstract ConfigType GetConfigType();

    public bool Public;
    public string ConfigName;

    public abstract void Load(string value);
}

public class ConstantNumBlock : ConstantBlock
{
    public float Value;

    protected override IEnumerable<(string, string)> OutputVars => [("Value", "Number")];

    protected override string Name => "Constant (Number)";

    public override object GetValue(string id)
    {
        return Value;
    }

    public override ConfigType GetConfigType()
    {
        return ConfigurationManager.RegisterConfigType(new FloatConfigType(ConfigName, $"prefab_config_{BlockId}", (o, f) =>
        {
            o.GetComponent<Prefab>().ApplyConfig(BlockId, f.SerializeValue());
        }).WithDefaultValue(Value));
    }

    public override void Load(string value)
    {
        float.TryParse(value, out Value);
    }
}

public class ConstantBoolBlock : ConstantBlock
{
    public bool Value;

    protected override IEnumerable<(string, string)> OutputVars => [("Value", "Boolean")];

    protected override string Name => "Constant (Bool)";

    public override object GetValue(string id)
    {
        return Value;
    }

    public override ConfigType GetConfigType()
    {
        return ConfigurationManager.RegisterConfigType(new BoolConfigType(ConfigName, $"prefab_config_{BlockId}", (o, f) =>
        {
            o.GetComponent<Prefab>().ApplyConfig(BlockId, f.SerializeValue());
        }).WithDefaultValue(Value));
    }

    public override void Load(string value)
    {
        bool.TryParse(value, out Value);
    }
}

public class ConstantTextBlock : ConstantBlock
{
    public string Value = string.Empty;

    protected override IEnumerable<(string, string)> OutputVars => [("Value", "Text")];

    protected override string Name => "Constant (Text)";

    public override object GetValue(string id)
    {
        return Value;
    }

    public override ConfigType GetConfigType()
    {
        return ConfigurationManager.RegisterConfigType(new StringConfigType(ConfigName, $"prefab_config_{BlockId}", (o, f) =>
        {
            o.GetComponent<Prefab>().ApplyConfig(BlockId, f.SerializeValue());
        }).WithDefaultValue(Value));
    }

    public override void Load(string value)
    {
        Value = value;
    }
}