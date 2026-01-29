using Architect.Placements;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class Plasmifier : MonoBehaviour
{
    public int mode;
    public string id;
    public int heal;

    private bool _plasmified;

    public void Plasmify()
    {
        if (_plasmified) return;

        _plasmified = true;
        
        if (!PlacementManager.Objects.TryGetValue(id, out var target)) return;
        
        var dupe = target.GetComponent<ObjectDuplicator>();
        if (dupe) dupe.plasmifier = this;
        else Plasmify(target);
    }

    public void Plasmify(GameObject target)
    {
        var hm = target.GetComponentInChildren<HealthManager>();
        if (!hm) return;

        target.AddComponent<LifebloodState>().healAmount = heal;
    }

    private void Update()
    {
        if (mode == 0 && !_plasmified) Plasmify();
    }
}