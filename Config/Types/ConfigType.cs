using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Architect.Config.Types;

public abstract class ConfigType(string name, string id)
{
    public readonly string Name = name;
    public readonly string Id = id;

    public int Priority;

    public abstract ConfigValue Deserialize(string data);

    [CanBeNull]
    public abstract ConfigValue GetDefaultValue();
    
    public abstract ConfigElement CreateInput(GameObject parent, Button apply, Vector3 pos, [CanBeNull] string currentVal);

    internal abstract void RunAction(GameObject obj, ConfigValue value);
    
    internal abstract void RunActionPreview(GameObject obj, ConfigValue value, ConfigurationManager.PreviewContext context);
}

public abstract class ConfigType<TValue>(
    string name, 
    string id, 
    Action<GameObject, TValue> action,
    [CanBeNull] Action<GameObject, TValue, ConfigurationManager.PreviewContext> previewAction) : ConfigType(name, id)
    where TValue : ConfigValue
{
    /** Higher priority values run after lower priority values, negative values run before Awake */
    public ConfigType<TValue> WithPriority(int priority)
    {
        Priority = priority;
        return this;
    }

    internal override void RunAction(GameObject obj, ConfigValue value) => action(obj, value as TValue);

    internal override void RunActionPreview(GameObject obj, ConfigValue value, ConfigurationManager.PreviewContext context)
    {
        previewAction?.Invoke(obj, value as TValue, context);
    }
}

public abstract class ConfigValue
{
    public abstract string SerializeValue();

    public abstract string GetTypeId();

    public abstract string GetName();

    public abstract int GetPriority();

    public abstract void Setup(GameObject obj, string extraId);
    
    public abstract void SetupPreview(GameObject obj, ConfigurationManager.PreviewContext context);
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

    public override void Setup(GameObject obj, string extraId)
    {
        type.RunAction(obj, this);
    }

    public override void SetupPreview(GameObject obj, ConfigurationManager.PreviewContext context)
    {
        type.RunActionPreview(obj, this, context);
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
