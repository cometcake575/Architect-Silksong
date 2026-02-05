using System;
using System.Globalization;
using Architect.Utils;
using Architect.Workshop.Items;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Architect.Workshop.Types;

public class IntConfigType<T>(
    string name, 
    string id, 
    Action<T, IntConfigValue<T>> action
    ) : ConfigType<T, IntConfigValue<T>>(name, id, action)
    where T : WorkshopItem
{
    private int? _defaultValue;

    public IntConfigType<T> WithDefaultValue(int value)
    {
        _defaultValue = value;
        return this;
    }

    public override ConfigValue GetDefaultValue()
    {
        return _defaultValue.HasValue ? new IntConfigValue<T>(this, _defaultValue.Value) : null;
    }

    public override ConfigElement CreateInput(GameObject parent, Button apply, Vector3 pos, string currentVal)
    {
        return new IntConfigElement(parent, apply, pos, currentVal);
    }

    public override ConfigValue Deserialize(string data)
    {
        return new IntConfigValue<T>(this, Convert.ToInt32(data, CultureInfo.InvariantCulture));
    }
}

public class IntConfigValue<T>(IntConfigType<T> type, int value) 
    : ConfigValue<IntConfigType<T>>(type)
    where T : WorkshopItem
{
    public int GetValue()
    {
        return value;
    }

    public override string SerializeValue()
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }
}

public class IntConfigElement : ConfigElement
{
    private readonly InputField _input;

    public IntConfigElement(GameObject parent, Button apply, Vector3 pos, [CanBeNull] string currentVal)
    {
        (_input, _) = UIUtils.MakeTextbox("Config Input", parent, pos, 
            new Vector2(0, 0.5f), new Vector2(0, 0.5f), 
            120, 25);

        if (currentVal != null) _input.text = currentVal;
        var last = _input.text;

        _input.characterValidation = InputField.CharacterValidation.Integer;
        
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