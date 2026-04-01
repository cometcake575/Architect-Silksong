using System;
using System.Collections.Generic;
using System.Linq;
using Architect.Utils;
using BepInEx;
using MonoMod.RuntimeDetour;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Architect.Workshop.Items;

public class CustomNeedle : SpriteItem
{
    public static readonly Dictionary<string, CustomNeedle> Needles = [];
    private static InventoryItemNail _nail;
    private static GameObject _nailPrefab;
    
    private InventoryItemNail.DisplayState _displayState;
    
    public int Damage;
    public LocalStr Name = string.Empty;
    public LocalStr Desc = string.Empty;

    public float NeedleRangeMult = 1;
    public int NeedleColourActive = 0;
    public Color NeedleColour = Color.white;
    
    public static void Init()
    {
        typeof(InventoryItemNail).Hook(nameof(InventoryItemNail.Awake),
            (Action<InventoryItemNail> orig, InventoryItemNail self) =>
            {
                orig(self);
                _nail = self;
                _nailPrefab = _nail.displayStates[4].DisplayObject;
                
                foreach (var needle in Needles.Values) needle.Register();
            });
        
        typeof(InventoryItemNail).Hook(nameof(InventoryItemNail.UpdateState),
            (Action<InventoryItemNail> orig, InventoryItemNail self) =>
            {
                orig(self);
                var needle = ArchitectData.Instance.CustomNeedle;
                if (!needle.IsNullOrWhiteSpace() && Needles.TryGetValue(needle, out var upgrade))
                {
                    foreach (var displayState in self.displayStates)
                    {
                        if (displayState.DisplayObject) displayState.DisplayObject.SetActive(false);
                    }

                    self.currentState = upgrade._displayState;
                    upgrade._displayState.DisplayObject.SetActive(true);
                }
            });

        _ = new Hook(typeof(PlayerData).GetProperty(nameof(PlayerData.nailDamage))!.GetGetMethod(),
            (Func<PlayerData, int> orig, PlayerData self) =>
            {
                var needle = ArchitectData.Instance.CustomNeedle;
                if (!needle.IsNullOrWhiteSpace() && Needles.TryGetValue(needle, out var upgrade))
                    return upgrade.Damage;

                return orig(self);
            });
        
        typeof(NailAttackBase).Hook(nameof(NailAttackBase.OnSlashStarting),
            (Action<NailAttackBase> orig, NailAttackBase self) =>
            {
                var needle = ArchitectData.Instance.CustomNeedle;
                if (!needle.IsNullOrWhiteSpace() && Needles.TryGetValue(needle, out var upgrade))
                {
                    if (upgrade.NeedleColourActive == 1)
                    {
                        if (self.slashSprite) self.slashSprite.color = upgrade.NeedleColour;
                        if (self.imbuedSlashAnim) self.imbuedSlashSprite.color = upgrade.NeedleColour;
                    }
                    orig(self);
                    self.transform.localScale *= upgrade.NeedleRangeMult;
                    if (upgrade.NeedleColourActive == 2)
                    {
                        if (self.slashSprite) self.slashSprite.color = upgrade.NeedleColour;
                        if (self.imbuedSlashAnim) self.imbuedSlashSprite.color = upgrade.NeedleColour;
                    }
                } else orig(self);
            });
        
        typeof(HeroExtraNailSlash).Hook(nameof(HeroExtraNailSlash.OnEnable),
            (Action<HeroExtraNailSlash> orig, HeroExtraNailSlash self) =>
            {
                var needle = ArchitectData.Instance.CustomNeedle;
                if (!needle.IsNullOrWhiteSpace() && Needles.TryGetValue(needle, out var upgrade))
                {
                    if (upgrade.NeedleColourActive == 1)
                    {
                        foreach (var tintSprite in self.tintSprites)
                        {
                            if (tintSprite) tintSprite.color = upgrade.NeedleColour;
                        }
                        foreach (var tintTk2dSprite in self.tintTk2dSprites)
                        {
                            if (tintTk2dSprite) tintTk2dSprite.color = upgrade.NeedleColour;
                        }
                    }
                    orig(self);
                    if (upgrade.NeedleColourActive == 2)
                    {
                        foreach (var tintSprite in self.tintSprites)
                        {
                            if (tintSprite) tintSprite.color = upgrade.NeedleColour;
                        }
                        foreach (var tintTk2dSprite in self.tintTk2dSprites)
                        {
                            if (tintTk2dSprite) tintTk2dSprite.color = upgrade.NeedleColour;
                        }
                    }
                } else orig(self);
            });
    }
    
    public override void Register()
    {
        if (!Needles.ContainsKey(Id)) Needles[Id] = this;
        if (!_nail) return;
        
        var nail = Object.Instantiate(_nailPrefab, _nail.transform);
        nail.name = Id;
        _displayState = new InventoryItemNail.DisplayState
        {
            DisplayObject = nail,
            DisplayName = Name,
            Description = Desc
        };
        
        var states = _nail.displayStates.ToList();
        states.Add(_displayState);
        _nail.displayStates = states.ToArray();
        
        base.Register();
    }
    
    public override void Unregister()
    {
        if (_nail)
        {
            var states = _nail.displayStates.ToList();
            states.Remove(_displayState);
            _nail.displayStates = states.ToArray();

            Object.Destroy(_displayState.DisplayObject);
        }

        Needles.Remove(Id);
    }

    protected override void OnReadySprite()
    {
        _displayState.DisplayObject.GetComponent<SpriteRenderer>().sprite = Sprite;
    }
}