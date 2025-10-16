using System;
using GlobalEnums;
using MonoMod.RuntimeDetour;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class CustomTransitionPoint : MonoBehaviour
{
    public int pointType;

    public static void Init()
    {
        _ = new Hook(typeof(TransitionPoint).GetMethod(nameof(TransitionPoint.GetGatePosition)),
            (Func<TransitionPoint, GatePosition> orig, TransitionPoint self) =>
            {
                var ctp = self.GetComponent<CustomTransitionPoint>();
                return ctp ? ctp.GetGatePosition() : orig(self);
            });
    }

    private void Start()
    {
        var tp = GetComponent<TransitionPoint>();
        SceneTeleportMap.AddTransitionGate(tp.targetScene, tp.entryPoint);

        tp.InteractLabel = InteractableBase.PromptLabels.Enter;
    }

    public GatePosition GetGatePosition()
    {
        return pointType switch
        {
            0 => GatePosition.door,
            1 => GatePosition.left,
            2 => GatePosition.right,
            3 => GatePosition.top,
            _ => GatePosition.bottom
        };
    }
}