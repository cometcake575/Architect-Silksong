using System;
using System.Globalization;
using Architect.Utils;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Architect.Config.Types;

public class IntConfigType(
    string name, 
    string id, 
    Action<GameObject, IntConfigValue> action,
    [CanBeNull] Action<GameObject, IntConfigValue, ConfigurationManager.PreviewContext> previewAction = null
    ) : ConfigType<IntConfigValue>(name, id, action, previewAction)
{
    private int? _defaultValue;

    public IntConfigType WithDefaultValue(int value)
    {
        _defaultValue = value;
        return this;
    }

    public override ConfigValue GetDefaultValue()
    {
        return _defaultValue.HasValue ? new IntConfigValue(this, _defaultValue.Value) : null;
    }

    public override ConfigElement CreateInput(GameObject parent, Button apply, Vector3 pos, string currentVal)
    {
        return new IntConfigElement(parent, apply, pos, currentVal);
    }

    public override ConfigValue Deserialize(string data)
    {
        return new IntConfigValue(this, Convert.ToInt32(data, CultureInfo.InvariantCulture));
    }
}

public class IntConfigValue(IntConfigType type, int value) : ConfigValue<IntConfigType>(type)
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
        (_input, _) = UIUtils.MakeTextbox("Config Input", parent, pos, Vector2.zero, Vector2.zero, 
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