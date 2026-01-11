using System;
using Architect.Utils;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Architect.Config.Types;

public class IdConfigType(
    string name, 
    string id, 
    Action<GameObject, IdConfigValue> action,
    [CanBeNull] Action<GameObject, IdConfigValue, ConfigurationManager.PreviewContext> previewAction = null
) : ConfigType<IdConfigValue>(name, id, action, previewAction)
{
    [CanBeNull] private string _defaultValue;

    public IdConfigType WithDefaultValue(string value)
    {
        _defaultValue = value;
        return this;
    }

    public override ConfigValue GetDefaultValue()
    {
        return _defaultValue == null ? null : new IdConfigValue(this, _defaultValue);
    }

    public override ConfigElement CreateInput(GameObject parent, Button apply, Vector3 pos, string currentVal)
    {
        return new IdConfigElement(parent, apply, pos, currentVal);
    }

    public override ConfigValue Deserialize(string data)
    {
        return new IdConfigValue(this, data);
    }
}

public class IdConfigValue(IdConfigType type, string value) : ConfigValue<IdConfigType>(type)
{
    private string _extraId;
    
    public string GetValue()
    {
        return value + (_extraId ?? "");
    }

    public override string SerializeValue()
    {
        return value;
    }
    
    public override void Setup(GameObject obj, string extraId)
    {
        _extraId = extraId;
        base.Setup(obj, extraId);
    }
}

public class IdConfigElement : ConfigElement
{
    private readonly InputField _input;

    public IdConfigElement(GameObject parent, Button apply, Vector3 pos, [CanBeNull] string currentVal)
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

    public InputField GetField() => _input;

    public override RectTransform GetElement()
    {
        return _input.transform as RectTransform;
    }

    public override string GetValue()
    {
        return _input.text;
    }
}