using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Architect.Utils;
using GlobalSettings;
using MonoMod.RuntimeDetour;
using UnityEngine;

namespace Architect.Behaviour.Custom;

public class FakePerformanceRegion : HeroPerformanceRegion
{
    private static readonly List<FakePerformanceRegion> Regions = [];
    public float rangeMult = 1;
    
    public static void Init()
    {
        typeof(HeroPerformanceRegion).Hook("IsInRange",
            (Func<Vector2, Vector2, Vector2, bool> orig, Vector2 pos, Vector2 centre, Vector2 size) =>
            {
                return Regions.Aggregate(orig(pos, centre, size) && _instance.isPerforming,
                    (current, r) =>
                    {
                        var s = size;
                        if (Gameplay.MusicianCharmTool.IsEquipped) s /= Gameplay.MusicianCharmNeedolinRangeMult;
                        s *= r.rangeMult;
                        return current || orig(pos, r.transform.position, s);
                    });
            });

        _ = new Hook(typeof(HeroPerformanceRegion).GetProperty("IsPerforming",
                BindingFlags.Public | BindingFlags.Static)!.GetGetMethod(),
            (Func<bool> orig) => orig() || Regions.Count > 0);
    }
    
    public new void Awake()
    {
        
    }

    private void OnEnable()
    {
        Regions.Add(this);
    }

    private void OnDisable()
    {
        Regions.Remove(this);
    }
}