using System;
using System.Globalization;
using Architect.Utils;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Architect.Config.Types;

public class FloatConfigType(
    string name, 
    string id, 
    Action<GameObject, FloatConfigValue> action,
    [CanBeNull] Action<GameObject, FloatConfigValue, ConfigurationManager.PreviewContext> previewAction = null
    ) : ConfigType<FloatConfigValue>(name, id, action, previewAction)
{
    private float? _defaultValue;

    public FloatConfigType WithDefaultValue(float value)
    {
        _defaultValue = value;
        return this;
    }

    public override ConfigValue GetDefaultValue()
    {
        return _defaultValue.HasValue ? new FloatConfigValue(this, _defaultValue.Value) : null;
    }

    public override ConfigElement CreateInput(GameObject parent, Button apply, Vector3 pos, string currentVal)
    {
        return new FloatConfigElement(parent, apply, pos, currentVal);
    }

    public override ConfigValue Deserialize(string data)
    {
        float f;
        try
        {
            f = Convert.ToSingle(data, CultureInfo.InvariantCulture);
        }
        catch (FormatException)
        {
            f = 9999999;
        }
        return new FloatConfigValue(this, f);
    }
}

public class FloatConfigValue(FloatConfigType type, float value) : ConfigValue<FloatConfigType>(type)
{
    public float GetValue()
    {
        return value;
    }

    public override string SerializeValue()
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }
}

public class FloatConfigElement : ConfigElement
{
    private readonly InputField _input;

    public FloatConfigElement(GameObject parent, Button apply, Vector3 pos, [CanBeNull] string currentVal)
    {
        (_input, _) = UIUtils.MakeTextbox("Config Input", parent, pos, Vector2.zero, Vector2.zero, 
            120, 25);

        if (currentVal != null) _input.text = currentVal;
        var last = _input.text;

        _input.characterValidation = InputField.CharacterValidation.Decimal;
        
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