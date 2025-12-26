using System;
using System.Collections.Generic;
using System.Linq;
using Architect.Events.Blocks.Objects;
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

        var obj = _placement.SpawnObject(transform.position);
        if (!obj) return;
        
        obj.name += " Copy " + Guid.NewGuid();
        obj.RemoveComponent<PersistentBoolItem>();
        obj.SetActive(true);

        var o = PlacementManager.Objects[id];
        var obr = o.GetComponent<ObjectBlock.ObjectBlockReference>();
        if (obr)
        {
            obr.Spawns.Add(obj);
            obj.AddComponent<ObjectBlock.ObjectBlockReference>().Block = obr.Block;
        }
    }
}