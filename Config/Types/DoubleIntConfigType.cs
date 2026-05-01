using System;
using System.Globalization;
using Architect.Utils;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace Architect.Config.Types;

public class DoubleIntConfigType(
    string name, 
    string id, 
    Action<GameObject, DoubleIntConfigValue> action,
    [CanBeNull] Action<GameObject, DoubleIntConfigValue, ConfigurationManager.PreviewContext> previewAction = null
    ) : ConfigType<DoubleIntConfigValue>(name, id, action, previewAction)
{
    private (int, int)? _defaultValue;

    public DoubleIntConfigType WithDefaultValue((int, int) value)
    {
        _defaultValue = value;
        return this;
    }

    public override ConfigValue GetDefaultValue()
    {
        return _defaultValue.HasValue ? new DoubleIntConfigValue(this, _defaultValue.Value) : null;
    }

    public override ConfigElement CreateInput(GameObject parent, Button apply, Vector3 pos, string currentVal)
    {
        return new DoubleIntConfigElement(parent, apply, pos, currentVal);
    }

    public override ConfigValue Deserialize(string data)
    {
        (int, int) f;
        try
        {
            f = JsonConvert.DeserializeObject<(int, int)>(data);
        }
        catch
        {
            f = (0, 0);
        }
        return new DoubleIntConfigValue(this, f);
    }
}

public class DoubleIntConfigValue(DoubleIntConfigType type, (int, int) value) : ConfigValue<DoubleIntConfigType>(type)
{
    public (int, int) GetValue()
    {
        return value;
    }

    public override string SerializeValue()
    {
        return JsonConvert.SerializeObject(value);
    }
}

public class DoubleIntConfigElement : ConfigElement
{
    private readonly RectTransform _parent;
    private readonly InputField _inputX;
    private readonly InputField _inputY;

    public DoubleIntConfigElement(GameObject parent, Button apply, Vector3 pos, [CanBeNull] string currentVal)
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
        
        (_inputX, _) = UIUtils.MakeTextbox("X", o, new Vector3(-17.5f, 0), Vector2.zero, Vector2.zero, 
            80, 25);
        (_inputY, _) = UIUtils.MakeTextbox("Y", o, new Vector2(17.5f, 0), Vector2.zero, Vector2.zero, 
            80, 25);

        if (currentVal != null)
        {
            var v3 = JsonConvert.DeserializeObject<(int, int)>(currentVal);
            _inputX.text = v3.Item1.ToString(CultureInfo.InvariantCulture);
            _inputY.text = v3.Item2.ToString(CultureInfo.InvariantCulture);
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
        if (!int.TryParse(_inputX.text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var x)) x = 1;
        if (!int.TryParse(_inputY.text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var y)) y = 1;
        
        return JsonConvert.SerializeObject((x, y));
    }
}