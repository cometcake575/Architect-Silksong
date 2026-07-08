using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Architect.Storage;
using Architect.Utils;
using Silksong.ModMenu;
using Silksong.ModMenu.Elements;
using Silksong.ModMenu.Models;
using Silksong.ModMenu.Screens;
using TeamCherry.Localization;
using UnityEngine;
using UnityEngine.Bindings;

namespace Architect.Workshop.Items;

public abstract class CustomConfig : WorkshopItem
{
    public static readonly Dictionary<string, CustomConfig> Configs = [];
    private static readonly Dictionary<string, Dictionary<string, CustomConfig>> ExtConfigs = [];
    
    public string Name = string.Empty;

    public static void Init()
    {
        MakeScreen(LocalizedText.Key(new LocalisedString("ArchitectMap", "ArchitectMap")), Configs, 
            () => GlobalArchitectData.Instance.MapLabel);
    }

    private static void MakeScreen(LocalizedText text, Dictionary<string, CustomConfig> configs, Func<string> mapLabel)
    {
        SimpleMenuScreen mapKeybinds = null;
        List<SelectableElement> keyButtons = [];
        
        Registry.AddModMenu("Architect Map", () =>
        {
            var tb = new TextButton(text);
            tb.OnSubmit += () =>
            {
                mapKeybinds?.Dispose();
                foreach (var kb in keyButtons) kb.Dispose();
                keyButtons.Clear();
                
                mapKeybinds = new SimpleMenuScreen(mapLabel());
                MenuScreenNavigation.Show(mapKeybinds);

                foreach (var kbe in configs.Values.Select(cc => cc.GetElement()))
                {
                    keyButtons.Add(kbe);
                    mapKeybinds.Add(kbe);
                }
            };
            return tb;
        });
    }
    
    public override void Register()
    {
        if (ExternalSource == null) Configs.Add(Id, this);
        else
        {
            if (!ExtConfigs.TryGetValue(ExternalSource, out var configs))
            {
                configs = ExtConfigs[ExternalSource] = [];
                MakeScreen(LocalizedText.Raw(ExternalSource), configs, () => ExternalSource);
            }

            configs.Add(Id, this);
        }
    }

    public override void Unregister()
    {
        if (ExternalSource == null) Configs.Remove(Id);
        else if (ExtConfigs.TryGetValue(ExternalSource, out var keybinds)) 
            keybinds.Remove(Id);
    }

    protected abstract SelectableElement GetElement();
}

public class CustomKeybind : CustomConfig
{
    private static readonly Sprite Icon = ResourceUtils.LoadSpriteResource("key_listener", FilterMode.Point);
    
    public KeyCode Default = KeyCode.None;
    
    public override void Register()
    {
        base.Register();
        
        if (!GlobalArchitectData.Instance.Keybinds.ContainsKey(Id) || Settings.TestMode.Value)
            GlobalArchitectData.Instance.Keybinds[Id] = Default;
    }
    
    public override void Unregister()
    {
        base.Unregister();
        
        if (GlobalArchitectData.Instance.Keybinds.TryGetValue(Id, out var old) && old == KeyCode.None)
            GlobalArchitectData.Instance.Keybinds.Remove(Id);
    }

    protected override SelectableElement GetElement()
    {
        return new CustomKeyBindElement(Name,
            new ValueModel<KeyCode>(GlobalArchitectData.Instance.Keybinds[Id]))
        {
            Keybind = this
        };
    }

    public override Sprite GetIcon() => Icon;

    private class CustomKeyBindElement : KeyBindElement
    {
        public CustomKeybind Keybind;
        
        public CustomKeyBindElement(string label, [NotNull] IValueModel<KeyCode> model) : base(label, model)
        {
            OnValueChanged += DoChange;
            OnDispose += DoDispose;
        }
        
        private void DoChange(KeyCode code)
        {
            GlobalArchitectData.Instance.Keybinds[Keybind.Id] = code;
        }
        
        private void DoDispose()
        {
            OnValueChanged -= DoChange;
            OnDispose -= DoDispose;
        }
    }
}

public class CustomOption : CustomConfig
{
    private static readonly Sprite Icon = ResourceUtils.LoadSpriteResource("choice_display", FilterMode.Point);

    public OptionType OType;
    public string Default = string.Empty;
    public string Desc = string.Empty;

    public override void Register()
    {
        base.Register();
        switch (OType)
        {
            case OptionType.Text:
                if (!GlobalArchitectData.Instance.StringVariables.ContainsKey(Id) || Settings.TestMode.Value)
                {
                    GlobalArchitectData.Instance.StringVariables[Id] = Default;
                }

                break;
            case OptionType.Float:
            case OptionType.Int:
                if ((!GlobalArchitectData.Instance.FloatVariables.ContainsKey(Id) || Settings.TestMode.Value)
                    && float.TryParse(Default, out var f))
                {
                    GlobalArchitectData.Instance.FloatVariables[Id] = f;
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    protected override SelectableElement GetElement()
    {
        SelectableElement sm = OType switch
        {
            OptionType.Text => new CustomOptionElement<string>(Name, Desc,
                new ParserTextModel<string>((t, out v) =>
                {
                    v = t;
                    return true;
                }, (value, out text) =>
                {
                    text = value;
                    return true;
                }), GlobalArchitectData.Instance.StringVariables.GetValueOrDefault(Id, Default), this),
            OptionType.Int => new CustomOptionElement<int>(Name, Desc,
                new ParserTextModel<int>(
                    (t, out v) => int.TryParse(t, out v), 
                    (value, out text) =>
                {
                    text = value.ToString();
                    return true;
                }), Mathf.RoundToInt(GlobalArchitectData.Instance.FloatVariables.GetValueOrDefault(Id, int.TryParse(Default, out var r) ? r : 0)), this),
            OptionType.Float => new CustomOptionElement<float>(Name, Desc,
                new ParserTextModel<float>(
                    (t, out v) => float.TryParse(t, out v), 
                    (value, out text) =>
                {
                    text = value.ToString(CultureInfo.InvariantCulture);
                    return true;
                }), GlobalArchitectData.Instance.FloatVariables.GetValueOrDefault(Id, float.TryParse(Default, out var r) ? r : 0), this),
            _ => throw new ArgumentOutOfRangeException()
        };
        return sm;
    }

    public override Sprite GetIcon() => Icon;

    private class CustomOptionElement<T> : TextInput<T>
    {
        private readonly CustomOption _option;
        
        public CustomOptionElement(string label, string desc, ParserTextModel<T> parser, T def, CustomOption opt) 
            : base(label, parser, desc)
        {
            OnValueChanged += DoChange;
            OnDispose += DoDispose;
            _option = opt;
            
            InputField.text = def.ToString();
        }
        
        private void DoChange(T val)
        {
            switch (val)
            {
                case string s:
                    GlobalArchitectData.Instance.StringVariables[_option.Id] = s;
                    break;
                case float f:
                    GlobalArchitectData.Instance.FloatVariables[_option.Id] = f;
                    break;
                case int i:
                    GlobalArchitectData.Instance.FloatVariables[_option.Id] = i;
                    break;
                case bool b:
                    GlobalArchitectData.Instance.BoolVariables[_option.Id] = b;
                    break;
            }
        }
        
        private void DoDispose()
        {
            OnValueChanged -= DoChange;
            OnDispose -= DoDispose;
        }
    }

    public enum OptionType
    {
        Text,
        Int,
        Float
    }
}

public class CustomToggle : CustomConfig
{
    private static readonly Sprite Icon = ResourceUtils.LoadSpriteResource("choice_display", FilterMode.Point);

    public bool Default = false;
    public string Desc = string.Empty;

    public override void Register()
    {
        base.Register();

        if (!GlobalArchitectData.Instance.BoolVariables.ContainsKey(Id)
            || Settings.TestMode.Value)
        {
            GlobalArchitectData.Instance.BoolVariables[Id] = Default;
        }
    }

    protected override SelectableElement GetElement()
    {
        return new CustomOptionElement(Name, Desc, this);
    }

    public override Sprite GetIcon() => Icon;

    private class CustomOptionElement : ChoiceElement<bool>
    {
        private readonly CustomToggle _option;
        
        public CustomOptionElement(string label, string desc, CustomToggle opt) 
            : base(label, [false, true], desc)
        {
            OnValueChanged += DoChange;
            OnDispose += DoDispose;
            _option = opt;

            Value = GlobalArchitectData.Instance.BoolVariables.TryGetValue(_option.Id, out var b) 
                ? b : _option.Default;
        }
        
        private void DoChange(bool val)
        {
            GlobalArchitectData.Instance.BoolVariables[_option.Id] = val;
        }
        
        private void DoDispose()
        {
            OnValueChanged -= DoChange;
            OnDispose -= DoDispose;
        }
    }
}