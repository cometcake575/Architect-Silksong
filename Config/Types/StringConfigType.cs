using System;
using Architect.Utils;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Architect.Config.Types;

public class StringConfigType(
    string name, 
    string id, 
    Action<GameObject, StringConfigValue> action,
    [CanBeNull] Action<GameObject, StringConfigValue, ConfigurationManager.PreviewContext> previewAction = null
) : ConfigType<StringConfigValue>(name, id, action, previewAction)
{
    [CanBeNull] private string _defaultValue;

    public StringConfigType WithDefaultValue(string value)
    {
        _defaultValue = value;
        return this;
    }

    public override ConfigValue GetDefaultValue()
    {
        return _defaultValue == null ? null : new StringConfigValue(this, _defaultValue);
    }

    public override ConfigElement CreateInput(GameObject parent, Button apply, Vector3 pos, string currentVal)
    {
        return new StringConfigElement(parent, apply, pos, currentVal);
    }

    public override ConfigValue Deserialize(string data)
    {
        return new StringConfigValue(this, data);
    }
}

public class StringConfigValue(StringConfigType type, string value) : ConfigValue<StringConfigType>(type)
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
        (_input, _) = UIUtils.MakeTextbox("Config Input", parent, pos, Vector2.zero, Vector2.zero, 
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