using System.Collections.Generic;
using System.Linq;

namespace Architect.Objects.Categories;

public class AllCategory : AbstractCategory
{
    public static readonly AllCategory Instance = new();

    private static List<SelectableObject> _objects;

    public override List<SelectableObject> GetObjects()
    {
        return _objects ??= Categories.AllCategories
            .OfType<Category>()
            .Where(category => category.Priority >= 0)
            .OrderBy(category => category.Priority)
            .SelectMany(category => category.GetObjects()).ToList();
    }

    public override string GetName()
    {
        return "All";
    }
}