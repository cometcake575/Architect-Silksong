using System.Collections.Generic;
using System.Linq;
using Architect.Editor;
using Architect.Events.Blocks.Events;
using Architect.Events.Blocks.Outputs;
using Architect.Objects.Placeable;
using Architect.Placements;
using Architect.Prefabs;
using Architect.Storage;
using UnityEngine;

namespace Architect.Events.Blocks.Objects;

public class ObjectBlock : ScriptBlock
{
    public string TargetId;
    public string TypeId;

    private PlaceableObject ObjectType => PlaceableObject.RegisteredObjects.GetValueOrDefault(TypeId);

    protected override IEnumerable<string> Inputs
    {
        get
        {
            switch (ObjectType)
            {
                case null:
                    return [];
                case PrefabObject prefab:
                {
                    if (!PrefabManager.Prefabs.TryGetValue(prefab.Name, out var o)) 
                        o = PrefabManager.Prefabs[prefab.Name] = StorageManager.LoadScene($"Prefab_{prefab.Name}");
                    foreach (var sb in o.ScriptBlocks) 
                    foreach (var (_, c) in sb.CurrentConfig) c.Setup(sb);
                    return o.ScriptBlocks
                        .Where(block => block is ReceiveBlock { Local: true })
                        .Cast<ReceiveBlock>()
                        .Select(rb => rb.EventName)
                        .Distinct()
                        .Append("prefab_start");
                }
                default:
                    return ObjectType.ReceiverGroup.Select(o => o.Id);
            }
        }
    }

    protected override IEnumerable<string> Outputs
    {
        get
        {
            switch (ObjectType)
            {
                case null:
                    return [];
                case PrefabObject prefab:
                {
                    if (!PrefabManager.Prefabs.TryGetValue(prefab.Name, out var o)) 
                        o = PrefabManager.Prefabs[prefab.Name] = StorageManager.LoadScene($"Prefab_{prefab.Name}");
                    foreach (var sb in o.ScriptBlocks) 
                    foreach (var (_, c) in sb.CurrentConfig) c.Setup(sb);
                    return o.ScriptBlocks
                        .Where(block => block is BroadcastBlock { Local: true })
                        .Cast<BroadcastBlock>()
                        .Select(rb => rb.EventName)
                        .Distinct();
                }
                default:
                    return ObjectType.BroadcasterGroup;
            }
        }
    }
    
    protected override IEnumerable<(string, string)> OutputVars => 
        ObjectType?.OutputGroup.Select(o => (o.Id, o.GetTypeId())) ?? [];
    protected override IEnumerable<(string, string)> InputVars => ObjectType?.InputGroup ?? [];

    internal static readonly Color ValidColor = new(0.7f, 0.3f, 0.9f);
    private static readonly Color InvalidColor = new(0.6f, 0, 0);
    protected override Color Color => IsValid ? ValidColor : InvalidColor;
    
    protected override string Name => IsValid ? $"{ObjectType?.GetName() ?? "Deleted"} ({TargetId})" :
        $"{ObjectType?.GetName() ?? "Deleted"} (Invalid)";
    
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
        if (!PlacementManager.Objects.TryGetValue(TargetId, out _referencedObject)) return;
        _reference = _referencedObject.AddComponent<ObjectBlockReference>();
        _reference.Block = this;
    }

    protected override void Trigger(string trigger)
    {
        if (!_referencedObject) return;
        var receiver = EventManager.GetReceiverType(trigger);
        DoTrigger(_referencedObject);
        foreach (var spawn in _reference.Spawns.Where(spawn => spawn))
            DoTrigger(spawn);
        
        return;

        void DoTrigger(GameObject obj)
        {
            if (obj.activeInHierarchy || receiver is { RunWhenInactive: true })
            {
                if (receiver != null)
                {
                    receiver.Trigger(obj, this);
                }
                else
                {
                    var prefab = obj.GetComponent<Prefab>();
                    if (!prefab) return;
                    prefab.Receive(trigger);
                }
            }
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
        public bool canEvent = true;

        public void OnEvent(string eName)
        {
            if (canEvent) Block.Event(eName);
        }
    }
}