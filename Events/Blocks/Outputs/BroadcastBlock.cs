using System.Collections.Generic;
using System.Linq;
using Architect.Events.Blocks.Events;
using Architect.Utils;
using UnityEngine;

namespace Architect.Events.Blocks.Outputs;

public class BroadcastBlock : LocalBlock
{
    protected override IEnumerable<string> Inputs => ["Broadcast"];

    private static readonly Color DefaultColor = new(0.8f, 0.2f, 0.8f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Broadcast";

    public GameObject TargetPrefab;

    public string ActualEventName;
    public string EventName;

    protected override void Trigger(string id)
    {
        DoBroadcast(EventName);
        if (TargetPrefab) TargetPrefab.BroadcastEvent(ActualEventName);
    }

    public static void DoBroadcast(string eventName)
    {
        foreach (var e in ReceiveBlock.RcEvent.Events
                     .Where(e => e.Block.EventName == eventName))
        {
            e.Block.Event("OnReceive");
        }
    } 
}