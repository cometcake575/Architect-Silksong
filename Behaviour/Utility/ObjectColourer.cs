using Architect.Behaviour.Fixers;
using Architect.Placements;
using Architect.Utils;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class ObjectColourer : MonoBehaviour
{
    public string targetId;

    public float r;
    public float g;
    public float b;
    public float a;

    public bool startApplied;
    
    private GameObject _target;

    private Color _color;
    private bool _setup;

    private void Update()
    {
        if (!_setup)
        {
            _setup = true;
            _color = new Color(r, g, b, a);
            
            PlacementManager.Objects.TryGetValue(targetId, out _target);
            if (startApplied) Apply();
        }
    }

    public void Apply()
    {
        if (!_target) return;
        
        var lk = _target.GetOrAddComponent<MiscFixers.ColorLock>();
        lk.enabled = false;

        foreach (var sr in _target.GetComponentsInChildren<SpriteRenderer>()) sr.color = _color;
        foreach (var sr in _target.GetComponentsInChildren<tk2dSprite>()) sr.color = _color;

        lk.enabled = true;
    }
}