using System;
using System.Globalization;
using Architect.Utils;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace Architect.Config.Types;

public class Vector3ConfigType(
    string name, 
    string id, 
    Action<GameObject, Vector3ConfigValue> action,
    [CanBeNull] Action<GameObject, Vector3ConfigValue, ConfigurationManager.PreviewContext> previewAction = null
    ) : ConfigType<Vector3ConfigValue>(name, id, action, previewAction)
{
    private Vector3? _defaultValue;

    public Vector3ConfigType WithDefaultValue(Vector3 value)
    {
        _defaultValue = value;
        return this;
    }

    public override ConfigValue GetDefaultValue()
    {
        return _defaultValue.HasValue ? new Vector3ConfigValue(this, _defaultValue.Value) : null;
    }

    public override ConfigElement CreateInput(GameObject parent, Button apply, Vector3 pos, string currentVal)
    {
        return new Vector3ConfigElement(parent, apply, pos, currentVal);
    }

    public override ConfigValue Deserialize(string data)
    {
        Vector3 f;
        try
        {
            f = JsonConvert.DeserializeObject<Vector3>(data);
        }
        catch
        {
            f = Vector3.zero;
        }
        return new Vector3ConfigValue(this, f);
    }
}

public class Vector3ConfigValue(Vector3ConfigType type, Vector3 value) : ConfigValue<Vector3ConfigType>(type)
{
    public Vector3 GetValue()
    {
        return value;
    }

    public override string SerializeValue()
    {
        return JsonConvert.SerializeObject(value);
    }
}

public class Vector3ConfigElement : ConfigElement
{
    private readonly RectTransform _parent;
    private readonly InputField _inputX;
    private readonly InputField _inputY;
    private readonly InputField _inputZ;

    public Vector3ConfigElement(GameObject parent, Button apply, Vector3 pos, [CanBeNull] string currentVal)
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
        
        (_inputX, _) = UIUtils.MakeTextbox("X", o, new Vector3(-35, 0), Vector2.zero, Vector2.zero, 
            80, 25);
        (_inputY, _) = UIUtils.MakeTextbox("Y", o, Vector2.zero, Vector2.zero, Vector2.zero, 
            80, 25);
        (_inputZ, _) = UIUtils.MakeTextbox("Z", o, new Vector3(35, 0), Vector2.zero, Vector2.zero, 
            80, 25);

        if (currentVal != null)
        {
            var v3 = JsonConvert.DeserializeObject<Vector3>(currentVal);
            _inputX.text = v3.x.ToString(CultureInfo.InvariantCulture);
            _inputY.text = v3.y.ToString(CultureInfo.InvariantCulture);
            _inputZ.text = v3.z.ToString(CultureInfo.InvariantCulture);
        }
        
        var lastX = _inputX.text;
        var lastY = _inputY.text;
        var lastZ = _inputZ.text;

        _inputX.characterValidation = _inputY.characterValidation = _inputZ.characterValidation = 
            InputField.CharacterValidation.Decimal;
        
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
        _inputZ.onValueChanged.AddListener(s =>
        {
            if (lastZ == s) return;
            lastZ = s;
            apply.interactable = true;
        });
    }

    public override RectTransform GetElement()
    {
        return _parent;
    }

    public override string GetValue()
    {
        var v3 = new Vector3();
        
        if (float.TryParse(_inputX.text, NumberStyles.Float, CultureInfo.InvariantCulture, out var x)) 
            v3.x = x;
        if (float.TryParse(_inputY.text, NumberStyles.Float, CultureInfo.InvariantCulture, out var y)) 
            v3.y = y;
        if (float.TryParse(_inputZ.text, NumberStyles.Float, CultureInfo.InvariantCulture, out var z)) 
            v3.z = z;
        
        return JsonConvert.SerializeObject(v3);
    }
}