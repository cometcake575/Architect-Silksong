using System;
using Architect.Utils;
using Architect.Workshop.Items;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Architect.Workshop.Types;

public class StringConfigType<T>(
    string name, 
    string id, 
    Action<T, StringConfigValue<T>> action)
    : ConfigType<T, StringConfigValue<T>>(name, id, action)
    where T : WorkshopItem
{
    [CanBeNull] private string _defaultValue;

    public StringConfigType<T> WithDefaultValue(string value)
    {
        _defaultValue = value;
        return this;
    }

    public override ConfigValue GetDefaultValue()
    {
        return _defaultValue == null ? null : new StringConfigValue<T>(this, _defaultValue);
    }

    public override ConfigElement CreateInput(GameObject parent, Button apply, Vector3 pos, string currentVal)
    {
        return new StringConfigElement(parent, apply, pos, currentVal);
    }

    public override ConfigValue Deserialize(string data)
    {
        return new StringConfigValue<T>(this, data);
    }
}

public class StringConfigValue<T>(StringConfigType<T> type, string value) :
    ConfigValue<StringConfigType<T>>(type) 
    where T : WorkshopItem
{
    public string GetValue()
    {
        return value;
    }

    public override string SerializeValue()
    {
        return value;
    }
}

public class StringConfigElement : ConfigElement
{
    private readonly InputField _input;

    public StringConfigElement(GameObject parent, Button apply, Vector3 pos, [CanBeNull] string currentVal)
    {
        (_input, _) = UIUtils.MakeTextbox("Config Input", parent, pos, 
            new Vector2(0, 0.5f), new Vector2(0, 0.5f), 
            220, 25);

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