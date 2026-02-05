using System;
using Architect.Workshop.Items;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Architect.Workshop.Types;

public abstract class ConfigType(string name, string id)
{
    public readonly string Name = name;
    public readonly string Id = id;

    public int Priority;

    public abstract ConfigValue Deserialize(string data);

    [CanBeNull]
    public abstract ConfigValue GetDefaultValue();
    
    public abstract ConfigElement CreateInput(GameObject parent, Button apply, Vector3 pos, [CanBeNull] string currentVal);

    internal abstract void RunAction(WorkshopItem item, ConfigValue value);
}

public abstract class ConfigType<TType, TValue>(
    string name, 
    string id, 
    Action<TType, TValue> action) : ConfigType(name, id)
    where TType : WorkshopItem
    where TValue : ConfigValue
{
    /** Higher priority values run after lower priority values, negative values run before Awake */
    public ConfigType<TType, TValue> WithPriority(int priority)
    {
        Priority = priority;
        return this;
    }

    internal override void RunAction(WorkshopItem item, ConfigValue value) => action(item as TType, value as TValue);
}

public abstract class ConfigValue
{
    public abstract string SerializeValue();

    public abstract string GetTypeId();

    public abstract string GetName();

    public abstract int GetPriority();

    public abstract void Setup(WorkshopItem item);
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

    public override void Setup(WorkshopItem item)
    {
        type.RunAction(item, this);
    }

    public override int GetPriority()
    {
        return type.Priority;
    }
}

public abstract class ConfigElement
{
    public abstract RectTransform GetElement();

    public abstract string GetValue();
}
