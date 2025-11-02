using System.Collections.Generic;
using System.Linq;
using Architect.Multiplayer;
using UnityEngine;

namespace Architect.Events;

public static class EventManager
{
    private static readonly Dictionary<string, List<EventReceiverInstance>> Receivers = [];
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
    
    public static void RegisterReceiver(EventReceiverInstance instance)
    {
        if (!Receivers.ContainsKey(instance.eventName)) Receivers[instance.eventName] = [];
        Receivers[instance.eventName].Add(instance);
    }

    public static void BroadcastEvent(GameObject obj, string triggerName, bool multiplayer = false)
    {
        if (!obj) return;
        foreach (var broadcaster in obj.GetComponents<EventBroadcasterInstance>())
        {
            broadcaster.Broadcast(triggerName, multiplayer);
        }
    }

    public static void Broadcast(string eventName, bool multiplayer)
    {
        if (multiplayer && CoopManager.Instance.IsActive())
        {
            CoopManager.Instance.ShareEvent(GameManager.instance.sceneName, eventName);
        }
        
        if (!Receivers.TryGetValue(eventName, out var connectedReceivers)) return;
        connectedReceivers.RemoveAll(o => !o);
        foreach (var receiver in connectedReceivers.ToList()) receiver.ReceiveEvent(eventName);
    }
}