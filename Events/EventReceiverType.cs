using System;
using Architect.Events.Blocks.Objects;
using UnityEngine;

namespace Architect.Events;

public class EventReceiverType(string id, string name, Action<GameObject, ObjectBlock> trigger, bool runWhenInactive = false)
{
    public EventReceiverType(string id, string name, Action<GameObject> trigger, bool runWhenInactive = false) :
        this(id, name, (o, _) => trigger(o), runWhenInactive) { }
    
    public readonly string Id = id;
    
    public readonly string Name = name;
    
    public readonly Action<GameObject, ObjectBlock> Trigger = trigger;

    public readonly bool RunWhenInactive = runWhenInactive;
}