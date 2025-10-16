using System.Collections.Generic;
using System.Linq;
using Architect.Objects.Placeable;
using JetBrains.Annotations;

namespace Architect.Objects.Categories;

public class FavouritesCategory : AbstractCategory
{
    public static readonly FavouritesCategory Instance = new();

    public static List<string> Favourites;
    
    public override List<SelectableObject> GetObjects()
    {
        return Favourites.Select(SelectableObject (id) => PlaceableObject.RegisteredObjects[id]).ToList();
    }

    [CanBeNull]
    public override string GetName()
    {
        return "Favourites";
    }

    public static bool ToggleFavourite(PlaceableObject obj)
    {
        if (Favourites.Contains(obj.GetId()))
        {
            Favourites.Remove(obj.GetId());
            return false;
        } 
        Favourites.Add(obj.GetId());
        return true;
    }

    public static bool IsFavourite(PlaceableObject obj)
    {
        var id = obj.GetId();
        return Favourites.Contains(id);
    }
}