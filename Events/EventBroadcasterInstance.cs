using UnityEngine;

namespace Architect.Events;

public class EventBroadcasterInstance : MonoBehaviour
{
    // The object event that triggers the broadcaster
    public string triggerName;
    
    // The Architect event that is broadcast
    public string eventName;

    public void Broadcast(string trigger, bool multiplayer)
    {
        if (trigger != triggerName) return;
        EventManager.Broadcast(eventName, multiplayer);
    }
}