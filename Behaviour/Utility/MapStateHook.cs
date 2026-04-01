using System;
using System.Collections;
using System.Collections.Generic;
using Architect.Utils;
using GlobalEnums;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class MapStateHook : MonoBehaviour
{
    private static readonly List<MapStateHook> MapStateHooks = [];
    public bool memory;
    
    public static void Init()
    {
        typeof(GameManager).Hook(nameof(GameManager.GetCurrentMapZoneEnum),
            (Func<GameManager, MapZone> orig, GameManager self) =>
            {
                var original = orig(self);
                if (MapStateHooks.Count > 0)
                {
                    var memory = MapStateHooks[0].memory;
                    if (original is MapZone.CLOVER or MapZone.MEMORY)
                    {
                        if (!memory) return MapZone.NONE;
                    } else if (memory) return MapZone.MEMORY;
                }

                return original;
            });
        
        typeof(GameManager).Hook(nameof(GameManager.EnterHero),
            (Action<GameManager> orig, GameManager self) =>
            {
                if (MapStateHooks.Count > 0 && self.RespawningHero)
                {
                    ArchitectPlugin.Instance.StartCoroutine(FadeIn());
                }

                orig(self);
            });
    }

    private static IEnumerator FadeIn()
    {
        yield return new WaitForSeconds(3);
        ScreenFaderUtils.Fade(new Color(0, 0, 0, 1), Color.clear, 1);
    }

    private void OnEnable()
    {
        MapStateHooks.Add(this);
    }

    private void OnDisable()
    {
        MapStateHooks.Remove(this);
    }
}