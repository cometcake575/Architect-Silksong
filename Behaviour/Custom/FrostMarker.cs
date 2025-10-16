using System;
using System.Collections.Generic;
using System.Linq;
using Architect.Editor;
using MonoMod.RuntimeDetour;
using UnityEngine;

namespace Architect.Behaviour.Custom;

public class FrostMarker : MonoBehaviour
{
    internal static readonly List<FrostMarker> ActiveFrost = [];

    public float frostSpeed;
    
    public static void Init()
    {
        _ = new Hook(typeof(CustomSceneManager).GetProperty(nameof(CustomSceneManager.FrostSpeed))!.GetGetMethod(),
            (Func<CustomSceneManager, float> orig, CustomSceneManager self) => 
                orig(self) + ActiveFrost.Sum(frost => frost.frostSpeed) - (EditManager.IsEditing ? 9999 : 0));
    }

    private void OnEnable()
    {
        ActiveFrost.Add(this);
    }

    private void OnDisable()
    {
        ActiveFrost.Remove(this);
    }
}