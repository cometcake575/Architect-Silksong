using System;
using Architect.Utils;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Architect.Events.Blocks.Config.Types;

public class IdConfigType<T>(
    string name, 
    string id, 
    Action<T, IdConfigValue<T>> action
) : ConfigType<T, IdConfigValue<T>>(name, id, action) where T : ScriptBlock
{
    [CanBeNull] private string _defaultValue;

    public IdConfigType<T> WithDefaultValue(string value)
    {
        _defaultValue = value;
        return this;
    }

    public override ConfigValue GetDefaultValue()
    {
        return _defaultValue == null ? null : new IdConfigValue<T>(this, _defaultValue);
    }

    public override ConfigElement CreateInput(GameObject parent, Button apply, Vector3 pos, string currentVal)
    {
        return new IdConfigElement(parent, apply, pos, currentVal);
    }

    public override ConfigValue Deserialize(string data)
    {
        return new IdConfigValue<T>(this, data);
    }
}

public class IdConfigValue<T>(IdConfigType<T> type, string value) 
    : ConfigValue<IdConfigType<T>>(type)
    where T : ScriptBlock
{
    public string GetValue()
    {
        return value;
    }

    public override string SerializeValue()
    {
        return value;
    }

    public override bool IsLocal => true;
}

public class IdConfigElement : ConfigElement
{
    private readonly InputField _input;

    public IdConfigElement(GameObject parent, Button apply, Vector3 pos, [CanBeNull] string currentVal)
    {
        (_input, _) = UIUtils.MakeTextbox("Config Input", parent, pos, Vector2.zero, Vector2.zero, 
            180, 25);

        if (currentVal != null) _input.text = currentVal;
        var last = _input.text;
        _input.onValueChanged.AddListener(s =>
        {
            if (last == s) return;
            last = s;
            apply.interactable = true;
        });
    }

    public override RectTransform GetElement()
    {
        return _input.transform as RectTransform;
    }

    public override string GetValue()
    {
        return _input.text;
    }
}