using System;
using Architect.Placements;
using Architect.Storage;
using Architect.Utils;
using GlobalEnums;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Animations;

namespace Architect.Behaviour.Utility;

public class ObjectAnchor : PreviewableBehaviour
{
    private static readonly Material TrailMaterial = new(Shader.Find("Sprites/Default"));
    
    public string targetId = "";
    public string parentId = "";
    
    public float trackDistance;
    public float startOffset;
    public float speed;
    public float rotationSpeed;
    public float smoothing;
    public float startRotation;
    public float pauseTime;

    public bool moving = true;
    
    public bool stickPlayer = true;
    
    public float rotation;
    
    private float _currentRotationSpeed;
    private float _currentSpeed;
    private bool _startFlipped;
    private bool _flipped;
    private float _offset;
    private float _pauseRemaining;

    private bool _setup;

    private Vector3 _startPos;
    private PositionConstraint _constraint;
    private RigidbodyConstraints2D _rigidbodyConstraints;
    [CanBeNull] private Rigidbody2D _rb2d;

    private MonoBehaviour _disableWhenMoving;
    
    // Used for preview
    private Vector3 _previewPos;
    private bool _previewInMotion;
    private TrailRenderer _previewTrail;

    private void Awake()
    {
        if (isAPreview) return;
        var anchorParent = new GameObject("[Architect] Anchor Parent").transform;
        anchorParent.position = transform.position;
        transform.SetParent(anchorParent, true);
    }

    private void Setup()
    {
        _setup = true;
        
        if (!PlacementManager.Objects.TryGetValue(targetId, out var target))
        {
            if (isAPreview) _setup = false;
            return;
        }

        if (!isAPreview && PlacementManager.Objects.TryGetValue(parentId, out var parent))
        {
            transform.parent.SetParent(parent.transform, true);
        }
        
        _offset = startOffset;
        rotation = startRotation + transform.rotation.eulerAngles.z;
        _currentSpeed = speed;

        if (speed < 0)
        {
            _startFlipped = true;
            _flipped = true;
        }

        // Moving platform fix so the player sticks to the platform
        // Uses a Motion Parent object as the parent and not the anchor itself as the anchor can be disabled
        if (target.layer == 8 && stickPlayer)
        {
            if (!target.transform.parent)
            {
                var motionParent = new GameObject("[Architect] Motion Parent").transform;
                target.transform.position = Vector3.zero;
                target.transform.SetParent(motionParent);
            }
            target.AddComponent<StickPlayer>();
            
            target = target.transform.parent.gameObject;
        }

        if (target.GetComponent<ObjectAnchor>())
        {
            target = target.transform.parent.gameObject;
        }
        
        _startPos = transform.localPosition;
        _startPos.z = target.transform.position.z;
        
        // Rope physics fix to target the pinned joint instead
        var comp = target.GetComponentInChildren<HingeJoint2D>();
        if (comp)
        {
            _startPos += comp.transform.position - target.transform.position;
            target = comp.gameObject;
            
            transform.localPosition = _startPos;
        }
        
        _constraint = target.GetOrAddComponent<PositionConstraint>();
        
        // Gets AmbientSway if present to disable it
        _disableWhenMoving = target.GetComponent<AmbientSway>();
        
        _rb2d = target.GetComponentInChildren<Rigidbody2D>();
        
        _constraint.AddSource(new ConstraintSource
        {
            sourceTransform = transform,
            weight = 1
        });
        
        if (isAPreview)
        {
            _previewTrail = target.GetOrAddComponent<TrailRenderer>();
            _previewTrail.Clear();
            _previewTrail.material = TrailMaterial;
            _previewTrail.startWidth = 0.1f;
            _previewTrail.time = 60;
            _previewTrail.enabled = false;
            _previewInMotion = false;
        }
        else
        {
            _constraint.constraintActive = true;
            if (_disableWhenMoving) _disableWhenMoving.enabled = false;
            if (_rb2d)
            {
                _rigidbodyConstraints = _rb2d.constraints;
                _rb2d.constraints = RigidbodyConstraints2D.FreezeAll;
            }
        }
    }

    private void OnDisable()
    {
        if (!_constraint) return;
        _constraint.constraintActive = false;
        if (_disableWhenMoving) _disableWhenMoving.enabled = true;
        if (_rb2d) _rb2d.constraints = _rigidbodyConstraints;
        
        if (isAPreview) ReleasePreview();
    }

    private void OnEnable()
    {
        if (!_constraint) return;
        _constraint.constraintActive = true;
        if (_disableWhenMoving) _disableWhenMoving.enabled = false;
        if (_rb2d) _rb2d.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    private void Update()
    {
        if (!_setup || (!_constraint && isAPreview && Settings.Preview.IsPressed)) Setup();
        if (!_constraint) return;

        if (isAPreview)
        {
            var key = Settings.Preview.IsPressed;

            switch (key)
            {
                case true when !_previewInMotion:
                    _previewPos = _constraint.transform.localPosition;
                    _constraint.constraintActive = true;
                    _constraint.translationOffset = _constraint.GetComponent<ObjectPlacement.PreviewObject>().offset;
                    
                    _previewInMotion = true;
                    _offset = startOffset;

                    if (_flipped != _startFlipped)
                    {
                        _flipped = !_flipped;
                        speed = -speed;
                        rotationSpeed = -rotationSpeed;
                    }

                    _currentSpeed = speed;
                    _currentRotationSpeed = rotationSpeed;
                    rotation = startRotation;
                    _pauseRemaining = 0;
                    
                    _previewTrail.Clear();
                    break;
                case false:
                {
                    ReleasePreview();
                    return;
                }
            }
        }

        if (!moving && !isAPreview) return;

        if (_rb2d) _rb2d.linearVelocity = Vector2.zero;
        
        if (_pauseRemaining > 0)
        {
            _pauseRemaining -= Time.deltaTime;
            if (_pauseRemaining < 0)
            {
                _offset += _currentSpeed * -_pauseRemaining;
                _pauseRemaining = 0;
            }
            else
            {
                return;
            }
        }

        if (_offset >= trackDistance && !_flipped)
        {
            _flipped = true;

            speed = -speed;
            rotationSpeed = -rotationSpeed;
        }
        else if (_offset <= 0 && _flipped)
        {
            _flipped = false;

            speed = -speed;
            rotationSpeed = -rotationSpeed;
        }
        
        var radians = rotation * Mathf.Deg2Rad;

        var newPos = _startPos + new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0) * _offset;
        transform.localPosition = newPos;

        var prevSpeed = _currentSpeed;

        _offset += _currentSpeed * Time.deltaTime;
        
        _currentSpeed = Mathf.Lerp(_currentSpeed, speed,
            smoothing <= 0 ? 1 : Mathf.Min(Time.deltaTime / smoothing, 1));

        if ((prevSpeed < 0 && _currentSpeed > 0) || (prevSpeed > 0 && _currentSpeed < 0))
        {
            _pauseRemaining = pauseTime;
            if (!isAPreview) gameObject.BroadcastEvent("OnReverse");
        }

        _currentRotationSpeed = Mathf.Lerp(_currentRotationSpeed, rotationSpeed,
            smoothing <= 0 ? 1 : Mathf.Min(Time.deltaTime / smoothing, 1));
        rotation += _currentRotationSpeed * Time.deltaTime;
        
        if (_previewTrail) _previewTrail.enabled = true;
    }

    private void ReleasePreview()
    {
        if (!_previewInMotion) return;
        
        _constraint.constraintActive = false;
        _constraint.gameObject.transform.position = _previewPos;
        _previewInMotion = false;
        _previewTrail.enabled = false;
        _previewTrail.Clear();
    }

    public static void Init()
    {
        typeof(HeroController).Hook(nameof(HeroController.TakeDamage),
            (Action<HeroController, GameObject, CollisionSide, int, HazardType, DamagePropertyFlags> orig, 
                HeroController self, 
                GameObject go, 
                CollisionSide damageSide, 
                int damageAmount, 
                HazardType hazardType, 
                DamagePropertyFlags damagePropertyFlags) =>
            {
                orig(self, go, damageSide, damageAmount, hazardType, damagePropertyFlags);
                if (hazardType != HazardType.ENEMY && 
                    hazardType != HazardType.NON_HAZARD && 
                    hazardType != HazardType.EXPLOSION) LeavePlayer(HeroController.instance.gameObject);
            });
        
        TrailMaterial.SetColor(Shader.PropertyToID("_Color"), new Color(1, 0, 0, 0.2f));
    }

    private static void LeavePlayer(GameObject obj)
    {
        var component1 = obj.GetComponent<HeroController>();
        if (component1) component1.SetHeroParent(null);
        else obj.transform.SetParent(null);
        var component2 = obj.GetComponent<Rigidbody2D>();
        if (!component2) return;
        component2.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    public class StickPlayer : MonoBehaviour
    {
        private void OnCollisionEnter2D(Collision2D collision)
        {
            var collisionObject = collision.gameObject;
            if (collisionObject.layer != 9) return;
            var component1 = collisionObject.GetComponent<HeroController>();
            if (component1) component1.SetHeroParent(transform.parent);
            else collisionObject.transform.SetParent(transform.parent);
            var component2 = collisionObject.GetComponent<Rigidbody2D>();
            if (!component2) return;
            component2.interpolation = RigidbodyInterpolation2D.None;
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            var collisionObject = collision.gameObject;
            if (collisionObject.layer != 9) return;
            LeavePlayer(collisionObject);
        }
    }
}