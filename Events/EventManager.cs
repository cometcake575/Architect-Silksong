using System.Collections.Generic;
using System.Linq;
using Architect.Events.Blocks;
using Architect.Events.Blocks.Objects;
using UnityEngine;

namespace Architect.Events;

public static class EventManager
{
    private static readonly Dictionary<string, List<LegacyReceiver>> Receivers = [];
    private static readonly Dictionary<string, EventReceiverType> ReceiverTypes = [];
    
    public static void ResetReceivers()
    {
        Receivers.Clear();
    }
    
    public static EventReceiverType RegisterReceiverType(EventReceiverType type)
    {
        return ReceiverTypes[type.Id] = type;
    }
    
    public static EventReceiverType GetReceiverType(string id)
    {
        return ReceiverTypes[id];
    }
    
    public static void RegisterReceiver(LegacyReceiver instance)
    {
        if (!Receivers.ContainsKey(instance.eventName)) Receivers[instance.eventName] = [];
        Receivers[instance.eventName].Add(instance);
    }

    public static void BroadcastEvent(GameObject obj, string triggerName)
    {
        if (!obj) return;
        foreach (var legacyBroadcaster in obj.GetComponents<LegacyBroadcaster>())
        {
            legacyBroadcaster.Broadcast(triggerName);
        }

        foreach (var block in obj.GetComponents<ObjectBlock.ObjectBlockReference>())
        {
            block.OnEvent(triggerName);
        }
    }

    public static void Broadcast(string eventName)
    {
        if (!Receivers.TryGetValue(eventName, out var connectedReceivers)) return;
        connectedReceivers.RemoveAll(o => !o);
        foreach (var receiver in connectedReceivers.ToList()) receiver.ReceiveEvent(eventName);
    }
}