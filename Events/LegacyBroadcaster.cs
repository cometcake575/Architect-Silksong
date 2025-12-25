using Architect.Api;
using UnityEngine;

namespace Architect.Events;

public class LegacyBroadcaster : MonoBehaviour
{
    // The object event that triggers the broadcaster
    public string triggerName;
    
    // The Architect event that is broadcast
    public string eventName;

    public void Broadcast(string trigger)
    {
        if (trigger != triggerName) return;
        EventHooks.OnEvent?.Invoke(gameObject, eventName);
        EventManager.Broadcast(eventName);
    }
}