using System;
using System.Globalization;
using Architect.Utils;
using Architect.Workshop.Items;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Architect.Workshop.Types;

public class ChoiceConfigType<T>(
    string name, 
    string id, 
    Action<T, ChoiceConfigValue<T>> action
    ) : ConfigType<T, ChoiceConfigValue<T>>(name, id, action)
    where T : WorkshopItem
{
    private int? _defaultValue;
    private string[] _options = [];

    public ChoiceConfigType<T> WithOptions(params string[] options)
    {
        _options = options;
        return this;
    }

    public ChoiceConfigType<T> WithDefaultValue(int value)
    {
        _defaultValue = value;
        return this;
    }

    public override ConfigValue GetDefaultValue()
    {
        return _defaultValue.HasValue ? new ChoiceConfigValue<T>(this, _defaultValue.Value) : null;
    }

    public override ConfigElement CreateInput(GameObject parent, Button apply, Vector3 pos, string currentVal)
    {
        return new ChoiceConfigElement(parent, apply, pos, currentVal, _options);
    }

    public override ConfigValue Deserialize(string data)
    {
        var value = data switch
        {
            "False" => 0,
            "True" => 1,
            _ => Convert.ToInt32(data, CultureInfo.InvariantCulture)
        };
        return new ChoiceConfigValue<T>(this, value);
    }

    public string GetOption(int index)
    {
        return _options[index >= _options.Length ? 0 : index];
    }
}

public class ChoiceConfigValue<T>(ChoiceConfigType<T> type, int value) 
    : ConfigValue<ChoiceConfigType<T>>(type)
    where T : WorkshopItem
{
    private readonly ChoiceConfigType<T> _type = type;

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
    private readonly string[] _options;
    private readonly Button _apply;
    private readonly UIUtils.Label _txt;
    private int _active = -1;

    public ChoiceConfigElement(GameObject parent, Button apply, Vector3 pos, [CanBeNull] string currentVal, string[] options)
    {
        (_input, _txt) = UIUtils.MakeTextButton("Config Input", "Default", parent, pos, 
            new Vector2(0, 0.5f), new Vector2(0, 0.5f), size:new Vector2(120, 25));
        _options = options;
        _apply = apply;
        
        var choice = _input.gameObject.AddComponent<ChoiceButton>();
        choice.Cce = this;

        if (currentVal != null)
        {
            _active = Convert.ToInt32(currentVal, CultureInfo.InvariantCulture);
            _txt.textComponent.text = options[_active];
        }
    }

    public override RectTransform GetElement()
    {
        return _input.transform as RectTransform;
    }

    public override string GetValue()
    {
        return _active.ToString(CultureInfo.InvariantCulture);
    }

    public class ChoiceButton : MonoBehaviour, IPointerClickHandler
    {
        public ChoiceConfigElement Cce;
        
        public void OnPointerClick(PointerEventData eventData)
        {
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                    Cce._active += 1;
                    break;
                case PointerEventData.InputButton.Right:
                    if (Cce._active != -1) Cce._active -= 1;
                    break;
                case PointerEventData.InputButton.Middle:
                default:
                    return;
            }

            if (Cce._active < 0) Cce._active += Cce._options.Length;
            Cce._active %= Cce._options.Length;
            
            Cce._txt.textComponent.text = Cce._options[Cce._active];
            Cce._apply.interactable = true;
        }
    }
}