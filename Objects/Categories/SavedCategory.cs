using System.Collections.Generic;
using System.Linq;
using Architect.Objects.Placeable;
using Architect.Placements;
using JetBrains.Annotations;

namespace Architect.Objects.Categories;

public class SavedCategory : AbstractCategory
{
    public static readonly SavedCategory Instance = new();

    public static List<SavedObject> Objects;
    
    public override List<SelectableObject> GetObjects()
    {
        return Objects.Cast<SelectableObject>().ToList();
    }

    [CanBeNull]
    public override string GetName()
    { 
        return "Saved";
    }

    public static void RemovePrefab(SavedObject saved)
    {
        Objects.Remove(saved);
    }

    public static void AddPrefab(SavedObject saved)
    {
        Objects.Add(saved);
    }
}