using Architect.Placements;
using Architect.Utils;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class Layerer : MonoBehaviour
{
    public string target;
    public int layer;
    public bool recursive;

    private GameObject _target;

    private void Start()
    {
        if (!PlacementManager.Objects.TryGetValue(target, out _target))
        {
            _target = ObjectUtils.FindGameObject(target);
        }
    }

    public void Apply()
    {
        if (!_target) return;
        
        if (recursive)
            foreach (var o in _target.GetComponentsInChildren<Transform>())
                o.gameObject.layer = layer;
        else _target.layer = layer;
    }
}