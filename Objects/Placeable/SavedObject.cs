using System.Linq;
using Architect.Placements;
using Architect.Storage;
using UnityEngine;

namespace Architect.Objects.Placeable;

public class SavedObject : SelectableObject
{
    private Sprite _extraSprite;

    public SavedObject(ObjectPlacement placement)
    {
        Placement = placement;
        PlaceableObject = placement.GetPlacementType();

        var cfg = placement.Config.FirstOrDefault(c => c.GetTypeId() == "png_url");
        if (cfg != null)
        {
            var url = cfg.SerializeValue();
            CustomAssetManager.DoLoadSprite(url, true, 100, 1, 1, sprites =>
            {
                _extraSprite = sprites[0];
            });
        }
    }
    
    public readonly ObjectPlacement Placement;
    public readonly PlaceableObject PlaceableObject;
    
    public override string GetName() => "Prefab Object";

    public override string GetDescription() => null;

    public override void Click(Vector3 mousePosition, bool first) { }

    public override Sprite GetUISprite()
    {
        return _extraSprite ?? PlaceableObject.Sprite;
    }
}