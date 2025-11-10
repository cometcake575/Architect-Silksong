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

        typeof(HeroPerformanceRegion).Hook("IsPlayingInRange",
            (Func<Vector2, float, bool> orig, Vector2 pos, float radius) =>
            {
                return Regions.Aggregate(orig(pos, radius) && _instance.isPerforming,
                    (current, r) => current ||
                                    r.InternalIsInRange(pos, radius * r.rangeMult));
            });

        typeof(HeroPerformanceRegion).Hook("InternalGetAffectedRangeWithRadius",
            (Func<HeroPerformanceRegion, Transform, float, AffectedState> orig, 
                HeroPerformanceRegion self, Transform otherTransform, float radius) =>
            {
                if (self != _instance) return orig(self, otherTransform, radius);
                var first = orig(self, otherTransform, radius);
                foreach (var other in Regions
                             .Select(r => orig(r, otherTransform, radius * r.rangeMult)))
                {
                    switch (first)
                    {
                        case AffectedState.None when other != AffectedState.None:
                        case AffectedState.ActiveInner when other == AffectedState.ActiveOuter:
                            first = other;
                            break;
                        case AffectedState.ActiveOuter:
                        default:
                            break;
                    }
                }

                return first;
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