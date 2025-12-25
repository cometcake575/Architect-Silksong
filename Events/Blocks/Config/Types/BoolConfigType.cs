using System;
using System.Globalization;
using Architect.Utils;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Architect.Events.Blocks.Config.Types;

public class BoolConfigType<T>(
    string name, 
    string id, 
    Action<T, BoolConfigValue<T>> action
    ) : ConfigType<T, BoolConfigValue<T>>(name, id, action) where T : ScriptBlock
{
    private bool? _defaultValue;

    public BoolConfigType<T> WithDefaultValue(bool value)
    {
        _defaultValue = value;
        return this;
    }

    public override ConfigValue GetDefaultValue()
    {
        return _defaultValue.HasValue ? new BoolConfigValue<T>(this, _defaultValue.Value) : null;
    }

    public override ConfigElement CreateInput(GameObject parent, Button apply, Vector3 pos, string currentVal)
    {
        return new BoolConfigElement(parent, apply, pos, currentVal);
    }

    public override ConfigValue Deserialize(string data)
    {
        return new BoolConfigValue<T>(this, Convert.ToBoolean(data, CultureInfo.InvariantCulture));
    }
}

public class BoolConfigValue<T>(BoolConfigType<T> type, bool value)
    : ConfigValue<BoolConfigType<T>>(type)
    where T : ScriptBlock
{
    public bool GetValue()
    {
        return value;
    }

    public override string SerializeValue()
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }
}

public class BoolConfigElement : ConfigElement
{
    private readonly Button _input;
    private bool _active = true;

    public BoolConfigElement(GameObject parent, Button apply, Vector3 pos, [CanBeNull] string currentVal)
    {
        (_input, var txt) = UIUtils.MakeTextButton("Config Input", "Default", parent, pos, 
            Vector2.zero, Vector2.zero, size:new Vector2(120, 25));

        if (currentVal != null)
        {
            txt.textComponent.text = currentVal;
            _active = Convert.ToBoolean(currentVal, CultureInfo.InvariantCulture);
        }
        
        _input.onClick.AddListener(() =>
        {
            _active = !_active;
            txt.textComponent.text = _active.ToString(CultureInfo.InvariantCulture);
            apply.interactable = true;
        });
    }

    public override RectTransform GetElement()
    {
        return _input.transform as RectTransform;
    }

    public override string GetValue()
    {
        return _active.ToString(CultureInfo.InvariantCulture);
    }
}