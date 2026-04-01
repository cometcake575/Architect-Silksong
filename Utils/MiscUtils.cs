using System;
using System.Collections;
using System.Collections.Generic;
using GlobalEnums;
using JetBrains.Annotations;
using TeamCherry.Localization;
using UnityEngine;

namespace Architect.Utils;

public static class MiscUtils
{
    public static IEnumerator FreeControl(this HeroController hero, Predicate<HeroController> condition = null)
    {
        var fsm = hero.sprintFSM;
        if (fsm.ActiveStateName.Contains("Sprint")) fsm.SendEvent("SKID END");
        hero.umbrellaFSM.SendEvent("END");

        yield return new WaitUntil(() => !hero.controlReqlinquished &&
                                         HeroController.instance.transitionState ==
                                         HeroTransitionState.WAITING_TO_TRANSITION &&
                                         (condition == null || condition.Invoke(hero)));
    }

    [CanBeNull]
    public static SavedItem GetSavedItem(string name)
    {
        if (CollectableItemManager.Instance.masterList.dictionary.TryGetValue(name, out var i1)) return i1;
        if (CollectableRelicManager.Instance.masterList.dictionary.TryGetValue(name, out var i2)) return i2;
        if (MateriumItemManager.Instance.masterList.dictionary.TryGetValue(name, out var i3)) return i3;
        return ToolItemManager.Instance.toolItems.dictionary.GetValueOrDefault(name);
    }

    public static int FirstPosMin<T>(this IEnumerable<T> enumerable, Func<T, float> rule, Func<T, float> backupRule)
    {
        var minV1 = float.MaxValue;
        var minV2 = float.MaxValue;
        var minI1 = -1;
        var i = -1;
        
        foreach (var n in enumerable)
        {
            i++;
            
            var v = rule(n);
            var v2 = backupRule(n);
            if (v <= 0) continue;
            if (v < minV1 || (Mathf.Approximately(v, minV1) && v2 < minV2))
            {
                minV1 = v;
                minV2 = v2;
                minI1 = i;
            }
        }
        
        return minI1;
    }
}

public class LocalStr(string s)
{
    public readonly string Content = s;
        
    public static implicit operator LocalisedString(LocalStr s)
    {
        return new LocalisedString("ArchitectMod", s.Content);
    }
        
    public static implicit operator string(LocalStr s)
    {
        return s.Content;
    }

    public static implicit operator LocalStr(string s)
    {
        return new LocalStr(s);
    }
}
