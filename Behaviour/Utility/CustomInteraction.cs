using System;
using Architect.Utils;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class CustomInteraction : InteractableBase
{
    public static readonly string[] Labels =
    [
        "Inspect",
        "Speak",
        "Listen",
        "Enter",
        "Ascend",
        "Rest",
        "Shop",
        "Travel",
        "Challenge",
        "Exit",
        "Descend",
        "Sit",
        "Trade",
        "Accept",
        "Watch",
        "Consume",
        "Track",
        "TurnIn",
        "Attack",
        "Give",
        "Take",
        "Claim",
        "Call",
        "Play",
        "Dive"
    ];
    
    public float xOffset;
    public float yOffset;
    public bool hideOnInteract;

    public static void Init() => typeof(InteractManager).Hook("AddInteractible", UseOffset);

    private static void UseOffset(Action<InteractableBase, Transform, Vector3> orig,
        InteractableBase interactible,
        Transform transform,
        Vector3 promptOffset)
    {
        var i = interactible.GetComponent<CustomInteraction>();
        if (i)
        {
            promptOffset.x = i.xOffset;
            promptOffset.y = i.yOffset;
        }

        orig(interactible, transform, promptOffset);
    }

    public override void Interact()
    {
        gameObject.BroadcastEvent("OnInteract");
        if (hideOnInteract) HideInteraction();
    }
}