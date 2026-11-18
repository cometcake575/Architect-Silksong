using System;
using System.Collections.Generic;
using Architect.Editor;
using Architect.Utils;
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

    public void SetupUI(GameObject parent, float x, float y)
    {
        var txt = UIUtils.MakeLabel(
            "Config Title",
            parent,
            new Vector3(x + 100, y),
            new Vector2(0, 0.5f),
            new Vector2(0, 0.5f)).textComponent;
        txt.text = Name;
        txt.fontSize = 8;
        txt.alignment = TextAnchor.MiddleLeft;
                
        var (btn, _) = UIUtils.MakeTextButton(
            "Config Apply",
            "Apply",
            parent, 
            new Vector2(x + 220, y),
            new Vector2(0, 0.5f),
            new Vector2(0, 0.5f));
        btn.interactable = false;
                
        var inp = CreateInput(parent, btn, new Vector3(x + 155, y), 
            (WorkshopUI.IsNew ?
                GetDefaultValue() :
                WorkshopUI.Current.CurrentConfig.GetValueOrDefault(Id))?.SerializeValue());
                
        btn.onClick.AddListener(Apply);
        if (WorkshopUI.IsNew) Apply();

        return;
                
        void Apply()
        {
            btn.interactable = false;
            var val = inp.GetValue();
            if (val.Length == 0)
                WorkshopUI.Current.CurrentConfig.Remove(Id);
            else
            {
                var cfg = Deserialize(inp.GetValue());
                cfg.Setup(WorkshopUI.Current);
                WorkshopUI.Current.CurrentConfig[Id] = cfg;
            }
        }
    }
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
    public abstract string GetValue();
}
