using System;
using System.Globalization;
using Architect.Utils;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace Architect.Config.Types;

public class Vector2ConfigType(
    string name, 
    string id, 
    Action<GameObject, Vector2ConfigValue> action,
    [CanBeNull] Action<GameObject, Vector2ConfigValue, ConfigurationManager.PreviewContext> previewAction = null
    ) : ConfigType<Vector2ConfigValue>(name, id, action, previewAction)
{
    private Vector2? _defaultValue;

    public Vector2ConfigType WithDefaultValue(Vector2 value)
    {
        _defaultValue = value;
        return this;
    }

    public override ConfigValue GetDefaultValue()
    {
        return _defaultValue.HasValue ? new Vector2ConfigValue(this, _defaultValue.Value) : null;
    }

    public override ConfigElement CreateInput(GameObject parent, Button apply, Vector3 pos, string currentVal)
    {
        return new Vector2ConfigElement(parent, apply, pos, currentVal);
    }

    public override ConfigValue Deserialize(string data)
    {
        Vector2 f;
        try
        {
            f = JsonConvert.DeserializeObject<Vector2>(data);
        }
        catch
        {
            f = Vector2.zero;
        }
        return new Vector2ConfigValue(this, f);
    }
}

public class Vector2ConfigValue(Vector2ConfigType type, Vector2 value) : ConfigValue<Vector2ConfigType>(type)
{
    public Vector2 GetValue()
    {
        return value;
    }

    public override string SerializeValue()
    {
        return JsonConvert.SerializeObject(value);
    }
}

public class Vector2ConfigElement : ConfigElement
{
    private readonly RectTransform _parent;
    private readonly InputField _inputX;
    private readonly InputField _inputY;

    public Vector2ConfigElement(GameObject parent, Button apply, Vector3 pos, [CanBeNull] string currentVal)
    {
        var o = new GameObject("Config Input")
        {
            transform =
            {
                parent = parent.transform,
                localScale = Vector3.one
            }
        };
        _parent = o.RemoveOffset();
        _parent.anchoredPosition = pos;
        
        (_inputX, _) = UIUtils.MakeTextbox("X", o, new Vector2(-17.5f, 0), Vector2.zero, Vector2.zero, 
            80, 25);
        (_inputY, _) = UIUtils.MakeTextbox("Y", o, new Vector2(17.5f, 0), Vector2.zero, Vector2.zero, 
            80, 25);

        if (currentVal != null)
        {
            var v2 = JsonConvert.DeserializeObject<Vector2>(currentVal);
            _inputX.text = v2.x.ToString(CultureInfo.InvariantCulture);
            _inputY.text = v2.y.ToString(CultureInfo.InvariantCulture);
        }
        
        var lastX = _inputX.text;
        var lastY = _inputY.text;

        _inputX.characterValidation = _inputY.characterValidation = InputField.CharacterValidation.Decimal;
        
        _inputX.onValueChanged.AddListener(s =>
        {
            if (lastX == s) return;
            lastX = s;
            apply.interactable = true;
        });
        _inputY.onValueChanged.AddListener(s =>
        {
            if (lastY == s) return;
            lastY = s;
            apply.interactable = true;
        });
    }

    public override RectTransform GetElement()
    {
        return _parent;
    }

    public override string GetValue()
    {
        var v2 = new Vector2();
        
        if (float.TryParse(_inputX.text, NumberStyles.Float, CultureInfo.InvariantCulture, out var x)) 
            v2.x = x;
        if (float.TryParse(_inputY.text, NumberStyles.Float, CultureInfo.InvariantCulture, out var y)) 
            v2.y = y;
        
        return JsonConvert.SerializeObject(v2);
    }
}