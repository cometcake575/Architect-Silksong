using Architect.Placements;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class ObjectMover : MonoBehaviour
{
    public string targetId = "";

    public float xOffset;
    public float yOffset;
    public float rotation;

    public bool moveX = true;
    public bool moveY = true;

    public int moveMode;
    public string moveTarget = "";
    public bool clearVelocity;

    private bool _setup;
    private Transform _source;
    private GameObject _target;
    private Rigidbody2D _rb2d;

    private void Update()
    {
        if (!_setup)
        {
            _setup = true;
            if (PlacementManager.Objects.TryGetValue(targetId, out _target) && clearVelocity)
            {
                _rb2d = _target.GetComponent<Rigidbody2D>();
                if (!_rb2d) clearVelocity = false;

                if (PlacementManager.Objects.TryGetValue(moveTarget, out var sourceObj)) 
                    _source = sourceObj.transform;
                else _source = moveMode switch
                {
                    0 => transform,
                    1 => _target.transform,
                    _ => HeroController.instance.transform
                };
            }
        }
    }

    public void Move(float eX, float eY)
    {
        if (!_target) return;
        if (!_source) return;

        if (clearVelocity) _rb2d.linearVelocity = Vector2.zero;

        var sourceRot = _source.eulerAngles;
        sourceRot.z += rotation;

        if (moveX) _target.transform.SetPositionX(_source.position.x + xOffset + eX);
        if (moveY) _target.transform.SetPositionY(_source.position.y + yOffset + eY);
        _target.transform.eulerAngles = sourceRot;
    }
}