using System;
using System.Globalization;
using Architect.Utils;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace Architect.Config.Types;

public class ColourConfigType(
    string name, 
    string id, 
    Action<GameObject, ColourConfigValue> action,
    bool alpha,
    [CanBeNull] Action<GameObject, ColourConfigValue, ConfigurationManager.PreviewContext> previewAction = null
    ) : ConfigType<ColourConfigValue>(name, id, action, previewAction)
{
    private Color? _defaultValue;

    public ColourConfigType WithDefaultValue(Color value)
    {
        _defaultValue = value;
        return this;
    }

    public override ConfigValue GetDefaultValue()
    {
        return _defaultValue.HasValue ? new ColourConfigValue(this, _defaultValue.Value) : null;
    }

    public override ConfigElement CreateInput(GameObject parent, Button apply, Vector3 pos, string currentVal)
    {
        return new ColourConfigElement(parent, apply, pos, currentVal, alpha);
    }

    public override ConfigValue Deserialize(string data)
    {
        Color f;
        try
        {
            f = JsonConvert.DeserializeObject<Color>(data);
        }
        catch
        {
            f = Color.white;
        }
        return new ColourConfigValue(this, f);
    }
}

public class ColourConfigValue(ColourConfigType type, Color value) : ConfigValue<ColourConfigType>(type)
{
    public Color GetValue()
    {
        return value;
    }

    public override string SerializeValue()
    {
        return JsonConvert.SerializeObject(value);
    }
}

public class ColourConfigElement : ConfigElement
{
    private readonly RectTransform _parent;
    
    private readonly InputField _inputR;
    private readonly InputField _inputG;
    private readonly InputField _inputB;
    private readonly InputField _inputA;

    public ColourConfigElement(GameObject parent, Button apply, Vector3 pos, [CanBeNull] string currentVal, bool alpha)
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

        var take = alpha ? 17.5f : 0;
        _inputR = UIUtils.MakeTextbox("R", o, new Vector3(-35 - take, 0), Vector2.zero, Vector2.zero, 
            80, 25).Item1;
        _inputG = UIUtils.MakeTextbox("G", o, new Vector3(-take, 0), Vector2.zero, Vector2.zero, 
            80, 25).Item1;
        _inputB = UIUtils.MakeTextbox("B", o, new Vector3(35 - take, 0), Vector2.zero, Vector2.zero, 
            80, 25).Item1;
        if (alpha) _inputA = UIUtils.MakeTextbox("A", o, new Vector3(52.5f, 0), Vector2.zero, Vector2.zero, 
            80, 25).Item1;

        if (currentVal != null)
        {
            var c = JsonConvert.DeserializeObject<Color>(currentVal);
            _inputR.text = c.r.ToString(CultureInfo.InvariantCulture);
            _inputG.text = c.g.ToString(CultureInfo.InvariantCulture);
            _inputB.text = c.b.ToString(CultureInfo.InvariantCulture);
            if (alpha) _inputA.text = c.a.ToString(CultureInfo.InvariantCulture);
        }
        
        var lastR = _inputR.text;
        var lastG = _inputG.text;
        var lastB = _inputB.text;

        _inputR.characterValidation = _inputG.characterValidation = _inputB.characterValidation = 
            InputField.CharacterValidation.Decimal;
        if (alpha) _inputA.characterValidation = InputField.CharacterValidation.Decimal;
        
        _inputR.onValueChanged.AddListener(s =>
        {
            if (lastR == s) return;
            lastR = s;
            apply.interactable = true;
        });
        _inputG.onValueChanged.AddListener(s =>
        {
            if (lastG == s) return;
            lastG = s;
            apply.interactable = true;
        });
        _inputB.onValueChanged.AddListener(s =>
        {
            if (lastB == s) return;
            lastB = s;
            apply.interactable = true;
        });

        if (alpha)
        {
            var lastA = _inputA.text;
            _inputA.onValueChanged.AddListener(s =>
            {
                if (lastA == s) return;
                lastA = s;
                apply.interactable = true;
            });
        }
    }

    public override RectTransform GetElement()
    {
        return _parent;
    }

    public override string GetValue()
    {
        if (!float.TryParse(_inputR.text, NumberStyles.Float, CultureInfo.InvariantCulture, out var r)) r = 1;
        if (!float.TryParse(_inputG.text, NumberStyles.Float, CultureInfo.InvariantCulture, out var g)) g = 1;
        if (!float.TryParse(_inputB.text, NumberStyles.Float, CultureInfo.InvariantCulture, out var b)) b = 1;

        float a = 1;
        if (_inputA)
        {
            float.TryParse(_inputA.text, NumberStyles.Float, CultureInfo.InvariantCulture, out a);
        }

        return JsonConvert.SerializeObject(new Color(r, g, b, a));
    }
}