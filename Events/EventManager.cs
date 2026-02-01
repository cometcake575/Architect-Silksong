using System.Collections.Generic;
using System.Linq;
using Architect.Events.Blocks;
using Architect.Events.Blocks.Events;
using Architect.Events.Blocks.Objects;
using Architect.Events.Vars;
using UnityEngine;

namespace Architect.Events;

public static class EventManager
{
    private static readonly Dictionary<string, EventReceiverType> ReceiverTypes = [];
    private static readonly Dictionary<string, OutputType> OutputTypes = [];
    
    public static EventReceiverType RegisterReceiverType(EventReceiverType type)
    {
        return ReceiverTypes[type.Id] = type;
    }
    
    public static EventReceiverType GetReceiverType(string id)
    {
        return ReceiverTypes.GetValueOrDefault(id);
    }
    
    public static OutputType RegisterOutputType(OutputType type)
    {
        return OutputTypes[type.Id] = type;
    }
    
    public static OutputType GetOutputType(string id)
    {
        return OutputTypes[id];
    }

    public static void BroadcastMp(string eventName)
    {
        foreach (var mp in MultiplayerOutBlock.MpEvent.Events.Where(o => o.Block.EventName == eventName)) 
            mp.Block.Event("OnReceive");
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
    
    #region Legacy
    private static readonly Dictionary<string, List<LegacyReceiver>> Receivers = [];
    
    public static void ResetReceivers()
    {
        Receivers.Clear();
    }
    
    public static void RegisterReceiver(LegacyReceiver instance)
    {
        if (!Receivers.ContainsKey(instance.eventName)) Receivers[instance.eventName] = [];
        Receivers[instance.eventName].Add(instance);
    }

    public static void Broadcast(string eventName)
    {
        if (!Receivers.TryGetValue(eventName, out var connectedReceivers)) return;
        connectedReceivers.RemoveAll(o => !o);
        foreach (var receiver in connectedReceivers.ToList()) receiver.ReceiveEvent(eventName);
    }
    #endregion
}