using System;
using Architect.Utils;
using GlobalEnums;
using UnityEngine;

namespace Architect.Behaviour.Custom;

public class Wind : MonoBehaviour
{
    private static readonly int EnemyLayer = LayerMask.NameToLayer("Enemies");
    private static readonly int ProjectileLayer = LayerMask.NameToLayer("Projectiles");
    private static readonly int AttackLayer = LayerMask.NameToLayer("Enemy Attack");
    
    private static readonly Material WindMaterial = new(Shader.Find("Sprites/Default"))
    {
        mainTexture = ResourceUtils.LoadSpriteResource("wind_particle", FilterMode.Point).texture
    };
    
    public static bool actuallyJumping;
    private static bool _windPlayer;

    public float speed = 30;

    public float r = 1;
    public float g = 1;
    public float b = 1;
    public float a = 1;

    public bool affectsPlayer = true;
    public bool affectsEnemies = true;
    public bool affectsProjectiles = true;
    
    private ParticleSystem.EmissionModule? _emission;
    private ParticleSystem.MainModule? _main;
    private bool _setForce;
    
    private Vector3 _force;
    private Vector3 _wallForce;

    private void Update()
    {
        if (!_setForce)
        {
            _setForce = true;
            if (transform.localScale.x < 0) speed = -speed;

            if (_emission.HasValue)
            {
                var emission = _emission.Value;
                emission.rateOverTimeMultiplier *= transform.localScale.y;
            }
        }
        
        if (_main.HasValue)
        {
            var main = _main.Value;
            main.startRotationMultiplier = Mathf.Deg2Rad * -transform.localRotation.eulerAngles.z;
        }
        
        _force = transform.localRotation * new Vector3(speed, 0, 0);
        _wallForce = new Vector3(0, _force.y, 0);
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<HeroController>())
        {
            _windPlayer = false;
        }
    }

    public void OnTriggerStay2D(Collider2D other)
    {
        if (!affectsEnemies && other.gameObject.layer == EnemyLayer) return;
        if (!affectsProjectiles &&
            (other.gameObject.layer == ProjectileLayer || other.gameObject.layer == AttackLayer)) return;

        var rb2d = other.GetComponent<Rigidbody2D>();
        if (!rb2d) return;

        var hc = other.GetComponent<HeroController>();
        if (!affectsPlayer && hc) return;

        var force = _force;
        if (hc)
        {
            if (hc.cState.touchingWall) force = _wallForce;

            if (hc.fsm_brollyControl.ActiveStateName == "Float Idle") force.y /= 3;
            else if (hc.controlReqlinquished)
            {
                if (_windPlayer)
                {
                    rb2d.linearVelocity = Vector2.zero;
                    _windPlayer = false;
                }
                return;
            }
        }
        
        rb2d.AddForce(force);
        
        if (!hc) return;
        
        _windPlayer = true;
        if (_force.y > 0)
        {
            hc.ResetHardLandingTimer();
            if (_force.y > 9 && 
                !hc.cState.jumping && !hc.cState.doubleJumping && !hc.cState.wallJumping) actuallyJumping = false;
        }

        if (hc.cState.onGround && !hc.CheckTouchingGround())
        {
            hc.cState.onGround = false;
            hc.cState.falling = true;
            hc.proxyFSM.SendEvent("HeroCtrl-LeftGround");
            hc.SetState(ActorStates.airborne);
        }
    }

    public static void Init()
    {
        HookUtils.OnHeroUpdate += _ =>
        {
            if (HeroController.instance.cState.jumping
                || HeroController.instance.cState.doubleJumping
                || HeroController.instance.cState.wallJumping) actuallyJumping = true;
            else if (HeroController.instance.GetComponent<Rigidbody2D>().linearVelocity.y <= 0) actuallyJumping = false;
        };

        typeof(HeroController).Hook("BackOnGround",
            (Action<HeroController, bool> orig, HeroController self, bool force) =>
            {
                actuallyJumping = false;
                orig(self, force);
            });

        typeof(HeroController).Hook("JumpReleased",
            (Action<HeroController> orig, HeroController self) =>
            {
                if (!actuallyJumping)
                {
                    self.jumpQueuing = false;
                    self.doubleJumpQueuing = false;
                    return;
                }
                orig(self);
            });
    }

    public void SetupParticles()
    {
        var particles = new GameObject("Particles");
        particles.transform.SetParent(transform, false);
        particles.transform.localPosition -= new Vector3(5f, 0, 0);
        particles.layer = LayerMask.NameToLayer("Particle");

        var ps = particles.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.scalingMode = ParticleSystemScalingMode.Shape;
        main.startLifetimeMultiplier = 100000;
        main.startSpeedMultiplier = Mathf.Sqrt(speed) * 2;
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(r, g, b, a));

        var emission = ps.emission;
        emission.rateOverTimeMultiplier = Mathf.Sqrt(speed);

        _emission = emission;
        _main = main;

        var shape = ps.shape;

        shape.shapeType = ParticleSystemShapeType.Rectangle;
        shape.scale = new Vector3(10, 1, 10);
        shape.rotation = new Vector3(0, 90, 270);

        var triggers = ps.trigger;
        triggers.enabled = true;
        var trigger = particles.AddComponent<BoxCollider2D>();
        trigger.offset = new Vector2(5, 0);
        trigger.size = new Vector2(10, 10);

        triggers.AddCollider(trigger);

        triggers.inside = ParticleSystemOverlapAction.Ignore;
        triggers.outside = ParticleSystemOverlapAction.Kill;

        ps.GetComponent<ParticleSystemRenderer>().material = WindMaterial;
    }
}
