using Architect.Placements;
using Architect.Utils;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class CollisionChanger : MonoBehaviour
{
    public string id1 = string.Empty;
    public string id2 = string.Empty;

    public bool startDisabled;

    private GameObject _t1;
    private GameObject _t2;

    private void Start()
    {
        if (!PlacementManager.TryGetValue(id1, out _t1))
        {
            _t1 = ObjectUtils.FindGameObject(id1);
            if (!_t1) return;
        }
        if (!PlacementManager.TryGetValue(id2, out _t2))
        {
            _t2 = ObjectUtils.FindGameObject(id2);
            if (!_t2) return;
        }
        
        if (startDisabled) DisableCollision();
    }

    public void EnableCollision()
    {
        if (_t1 && _t2) ToggleAll(false);
    }

    public void DisableCollision()
    {
        if (_t1 && _t2) ToggleAll(true);
    }
    
    private void ToggleAll(bool ignore)
    {
        foreach (var c1 in _t1.GetComponentsInChildren<Collider2D>(true))
        {
            foreach (var c2 in _t2.GetComponentsInChildren<Collider2D>(true))
            {
                Physics2D.IgnoreCollision(c1, c2, ignore);
            }
        }
    }
}