using System.Collections.Generic;
using Architect.Multiplayer;
using UnityEngine;

namespace Architect.Events.Blocks.Outputs;

public class MultiplayerInBlock : ScriptBlock
{
    protected override IEnumerable<string> Inputs => ["Broadcast"];

    private static readonly Color DefaultColor = new(0.8f, 0.2f, 0.8f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Multiplayer Event";
    
    public string EventName;

    protected override void Trigger(string id)
    {
        CoopManager.Instance.ShareEvent(GameManager.instance.sceneName, EventName);
    }
}
