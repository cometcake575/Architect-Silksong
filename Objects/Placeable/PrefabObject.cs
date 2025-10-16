using Architect.Placements;
using UnityEngine;

namespace Architect.Objects.Placeable;

public class PrefabObject(ObjectPlacement placement) : SelectableObject
{
    public readonly ObjectPlacement Placement = placement;
    public readonly PlaceableObject PlaceableObject = placement.GetPlacementType();
    
    public override string GetName() => "Prefab Object";

    public override string GetDescription() => null;

    public override void Click(Vector3 mousePosition, bool first) { }

    public override Sprite GetUISprite()
    {
        return PlaceableObject.Sprite;
    }
}