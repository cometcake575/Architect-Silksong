using System;
using System.Linq;
using Architect.Utils;
using UnityEngine;
using Object = System.Object;
using Random = System.Random;

namespace Architect.Behaviour.Utility;

public class CustomGuidComponent : GuidComponent
{
    public bool overrideAll;
    private bool _generated;
    
    public static void Init()
    {
        typeof(GuidComponent).Hook(nameof(GetGuid),
            (Func<GuidComponent, Guid> orig, GuidComponent self) =>
            {
                if (self is CustomGuidComponent s) return s.GenerateSeededGuid();
                return orig(self);
            });
    }

    private void Update()
    {
        if (overrideAll)
        {
            overrideAll = false;
            foreach (var m in HeroCorpseMarker._activeMarkers
                         .Where(m => m.guidComponent is not CustomGuidComponent)
                         .ToArray())
            {
                m.gameObject.SetActive(false);
            }
        }
    }

    public Guid GenerateSeededGuid()
    {
        // Since the GUID is seeded by the position there can be an overlap if two are in the same spot
        // If the code is stuck running forever, destroy this object (another exists here already)
        if (_generated)
        {
            Destroy(gameObject);
            return Guid.NewGuid();
        }

        _generated = true;
        var r = new Random(Mathf.FloorToInt(transform.position.x + transform.position.y));
        var guid = new byte[16];
        r.NextBytes(guid);

        return new Guid(guid);
    }
}