using System.Collections.Generic;
using Architect.Behaviour.Utility;
using UnityEngine;

namespace Architect.Events.Blocks.Outputs;

public class TransitionBlock : ScriptBlock
{
    protected override IEnumerable<string> Inputs => ["Transition"];

    private static readonly Color DefaultColor = new(0.2f, 0.4f, 0.8f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Scene Transition";

    public string Scene = "";
    public string Door = "";

    private TransitionPoint _point;

    protected override void SetupReference()
    {
        var customDoor = new GameObject("[Architect] Transition Block");
        customDoor.SetActive(false);
        
        _point = customDoor.AddComponent<TransitionPoint>();
        _point.nonHazardGate = true;

        _point.targetScene = Scene;
        _point.entryPoint = Door;

        var col = customDoor.AddComponent<BoxCollider2D>();
        col.size = new Vector2(3, 3);
        col.isTrigger = true;
        
        customDoor.AddComponent<CustomTransitionPoint>().pointType = 0;
        customDoor.SetActive(true);

        customDoor.transform.position = new Vector3(-9999, -9999);
    }

    protected override void Trigger(string trigger)
    {
        _point.OnTriggerEnter2D(HeroController.instance.GetComponent<Collider2D>());
    }
}
