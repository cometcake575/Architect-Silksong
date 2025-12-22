using System.Collections.Generic;
using System.Linq;
using Architect.Objects.Placeable;
using Architect.Placements;
using UnityEngine;

namespace Architect.Events.Blocks.Objects;

public class ObjectBlock(string targetId) : ScriptBlock
{
    public readonly string TargetId = targetId;

    protected override string Type => "object";

    public string Id => PlacementManager.GetPlacement(TargetId)?.GetPlacementType()?.GetId();
    
    private PlaceableObject ObjectType => PlaceableObject.RegisteredObjects[Id];
    
    public override IEnumerable<string> Inputs => ObjectType.BroadcasterGroup;
    public override IEnumerable<string> Outputs => ObjectType.ReceiverGroup.Select(o => o.Id);

    protected override int InputCount => ObjectType.BroadcasterGroup.Count;
    protected override int OutputCount => ObjectType.ReceiverGroup.Count;
    
    protected override Color Color => new(0.7f, 0.3f, 0.9f);
    protected override string Name => $"{ObjectType.GetName()} ({TargetId})";

    private GameObject _referencedObject;

    protected override bool SetupReference()
    {
        if (Id == null) return false;
        _referencedObject = PlacementManager.Objects[TargetId];
        _referencedObject.AddComponent<ObjectBlockReference>().Block = this;
        return true;
    }

    protected override void Trigger(string trigger)
    {
        if (!_referencedObject) return;
        var receiver = EventManager.GetReceiverType(trigger);
        receiver.Trigger(_referencedObject);
    }

    public class ObjectBlockReference : MonoBehaviour
    {
        public ObjectBlock Block;

        public void OnEvent(string eName) => Block.Event(eName);
    }
}