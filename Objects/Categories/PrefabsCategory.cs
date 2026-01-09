using System.Collections.Generic;
using System.Linq;
using Architect.Objects.Placeable;
using Architect.Placements;
using JetBrains.Annotations;

namespace Architect.Objects.Categories;

public class PrefabsCategory : AbstractCategory
{
    public static readonly PrefabsCategory Instance = new();

    public static List<PrefabObject> Prefabs;
    
    public override List<SelectableObject> GetObjects()
    {
        return Prefabs.Cast<SelectableObject>().ToList();
    }

    [CanBeNull]
    public override string GetName()
    { 
        return "Saved";
    }

    public static void RemovePrefab(PrefabObject prefab)
    {
        Prefabs.Remove(prefab);
    }

    public static void AddPrefab(PrefabObject prefab)
    {
        Prefabs.Add(prefab);
    }
}