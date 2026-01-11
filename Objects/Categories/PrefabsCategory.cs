using System.Collections.Generic;
using System.Linq;
using Architect.Editor;
using Architect.Prefabs;
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
        return "Prefabs";
    }

    public static void Remove(string name)
    {
        Prefabs.RemoveAll(o => o.Name == name);
        if (EditorUI.CurrentCategory == Instance)
        {
            EditorUI.PageIndex = 0;
            EditorUI.RefreshCurrentPage();
        }
    }

    public static void Add(string name)
    {
        if (Prefabs.FirstOrDefault(o => o.Name == name) == null)
        {
            Prefabs.Add(new PrefabObject(name));
        }
    }
}
