using System.Collections.Generic;
using UnityEngine;

namespace Architect.Events.Blocks.Operators;

public class SceneNameBlock : TimeBlockType
{
    protected override string Name => "Scene Name";

    protected override IEnumerable<(string, string)> OutputVars =>
    [
        ("Name", "Text")
    ];

    public override object GetValue(string id)
    {
        return GameManager.instance.sceneName;
    }
}

public class GameplayBlock : TimeBlockType
{
    protected override string Name => "Gameplay Control";

    protected override IEnumerable<string> Inputs => ["SetGravity"];

    protected override IEnumerable<(string, string)> InputVars =>
    [
        ("GravX", "Number"),
        ("GravY", "Number")
    ];
    
    protected override IEnumerable<(string, string)> OutputVars =>
    [
        ("Scene", "Text"),
        ("Paused", "Boolean")
    ];

    protected override void Trigger(string trigger)
    {
        Physics2D.gravity = new Vector2(GetVariable<float>("GravX"), GetVariable<float>("GravY", -60));
    }

    public override object GetValue(string id)
    {
        if (id == "Paused") return GameManager.instance.isPaused;
        return GameManager.instance.sceneName;
    }
}
