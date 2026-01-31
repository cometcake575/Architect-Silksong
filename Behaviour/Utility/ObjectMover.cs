using Architect.Events.Blocks.Objects;
using Architect.Placements;
using Architect.Prefabs;
using Architect.Utils;
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

    public GameObject overrideTarget;

    private bool _setup;
    private Transform _source;
    private GameObject _target;
    private Rigidbody2D _rb2d;

    private void Update()
    {
        if (!_setup)
        {
            _setup = true;
            if (overrideTarget) _target = overrideTarget;
            if (_target || PlacementManager.Objects.TryGetValue(targetId, out _target))
            {
                if (_target.GetComponent<ObjectAnchor>())
                {
                    _target = _target.transform.parent.gameObject;
                    if (!_target)
                    {
                        _setup = false;
                        return;
                    }
                }
                if (PlacementManager.Objects.TryGetValue(moveTarget, out var sourceObj)) 
                    _source = sourceObj.transform;
                else _source = moveMode switch
                {
                    0 => transform,
                    1 => _target.transform,
                    2 => HeroController.instance.transform,
                    _ => new GameObject("[Architect] Origin")
                    {
                        transform = { position = Vector3.zero }
                    }.transform 
                };
                
                var prefab = _target.GetComponent<Prefab>();
                if (prefab)
                {
                    gameObject.SetActive(false);
                    foreach (var spawn in prefab.spawns)
                    {
                        var go = Instantiate(gameObject);
                        go.RemoveComponentsInChildren<ObjectBlock.ObjectBlockReference>();
                        go.transform.position = (transform.position - _target.transform.position + 
                                                 spawn.transform.position)
                            .Where(z: 0);
                        go.GetComponent<ObjectMover>().overrideTarget = spawn;
                    
                        var obrs = GetComponents<ObjectBlock.ObjectBlockReference>();
                        foreach (var obr in obrs)
                        {
                            obr.Spawns.Add(go);
                            var nObr = go.AddComponent<ObjectBlock.ObjectBlockReference>();
                            nObr.canEvent = false;
                            nObr.Block = obr.Block;
                        }
                        go.SetActive(true);
                    }
                    gameObject.SetActive(true);
                }
                
                if (clearVelocity)
                {
                    _rb2d = _target.GetComponent<Rigidbody2D>();
                    if (!_rb2d) clearVelocity = false;
                }
            }
        }
    }
    
    public void Move(float eX, float eY, float eRot)
    {
        if (!_target) return;
        if (!_source) return;

        if (clearVelocity) _rb2d.linearVelocity = Vector2.zero;

        var sourceRot = _source.eulerAngles;
        sourceRot.z += rotation + eRot;

        if (moveX) _target.transform.SetPositionX(_source.position.x + xOffset + eX);
        if (moveY) _target.transform.SetPositionY(_source.position.y + yOffset + eY);
        _target.transform.eulerAngles = sourceRot;
    }
}