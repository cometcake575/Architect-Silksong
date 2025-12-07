using System;
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