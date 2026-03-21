using System.Linq;
using Architect.Placements;
using Architect.Utils;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class ComponentHook : MonoBehaviour
{
    public bool recursive;
    public string id;
    public string componentName;
    public int mode;

    private bool _done;

    public void Setup()
    {
        if (!PlacementManager.Objects.TryGetValue(id, out var target))
        {
            target = ObjectUtils.FindGameObject(id);
            if (!target) return;
        }
        
        var components = (recursive ?
            target.GetComponentsInChildren<UnityEngine.Behaviour>() : 
            target.GetComponents<UnityEngine.Behaviour>()).Where(c => c.GetType().Name == componentName);
        
        foreach (var c in components)
        {
            if (mode == 0) Destroy(c);
            else c.enabled = mode == 2;
        }
    }

    private void Update()
    {
        if (_done) return;
        _done = true;
        Setup();
    }
}