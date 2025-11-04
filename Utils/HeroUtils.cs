using System;
using System.Collections;
using UnityEngine;

namespace Architect.Utils;

public static class HeroUtils
{
    public static IEnumerator FreeControl(this HeroController hero, Predicate<HeroController> condition = null)
    {
        var fsm = hero.sprintFSM;
        if (fsm.ActiveStateName.Contains("Sprint")) fsm.SendEvent("SKID END");
        hero.umbrellaFSM.SendEvent("END");
        
        yield return new WaitUntil(() => !hero.controlReqlinquished && 
                                         (condition == null || condition.Invoke(hero)));
    }
}