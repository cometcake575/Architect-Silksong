using System.Collections.Generic;
using Architect.Objects.Placeable;
using JetBrains.Annotations;

namespace Architect.Objects.Categories;

public class BlankCategory : AbstractCategory
{
    public static readonly BlankCategory Instance = new();
    
    public override List<SelectableObject> GetObjects()
    {
        return [];
    }

    [CanBeNull]
    public override string GetName()
    {
        return null;
    }
}