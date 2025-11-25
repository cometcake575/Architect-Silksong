using System;
using Architect.Events;
using UnityEngine;

namespace Architect.Api;

public static class EventHooks
{
    public static Action<GameObject, string> OnEvent;

    public static void AddEvent(GameObject obj, string triggerName, string eventName)
    {
        var bci = obj.AddComponent<EventBroadcasterInstance>();

        bci.triggerName = triggerName;
        bci.eventName = eventName;
    }
}