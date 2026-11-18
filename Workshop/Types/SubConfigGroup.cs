using System.Collections.Generic;
using System.Linq;
using Architect.Utils;
using Architect.Workshop.Items;
using UnityEngine;
using UnityEngine.UI;

namespace Architect.Workshop.Types;

public class SubConfigGroup(string name, Dictionary<string, List<ConfigType>> groups) : ConfigType<WorkshopItem, NoteConfigValue>(name, string.Empty, (_, _) => { })
{
    public override ConfigValue Deserialize(string data)
    {
        return new NoteConfigValue(data);
    }

    public override ConfigValue GetDefaultValue() => null;

    public override ConfigElement CreateInput(GameObject parent, Button apply, Vector3 pos, string currentVal)
    {
        return new SubConfigElement(parent, apply, pos, groups);
    }
}

public class SubConfigElement : ConfigElement
{
    public SubConfigElement(GameObject parent, Button apply, Vector3 pos, Dictionary<string, List<ConfigType>> groups)
    {
        apply.gameObject.SetActive(false);
        apply.GetComponent<UIUtils.LabelledButton>().label.gameObject.SetActive(false);
        
        var (input, txt) = UIUtils.MakeTextButton("Config Input", "Default", parent, pos, 
            new Vector2(0, 0.5f), new Vector2(0, 0.5f), size:new Vector2(120, 25));

        List<GameObject> objs = [];
        var keys = groups.Keys.ToArray();
        var i = 0;
        input.onClick.AddListener(() =>
        {
            i = (i + 1) % groups.Count;
            Refresh();
        });
        
        foreach (var (name, group) in groups)
        {
            var sub = new GameObject($"Sub {name}")
            {
                transform =
                {
                    parent = parent.transform,
                    localScale = Vector3.one
                }
            };
            objs.Add(sub);
            sub.RemoveOffset();

            var y = pos.y - 18;
            foreach (var type in group)
            {
                type.SetupUI(sub, pos.x - 155, y);
                y -= 12;
            }
        }
        
        Refresh();

        return;

        void Refresh()
        {
            txt.textComponent.text = keys[i];
            foreach (var o in objs) o.SetActive(false);
            objs[i].SetActive(true);
        }
    }

    public override string GetValue()
    {
        return "";
    }
}