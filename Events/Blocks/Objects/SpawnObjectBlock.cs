using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Architect.Objects.Placeable;
using Architect.Placements;
using Architect.Utils;
using BepInEx;
using UnityEngine;

namespace Architect.Events.Blocks.Objects;

public class SpawnObjectBlock : ScriptBlock
{
    protected override IEnumerable<string> Inputs => ["Spawn"];
    protected override IEnumerable<string> Outputs => ["OnSpawned"];
    protected override IEnumerable<(string, string)> InputVars => [
        Space,
        Space,
        ("Type", "Text"),
        ("X", "Number"), 
        ("Y", "Number"),
        ("Rot", "Number"),
        ("Scale", "Number"),
        ("Flipped", "Boolean")];
    protected override IEnumerable<(string, string)> OutputVars => [
        ("Last", "Object")];

    public override Color Color => ObjectBlock.ValidColor;
    protected override string Name => "Spawn Object";

    public override void Reset()
    {
        OffsetX = 0;
        OffsetY = 0;
    }

    public float OffsetX;
    public float OffsetY;

    protected override void Trigger(string trigger)
    {
        var type = GetVariable<string>("Type");
        if (type.IsNullOrWhiteSpace()) return;
        var placeable = PlaceableObject.RegisteredObjects.Values
            .FirstOrDefault(p => p.GetName() == type);
        if (placeable == null) return;

        ArchitectPlugin.Instance.StartCoroutine(DoSpawn(placeable));
    }

    private GameObject _last;

    public override object GetValue(string id) => _last;

    private IEnumerator DoSpawn(PlaceableObject placeable)
    {
        yield return placeable.EnsureLoaded();
        
        var placement = new ObjectPlacement(placeable, new Vector3(
                GetVariable<float>("X") + OffsetX,
                GetVariable<float>("Y") + OffsetY,
                placeable.ZPosition
            ), BlockId, GetVariable<bool>("Flipped"), GetVariable<float>("Rot"),
            GetVariable<float>("Scale", 1), false, 0, [], [],
            placeable.ConfigGroup.Select(c => c.GetDefaultValue())
                .Where(c => c != null).ToArray());

        _last = placement.SpawnObject();
        _last.RemoveComponent<PersistentBoolItem>();
        _last.SetActive(true);
        
        Event("OnSpawned");
    }
}