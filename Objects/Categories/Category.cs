using System.Collections.Generic;
using Architect.Objects.Placeable;
using JetBrains.Annotations;

namespace Architect.Objects.Categories;

public class Category(string name, int priority) : AbstractCategory
{
    public readonly float Priority = priority;
    
    private readonly List<SelectableObject> _objects = [];

    public PlaceableObject Add(PlaceableObject obj) {
        _objects.Add(obj);
        return obj;
    }

    public PlaceableObject AddStart(PlaceableObject obj) {
        _objects.Insert(0, obj);
        return obj;
    }

    public override List<SelectableObject> GetObjects()
    {
        return _objects;
    }

    [CanBeNull]
    public override string GetName()
    {
        return name;
    }
}