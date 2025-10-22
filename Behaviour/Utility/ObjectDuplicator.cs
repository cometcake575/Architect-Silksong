using System;
using System.Linq;
using Architect.Placements;
using Architect.Utils;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class ObjectDuplicator : MonoBehaviour
{
    public string id;
    private ObjectPlacement _placement;

    public void Duplicate()
    {
        if (_placement == null)
        {
            _placement = PlacementManager.GetPlacement(id);
            if (_placement == null) return;
        }

        var obj = _placement.SpawnObject();
        if (!obj) return;
        
        obj.name += " Copy " + Guid.NewGuid();
        obj.RemoveComponent<PersistentBoolItem>();
        obj.SetActive(true);
    }
}