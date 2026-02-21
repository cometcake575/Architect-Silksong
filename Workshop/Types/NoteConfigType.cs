using Architect.Utils;
using Architect.Workshop.Items;
using UnityEngine;
using UnityEngine.UI;

namespace Architect.Workshop.Types;

public class NoteConfigType(
    string name)
    : ConfigType<WorkshopItem, NoteConfigValue>(name, string.Empty, (_, _) => { })
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
        return new NoteConfigValue(this, data);
    }
        
    public static implicit operator NoteConfigType(string s)
    {
        return new NoteConfigType(s);
    }
}

public class NoteConfigValue(NoteConfigType type, string value) :
    ConfigValue<NoteConfigType>(type)
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