using System.Collections.Generic;
using PrepatcherPlugin;
using UnityEngine;

namespace Architect.Events.Blocks.Operators;

public class SceneNameBlock : ScriptBlock
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

public class GameplayBlock : ScriptBlock
{
    public static void Init()
    {
        PlayerDataVariableEvents.OnGetBool += (_, name, current) =>
        {
            if (ArchitectData.Instance.HideVanillaMaps && name.StartsWith("Has") && name.EndsWith("Map")
                && name != "HasAnyMap") return false;
            return current;
        };
    }
    
    protected override string Name => "Gameplay Control";

    protected override IEnumerable<string> Inputs => ["SetGravity", "HideVanillaMaps", "ShowVanillaMaps"];

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
        if (trigger == "SetGravity")
        {
            Physics2D.gravity = new Vector2(GetVariable<float>("GravX"), GetVariable<float>("GravY", -60));
        }
        else ArchitectData.Instance.HideVanillaMaps = trigger == "HideVanillaMaps";
    }

    public override object GetValue(string id)
    {
        if (id == "Paused") return GameManager.instance.isPaused;
        return GameManager.instance.sceneName;
    }
}
