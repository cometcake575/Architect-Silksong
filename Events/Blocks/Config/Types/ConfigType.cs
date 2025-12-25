using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Architect.Events.Blocks.Config.Types;

public abstract class ConfigType(string name, string id)
{
    public readonly string Name = name;
    public readonly string Id = id;

    public abstract ConfigValue Deserialize(string data);

    [CanBeNull]
    public abstract ConfigValue GetDefaultValue();
    
    public abstract ConfigElement CreateInput(GameObject parent, Button apply, Vector3 pos, [CanBeNull] string currentVal);

    internal abstract void RunAction(ScriptBlock block, ConfigValue value);
}

public abstract class ConfigType<TType, TValue>(
    string name, 
    string id, 
    Action<TType, TValue> action) : ConfigType(name, id)
    where TValue : ConfigValue 
    where TType : ScriptBlock
{
    internal override void RunAction(ScriptBlock obj, ConfigValue value) => action(obj as TType, value as TValue);
}

public abstract class ConfigValue
{
    public abstract string SerializeValue();

    public abstract string GetTypeId();

    public abstract string GetName();

    public abstract void Setup(ScriptBlock block);
}

public abstract class ConfigValue<TType>(TType type) : ConfigValue
    where TType : ConfigType
{
    public override string GetTypeId()
    {
        return type.Id;
    }

    public override string GetName()
    {
        return type.Name;
    }

    public override void Setup(ScriptBlock block)
    {
        type.RunAction(block, this);
    }
}

public abstract class ConfigElement
{
    public abstract RectTransform GetElement();

    public abstract string GetValue();
}
