using System;
using UnityEngine;

namespace Architect.Events;

public class LegacyReceiver : MonoBehaviour
{
    // The Architect event that triggers the receiver
    public string eventName;
    
    // The object trigger that runs when the event is received
    public EventReceiverType ReceiverType;
    
    // The number of times the event must be triggered in order to run
    public int requiredCalls = 1;
    
    // The number of times the event has been triggered since the last run
    public int calls;

    public void ReceiveEvent(string eve)
    {
        if (ReceiverType == null) return;
        if (!ReceiverType.RunWhenInactive && !gameObject.activeInHierarchy) return;
        
        calls++;
        if (calls < requiredCalls) return;
        
        calls = 0;

        try
        {
            ReceiverType.Trigger.Invoke(gameObject);
        }
        catch (Exception exception)
        {
            ArchitectPlugin.Logger.LogError(exception);
        }
    }
}