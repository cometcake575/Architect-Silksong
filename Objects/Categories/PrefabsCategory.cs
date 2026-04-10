using System.Collections.Generic;
using System.Linq;
using Architect.Editor;
using Architect.Placements;
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
            EditorUI.DoRefreshCurrentPage();
        }
    }

    public static void Add(string name, LevelData data)
    {
        var old = Prefabs.FirstOrDefault(o => o.Name == name);
        if (old == null) Prefabs.Add(new PrefabObject(name, data));
        else old.RefreshConfig(data);
    }
}
