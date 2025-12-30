using System.Collections.Generic;
using Architect.Events.Blocks.Events;
using UnityEngine;

namespace Architect.Events.Blocks.Outputs;

public class BroadcastBlock : ScriptBlock
{
    protected override IEnumerable<string> Inputs => ["Broadcast"];

    private static readonly Color DefaultColor = new(0.8f, 0.2f, 0.8f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Broadcast";
    
    public string EventName;

    protected override void Trigger(string id)
    {
        foreach (var e in ReceiveBlock.RcEvent.Events)
        {
            if (e.Block.EventName == EventName) e.Block.Event("OnReceive");
        }
    }
}