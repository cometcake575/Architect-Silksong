using System;
using System.Collections.Generic;
using System.Linq;
using Architect.Utils;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class CameraRotator : PreviewableBehaviour
{
    private static readonly List<CameraRotator> Rotators = [];
    
    public static void Init()
    {
        typeof(CameraController).Hook("LateUpdate",
            (Action<CameraController> orig, CameraController self) =>
            {
                orig(self);
                self.transform.SetRotation2D(Rotators.Sum(o => -o.transform.GetRotation2D()));
            });
    }
    
    private void OnEnable()
    {
        if (isAPreview) return;
        Rotators.Add(this);
    }
    
    private void OnDisable()
    {
        if (isAPreview) return;
        Rotators.Remove(this);
    }
}