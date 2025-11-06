using System;
using System.Globalization;
using Architect.Utils;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Architect.Config.Types;

public class ChoiceConfigType(
    string name, 
    string id, 
    Action<GameObject, ChoiceConfigValue> action,
    [CanBeNull] Action<GameObject, ChoiceConfigValue, ConfigurationManager.PreviewContext> previewAction = null
    ) : ConfigType<ChoiceConfigValue>(name, id, action, previewAction)
{
    private int? _defaultValue;
    private string[] _options = [];

    public ChoiceConfigType WithOptions(params string[] options)
    {
        _options = options;
        return this;
    }

    public ChoiceConfigType WithDefaultValue(int value)
    {
        _defaultValue = value;
        return this;
    }

    public override ConfigValue GetDefaultValue()
    {
        return _defaultValue.HasValue ? new ChoiceConfigValue(this, _defaultValue.Value) : null;
    }

    public override ConfigElement CreateInput(GameObject parent, Button apply, Vector3 pos, string currentVal)
    {
        return new ChoiceConfigElement(parent, apply, pos, currentVal, _options);
    }

    public override ConfigValue Deserialize(string data)
    {
        return new ChoiceConfigValue(this, Convert.ToInt32(data, CultureInfo.InvariantCulture));
    }

    public string GetOption(int index)
    {
        return _options[index >= _options.Length ? 0 : index];
    }
}

public class ChoiceConfigValue(ChoiceConfigType type, int value) : ConfigValue<ChoiceConfigType>(type)
{
    private readonly ChoiceConfigType _type = type;

    public int GetValue()
    {
        return value;
    }

    public string GetStringValue()
    {
        return _type.GetOption(value);
    }

    public override string SerializeValue()
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }
}

public class ChoiceConfigElement : ConfigElement
{
    private readonly Button _input;
    private int _active = -1;

    public ChoiceConfigElement(GameObject parent, Button apply, Vector3 pos, [CanBeNull] string currentVal, string[] options)
    {
        (_input, var txt) = UIUtils.MakeTextButton("Config Input", "Default", parent, pos, 
            Vector2.zero, Vector2.zero, size:new Vector2(120, 25));

        if (currentVal != null)
        {
            _active = Convert.ToInt32(currentVal, CultureInfo.InvariantCulture);
            txt.textComponent.text = options[_active];
        }

        _input.onClick.AddListener(() =>
        {
            _active = (_active + 1) % options.Length;
            txt.textComponent.text = options[_active];
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