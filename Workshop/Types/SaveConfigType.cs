using System;
using System.Globalization;
using Architect.Utils;
using Architect.Workshop.Items;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Architect.Workshop.Types;

public class SaveConfigType<T>(
    string name, 
    string id, 
    Action<T, SaveConfigValue<T>> action
    ) : ConfigType<T, SaveConfigValue<T>>(name, id, action)
    where T : WorkshopItem
{
    public override ConfigValue GetDefaultValue()
    {
        return null;
    }

    public override ConfigElement CreateInput(GameObject parent, Button apply, Vector3 pos, string currentVal)
    {
        return new SaveConfigElement(parent, apply, pos, currentVal);
    }

    public override ConfigValue Deserialize(string data)
    {
        return new SaveConfigValue<T>(this, data);
    }
}

public class SaveConfigValue<T>(SaveConfigType<T> type, string value) 
    : ConfigValue<SaveConfigType<T>>(type)
    where T : WorkshopItem
{
    public string GetValue()
    {
        return value;
    }

    public override string SerializeValue()
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }
}

public class SaveConfigElement : ConfigElement
{
    private readonly Button _input;
    private readonly Button _apply;
    private string _value = string.Empty;

    public SaveConfigElement(GameObject parent, Button apply, Vector3 pos, [CanBeNull] string currentVal)
    {
        (_input, _) = UIUtils.MakeTextButton("Config Input", "Extract", parent, pos, 
            new Vector2(0, 0.5f), new Vector2(0, 0.5f), size:new Vector2(120, 25));
        _apply = apply;
        
        var choice = _input.gameObject.AddComponent<ChoiceButton>();
        choice.Cce = this;

        if (currentVal != null) _value = currentVal;
    }

    public override RectTransform GetElement()
    {
        return _input.transform as RectTransform;
    }

    public override string GetValue()
    {
        return _value;
    }

    public class ChoiceButton : MonoBehaviour, IPointerClickHandler
    {
        public SaveConfigElement Cce;
        
        public void OnPointerClick(PointerEventData eventData)
        {
            Cce._value = SaveDataUtility.SerializeSaveData(new SaveGameData(
                GameManager.instance.playerData,
                GameManager.instance.sceneData));
            
            Cce._apply.interactable = true;
        }
    }
}