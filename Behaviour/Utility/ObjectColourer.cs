using Architect.Behaviour.Fixers;
using Architect.Events.Blocks.Objects;
using Architect.Placements;
using Architect.Prefabs;
using Architect.Utils;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class ObjectColourer : MonoBehaviour
{
    public string targetId;

    public bool useAlpha;
    public bool directSet;

    public float r;
    public float g;
    public float b;
    public float a;

    public bool startApplied;
    
    public GameObject target;

    private Color _color;
    private bool _setup;

    private void Update()
    {
        if (!_setup)
        {
            _setup = true;
            _color = new Color(r, g, b, a);

            GameObject t;
            if (target) t = target;
            else if (!PlacementManager.Objects.TryGetValue(targetId, out t)) return;
            
            var prefab = t.GetComponent<Prefab>();
            if (prefab)
            {
                foreach (var spawn in prefab.spawns)
                {
                    var go = Instantiate(gameObject);
                    go.transform.position = transform.position - t.transform.position + spawn.transform.position;
                    go.GetComponent<ObjectColourer>().target = spawn;
                    
                    var obrs = GetComponents<ObjectBlock.ObjectBlockReference>();
                    foreach (var obr in obrs)
                    {
                        obr.Spawns.Add(go);
                        go.AddComponent<ObjectBlock.ObjectBlockReference>().Block = obr.Block;
                    }
                }
                return;
            }

            target = t;
            
            if (startApplied) Apply();
        }
    }

    public void Apply()
    {
        if (!target) return;
        
        var lk = target.GetOrAddComponent<MiscFixers.ColorLock>();
        lk.enabled = false;

        foreach (var sr in target.GetComponentsInChildren<SpriteRenderer>(true))
        {
            var col = _color;
            if (!useAlpha) col.a = sr.color.a;
            if (directSet) sr.color = col;
            else sr.color *= col;
        }

        foreach (var sr in target.GetComponentsInChildren<tk2dSprite>(true))
        {
            var col = _color;
            if (!useAlpha) col.a = sr.color.a;
            if (directSet) sr.color = col;
            else sr.color *= col;
        }

        lk.enabled = true;
    }
}