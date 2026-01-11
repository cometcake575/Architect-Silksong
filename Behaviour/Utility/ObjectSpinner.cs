using Architect.Placements;
using Architect.Prefabs;
using Architect.Storage;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class ObjectSpinner : PreviewableBehaviour
{
    public string targetId;
    public float speed;

    private float _startRot;

    private GameObject _target;
    private ObjectAnchor _anchor;

    private bool _setup;

    private bool _previewing;

    private void Update()
    {
        if (!_setup)
        {
            if (!PlacementManager.Objects.TryGetValue(targetId, out _target)) return;
            _setup = true;
            _startRot = _target.transform.GetRotation2D();
            _anchor = _target.GetComponent<ObjectAnchor>();
        }
        if (!_target) return;

        if (isAPreview)
        {
            if (_target.GetComponentInChildren<Prefab>()) return;
            if (Settings.Preview.IsPressed) _previewing = true;
            else if (_previewing)
            {
                _previewing = false;
                _target.transform.SetRotation2D(_startRot);
            }

            if (!_previewing) return;
        } else if (_anchor) _anchor.rotation += speed * Time.deltaTime;
        
        _target.transform.Rotate(0, 0, speed * Time.deltaTime);
    }
}