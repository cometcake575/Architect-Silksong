using System.Collections.Generic;
using Architect.Utils;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class VignetteDisabler : MonoBehaviour
{
    private static readonly List<VignetteDisabler> Disablers = [];

    private static bool _vignetteOff;
    
    public static void Init()
    {
        HookUtils.OnHeroUpdate += _ =>
        {
            if (Disablers.Count > 0 != _vignetteOff)
            {
                HeroController.instance.vignette.gameObject.SetActive(_vignetteOff);
                _vignetteOff = !_vignetteOff;
            }
        };
    }

    private void OnEnable()
    {
        Disablers.Add(this);
    }

    private void OnDisable()
    {
        Disablers.Remove(this);
    }
}