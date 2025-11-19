using System;
using System.Collections;
using Architect.Content.Custom;
using Architect.Utils;
using MonoMod.RuntimeDetour;
using UnityEngine;

namespace Architect.Behaviour.Abilities;

public class CrestBinding : Binding
{
    private static bool _isOverridingCrest;
    
    private static string _actualCrest;
    private static string _forcedCrest;
    
    public static void InitCrestBindings()
    {
        typeof(ToolItemManager).Hook(nameof(ToolItemManager.SetEquippedCrest),
            (Action<string> orig, string crest) =>
            {
                if (_isOverridingCrest && crest != _forcedCrest)
                {
                    _actualCrest = crest;
                    PlayerData.instance.PreviousCrestID = crest;
                    return;
                }
                orig(crest);
            });
        
        typeof(ToolItemManager).Hook(nameof(ToolItemManager.SendEquippedChangedEvent),
            (Action<bool> orig, bool force) =>
            {
                if (_isOverridingCrest && PlayerData.instance.CurrentCrestID != _forcedCrest) return;
                orig(force);
            });
        
        typeof(GameManager).Hook("SaveGame",
            (Action<GameManager, int, Action<bool>, bool, AutoSaveName> orig, 
                GameManager self, 
                int saveSlot,
                Action<bool> ogCallback,
                bool withAutoSave,
                AutoSaveName autoSaveName) =>
            {
                ogCallback += _ => RefreshCrest();
                orig(self, saveSlot, ogCallback, withAutoSave, autoSaveName);
            }, typeof(int), typeof(Action<bool>), typeof(bool), typeof(AutoSaveName));
        
        _ = new Hook(typeof(InventoryToolCrest).GetProperty(nameof(InventoryToolCrest.IsHidden))!.GetGetMethod(),
            (Func<InventoryToolCrest, bool> orig, InventoryToolCrest self) => _isOverridingCrest || orig(self));
    }
    
    public override void OnToggle()
    {
        base.OnToggle();
        ArchitectPlugin.Instance.StartCoroutine(RefreshCrestWhenReady());
    }

    private static IEnumerator RefreshCrestWhenReady()
    {
        var hc = HeroController.instance;
        yield return new WaitUntil(() => !hc.controlReqlinquished &&
                                         !hc.cState.dashing &&
                                         !hc.cState.downAttacking &&
                                         !hc.cState.downSpikeAntic &&
                                         !hc.cState.airDashing);
        RefreshCrest();
        yield return null;
        hc.RegainControl();
        hc.StartAnimationControl();
    }

    private static void RefreshCrest()
    {
        var pd = PlayerData.instance;
        
        var wasOverridingCrest = _isOverridingCrest;
        
        var binding = AbilityObjects.GetActiveCrestBinding();
        
        if (binding.HasValue)
        {
            _isOverridingCrest = true;

            if (!wasOverridingCrest)
            {
                _actualCrest = pd.IsCurrentCrestTemp ? pd.PreviousCrestID : pd.CurrentCrestID;
            }

            _forcedCrest = binding.Value.Item1;
            var crest = ToolItemManager.GetCrestByName(_forcedCrest);
            
            pd.IsCurrentCrestTemp = true;
            ToolItemManager.AutoEquip(crest, false, false);
            
            pd.IsCurrentCrestTemp = true;
            pd.PreviousCrestID = _actualCrest;
            
            HeroController.instance.UpdateSilkCursed();
        }
        else if (wasOverridingCrest)
        {
            _isOverridingCrest = false;
            ToolItemManager.AutoEquip(ToolItemManager.GetCrestByName(_actualCrest), false, false);
            
            HeroController.instance.UpdateSilkCursed();
        }
    }
}