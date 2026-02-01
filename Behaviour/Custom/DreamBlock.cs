using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Architect.Utils;
using GlobalEnums;
using UnityEngine;

namespace Architect.Behaviour.Custom;


public class DreamBlock : MonoBehaviour
{
    private const string DREAM_BLOCK_SOURCE = "DreamBlock";

    private static readonly List<DreamBlock> ActiveBlocks = [];
    private static readonly List<DreamBlock> TouchingBlocks = [];
    private static bool _damaging;

    private static int _wallJumpBuffer;
    private static int _turnaroundBuffer;
    private static bool _extendedJump;
    private static AudioSource _source;

    private static AudioClip _enter;
    private static AudioClip _loop;
    private static AudioClip _exit;

    private BoxCollider2D _collider;

    private ParticleSystem.EmissionModule _emission;

    private bool _setup;
    private ParticleSystem.ShapeModule _shape;
    private BoxCollider2D _trigger;

    private void Awake()
    {
        _collider = GetComponents<BoxCollider2D>().First(obj => !obj.isTrigger);
        _trigger = GetComponents<BoxCollider2D>().First(obj => obj.isTrigger);
    }

    private void Start()
    {
        var sounds = HeroController.instance.transform.Find("Sounds");
        _source = sounds.Find(DREAM_BLOCK_SOURCE)?.GetComponent<AudioSource>();
        if (!_source)
            _source = new GameObject(DREAM_BLOCK_SOURCE)
            {
                transform = { parent = sounds }
            }.AddComponent<AudioSource>();
    }

    private void Update()
    {
        Physics2D.IgnoreCollision(HeroController.instance.col2d, _collider, 
            InputHandler.Instance.inputActions.Dash.IsPressed);
        
        if (!_setup)
        {
            _setup = true;

            _emission.rateOverTimeMultiplier *= transform.localScale.x * transform.localScale.y;
            _shape.scale = new Vector3(10 - 3 / transform.localScale.x, 10 - 3 / transform.localScale.y,
                10 / transform.localScale.z);

            _trigger.size = new Vector2(_collider.size.x - 0.2f / transform.localScale.x,
                _collider.size.y - 0.2f / transform.localScale.y);
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.layer != 9) return;
        TouchingBlocks.Add(this);
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.layer != 9) return;
        TouchingBlocks.Remove(this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer != 9) return;

        if (!ActiveBlocks.Contains(this)) return;
        ActiveBlocks.Remove(this);

        if (ActiveBlocks.Count == 0) MoveOut();

        if (!HeroController.instance.dashingDown) _wallJumpBuffer = 2;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.layer != 9) return;
        
        if (ActiveBlocks.Contains(this)) return;

        if (!_trigger.bounds.Contains(new Vector3(other.bounds.center.x, other.bounds.center.y, 0.01f))) return;

        var hc = HeroController.instance;
        hc.renderer.enabled = false;
        if (!hc.dashingDown) hc.rb2d.constraints |= RigidbodyConstraints2D.FreezePositionY;
        ActiveBlocks.Add(this);
        if (ActiveBlocks.Count == 1) StartCoroutine(Play());
    }

    public static void Init()
    {
        ResourceUtils.LoadClipResource("Dream.dream_block_enter", clip => _enter = clip);
        ResourceUtils.LoadClipResource("Dream.dream_block_loop", clip => _loop = clip);
        ResourceUtils.LoadClipResource("Dream.dream_block_exit", clip => _exit = clip);
        
        typeof(HeroController).Hook("SceneInit", (Action<HeroController> orig, HeroController self) =>
        {
            orig(self);
            ActiveBlocks.Clear();
            TouchingBlocks.Clear();
        });

        HookUtils.OnHeroUpdate += self =>
        {
            if (_wallJumpBuffer > 0) _wallJumpBuffer--;
            if (_turnaroundBuffer > 0) _turnaroundBuffer--;

            if (ActiveBlocks.Count > 0)
            {
                self.cState.dashing = true;
                self.dash_timer = self.dashingDown ? self.DOWN_DASH_TIME : self.DASH_TIME;
            }
            
            if (_extendedJump)
            {
                self.JUMP_STEPS = 16;
                self.WJ_KICKOFF_SPEED = 40;
            }
            else
            {
                self.JUMP_STEPS = 8;
                self.WJ_KICKOFF_SPEED = 25;
            }

            if (InputHandler.Instance.inputActions.Jump.WasPressed
                && !self.dashingDown) _turnaroundBuffer = 3;
        };

        typeof(HeroController).Hook("CanWallJump", (Func<HeroController, bool, bool> orig, 
            HeroController self, bool mustBeNearWall) =>
            {
                _extendedJump = false;
                if (orig(self, mustBeNearWall)) return true;
                if (_wallJumpBuffer <= 0) return false;

                if (self.cState.facingRight) self.touchingWallL = true;
                else self.touchingWallR = true;
                if (self.playerData.hasWalljump && !self.cState.touchingNonSlider)
                {
                    _extendedJump = true;
                    return true;
                }

                return false;
            });

        typeof(HeroController).Hook("DoWallJump", (Action<HeroController> orig, HeroController self) =>
        {
            _wallJumpBuffer = 0;
            orig(self);
        });

        typeof(HeroController).Hook("TakeDamage",
            (Action<HeroController, GameObject, CollisionSide, int, HazardType, DamagePropertyFlags> orig,
                HeroController self, GameObject go, CollisionSide side, int amount, HazardType type, 
                DamagePropertyFlags damagePropertyFlags) =>
            {
                if (ActiveBlocks.Count > 0)
                {
                    _damaging = true;
                    ActiveBlocks.Clear();
                    MoveOut();
                }

                orig(self, go, side, amount, type, damagePropertyFlags);
            });

        var blockDir = false;
        typeof(HeroController).Hook("HeroDash", (Action<HeroController, bool> orig, 
            HeroController self, bool startAlreadyDashing) =>
        {
            if (TouchingBlocks.Count > 0 && self.cState.wallSliding)
            {
                var actions = InputHandler.Instance.inputActions;

                self.StartCoroutine(DashLater(
                    self.touchingWallR && !actions.Left.IsPressed,
                    self.touchingWallL && !actions.Right.IsPressed));
                return;
            }

            orig(self, startAlreadyDashing);
            blockDir = false;
            return;
            
            IEnumerator DashLater(bool willRight, bool willLeft)
            {
                yield return null;
                
                if (willRight) self.FaceRight();
                else if (willLeft) self.FaceLeft();
                
                self.cState.touchingWall = false;
                self.cState.wallSliding = false;
                
                blockDir = true;
                
                orig(self, startAlreadyDashing);

                yield return null;
                blockDir = false;
            }
        });
        
        typeof(HeroController).Hook("CanWallScramble", (Func<HeroController, bool> orig,
            HeroController self) => orig(self) && TouchingBlocks.Count == 0);
        
        typeof(HeroController).Hook("FaceLeft", (Action<HeroController> orig, HeroController self) =>
        {
            if (blockDir) return;
            orig(self);
        });
        
        typeof(HeroController).Hook("FaceRight", (Action<HeroController> orig, HeroController self) =>
        {
            if (blockDir) return;
            orig(self);
        });

        typeof(HeroController).Hook("OnCollisionEnter2D", (Action<HeroController, Collision2D> orig,
            HeroController self, Collision2D collision) =>
        {
            if (ActiveBlocks.Count > 0 && !collision.gameObject.GetComponent<DreamBlock>())
            {
                if (self.playerData.hasWalljump &&
                    (_turnaroundBuffer > 0 || InputHandler.Instance.inputActions.Jump.WasPressed))
                {
                    self.touchingWallR = !self.cState.facingRight;
                    self.touchingWallL = self.cState.facingRight;

                    self.cState.touchingWall = true;
                    self.cState.wallSliding = true;
                    self.cState.wallJumping = false;
                    self.DoWallJump();

                    if (self.cState.facingRight) self.FaceLeft();
                    else self.FaceRight();
                }
                else
                {
                    self.TakeDamage(collision.gameObject, CollisionSide.other, 1, HazardType.SPIKES);
                }
            }

            orig(self, collision);
        });
    }

    private static IEnumerator Play()
    {
        _source.volume = GameManager.instance.GetImplicitCinematicVolume() / 5;

        _source.loop = false;
        _source.clip = _enter;
        _source.Play();
        var paused = false;

        while (ActiveBlocks.Count > 0)
        {
            if (GameManager.instance.isPaused)
            {
                if (!paused) _source.Pause();
                paused = true;
                yield return null;
            }
            else if (paused)
            {
                paused = false;
                _source.Play();
            }

            if (!_source.isPlaying && !paused)
            {
                _source.loop = true;
                _source.clip = _loop;
                _source.Play();
            }

            yield return null;
        }
    }

    private static void MoveOut()
    {
        var hc = HeroController.instance;

        if (!_damaging) hc.renderer.enabled = true;
        hc.rb2d.constraints &= ~RigidbodyConstraints2D.FreezePositionY;
        hc.rb2d.gravityScale = 1;
        _damaging = false;
        hc.ResetAirMoves();

        _source.Stop();
        _source.PlayOneShot(_exit);

        hc.dash_timer = 0;
    }

    public void SetupParticles()
    {
        var ps = gameObject.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.startLifetimeMultiplier /= 2;
        main.startSpeedMultiplier = 0;

        var mmg = new ParticleSystem.MinMaxGradient
        {
            mode = ParticleSystemGradientMode.RandomColor,
            gradient = new Gradient
            {
                colorKeys =
                [
                    new GradientColorKey(new Color(1, 0, 0), 0),
                    new GradientColorKey(new Color(0, 1, 0), 0.33f),
                    new GradientColorKey(new Color(0, 0, 1), 0.66f),
                    new GradientColorKey(new Color(1, 1, 0), 1)
                ]
            }
        };
        main.startColor = mmg;
        main.scalingMode = ParticleSystemScalingMode.Shape;

        var col = ps.colorOverLifetime;
        col.enabled = true;
        col.color = new ParticleSystem.MinMaxGradient(new Gradient
        {
            alphaKeys =
            [
                new GradientAlphaKey(0, 0),
                new GradientAlphaKey(1, 0.5f),
                new GradientAlphaKey(0, 1)
            ]
        });

        var rend = ps.GetComponent<ParticleSystemRenderer>();
        rend.material = new Material(Shader.Find("Sprites/Default"))
        {
            mainTexture = ResourceUtils.LoadSpriteResource("Dream.star", FilterMode.Point).texture
        };
        rend.sortingOrder = 1;

        _shape = ps.shape;
        _shape.shapeType = ParticleSystemShapeType.Box;

        var sol = ps.sizeOverLifetime;
        sol.size = new ParticleSystem.MinMaxCurve(0.4f, 0.6f);
        sol.enabled = true;
        sol.sizeMultiplier = 0.1f;

        _emission = ps.emission;
    }
}