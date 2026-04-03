using System;
using System.Collections.Generic;
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

public class CustomKeybind : WorkshopItem
{
    private static readonly Sprite Icon = ResourceUtils.LoadSpriteResource("key_listener", FilterMode.Point);
    public static readonly Dictionary<string, CustomKeybind> Keybinds = [];

    public string Name = string.Empty;
    public KeyCode Default = KeyCode.None;
    
    public override void Register()
    {
        if (!GlobalArchitectData.Instance.Keybinds.ContainsKey(Id) || Settings.TestMode.Value)
            GlobalArchitectData.Instance.Keybinds[Id] = Default;
        Keybinds.Add(Id, this);
    }
    
    public override void Unregister()
    {
        Keybinds.Remove(Id);
        if (GlobalArchitectData.Instance.Keybinds.TryGetValue(Id, out var old) && old == KeyCode.None)
            GlobalArchitectData.Instance.Keybinds.Remove(Id);
    }
    
    public override Sprite GetIcon() => Icon;
    
    public static void Init()
    {
        SimpleMenuScreen mapKeybinds = null;
        List<CustomKeyBindElement> keyButtons = [];
        
        Registry.AddModMenu("Architect Map", () =>
        {
            var tb = new TextButton(LocalizedText.Key(new LocalisedString("ArchitectMap", "ArchitectMap")));
            tb.OnSubmit += () =>
            {
                mapKeybinds?.Dispose();
                foreach (var kb in keyButtons) kb.Dispose();
                keyButtons.Clear();
                
                mapKeybinds = new SimpleMenuScreen(GlobalArchitectData.Instance.MapLabel);
                MenuScreenNavigation.Show(mapKeybinds);

                foreach (var (id, kb) in Keybinds)
                {
                    var kbe = new CustomKeyBindElement(kb.Name,
                        new ValueModel<KeyCode>(GlobalArchitectData.Instance.Keybinds[id]));
                    kbe.Keybind = kb;
                    keyButtons.Add(kbe);
                    mapKeybinds.Add(kbe);
                }
            };
            return tb;
        });
    }

    public class CustomKeyBindElement : KeyBindElement
    {
        public CustomKeybind Keybind;
        
        public CustomKeyBindElement(string label, [NotNull] IValueModel<KeyCode> model) : base(label, model)
        {
            OnValueChanged += DoChange;
            OnDispose += DoDispose;
        }
        
        public void DoChange(KeyCode code)
        {
            GlobalArchitectData.Instance.Keybinds[Keybind.Id] = code;
        }
        
        public void DoDispose()
        {
            OnValueChanged -= DoChange;
            OnDispose -= DoDispose;
        }
    }
}