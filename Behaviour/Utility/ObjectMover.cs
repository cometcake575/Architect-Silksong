using Architect.Placements;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class ObjectMover : MonoBehaviour
{
    public string targetId;

    public float xOffset;
    public float yOffset;
    public float rotation;

    public int moveMode;
    public bool clearVelocity;

    private bool _setup;
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
            }
        }
    }

    public void Move()
    {
        if (!_target) return;

        if (clearVelocity) _rb2d.linearVelocity = Vector2.zero;

        var (sourcePos, sourceRot) = moveMode switch
        {
            0 => (transform.position, transform.eulerAngles),
            1 => (_target.transform.position, _target.transform.eulerAngles),
            _ => (HeroController.instance.transform.position, HeroController.instance.transform.eulerAngles)
        };

        sourcePos.x += xOffset;
        sourcePos.y += yOffset;
        sourcePos.z = _target.transform.position.z;

        sourceRot.z += rotation;

        _target.transform.position = sourcePos;
        _target.transform.eulerAngles = sourceRot;
    }
}