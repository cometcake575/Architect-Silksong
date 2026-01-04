using System.Collections.Generic;
using System.Linq;
using Architect.Editor;
using Architect.Objects.Placeable;
using Architect.Placements;
using UnityEngine;

namespace Architect.Events.Blocks.Objects;

public class ObjectBlock : ScriptBlock
{
    public string TargetId;
    public string TypeId;

    private PlaceableObject ObjectType => PlaceableObject.RegisteredObjects[TypeId];

    protected override IEnumerable<string> Inputs => 
        ObjectType.ReceiverGroup.Select(o => o.Id);
    protected override IEnumerable<string> Outputs => ObjectType.BroadcasterGroup;
    protected override IEnumerable<(string, string)> OutputVars => 
        ObjectType.OutputGroup.Select(o => (o.Id, o.GetTypeId()));
    protected override IEnumerable<(string, string)> InputVars => ObjectType.InputGroup;

    protected override int InputCount => ObjectType.BroadcasterGroup.Count;
    protected override int OutputCount => ObjectType.ReceiverGroup.Count;
    
    protected override Color Color => IsValid ? new Color(0.7f, 0.3f, 0.9f) : new Color(0.6f, 0, 0);
    
    protected override string Name => IsValid ? $"{ObjectType.GetName()} ({TargetId})" :
        $"{ObjectType.GetName()} (Invalid)";
    
    private GameObject _referencedObject;
    private ObjectBlockReference _reference;

    public override bool IsValid
    {
        get
        {
            if (EditManager.IsEditing)
            {
                var p = PlacementManager.GetPlacement(TargetId);
                if (p == null) return false;
                return p.GetPlacementType().GetId() == TypeId;
            }

            return _referencedObject;
        }
    }

    protected override Dictionary<string, string> SerializeExtraData()
    {
        Dictionary<string, string> d = [];
        d.Add("object", TargetId);
        d.Add("object_type", TypeId);
        return d;
    }

    protected override void DeserializeExtraData(Dictionary<string, string> data)
    {
        TargetId = data["object"];
        TypeId = data["object_type"];
    }

    protected override void SetupReference()
    {
        SetupObject();
    }

    protected void SetupObject()
    {
        var placement = PlacementManager.GetPlacement(TargetId);
        if (placement == null) return;
        if (placement.GetPlacementType().GetId() != TypeId) return;

        _referencedObject = PlacementManager.Objects[TargetId];
        if (!_referencedObject) return;
        _reference = _referencedObject.AddComponent<ObjectBlockReference>();
        _reference.Block = this;
    }

    protected override void Trigger(string trigger)
    {
        if (!_referencedObject) return;
        var receiver = EventManager.GetReceiverType(trigger);
        if (_referencedObject.activeInHierarchy || receiver.RunWhenInactive) receiver.Trigger(_referencedObject, this);
        foreach (var spawn in _reference.Spawns.Where(spawn => spawn))
        {
            receiver.Trigger(spawn, this);
        }
    }

    protected override object GetValue(string id)
    {
        var output = EventManager.GetOutputType(id);
        return _referencedObject ? output.GetValue(_referencedObject) : null;
    }

    public class ObjectBlockReference : MonoBehaviour
    {
        public ObjectBlock Block;
        public readonly List<GameObject> Spawns = [];

        public void OnEvent(string eName) => Block.Event(eName);
    }
}