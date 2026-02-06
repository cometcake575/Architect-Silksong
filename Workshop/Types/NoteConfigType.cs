using System;
using Architect.Utils;
using Architect.Workshop.Items;
using UnityEngine;
using UnityEngine.UI;

namespace Architect.Workshop.Types;

public class NoteConfigType<T>(
    string name, 
    string id)
    : ConfigType<T, NoteConfigValue<T>>(name, id, (_, _) => { })
    where T : WorkshopItem
{
    public override ConfigValue GetDefaultValue()
    {
        return null;
    }

    public override ConfigElement CreateInput(GameObject parent, Button apply, Vector3 pos, string currentVal)
    {
        return new NoteConfigElement(apply);
    }

    public override ConfigValue Deserialize(string data)
    {
        return new NoteConfigValue<T>(this, data);
    }
}

public class NoteConfigValue<T>(NoteConfigType<T> type, string value) :
    ConfigValue<NoteConfigType<T>>(type) 
    where T : WorkshopItem
{
    public override string SerializeValue()
    {
        return value;
    }
}

public class NoteConfigElement : ConfigElement
{
    private readonly Button _input;

    public NoteConfigElement(Button apply)
    {
        _input = apply;
        apply.gameObject.SetActive(false);
        apply.GetComponent<UIUtils.LabelledButton>().label.gameObject.SetActive(false);
    }

    public override RectTransform GetElement()
    {
        return _input.transform as RectTransform;
    }

    public override string GetValue()
    {
        return "";
    }
}