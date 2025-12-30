using System;
using System.Collections;
using System.Collections.Generic;
using GlobalEnums;
using JetBrains.Annotations;
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
        return ToolItemManager.Instance.toolItems.dictionary.GetValueOrDefault(name);
    }
}