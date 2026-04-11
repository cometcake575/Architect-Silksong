using System.Collections.Generic;
using Architect.Multiplayer;
using UnityEngine;

namespace Architect.Events.Blocks.Outputs;

public class MultiplayerInBlock : ScriptBlock
{
    protected override IEnumerable<string> Inputs => ["Broadcast"];

    
    
    protected override string Name => "Multiplayer Event";
    
    public string EventName;

    protected override void Trigger(string id)
    {
        CoopManager.Instance.ShareEvent(GameManager.instance.sceneName, EventName);
    }
}
