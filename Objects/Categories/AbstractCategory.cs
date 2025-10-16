using System.Collections.Generic;

namespace Architect.Objects.Categories;

public abstract class AbstractCategory
{
    public abstract List<SelectableObject> GetObjects();
    public abstract string GetName();
}