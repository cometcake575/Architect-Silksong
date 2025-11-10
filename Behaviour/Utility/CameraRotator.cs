using System;
using System.Collections.Generic;
using System.Linq;
using Architect.Utils;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class CameraRotator : MonoBehaviour
{
    private static readonly List<CameraRotator> Rotators = [];
    
    public static void Init()
    {
        typeof(CameraController).Hook(nameof(CameraController.LateUpdate),
            (Action<CameraController> orig, CameraController self) =>
            {
                orig(self);
                self.transform.SetRotation2D(Rotators.Sum(o => -o.transform.GetRotation2D()));
            });
    }
    
    private void OnEnable()
    {
        Rotators.Add(this);
    }
    
    private void OnDisable()
    {
        Rotators.Remove(this);
    }
}