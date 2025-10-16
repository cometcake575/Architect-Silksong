using System;
using UnityEngine;

namespace Architect.Events;

public class EventReceiverType(string id, string name, Action<GameObject> trigger, bool runWhenInactive = false)
{
    public readonly string Id = id;
    
    public readonly string Name = name;
    
    public readonly Action<GameObject> Trigger = trigger;

    public readonly bool RunWhenInactive = runWhenInactive;
}