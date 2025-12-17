using System;
using System.Linq;
using Architect.Behaviour.Custom;
using Architect.Behaviour.Utility;
using Architect.Content.Preloads;
using Architect.Objects.Placeable;
using Architect.Utils;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Architect.Behaviour.Fixers;

public static class EnemyFixers
{
    // Skull Tyrant
    private static GameObject _bouldersPrefab;
    
    // Broodmother
    private static GameObject _freshflyCagePrefab;
    
    // Second Sentinel
    private static GameObject _robotParticles;
    
    // First Sinner
    private static GameObject _pinProjectiles;
    private static GameObject _loosePins;
    private static GameObject _blasts;
    
    // Seth
    private static GameObject _shieldTrail;

    private static readonly int EnemiesLayer = LayerMask.NameToLayer("Enemies");
    
    public static void Init()
    {
        HookUtils.OnFsmAwake += fsm =>
        {
            if (fsm.FsmName == "Control" && fsm.gameObject.name == "Last Judge Charge Bomb(Clone)")
            {
                fsm.GetState("Antic").DisableAction(2);
            }
        };
        
        typeof(HealthManager).Hook("ApplyDamageScaling",
            (Func<HealthManager, HitInstance, HitInstance> orig, HealthManager self, HitInstance hit) => 
                self.GetComponentInParent<DisableHealthScaling>() ? hit : orig(self, hit));
        
        typeof(DisplayBossTitle).Hook(nameof(DisplayBossTitle.OnEnter),
            (Action<DisplayBossTitle> orig, DisplayBossTitle self) =>
            {
                if (self.fsmComponent.GetComponent<DisableBossTitle>()) self.bossTitle.value = "";
                orig(self);
            });
        
        PreloadManager.RegisterPreload(new BasicPreload("Bonetown_boss", "Boss Scene/Boulders Battle", 
            o => _bouldersPrefab = o));
        PreloadManager.RegisterPreload(new BasicPreload("Slab_16b", 
            "Broodmother Scene Control/Broodmother Scene/Battle Scene Broodmother/Spawner Flies", 
            o => _freshflyCagePrefab = o));
        PreloadManager.RegisterPreload(new BasicPreload("Cog_Dancers_boss", 
            "Dancer Control/Death Chunks 1",
            o => _robotParticles = o));
        PreloadManager.RegisterPreload(new BasicPreload("Slab_10b", 
            "Boss Scene/Pin Projectiles",
            o => _pinProjectiles = o));
        PreloadManager.RegisterPreload(new BasicPreload("Slab_10b", 
            "Boss Scene/Loose Pins",
            o => _loosePins = o));
        PreloadManager.RegisterPreload(new BasicPreload("Slab_10b", 
            "Boss Scene/Blasts",
            o => _blasts = o));
        PreloadManager.RegisterPreload(new BasicPreload("Shellwood_22", 
            "Boss Scene/Pt Shield Trail",
            o => _shieldTrail = o));

        AknidMother.InitSounds();
    }
    
    public static void ApplyGravity(GameObject obj)
    {
        obj.GetOrAddComponent<Rigidbody2D>().gravityScale = 1;
    }
    
    public static void FixAknid(GameObject obj)
    {
        RemoveConstrainPosition(obj);
        obj.LocateMyFSM("Control").GetState("Get Next Point").actions[0].enabled = true;
    }

    public static void KeepActive(GameObject obj)
    {
        obj.RemoveComponentsInChildren<DeactivateIfPlayerdataTrue>();
        obj.RemoveComponentsInChildren<DeactivateIfPlayerdataFalse>();
        obj.RemoveComponentsInChildren<DeactivateIfPlayerdataFalseDelayed>();
        obj.RemoveComponentsInChildren<TestGameObjectActivator>();
    }

    public static void KeepActiveRemoveConstrainPos(GameObject obj)
    {
        KeepActive(obj);
        RemoveConstrainPosition(obj);
    }
    
    public static void FixJailer(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Control");
        fsm.GetState("Is Cursed Dead?").AddAction(() => fsm.SendEvent("FALSE"), 0);
        fsm.GetState("Is Here?").AddAction(() => fsm.SendEvent("FALSE"), 0);
        fsm.GetState("Is Cursed?").AddAction(() => fsm.SendEvent("FALSE"), 0);
    }

    public class CustomJailer : MonoBehaviour
    {
        public string targetScene = "Slab_03";
        public string targetDoor = "door_slabCaged";

        private void Awake()
        {
            if (targetDoor == "door_slabCaged" && targetScene == "Slab_03") return;
            var fsm = gameObject.LocateMyFSM("Control");

            fsm.GetState("Capture Flash").DisableAction(6);
            
            var cut = fsm.GetState("Audio Cut");
            for (var i = 0; i < 4; i++) cut.DisableAction(i);

            var caught = fsm.GetState("Start Caged Sequence");
            for (var i = 0; i < 8; i++) caught.DisableAction(i);

            ((BeginSceneTransition)caught.Actions[8]).sceneName = targetScene;
            ((BeginSceneTransition)caught.Actions[8]).entryGateName = targetDoor;
        
            SceneTeleportMap.AddTransitionGate(targetScene, targetDoor);
        }
    }

    private class LastJudge : MonoBehaviour
    {
        public bool moveLeft;
        public bool moveRight;
        public bool moveUp;
        public bool moveDown;

        private void OnCollisionStay2D(Collision2D other)
        {
            var firstContact = other.GetContact(0);
            var contactPoint = firstContact.point;
            var offset = transform.InverseTransformPoint(contactPoint);
            
            moveLeft = false;
            moveRight = false;

            switch (offset.y)
            {
                case > 0 when Mathf.Abs(offset.y) > Mathf.Abs(offset.x):
                    moveUp = false;
                    break;
                case < 0 when Mathf.Abs(offset.y) > Mathf.Abs(offset.x):
                    moveDown = false;
                    break;
            }
        }
    }

    public static void FixLastJudge(GameObject obj)
    {
        RemoveConstrainPosition(obj);
        var lj = obj.AddComponent<LastJudge>();

        var rb2d = obj.GetComponent<Rigidbody2D>();

        var fsm = obj.LocateMyFSM("Control");
        fsm.GetState("Dormant").AddAction(() => fsm.SetState("Intro Roar Antic"));
        
        fsm.GetState("First Idle").AddAction(() => fsm.SendEvent("ENCOUNTERED"), 0);

        // Variables to reposition
        var groundY = fsm.FsmVariables.FindFsmFloat("Ground Y");
        var jumpY = fsm.FsmVariables.FindFsmFloat("Jump Y");
        
        var xTarget = fsm.FsmVariables.FindFsmFloat("X Target");
        var xDist = fsm.FsmVariables.FindFsmFloat("X Distance");
        
        var censerSlamSelfY = fsm.FsmVariables.FindFsmFloat("Censer Slam Self Y");
        var censerBotY = fsm.FsmVariables.FindFsmFloat("Censer Bot Y");
        var censerTopY = fsm.FsmVariables.FindFsmFloat("Censer Top Y");
        var stompY = fsm.FsmVariables.FindFsmFloat("Stomp Y");
        
        fsm.GetState("Throw Rise").AddAction(AdjustCenserGroundPos, 11);
        fsm.GetState("Censer Slam").AddAction(AdjustCenserSlam, 0);

        var centre = obj.transform.position.x;
        fsm.FsmVariables.FindFsmFloat("Centre X").Value = centre;
        
        // Disable position checks
        var cc = fsm.GetState("Charge Check");
        cc.DisableAction(1);
        cc.DisableAction(2);
        
        var sc = fsm.GetState("Spin Check");
        sc.DisableAction(2);
        sc.DisableAction(3);
        
        var tc = fsm.GetState("Throw Check");
        tc.DisableAction(1);
        tc.DisableAction(2);
        
        var rr = fsm.GetState("Rage Roar 1");
        rr.DisableAction(3);
        rr.DisableAction(4);
        
        var fr = fsm.GetState("Flame Roar 1");
        fr.DisableAction(3);
        fr.DisableAction(4);
        
        obj.GetComponent<HealthManager>().IsInvincible = false;

        var ede = obj.GetComponent<EnemyDeathEffects>();
        
        ede.PreInstantiate();
        
        var corpseFsm = ede.GetInstantiatedCorpse(AttackTypes.Nail).LocateMyFSM("Control");

        var fall = (CheckYPosition)corpseFsm.GetState("Fall").Actions[2];
        var land = (SetPosition)corpseFsm.GetState("Land").Actions[0];

        var roar = fsm.GetState("Intro Roar");
        ((StartRoarEmitter)roar.actions[3]).stunHero = false;
        roar.AddAction(MakeDynamicWithGravity, 0);
        
        fsm.GetState("Charge Antic 1").AddAction(MakeDynamicWithGravity, 0);
        fsm.GetState("Dash Antic").AddAction(MakeDynamicWithGravity, 0);
        fsm.GetState("Evade Antic").AddAction(MakeDynamicWithGravity, 0);
        sc.AddAction(MakeDynamicWithGravity, 0);
        tc.AddAction(MakeDynamicWithGravity, 0);

        var idle = fsm.GetState("Idle");
        idle.AddAction(AdjustGroundPosForSelf, 0);
        idle.AddAction(MakeDynamicWithGravity, 0);
        
        var rangeCheck = fsm.GetState("Range Check");
        rangeCheck.AddAction(AdjustGroundPosForSelf, 0);
        rangeCheck.AddAction(MakeKinematic, 0);

        var stun = fsm.GetState("Stun Start");
        stun.AddAction(AdjustGroundPosForGround, 0);
        
        fsm.GetState("Stomp JumpAntic").AddAction(MakeDynamic, 0);
        fsm.GetState("OJump Antic").AddAction(MakeDynamic, 0);
        fsm.GetState("Jump Antic").AddAction(MakeDynamic, 0);
        
        var jumpRise = fsm.GetState("Jump Rise");
        var oJumpRise = fsm.GetState("Ojump Rise");
        var stompRise = fsm.GetState("Stomp Rise and Antic");
        var stompDown = fsm.GetState("Stomp Down");
        var jumpFall = fsm.GetState("Jump Fall");
        var jumpLand = fsm.GetState("Land");

        var tweenX = fsm.FsmVariables.FindFsmFloat("Tween X");
        var tweenY = fsm.FsmVariables.FindFsmFloat("Tween Y");
        
        // Use Rigidbody2D movement instead of changing position to add collision when jumping
        jumpRise.DisableAction(10);
        oJumpRise.DisableAction(14);
        stompRise.DisableAction(13);
        stompDown.DisableAction(0);
        jumpFall.DisableAction(0);
        jumpFall.DisableAction(6);
        jumpLand.DisableAction(2);

        ((SetVelocityByScale)stompDown.actions[3]).everyFrame = true;
        stompDown.AddAction(AdjustGroundPosForGround, 4, true);
        
        jumpRise.AddAction(() =>
        {
            rb2d.MovePosition(new Vector2(
                lj.moveRight || lj.moveLeft ? tweenX.Value : obj.transform.GetPositionX(), 
                lj.moveUp ? tweenY.Value : obj.transform.GetPositionY()
            ));

            if (!lj.moveLeft && !lj.moveRight)
            {
                xTarget.Value = obj.transform.GetPositionX();
                xDist.Value = 0;
            }
            if (!lj.moveUp) jumpY.Value = obj.transform.GetPositionY();
        }, 10, true);
        jumpRise.AddAction(() =>
        {
            lj.moveUp = true;
            lj.moveLeft = xTarget.Value < obj.transform.GetPositionX();
            lj.moveRight = !lj.moveLeft;
        }, 8);
        jumpRise.AddAction(AdjustGroundPosForTarget, 8);
        
        oJumpRise.AddAction(() =>
        {
            rb2d.MovePosition(new Vector2(
                lj.moveRight || lj.moveLeft ? tweenX.Value : obj.transform.GetPositionX(), 
                lj.moveUp ? tweenY.Value : obj.transform.GetPositionY()
            ));

            if (!lj.moveLeft && !lj.moveRight)
            {
                xTarget.Value = obj.transform.GetPositionX();
                xDist.Value = 0;
            }
            if (!lj.moveUp) jumpY.Value = obj.transform.GetPositionY();
        }, 14, true);
        oJumpRise.AddAction(() =>
        {
            lj.moveUp = true;
            lj.moveLeft = xTarget.Value < obj.transform.GetPositionX();
            lj.moveRight = !lj.moveLeft;
        }, 12);
        oJumpRise.AddAction(AdjustGroundPosForTarget, 12);
        
        stompRise.AddAction(() =>
        {
            rb2d.MovePosition(new Vector2(
                lj.moveRight || lj.moveLeft ? tweenX.Value : obj.transform.GetPositionX(), 
                lj.moveUp ? tweenY.Value : obj.transform.GetPositionY()
            ));
        }, 13, true);
        stompRise.AddAction(() =>
        {
            lj.moveUp = true;
            lj.moveLeft = xTarget.Value < obj.transform.GetPositionX();
            lj.moveRight = !lj.moveLeft;
        }, 11);
        
        jumpFall.AddAction(() =>
        {
            rb2d.MovePosition(new Vector2(
                lj.moveRight || lj.moveLeft ? tweenX.Value : obj.transform.GetPositionX(), 
                lj.moveDown ? tweenY.Value : obj.transform.GetPositionY()
            ));
        }, 6, true);
        jumpFall.AddAction(() =>
        {
            lj.moveDown = true;
        }, 4);
        
        AdjustGroundPosForSelf();
        return;
        
        // Adjusts ground positions based on current position
        void AdjustGroundPosForSelf()
        {
            var ground = obj.transform.GetPositionY() + 1.1057f;
            groundY.Value = ground;
            jumpY.Value = ground + 5.78f;
            stompY.Value = ground + 4.58f;
                
            fall.compareTo = ground - 1;
            land.y = ground - 1;
        }
        
        // Adjusts ground positions based on ground below current position
        void AdjustGroundPosForGround()
        {
            AdjustGroundPos(obj.transform.position, false);
        }
        
        // Adjusts ground positions based on target X position
        void AdjustGroundPosForTarget()
        {
            AdjustGroundPos(new Vector2(xTarget.value + xDist.value, obj.transform.GetPositionY()), true);
        }
        
        void AdjustGroundPos(Vector2 source, bool cannotJumpDown)
        {
            if (HeroController.instance.TryFindGroundPoint(out var pos, 
                    source, 
                    true))
            {
                var ground = pos.y + 2.7f;
                groundY.Value = ground;
                var jumpHeight = ground;
                if (cannotJumpDown) jumpHeight = Mathf.Max(source.y - 3, jumpHeight);
                jumpY.Value = jumpHeight + 5.78f;
                stompY.Value = ground + 4.58f;
                
                fall.compareTo = ground - 1;
                land.y = ground - 1;
            }
        }

        // Adjusts based on target so it can fall above or below Last Judge
        // Adds censer X distance to censer target X as target X is halfway
        void AdjustCenserGroundPos()
        {
            var selfY = obj.transform.GetPositionY();
            if (HeroController.instance.TryFindGroundPoint(out var pos, 
                    new Vector2(xTarget.Value + xDist.Value, selfY + 4.5f),
                    true))
            {
                var ground = pos.y - 0.94f;
                
                censerTopY.Value = Mathf.Clamp(ground, selfY - 2.5f, selfY) + 10.82f;
                censerBotY.Value = ground;
                if (selfY > ground)
                {
                    var dist = Mathf.Sqrt(Mathf.Abs(selfY - ground)) / 4;
                    if (xDist.Value < 0) dist = -dist;
                    xTarget.Value += dist;
                    xDist.Value += dist;
                }
            }
        }
        
        void AdjustCenserSlam()
        {
            var selfY = obj.transform.GetPositionY();
            censerSlamSelfY.value = censerBotY.Value - selfY;
        }

        void MakeDynamicWithGravity()
        {
            rb2d.bodyType = RigidbodyType2D.Dynamic;
            rb2d.gravityScale = 3;
        }

        void MakeDynamic()
        {
            rb2d.bodyType = RigidbodyType2D.Dynamic;
            rb2d.gravityScale = 0;
            rb2d.linearVelocity = Vector2.zero;
        }

        void MakeKinematic()
        {
            rb2d.bodyType = RigidbodyType2D.Kinematic;
            rb2d.gravityScale = 0;
            rb2d.linearVelocity = Vector2.zero;
        }
    }

    public static void RemoveConstrainPosition(GameObject obj)
    {
        obj.RemoveComponentsInChildren<ConstrainPosition>();
    }

    public static void FixBloatroachPreload(GameObject obj)
    {
        RemoveConstrainPosition(obj);
        var psr = obj.transform.Find("Pt Spit Void").GetChild(0).GetComponent<ParticleSystemRenderer>();
        psr.material = psr.material;
    }

    public static void FixBloatroach(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Control");
        var val = fsm.FsmVariables.FindFsmBool("In Attack Range");
        fsm.GetState("Idle").AddAction(() =>
        {
            var hPos = HeroController.instance.transform.position;
            var oPos = obj.transform.position;
            val.Value = Mathf.Abs(hPos.x - oPos.x) < 15 && Mathf.Abs(hPos.y - oPos.y) < 2;
        }, 4, true);
    }

    public class DisableHealthScaling : MonoBehaviour;
    public class DisableBossTitle : MonoBehaviour;

    public static void FixDuctsucker(GameObject obj)
    {
        RemoveConstrainPosition(obj);

        var fsm = obj.LocateMyFSM("Control");
        fsm.GetState("Return Type").AddAction(() => fsm.SendEvent("UNALERT"), 0);
    }

    public static void FixFlintFlyer(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Behaviour");
        fsm.GetState("Init").AddAction(() => fsm.SendEvent("FINISHED"), 6);
    }

    public static void FixSpearSpawned(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Control") ?? obj.LocateMyFSM("Behaviour");
        fsm.GetState("Spear Spawn Pause").AddAction(() => fsm.SendEvent("FINISHED"), 3);
    }

    public static void FixYumama(GameObject obj)
    {
        obj.LocateMyFSM("Control").FsmVariables.FindFsmBool("Idle Patrol").value = true;
    }

    public static void FixKaraka(GameObject obj)
    {
        FixSpearSpawned(obj);
        var fsm = obj.LocateMyFSM("Control");
        fsm.GetState("Init").AddAction(() => fsm.SendEvent("FINISHED"), 4);
    }
    
    public static void ClearRotation(GameObject obj)
    {
        obj.transform.rotation = Quaternion.Euler(0, 0, 0);
    }
    
    public static void FixDriller(GameObject obj)
    {
        FixSpearSpawned(obj);
        var fsm = obj.LocateMyFSM("Control");
        fsm.GetState("Init").AddAction(() => fsm.SendEvent("SPEAR SPAWNER"), 2);
        fsm.GetState("Memory Arena Clamp").AddAction(() => fsm.SendEvent("FINISHED"), 0);
    }

    public static void FixSkrill(GameObject obj)
    {
        var sourcePos = obj.transform.position;
        var fsm = obj.LocateMyFSM("Behaviour");
        fsm.GetState("Hidden").AddAction(() =>
        {
            if ((HeroController.instance.transform.position - sourcePos).sqrMagnitude < 1024) return;
            fsm.SendEvent("HERO GONE");
        }, 0, true);
    }

    public static void FixForumEnemy(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Control");
        fsm.GetState("Init").AddAction(() => fsm.SendEvent("FINISHED"), 4);
    }

    public static void FixMinister(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Control");
        fsm.GetState("Fly In Ready").AddAction(() =>
        {
            fsm.SetState("Activate");
        }, 0);

        var started = false;
        fsm.GetState("Chase").AddAction(() =>
        {
            if (started) return;
            started = true;
            fsm.SendEvent("UNALERT");
        }, 0);

        var patrol = fsm.FsmVariables.FindFsmBool("In Patrol Range");
        fsm.GetState("Unalert Patrol").AddAction(() =>
        {
            patrol.value = true;
        }, 5);
    }

    public static void FixMaestro(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Control");
        fsm.GetState("Init").AddAction(() =>
        {
            fsm.SendEvent("FINISHED");
        }, 42);
        
        var started = false;
        fsm.GetState("Fly").AddAction(() =>
        {
            if (started) return;
            started = true;
            fsm.SendEvent("UNALERT");
        }, 0);
        
        FsmGameObject[] flies = [
            fsm.FsmVariables.FindFsmGameObject("Fly 1"),
            fsm.FsmVariables.FindFsmGameObject("Fly 2"), 
            fsm.FsmVariables.FindFsmGameObject("Fly 3")
        ];
        
        fsm.GetState("Fly").AddAction(FixCogflies, 0);
        fsm.GetState("Summon").AddAction(FixCogflies, 3);

        return;

        void FixCogflies()
        {
            foreach (var fly in flies)
            {
                var v = fly.value;
                if (!v) return;
                
                var oBts = obj.GetComponent<BlackThreadState>();
                var hasBlackThreadState = oBts && oBts.CheckIsBlackThreaded();
                
                var bts = v.GetComponent<BlackThreadState>();
                if (bts.CheckIsBlackThreaded() == hasBlackThreadState) return;
                
                bts.SetBlackThreadAmount(0);
                bts.isBlackThreadWorld = hasBlackThreadState;
                bts.ResetThreaded();
            }
        }
    }

    public static void FixSurgeon(GameObject obj)
    {
        var anim = obj.GetComponent<tk2dSpriteAnimator>();
        anim.defaultClipId = anim.GetClipIdByName("Idle");

        obj.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
    }

    public class Teleplane : MonoBehaviour
    {
        public float width;

        private void Start()
        {
            var fsm = gameObject.LocateMyFSM("Attack");
            var teleplane = new GameObject(name + " Teleplane")
            {
                transform = { position = transform.position },
                tag = "Teleplane"
            };
            var col = teleplane.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(width, 1);
            ((FindClosest)fsm.GetState("Get Teleplane").actions[0]).name = teleplane.name;
        }
    }

    public static void FixDregwheel(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Control");
        
        fsm.GetState("Dormant").AddAction(() => fsm.SendEvent("SPAWN M"), 0);
        ((SetFloatValue)fsm.GetState("Spawn Mid").actions[0]).floatValue.value = obj.transform.position.x;
        fsm.FsmVariables.FindFsmFloat("Ground Y").value = obj.transform.position.y;
        
        fsm.GetState("Spawn Roll Clamp").DisableAction(2);
        
        fsm.FsmVariables.FindFsmBool("No Respawn").value = true;
        obj.GetComponent<HealthManager>().hasSpecialDeath = false;
        
        var roll = fsm.GetState("Spawn Roll");
        roll.DisableAction(6);
        roll.DisableAction(9);
        roll.DisableAction(10);
        
        var spawn = fsm.GetState("Spawn Antic");
        ((SetPosition)spawn.actions[1]).y = obj.transform.position.y;
        spawn.DisableAction(4);
    }

    public static void FixDregHusk(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Control");
        fsm.FsmVariables.FindFsmBool("Spawner").value = false;
        obj.GetComponent<HealthManager>().invincible = false;
    }

    public static void FixMoorwing(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Control");
        fsm.GetState("Dormant").AddAction(() => fsm.SendEvent("BATTLE START"));
        var zoom = fsm.GetState("Zoom Down");
        zoom.DisableAction(2);
        zoom.DisableAction(3);
        zoom.AddAction(() => fsm.SendEvent("FINISHED"));
        ((StartRoarEmitter)fsm.GetState("Quick Roar").actions[3]).stunHero = false;
    }

    public static void FixGroal(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Control");

        var hm = obj.GetComponent<HealthManager>();
        hm.invincible = false;


        // Prevent break on death
        var fly = fsm.GetState("Fly Idle");
        fly.transitions = fly.transitions
            .Where(trans => trans.EventName != "HORNET DEAD").ToArray();
        var choice = fsm.GetState("Attack Choice");
        choice.transitions = choice.transitions
            .Where(trans => trans.EventName != "HORNET DEAD").ToArray();

        // Spawn instantly
        fsm.GetState("Dormant").AddAction(() => fsm.SendEvent("BATTLE START"), 0);
        fsm.GetState("Fake Battle End").AddAction(() => fsm.SendEvent("FINISHED"), 0);
        fsm.GetState("Entry Antic").AddAction(() => fsm.SendEvent("FINISHED"), 0);
        fsm.GetState("Entry Dive").AddAction(() => fsm.SendEvent("FINISHED"), 4);

        // Roar does not stun player
        ((StartRoarEmitter)fsm.GetState("Entry Roar").actions[1]).stunHero = false;

        // Adjust for X position
        fsm.FsmVariables.FindFsmFloat("X Centre").value = obj.transform.position.x;
        fsm.FsmVariables.FindFsmFloat("X Min").value = obj.transform.position.x - 13.75f;
        fsm.FsmVariables.FindFsmFloat("X Max").value = obj.transform.position.x + 13.75f;

        // Adjust for ground position
        var ground = HeroController.instance.FindGroundPointY(
            obj.transform.position.x,
            obj.transform.position.y,
            true);
        fsm.FsmVariables.FindFsmFloat("Y Underwater ").value = ground - 4.56f;
        fsm.FsmVariables.FindFsmFloat("Splash In Y").value = ground - 3.2f;
        fsm.FsmVariables.FindFsmFloat("Pt WaterSplash Y").value = ground - 3.2f;

        // Disable music
        fsm.GetState("Fight Start").DisableAction(0);

        // Hooks summon and trap events to Architect events
        fsm.GetState("Trap").AddAction(() => obj.BroadcastEvent("TrySpikeTrap"), 1);
        var summon = fsm.GetState("Summon");
        summon.AddAction(() => obj.BroadcastEvent("TrySpikeTrap"), 3);
        summon.AddAction(() => obj.BroadcastEvent("TrySummon"), 1);

        // No longer forces range to prevent infinite loop if leaving range
        fsm.GetState("Dive Up Antic").DisableAction(4);
        
        fsm.GetState("End Battle").AddAction(() => Object.Destroy(obj));
    }

    
    public static void FixStilkin(GameObject obj)
    {
        RemoveConstrainPosition(obj);
        var fsm = obj.LocateMyFSM("Control");
        
        fsm.GetState("Init").AddAction(() => fsm.SetState("Set Evade Hops"), 6);
        fsm.GetState("Hidden G").AddAction(() =>
        {
            if ((obj.transform.position - HeroController.instance.transform.position).magnitude < 12) 
                fsm.SendEvent("AMBUSH");
        }, 0, true);
        fsm.GetState("Positioning Check").AddAction(() => fsm.SendEvent("FINISHED"), 0);
        fsm.GetState("Hidden Underwater").DisableAction(0);
    }

    public static void FixStilkinTrapper(GameObject obj)
    {
        RemoveConstrainPosition(obj);
        var fsm = obj.LocateMyFSM("Control");
        
        fsm.GetState("Init").AddAction(() =>
        {
            obj.GetComponent<tk2dSpriteAnimator>().Play("Hop");
            fsm.SetState("Flutter Start");
        }, 2);
        fsm.GetState("Hidden G").AddAction(() =>
        {
            if ((obj.transform.position - HeroController.instance.transform.position).magnitude < 12) 
                fsm.SendEvent("AMBUSH");
        }, 0, true);
        fsm.GetState("Node Check").AddAction(() => fsm.SendEvent("FINISHED"), 0);
        
        var digOut1 = fsm.GetState("Dig Out G 1");
        ((Tk2dPlayAnimationWithEvents)digOut1.actions[2]).clipName = "Hop";
        digOut1.AddAction(new Wait
        {
            time = 0.4f,
            finishEvent = FsmEvent.Finished
        });
        
        var digOut2 = fsm.GetState("Dig Out G 2");
        digOut2.DisableAction(0);
        digOut2.AddAction(() =>
        {
            fsm.SetState("Water Exit Q");
        });
        
        fsm.GetState("Water Exit Q").DisableAction(6);
        fsm.GetState("Hide Underwater").DisableAction(0);
        fsm.GetState("Water Pos").DisableAction(1);
    }

    public static void FixLastClawPreload(GameObject obj)
    {
        obj.transform.SetPositionZ(0.006f);
        var anim = obj.GetComponent<tk2dSpriteAnimator>();
        anim.defaultClipId = anim.GetClipIdByName("Fly");
    }

    public static void FixLastClaw(GameObject obj)
    {
        // Act like a regular enemy, flies idly and is alert when Hornet is nearby
        obj.layer = 11;
        
        var fsm = obj.LocateMyFSM("Control");
        
        var guard = fsm.GetState("Guard");
        guard.DisableAction(0);
        guard.AddAction(() =>
        {
            if ((obj.transform.position - HeroController.instance.transform.position).magnitude < 12) 
                fsm.SetState("Roar");
        }, everyFrame:true);
        guard.AddAction(new IdleBuzz
        {
            gameObject = new FsmOwnerDefault { gameObject = obj },
            roamingRange = 1,
            waitMax = 1,
            waitMin = 0.5f,
            accelerationMax = 20,
            speedMax = 5
        });
        
        var firstMoveTime = 0f;

        var rb2d = obj.GetComponent<Rigidbody2D>();

        var charge = fsm.GetState("Charge");
        
        charge.AddAction(() =>
        {
            firstMoveTime = Time.time;
        });
        charge.AddAction(() =>
        {
            if (Time.time - firstMoveTime < 0.1f) return;

            if (Mathf.Abs(rb2d.linearVelocityX) < 0.05f) fsm.SendEvent("END");
        }, everyFrame: true);

        var dThrust = fsm.GetState("Dthrust");
        dThrust.AddAction(() =>
        {
            firstMoveTime = Time.time;
        });
        dThrust.AddAction(() =>
        {
            if (Time.time - firstMoveTime < 0.1f) return;
            
            if (Mathf.Abs(rb2d.linearVelocityY) < 0.05f) fsm.SendEvent("LAND");
        }, everyFrame: true);

        fsm.FsmVariables.FindFsmFloat("Max Height").value = obj.transform.GetPositionY() + 15;
    }

    public static void ScaleLastClaw(GameObject obj, float scale)
    {
        obj.transform.localScale *= scale;
        
        if (scale < 1) return;
        
        var fsm = obj.LocateMyFSM("Control");
        
        ((RayCast2dV2)fsm.GetState("Charge").actions[4]).distance.value *= scale;
        ((RayCast2dV2)fsm.GetState("Dthrust").actions[8]).distance.value *= scale;
    }

    public static void FixMossMother(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Control");
        
        // Variables to align
        var vars = fsm.FsmVariables;
        var centreX = vars.FindFsmFloat("Centre X");
        var leftX = vars.FindFsmFloat("Left X");
        var rightX = vars.FindFsmFloat("Right X");
        var swoopY = vars.FindFsmFloat("Swoop Height");
        var maxY = vars.FindFsmFloat("Max Height");
        
        // Align based on self
        fsm.GetState("Init").AddAction(() => Realign(obj.transform.position), 0);
        
        // Wake
        obj.GetComponent<MeshRenderer>().enabled = true;
        obj.transform.GetChild(0).gameObject.SetActive(false);
        fsm.GetState("Dormant").AddAction(() =>
        {
            if (obj.GetComponent<HealthManager>().isDead) return;
            fsm.SetState("Roar");
        }, 0);
        
        // Disable stun
        ((StartRoarEmitter)fsm.GetState("Roar").actions[9]).stunHero = false;
        
        // Align based on player
        fsm.GetState("Idle").AddAction(() => Realign(HeroController.instance.transform.position), 0);
        
        // Broadcast slam event
        fsm.GetState("Slam").AddAction(() => obj.BroadcastEvent("Slam"), 0);
        
        // Fix stuck
        fsm.GetState("Slam RePos").AddAction(() => fsm.SendEvent("FINISHED"), 0);

        // Swoop follow alignment
        var swoop = fsm.GetState("Swoop");
        swoop.DisableAction(1);
        swoop.DisableAction(2);
        swoop.DisableAction(3);
        
        // Disable music
        var roarEnd = fsm.GetState("Roar End");
        roarEnd.DisableAction(3);
        roarEnd.DisableAction(4);
        
        return;

        void Realign(Vector2 source)
        {
            centreX.value = source.x;
            leftX.value = source.x - 10.5f;
            rightX.value = source.x + 10.5f;
            swoopY.value = source.y;
            maxY.value = source.y + 6;
        }
    }

    public static void FixSkullTyrant(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Behaviour");
        
        // Wake
        fsm.GetState("State Check").AddAction(() => fsm.SendEvent("INVADING"), 0);
        fsm.GetState("Check Respawn Pos").AddAction(() => fsm.SendEvent("FINISHED"), 0);
        fsm.GetState("Standard Respawn").AddAction(() => fsm.SendEvent("FINISHED"), 0);

        var pause = fsm.GetState("Invading Pause");
        pause.DisableAction(6);
        pause.DisableAction(7);
        pause.AddAction(new Wait
        {
            time = 0.1f,
            finishEvent = FsmEvent.Finished
        });

        var idle = fsm.GetState("Invading Idle");
        idle.DisableAction(0);
        idle.DisableAction(1);
        idle.AddAction(() =>
        {
            if (obj.GetComponent<HealthManager>().isDead) return;
            fsm.SendEvent("WAKE");
        }, 10);
        
        fsm.GetState("Invading").AddAction(() => fsm.SendEvent("ROAR"), 0);
        fsm.GetState("Invading Idle 2").AddAction(() => fsm.SendEvent("FINISHED"), 0);
        
        fsm.GetState("ReWake Roar").AddAction(fsm.GetState("Wake Roar").actions[2], 3);

        // Disable music
        var end = fsm.GetState("Roar End");
        end.DisableAction(1);
        end.DisableAction(2);
        
        // Don't kill Pilby
        fsm.GetState("Kill Pilby? 2").DisableAction(1);
        
        // Boulders
        var boulders = Object.Instantiate(_bouldersPrefab);
        boulders.SetActive(true);
        fsm.FsmVariables.FindFsmGameObject("Boulders Battle").value = boulders;
        
        var choice = fsm.GetState("Boulder Choice");
        choice.AddAction(() =>
        {
            obj.BroadcastEvent("Stomp");
            boulders.transform.SetPositionX(obj.transform.position.x);
        }, 0);
    }

    public static void FixSavageBeastfly(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Control");
        
        // Not invulnerable
        var hm = obj.GetComponent<HealthManager>();
        hm.IsInvincible = false;
        hm.hasSpecialDeath = false;
        
        // Fix visual bug on start
        var anim = obj.GetComponent<tk2dSpriteAnimator>();
        anim.Play(anim.DefaultClip);
        
        // Disable stun
        ((StartRoarEmitter)fsm.GetState("Intro Roar").actions[1]).stunHero = false;
        
        // Fix player not entered
        var entered = fsm.FsmVariables.FindFsmBool("Hero Entered");
        var slam = fsm.FsmVariables.FindFsmBool("Final Slam");
        fsm.GetState("Choice").AddAction(() =>
        {
            if (entered.value) return;
            if ((obj.transform.position - HeroController.instance.transform.position).magnitude < 12) 
                fsm.SendEvent("INTRO");
        }, 0);
        fsm.GetState("Intro Antic").AddAction(() =>
        {
            entered.value = true;
            slam.value = true;
        }, 0);
        
        // Summon event
        fsm.GetState("Roar Pos").DisableAction(4);
        fsm.GetState("Summon Type").AddAction(() => obj.BroadcastEvent("TrySummon"), 0);

        // Stomp fix
        var stompY = new GameObject(obj.name + " Stomp Y");
        fsm.FsmVariables.FindFsmGameObject("Stomp Y").value = stompY;
        stompY.transform.position = obj.transform.position;
        fsm.FsmVariables.FindFsmFloat("High Y Pos").value = obj.transform.GetPositionY();
        
        // Disable music
        var recover = fsm.GetState("Intro Recover");
        recover.DisableAction(4);
        recover.DisableAction(5);
    }

    public static void FixElderPilgrim(GameObject obj)
    {
        KeepActive(obj);
        var anim = obj.GetComponent<tk2dSpriteAnimator>();
        anim.defaultClipId = anim.GetClipIdByName("Idle");
    }

    public static void FixBonegravePilgrim(GameObject obj)
    {
        var fsm = obj.GetComponent<PlayMakerFSM>();
        fsm.fsmTemplate = null;
        fsm.GetState("Init").AddAction(() => fsm.SendEvent("FINISHED"), 0);
    }

    public static void FixWatcher(GameObject obj)
    {
        var watcher = obj.AddComponent<Watcher>();
        
        var fsm = obj.LocateMyFSM("Control");
        
        fsm.GetState("Start State").AddAction(() => { fsm.SendEvent(watcher.startAwake ? "WAKE" : "SLEEP"); }, 0);
        fsm.GetState("Away").DisableAction(2);
        fsm.GetState("Refight Ready").DisableAction(3);
        
        fsm.GetState("Die").DisableAction(0);
    }

    public class Watcher : MonoBehaviour
    {
        public bool startAwake;
    }

    public static void FixZango(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Control");
        fsm.GetState("Init").DisableAction(3);

        var rest = fsm.GetState("Rest");
        rest.DisableAction(1);
        rest.AddAction(() =>
        {
            if ((obj.transform.position - HeroController.instance.transform.position).magnitude < 12)
                fsm.SendEvent("WAKE");
        }, everyFrame: true);

        ((StartRoarEmitter)fsm.GetState("Roar").actions[2]).stunHero = false;
    }

    public static void FixKarmelitaPreload(GameObject obj)
    {
        RemoveConstrainPosition(obj);
        
        obj.LocateMyFSM("Control").enabled = false;
        
        obj.transform.parent.DetachChildren();
        obj.transform.SetPositionZ(0.006f);
    }

    public static void FixKarmelita(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Control");
        fsm.enabled = true;
        fsm.GetState("BG Dance").AddAction(() => fsm.SendEvent("CHALLENGE"), 0);
    }

    public static void FixCraggler(GameObject obj)
    {
        KeepActive(obj);
        var fsm = obj.LocateMyFSM("Control");
        fsm.GetState("Dormant").AddAction(() => fsm.SendEvent("WAKE"));
    }

    public static void FixFlyin(GameObject obj)
    {
        var anim = obj.GetComponent<tk2dSpriteAnimator>();
        anim.defaultClipId = anim.GetClipIdByName("Fly");
        
        obj.GetComponent<tk2dSprite>().color = Color.white;

        obj.GetComponent<MeshRenderer>().enabled = true;
        
        obj.LocateMyFSM("Control").FsmVariables.FindFsmBool("Battler").value = false;
    }

    public static void FixClawmaiden(GameObject obj)
    {
        var anim = obj.GetComponent<tk2dSpriteAnimator>();
        anim.defaultClipId = anim.GetClipIdByName("Rest");

        obj.LocateMyFSM("Control").FsmVariables.FindFsmBool("Off Plane").value = false;

        obj.transform.SetPositionZ(0.006f);
    }

    public static void FixVaultborn(GameObject obj)
    {
        var anim = obj.GetComponent<tk2dSpriteAnimator>();
        anim.defaultClipId = anim.GetClipIdByName("Idle");
    }

    public static void FixBrushflit(GameObject obj)
    {
        obj.GetComponent<FlockFlyer>().activeProbability = 1;
    }

    public static void ScaleBrushflit(GameObject obj, float scale)
    {
        var flyer = obj.GetComponent<FlockFlyer>();
        flyer.minScale *= scale;
        flyer.maxScale *= scale;
    }

    public static void FixSummonedSaviour(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Control");

        fsm.GetState("Dormant").AddAction(() =>
        {
            obj.GetComponent<BoxCollider2D>().isTrigger = false;
            fsm.SendEvent("WAKE");
        });

        ((StartRoarEmitter)fsm.GetState("Roar").actions[2]).stunHero = false;

        var dash = fsm.GetState("Dash Dir");
        dash.DisableAction(1);
        dash.DisableAction(2);
    }

    public static void FixBroodmother(GameObject obj)
    {
        RemoveConstrainPosition(obj);
        
        var fsm = obj.LocateMyFSM("Control");
        var flies = Object.Instantiate(_freshflyCagePrefab);
        flies.SetActive(true);
        fsm.FsmVariables.FindFsmGameObject("Spawner Flies").value = flies;
        
        ((StartRoarEmitter)fsm.GetState("Roar").actions[7]).stunHero = false;

        fsm.GetState("Dormant").AddAction(new Wait
        {
            time = 0.1f,
            finishEvent = FsmEvent.GetFsmEvent("BATTLE START")
        });
        fsm.GetState("Entry Antic").AddAction(() =>
        {
            var hm = obj.GetComponent<HealthManager>();
            if (hm.isDead)
            {
                fsm.enabled = false;
                return;
            }

            hm.enemyDeathEffects.GetInstantiatedCorpse(AttackTypes.Generic).RemoveComponent<ConstrainPosition>();
            fsm.SendEvent("FINISHED");
        }, 0);
        fsm.GetState("Burst In").AddAction(() => fsm.SendEvent("FINISHED"), 0);
    }

    public static void FixFreshfly(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Bouncer Control");
        fsm.fsmTemplate = null;
        fsm.GetState("Start Pause").AddAction(() => fsm.SetState("Left or Right?"), 11);
        obj.GetComponent<HealthManager>().hasSpecialDeath = false;

        obj.AddComponent<Freshfly>().pos = obj.transform.position;
    }

    private class Freshfly : MonoBehaviour
    {
        public Vector3 pos;
        
        private void OnEnable()
        {
            var fsm = GetComponent<PlayMakerFSM>();
            if (fsm.enabled) return; 
            transform.position = pos;
            fsm.enabled = true;
        }
    }

    public static void FixCorrcrustKaraka(GameObject obj)
    {
        FixSpearSpawned(obj);
        RemoveConstrainPosition(obj);
        var ck = obj.AddComponent<CorrcrustKaraka>();

        var fsm = obj.LocateMyFSM("Control");
        var rise = fsm.GetState("Stomp Rise");
        
        // Disable normal behaviour
        rise.DisableAction(2);
        rise.DisableAction(3);

        // Target positions
        var targetX = fsm.FsmVariables.FindFsmFloat("Target X");
        var targetY = fsm.FsmVariables.FindFsmFloat("Target Y");

        // Initial positions
        var runningTime = 0f;
        var fromX = 0f;
        var fromY = 0f;
        
        rise.AddAction(() =>
        {
            runningTime = 0;
            fromX = obj.transform.GetPositionX();
            fromY = obj.transform.GetPositionY();
            ck.moveValid = true;
        }, 2);
        
        // Move with Rigidbody2D
        var rb2d = obj.GetComponent<Rigidbody2D>();
        rise.AddAction(() =>
        {
            if (!ck.moveValid) return;
            
            runningTime += Time.deltaTime;

            var percentage = Mathf.Clamp01(runningTime * 2);
            
            rb2d.MovePosition(new Vector2(
                EaseInOutSine(fromX, targetX.Value, percentage),
                EaseOutCubic(fromY, targetY.Value, percentage))
            );
        }, 3, true);

        return;

        float EaseOutCubic(float start, float end, float value)
        {
            --value;
            end -= start;
            return end * (float) (value * value * value + 1.0) + start;
        }

        float EaseInOutSine(float start, float end, float value)
        {
            end -= start;
            return (float)(-(double)end / 2.0 * (Mathf.Cos((float)(3.1415927410125732 * value / 1.0)) - 1.0)) + start;
        }
    }

    private class CorrcrustKaraka : MonoBehaviour
    {
        public bool moveValid;
        
        private void OnCollisionStay2D(Collision2D other)
        {
            if (other.gameObject.layer == 8) moveValid = false;
        }
    }

    public static void FixKarakGor(GameObject obj)
    {
        obj.LocateMyFSM("Control").FsmVariables.FindFsmBool("Spear Spawner").Value = false;
        obj.GetComponent<tk2dSpriteAnimator>().defaultClipId = 8;
    }

    public static void FixAlita(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Control");
        fsm.FsmVariables.FindFsmBool("Spear Spawner").Value = false;

        var ground = obj.transform.GetPositionY();
        fsm.FsmVariables.FindFsmFloat("Tele Air Y Max").Value = ground + 8;
        fsm.FsmVariables.FindFsmFloat("Tele Air Y Min").Value = ground + 2;
        fsm.FsmVariables.FindFsmFloat("Tele Ground Y").Value = ground;

        fsm.FsmVariables.FindFsmFloat("Tele X Max").Value = obj.transform.GetPositionX() + 11;
        fsm.FsmVariables.FindFsmFloat("Tele X Min").Value = obj.transform.GetPositionX() - 11;

        fsm.FsmVariables.FindFsmGameObject("Aiming Cursor").Value = new GameObject(obj.name + " Aim Cursor");
    }

    public static void FixSecondSentinelBoss(GameObject obj)
    {
        var ede = obj.GetComponent<EnemyDeathEffects>();
        ede.PreInstantiate();
        var corpse = ede.GetInstantiatedCorpse(AttackTypes.Generic);
        var blow = corpse.LocateMyFSM("Death").GetState("Blow");
        blow.transitions = [];
        blow.AddAction(() =>
        {
            var rp = Object.Instantiate(_robotParticles, corpse.transform.position, corpse.transform.rotation);
            rp.transform.Find("Chunks").gameObject.SetActive(false);
            rp.SetActive(true);
            corpse.SetActive(false);
        });
        
        var fsm = obj.LocateMyFSM("Control");
        fsm.fsmTemplate = null;
        var pdbt = (PlayerDataBoolTest)fsm.GetState("Init").actions[7];
        pdbt.isFalse = pdbt.isTrue;
        fsm.FsmVariables.FindFsmBool("Hornet Dead").Value = false;
        fsm.GetState("Dash Slash 3").DisableAction(2);
        
        obj.transform.Find("Catch Markers").gameObject.RemoveComponent<ConstrainPosition>();
        Object.Destroy(obj.LocateMyFSM("Save Hero"));
    }

    public static void FixLugoli(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Control");
        fsm.GetState("Dormant").AddAction(() => fsm.SendEvent("BATTLE START"));
    }

    public static void FixGms(GameObject obj)
    {
        RemoveConstrainPosition(obj);

        var fsm = obj.LocateMyFSM("Control");

        fsm.FsmVariables.FindFsmFloat("Idle Y").Value = obj.transform.GetPositionY();
        var ground1 = fsm.FsmVariables.FindFsmFloat("Stun Y");
        var ground2 = fsm.FsmVariables.FindFsmFloat("Bell Stun Y");

        fsm.GetState("Dormant").AddAction(() => fsm.SendEvent("READY"), 0);
        fsm.GetState("Ready").AddAction(() => fsm.SendEvent("BEGIN"));

        var up = fsm.GetState("Intro Up");
        up.DisableAction(7);
        ((Wait)up.actions[8]).time = 0;

        FixGround();

        return;

        void FixGround()
        {
            if (HeroController.instance.TryFindGroundPoint(out var pos,
                    obj.transform.position,
                    true))
            {
                var y = pos.y + 3.3091f;
                ground1.Value = y;
                ground2.Value = y;
            }
        }
    }

    public static void FixAknidMother(GameObject obj)
    {
        obj.AddComponent<AknidMother>();
    }

    private class AknidMother : SoundMaker
    {
        private static readonly AudioClip[] Clips = new AudioClip[4];
        private static AudioClip _sporeClip;

        public static void InitSounds()
        {
            ResourceUtils.LoadClipResource("AknidMother.hit_1", clip => Clips[0] = clip);
            ResourceUtils.LoadClipResource("AknidMother.hit_2", clip => Clips[1] = clip);
            ResourceUtils.LoadClipResource("AknidMother.hit_3", clip => Clips[2] = clip);
            ResourceUtils.LoadClipResource("AknidMother.hit_4", clip => Clips[3] = clip);
            ResourceUtils.LoadClipResource("AknidMother.spore", clip => _sporeClip = clip);
        }
        
        private void Start()
        {
            GetComponent<EnemyHitEffectsRegular>().ReceivedHitEffect += (_, _) =>
            {
                PlaySound(Clips[Random.RandomRangeInt(0, 4)]);
            };
            var ctrl = gameObject.LocateMyFSM("Control");
            ctrl.GetState("Sing End").transitions[0].toFsmState = ctrl.GetState("Recover");
            
            
        }
    }

    public static void FixSplinter(GameObject obj)
    {
        var anim = obj.GetComponent<tk2dSpriteAnimator>();
        anim.defaultClipId = anim.GetClipIdByName("Branch 01 Idle");
    }

    public static void FixSisterSplinter(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Control");
        
        fsm.GetState("Dormant").AddAction(() => fsm.SendEvent("BATTLE START"));
        fsm.GetState("Intro Shake").AddAction(() => fsm.SendEvent("FINISHED"), 0);
        fsm.GetState("Emerge Antic").AddAction(() => fsm.SendEvent("FINISHED"), 0);
        fsm.GetState("Reappear 4").AddAction(() => fsm.SendEvent("FINISHED"), 1);
        fsm.GetState("Reappear 5").AddAction(() => fsm.SendEvent("FINISHED"), 0);
        fsm.GetState("Reappear 6").AddAction(() => fsm.SendEvent("FINISHED"), 0);

        var roar = fsm.GetState("Roar 4");
        roar.DisableAction(0);
        roar.DisableAction(2);
        ((StartRoarEmitter)roar.actions[3]).stunHero = false;

        var roarEnd = fsm.GetState("Roar End 4");
        roarEnd.DisableAction(3);
        roarEnd.DisableAction(4);
        roarEnd.DisableAction(5);
        
        fsm.GetState("Can Summon?").DisableAction(1);

        var xMid = fsm.FsmVariables.FindFsmFloat("Mid X");
        var xMin = fsm.FsmVariables.FindFsmFloat("X Min");
        var xMax = fsm.FsmVariables.FindFsmFloat("X Max");
        
        var groundY = fsm.FsmVariables.FindFsmFloat("Ground Y");
        var ede = obj.GetComponent<EnemyDeathEffects>();
        ede.PreInstantiate();
        var corpse = ede.GetInstantiatedCorpse(AttackTypes.Generic);
        var deathGroundY = corpse.LocateMyFSM("Death").FsmVariables.FindFsmFloat("Ground Y");
        
        fsm.GetState("Idle").AddAction(AdjustAllPos, 0);
        fsm.GetState("Stun Hit").AddAction(AdjustYPos, 0);
        AdjustAllPos();

        /*fsm.FsmVariables.FindFsmGameObject("Spikes Folder").Value = 
            Object.Instantiate(_splinterSpikes, obj.transform.position, obj.transform.rotation);*/

        return;

        void AdjustAllPos()
        {
            xMid.Value = obj.transform.position.x;
            xMin.Value = obj.transform.position.x - 10.5f;
            xMax.Value = obj.transform.position.x + 10.5f;
            AdjustYPos();
        }

        void AdjustYPos()
        {
            if (!HeroController.instance.TryFindGroundPoint(
                    out var point, 
                    obj.transform.position, 
                    true)) return;
            groundY.Value = point.y - 1.8f;
            deathGroundY.Value = point.y - 1.8f;
        }
    }

    public static void FixFirstSinner(GameObject obj)
    {
        RemoveConstrainPosition(obj);

        var rb2d = obj.GetComponent<Rigidbody2D>();
        var fsm = obj.LocateMyFSM("Control");

        // Disable music and roar stun
        var roarEnd = fsm.GetState("Intro Roar End");
        roarEnd.DisableAction(0);
        roarEnd.DisableAction(1);
        
        var p2Tele = fsm.GetState("P2 Tele Pause");
        p2Tele.DisableAction(0);
        p2Tele.AddAction(NoGravity, 0);
        fsm.GetState("Music Stop").DisableAction(0);
        
        var p2Roar = fsm.GetState("P2 Roar");
        p2Roar.DisableAction(0);
        p2Roar.DisableAction(2);
        ((StartRoarEmitter)p2Roar.actions[4]).stunHero = false;

        var roar = fsm.GetState("Roar");
        ((StartRoarEmitter)roar.actions[6]).stunHero = false;

        // Start fight
        fsm.GetState("Dormant").AddAction(() => fsm.SendEvent("REFIGHT START"));

        var appear = fsm.GetState("Refight Appear");
        appear.DisableAction(1);
        appear.DisableAction(5);

        // General positions
        var groundY = fsm.FsmVariables.FindFsmFloat("Ground Y");
        var slashY = fsm.FsmVariables.FindFsmFloat("Slash Y");
        var teleMinX = fsm.FsmVariables.FindFsmFloat("Tele Min X");
        var teleMaxX = fsm.FsmVariables.FindFsmFloat("Tele Max X");
        var slashMinX = fsm.FsmVariables.FindFsmFloat("Slash Min X");
        var slashMaxX = fsm.FsmVariables.FindFsmFloat("Slash Max X");

        var teleY = fsm.FsmVariables.FindFsmFloat("Tele Y");
        var bombTeleY1 = ((RandomFloatEither)fsm.GetState("Set Bomb").actions[3]).value1;
        var bombTeleY2 = ((SetFloatValue)fsm.GetState("Set P2 Start").actions[3]).floatValue;
        
        // Pins
        var loosePins = Object.Instantiate(_loosePins);
        for (var i = 0; i < loosePins.transform.childCount; i++)
        {
            var child = loosePins.transform.GetChild(i).gameObject;
            RemoveConstrainPosition(child);
            child.SetActive(true);
            child.transform.position = obj.transform.position + new Vector3(Random.Range(-2f, 2f), -1.25f);
        }
        loosePins.SetActive(true);
        var pins = Object.Instantiate(_pinProjectiles);
        for (var i = 0; i < pins.transform.childCount; i++)
        {
            var child = pins.transform.GetChild(i).gameObject;
            child.SetActive(true);
            var pinFsm = child.LocateMyFSM("Control");
            pinFsm.FsmVariables.FindFsmGameObject("Loose Pins").Value = loosePins;
        }
        var pinsFsm = pins.LocateMyFSM("Pattern Control");
        var sceneCentreX = pinsFsm.FsmVariables.FindFsmFloat("Scene Centre X");
        var pincerMinX = pinsFsm.FsmVariables.FindFsmFloat("Pincer Min X");
        var pincerMaxX = pinsFsm.FsmVariables.FindFsmFloat("Pincer Max X");

        var sweepR = pinsFsm.GetState("Sweep R");
        var sweepR1Y = ((SetPosition)sweepR.actions[5]).y;
        var sweepR2Y = ((SetPosition)sweepR.actions[12]).y;
        var sweepL = pinsFsm.GetState("Sweep L");
        var sweepL1Y = ((SetPosition)sweepL.actions[5]).y;
        var sweepL2Y = ((SetPosition)sweepL.actions[12]).y;

        var clawR = pinsFsm.GetState("Claw R");
        var clawR1Y = ((SetPosition)clawR.actions[5]).y;
        var clawR2Y = ((SetPosition)clawR.actions[12]).y;
        var clawR3Y = ((SetPosition)clawR.actions[19]).y;
        var clawR1X = ((FloatOperator)clawR.actions[3]).float1;
        var clawR2X = ((FloatOperator)clawR.actions[10]).float1;
        var clawR3X = ((FloatOperator)clawR.actions[17]).float1;
        var clawL = pinsFsm.GetState("Claw L");
        var clawL1Y = ((SetPosition)clawL.actions[5]).y;
        var clawL2Y = ((SetPosition)clawL.actions[12]).y;
        var clawL3Y = ((SetPosition)clawL.actions[19]).y;
        var clawL1X = ((FloatOperator)clawL.actions[3]).float1;
        var clawL2X = ((FloatOperator)clawL.actions[10]).float1;
        var clawL3X = ((FloatOperator)clawL.actions[17]).float1;

        var rain1 = pinsFsm.GetState("Rain 1");
        var rain1XMin = ((RandomFloat)rain1.actions[0]).min;
        var rain1XMax = ((RandomFloat)rain1.actions[0]).max;
        var rain1YMin = ((RandomFloat)rain1.actions[1]).min;
        var rain1YMax = ((RandomFloat)rain1.actions[1]).max;
        var rain2 = pinsFsm.GetState("Rain 2");
        var rain2YMin = ((RandomFloat)rain2.actions[1]).min;
        var rain2YMax = ((RandomFloat)rain2.actions[1]).max;
        
        pins.SetActive(true);
        
        fsm.FsmVariables.FindFsmGameObject("Pin Projectiles").Value = pins;

        var attackEvent = fsm.FsmVariables.FindFsmString("Attack Event");

        // Running position fixes
        fsm.GetState("First Idle").DisableAction(0);
        
        var idle = fsm.GetState("Idle");
        idle.DisableAction(0);
        idle.AddAction(FixPositions, 0);
        var teleOut = fsm.GetState("Tele Out");
        teleOut.AddAction(NoGravity, 0);
        fsm.GetState("Phase Check").AddAction(FixPositions, 0);
        fsm.GetState("Tele In").AddAction(FixPositions, 0);
        fsm.GetState("After Tele").AddAction(FixPositions, 0);
        
        fsm.GetState("Death Stagger F").DisableAction(11);
        
        fsm.GetState("Fall").AddAction(() =>
        {
            if (rb2d.linearVelocityY == 0) fsm.SendEvent("LAND");
        }, everyFrame: true);
        
        fsm.GetState("Charge").AddAction(() =>
        {
            if ((obj.transform.position - HeroController.instance.transform.position).magnitude > 25) 
                fsm.SendEvent("FINISHED");
        }, 3, true);

        // Blasts
        var blast = Object.Instantiate(_blasts);
        blast.SetActive(true);
        foreach (var blastFsm in blast.GetComponentsInChildren<PlayMakerFSM>())
        {
            var wait = blastFsm.GetState("Wait");
            if (wait == null) continue;
            var xMin = blastFsm.FsmVariables.FindFsmFloat("X Min");
            var xMax = blastFsm.FsmVariables.FindFsmFloat("X Max");

            var low = blastFsm.GetState("Pos Low");
            var high = blastFsm.GetState("Pos High");
            var lowMin = ((RandomFloat)low.actions[0]).min;
            var lowMax = ((RandomFloat)low.actions[0]).max;
            var highMin = ((RandomFloat)high.actions[0]).min;
            var highMax = ((RandomFloat)high.actions[0]).max;
            wait.AddAction(() =>
            {
                xMin.Value = obj.transform.GetPositionX() - 12;
                xMax.Value = obj.transform.GetPositionX() + 12;

                lowMin.Value = groundY.Value - 0.09f;
                lowMax.Value = groundY.Value + 0.91f;
                highMin.Value = groundY.Value + 3.8f;
                highMax.Value = groundY.Value + 6.3f;
            }, 0);
        }
        fsm.FsmVariables.FindFsmGameObject("Blasts").Value = blast;
        
        // Don't break after death
        idle.transitions = idle.transitions
            .Where(trans => trans.EventName != "HORNET DEAD").ToArray();
        teleOut.transitions = teleOut.transitions
            .Where(trans => trans.EventName != "HORNET DEAD").ToArray();

        rb2d.gravityScale = 1;
        rb2d.bodyType = RigidbodyType2D.Dynamic;
        FixPositions();
        return;

        void NoGravity()
        {
            rb2d.gravityScale = 0;
            rb2d.linearVelocityY = 0;
        }

        void FixPositions()
        {
            if (fsm.ActiveStateName != "Tele In" || attackEvent.Value != "BOMB" && rb2d.gravityScale == 0)
            {
                obj.transform.SetPositionX(obj.transform.GetPositionX() + 0.2f);
                rb2d.gravityScale = 1;
                rb2d.bodyType = RigidbodyType2D.Dynamic;
            }

            var posX = HeroController.instance.transform.GetPositionX();
            teleMinX.Value = slashMinX.Value = posX - 12.5f;
            teleMaxX.Value = slashMaxX.Value = posX + 12.5f;
            
            // Pins
            sceneCentreX.Value = obj.transform.GetPositionX();
            pincerMinX.Value = sceneCentreX.Value - 7.5f;
            pincerMaxX.Value = sceneCentreX.Value + 7.5f;

            clawR1X.Value = sceneCentreX.Value - 14.2f;
            clawR2X.Value = sceneCentreX.Value - 7.2f;
            clawR3X.Value = sceneCentreX.Value - 0.2f;
            clawL1X.Value = sceneCentreX.Value + 14.2f;
            clawL2X.Value = sceneCentreX.Value + 7.2f;
            clawL3X.Value = sceneCentreX.Value + 0.2f;

            rain1XMin.Value = sceneCentreX.Value - 14f;
            rain1XMax.Value = sceneCentreX.Value - 10f;

            if (!HeroController.instance.TryFindGroundPoint(
                    out var point,
                    new Vector2(posX,
                        Mathf.Max(obj.transform.position.y, HeroController.instance.transform.position.y) + 3),
                    true)) return;
            groundY.Value = point.y + 1.68f;
            slashY.Value = point.y + 5.98f;
            teleY.Value = point.y + 1.68f;
            bombTeleY1.Value = bombTeleY2.Value = point.y + 4.48f;
            
            // Pins
            sweepL1Y.Value = sweepR1Y.Value = point.y + 0.48f;
            sweepL2Y.Value = sweepR2Y.Value = point.y + 7.48f;

            clawR1Y.Value = clawL1Y.Value = point.y + 8.78f;
            clawR2Y.Value = clawL2Y.Value = point.y + 9.78f;
            clawR3Y.Value = clawL3Y.Value = point.y + 10.78f;

            rain1YMin.Value = rain2YMin.Value = point.y + 9.48f;
            rain1YMax.Value = rain2YMax.Value = point.y + 10.08f;
        }
    }

    public static void FixRoachserver(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Control");
        var range = obj.GetComponentInChildren<AlertRange>();

        obj.layer = EnemiesLayer;

        var work = fsm.GetState("BG Work");
        work.DisableAction(0);
        work.DisableAction(1);
        work.AddAction(() =>
        {
            if (range.IsHeroInRange()) fsm.SendEvent("BATTLE START");
        }, 0, true);
    }

    public static PlayMakerFSM FixForebrother(GameObject obj)
    {
        RemoveConstrainPosition(obj);
        var fsm = obj.LocateMyFSM("Control");

        var xMin = fsm.FsmVariables.FindFsmFloat("X Min");
        var xMax = fsm.FsmVariables.FindFsmFloat("X Max");
        
        fsm.GetState("Idle").AddAction(FixXPos, 0);
        
        return fsm;

        void FixXPos()
        {
            var posX = HeroController.instance.transform.GetPositionX();
            xMin.Value = posX - 11;
            xMax.Value = posX + 11;
        }
    }

    public static void FixSignis(GameObject obj)
    {
        var fsm = FixForebrother(obj);
        var range = obj.GetComponentInChildren<AlertRange>();
        fsm.GetState("Pointing").AddAction(() =>
        {
            if (range.IsHeroInRange()) fsm.SendEvent("BATTLE START");
        }, 0);
    }

    public static void FixGron(GameObject obj)
    {
        var fsm = FixForebrother(obj);
        var pause = fsm.GetState("Start Pause");
        pause.DisableAction(0);
        pause.DisableAction(1);

        var range = obj.GetComponentInChildren<AlertRange>();
        pause.AddAction(() =>
        {
            if (range.IsHeroInRange()) fsm.SetState("Entry Land");
        }, 0);
    }

    public static void FixPhantom(GameObject obj)
    {
        RemoveConstrainPosition(obj);
        var fsm = obj.LocateMyFSM("Control");

        fsm.GetState("Dormant").AddAction(() => fsm.SendEvent("PHANTOM START"));

        var leftX = fsm.FsmVariables.FindFsmFloat("Left X");
        var rightX = fsm.FsmVariables.FindFsmFloat("Right X");

        fsm.GetState("Idle").AddAction(FixPositions, 0);
        fsm.GetState("Range Check").AddAction(FixPositions, 0);

        return;

        void FixPositions()
        {
            var xPos = HeroController.instance.transform.GetPositionX();
            leftX.Value = xPos - 8.5f;
            rightX.Value = xPos + 8.5f;
        }
    }

    public static void FixLostGarmondPreload(GameObject obj)
    {
        var old = obj.GetComponent<BlackThreadState>();
        var bts = obj.AddComponent<BlackThreader.CustomBlackThreadState>();

        bts.customAttack = old.attacks[0];

        bts.extraSpriteRenderers = obj.GetComponentsInChildren<SpriteRenderer>(true);
        bts.extraMeshRenderers = obj.GetComponentsInChildren<MeshRenderer>(true);

        var hm = obj.GetComponent<HealthManager>();

        hm.blackThreadState = bts;
        hm.hasBlackThreadState = true;

        Object.Destroy(old);
    }

    public static void FixLostGarmond(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Control");
        
        var range = obj.transform.Find("Stomp Range").GetComponent<AlertRange>();
        fsm.GetState("Out of Start Range").AddAction(() =>
        {
            if (range.IsHeroInRange()) fsm.SendEvent("ENTER");
        }, everyFrame: true);
        
        fsm.GetState("Roar Antic").DisableAction(1);
        ((StartRoarEmitter)fsm.GetState("Intro Roar").actions[4]).stunHero = false;
        var sting = fsm.GetState("Sting");
        sting.DisableAction(0);
        sting.DisableAction(1);
        var roarEnd = fsm.GetState("Roar End");
        roarEnd.DisableAction(1);
        roarEnd.DisableAction(2);
        
        var ede = obj.GetComponent<EnemyDeathEffects>();
        ede.PreInstantiate();
        var corpse = ede.GetInstantiatedCorpse(AttackTypes.Generic);
        corpse.RemoveComponent<ConstrainPosition>();
        
        var blow = corpse.LocateMyFSM("Control").GetState("Blow");
        blow.transitions = [];
        blow.AddAction(() =>
        {
            corpse.SetActive(false);
        });
        
    }

    public static void FixServitorBoran(GameObject obj)
    {
        RemoveConstrainPosition(obj);

        obj.transform.Find("Legs Container").GetChild(0).GetChild(0).gameObject.AddComponent<PlaceableObject.SpriteSource>();
    }

    public static void FixServitorIgnim(GameObject obj)
    {
        RemoveConstrainPosition(obj);
        
        obj.transform.SetRotation2D(0);
        obj.transform.SetPositionZ(0.006f);
    }

    public static void FixCogworkClapperAnim(GameObject obj)
    {
        KeepActiveRemoveConstrainPos(obj);
        obj.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        obj.transform.Find("Hero Solid").gameObject.SetActive(false);
        obj.GetComponent<HealthManager>().invincible = false;
        var anim = obj.GetComponent<tk2dSpriteAnimator>();
        anim.defaultClipId = anim.GetClipIdByName("Idle");
    }

    public static void FixCogworkClapper(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Control");
        fsm.GetState("Sleep").AddAction(() =>
        {
            fsm.SetState("Idle");
            obj.GetComponent<DamageHero>().enabled = true;
        }, 0);
    }

    public static void FixGiantFlea(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Control");
        var init = fsm.GetState("Init");
        init.DisableAction(4);

        ((StartRoarEmitter)fsm.GetState("Roar").actions[5]).stunHero = false;
    }

    public static void FixPharlidDiver(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Behaviour");
        fsm.FsmVariables.FindFsmGameObject("Home").Value = new GameObject(obj.name + " Home");
    }

    public static void FixShardillard(GameObject obj)
    {
        KeepActive(obj);
        obj.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        obj.GetComponent<DamageHero>().damageDealt = 1;
        
        var fsm = obj.LocateMyFSM("Control");
        fsm.GetState("Init").AddAction(() =>
        {
            fsm.SetState("Recover");
        }, 3);
    }

    public static void FixWingedFurm(GameObject obj)
    {
        var init = obj.LocateMyFSM("Tween").GetState("Init");
        init.DisableAction(0);
    }

    public static void FixFurm(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Control");
                
        var digPos = new GameObject(obj.name + " Dig Pos")
        {
            transform = { position = obj.transform.position }
        };
        fsm.FsmVariables.FindFsmGameObject("Dig Min X Obj").Value = digPos;
        fsm.FsmVariables.FindFsmGameObject("Dig Max X Obj").Value = digPos;
                
        fsm.GetState("Init").AddAction(() => fsm.SendEvent("FINISHED"));
                
        fsm.GetState("Positioning Check").DisableAction(3);
                
        var set1 = fsm.GetState("Set To Ground");
        set1.DisableAction(4);
        set1.DisableAction(5);

        var set2 = fsm.GetState("Set To Wall L");
        set2.DisableAction(5);
        set2.DisableAction(6);
                
        var set3 = fsm.GetState("Set To Wall R");
        set3.DisableAction(5);
        set3.DisableAction(6);
                
        var set4 = fsm.GetState("Set To Roof");
        set4.DisableAction(5);
        set4.DisableAction(6);
        
        fsm.GetState("Surface Dig Prepare").AddAction(() => fsm.SendEvent("START"), 0);
        fsm.GetState("Surface Dig").AddAction(() => fsm.SendEvent("EMERGE"), 0);
    }

    public static void FixPatroller(GameObject obj)
    {
        obj.AddComponent<PatrollerFix>();
    }
    
    public class PatrollerFix : MonoBehaviour
    {
        public float xOffset;
        public float yOffset;
        
        private void Awake()
        {
            var fsm = GetComponent<PlayMakerFSM>();

            var startPoint = fsm.FsmVariables.FindFsmGameObject("Start Point") 
                             ?? fsm.FsmVariables.FindFsmGameObject("Patrol Start Point");

            startPoint.Value = new GameObject(name + " Start Point")
            {
                transform = { position = transform.position }
            };

            fsm.FsmVariables.FindFsmGameObject("Patrol Point").Value = new GameObject(name + " Patrol Point")
            {
                transform = { position = transform.position + new Vector3(xOffset, yOffset) }
            };
        }
    }

    public static void FixCogworkSpine(GameObject obj)
    {
        FixPatroller(obj);
        var anim = obj.GetComponent<tk2dSpriteAnimator>();
        anim.defaultClipId = anim.GetClipIdByName("Fly");
    }

    public static void FixFlintbeetlePreload(GameObject obj)
    {
        KeepActiveRemoveConstrainPos(obj);
        Object.Destroy(obj.transform.Find("CamLock").gameObject);
    }

    public static void FixFlintbeetle(GameObject obj)
    {
        obj.AddComponent<Flintbeetle>();
    }

    public abstract class Wakeable : MonoBehaviour
    {
        public abstract void DoWake();
    }

    public class Mossgrub : Wakeable
    {
        public override void DoWake() 
        {
            gameObject.LocateMyFSM("Noise Reaction").SendEvent("WAKE");
        }
    }

    public class SpearSkarr : Wakeable
    {
        public override void DoWake() 
        {
            var fsm = gameObject.LocateMyFSM("Control");
            fsm.GetState("Init").AddAction(() => fsm.SendEvent("FINISHED"), 0);
            fsm.SendEvent("WAKE");
        }
    }

    public class Judge : Wakeable
    {
        public override void DoWake() 
        {
            var fsm = gameObject.LocateMyFSM("Control");
            fsm.GetState("Init").AddAction(() => fsm.SendEvent("PATROL"), 2);
            fsm.SendEvent("WAKE");
        }
    }

    public class Driznit : Wakeable
    {
        public override void DoWake() 
        {
            gameObject.LocateMyFSM("Control").SendEvent("ALERT");
        }
    }

    public class Flintbeetle : Wakeable
    {
        public override void DoWake() 
        {
            var fsm = gameObject.LocateMyFSM("Control");
            fsm.GetState("Wall").AddAction(() => fsm.SendEvent("WAKE"));
        }
    }

    public static void FixMossgrub(GameObject obj)
    {
        obj.AddComponent<Mossgrub>();
    }

    public static void FixJudge(GameObject obj)
    {
        obj.AddComponent<Judge>();
    }

    public static void FixSpearSkarr(GameObject obj)
    {
        obj.AddComponent<SpearSkarr>();
    }

    public static void FixCrawJurorPreload(GameObject obj)
    {
        RemoveConstrainPosition(obj);
        
        obj.transform.SetPositionZ(0.006f);
        
        var anim = obj.GetComponent<tk2dSpriteAnimator>();
        anim.defaultClipId = anim.GetClipIdByName("Idle");
    }

    public static void FixTinyCrawJuror(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Behaviour");
        fsm.GetState("Init").AddAction(() => fsm.SendEvent("FG"), 14);
    }

    public static void FixCrawJuror(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Control");
        fsm.FsmVariables.FindFsmBool("Spawner").Value = false;
        fsm.FsmVariables.FindFsmBool("z_Summon").Value = false;
    }

    public static void FixUnderworksArenaEnemy(GameObject obj)
    {
        var anim = obj.GetComponent<tk2dSpriteAnimator>();
        anim.defaultClipId = anim.GetClipIdByName("Idle");

        obj.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        obj.GetComponent<MeshRenderer>().enabled = true;
        obj.layer = EnemiesLayer;
    }

    public static void FixDriznit(GameObject obj)
    {
        var anim = obj.GetComponent<tk2dSpriteAnimator>();
        anim.defaultClipId = anim.GetClipIdByName("Ceiling Hang");
        obj.AddComponent<Driznit>();
    }

    private static Material _gloomCorpseMaterial;
    
    public static void FixGargantGloomPreload(GameObject obj)
    {
        var psr = obj.transform.Find("Pt Spit").GetChild(0).GetComponent<ParticleSystemRenderer>();
        psr.material = psr.material;
        
        var ede = obj.GetComponent<EnemyDeathEffects>();
        ede.PreInstantiate();
        var corpse = ede.GetInstantiatedCorpse(AttackTypes.Generic);
        _gloomCorpseMaterial = corpse.transform.GetChild(0).GetChild(0).GetChild(1)
            .GetComponent<ParticleSystemRenderer>().material;
    }

    public static void FixGargantGloom(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Control");
        fsm.FsmVariables.FindFsmGameObject("Home").Value = new GameObject(obj.name + " Home");
        
        fsm.GetState("Start L").DisableAction(0);
        fsm.GetState("Start R").DisableAction(0);

        fsm.GetState("Dormant").AddAction(() => fsm.SendEvent("WAKE"));
        
        var ede = obj.GetComponent<EnemyDeathEffects>();
        ede.PreInstantiate();
        var corpse = ede.GetInstantiatedCorpse(AttackTypes.Generic);
        corpse.transform.GetChild(0).GetChild(0).GetChild(1)
            .GetComponent<ParticleSystemRenderer>().material = _gloomCorpseMaterial;
    }

    public static void FixGloomsac(GameObject obj)
    {
        obj.LocateMyFSM("Control").GetState("Capture").AddAction(() =>
        {
            obj.BroadcastEvent("OnDeath");
        });
    }

    public static void FixSeth(GameObject obj)
    {
        KeepActiveRemoveConstrainPos(obj);
        
        var fsm = obj.LocateMyFSM("Control");
        
        var idle = fsm.GetState("Idle");
        idle.transitions = idle.transitions
            .Where(trans => trans.EventName != "HORNET DEAD").ToArray();
        var attackChoice = fsm.GetState("Attack Choice");
        attackChoice.transitions = attackChoice.transitions
            .Where(trans => trans.EventName != "HORNET DEAD").ToArray();

        var trail = Object.Instantiate(_shieldTrail);
        trail.SetActive(true);
        fsm.FsmVariables.FindFsmGameObject("Pt Shield Trail").Value = trail;
        
        fsm.FsmVariables.FindFsmFloat("Tele Min X").Value = obj.transform.GetPositionX() - 50;
        fsm.FsmVariables.FindFsmFloat("Tele Max X").Value = obj.transform.GetPositionX() + 50;
        fsm.FsmVariables.FindFsmFloat("Jump X").Value = obj.transform.GetPositionX();
        fsm.FsmVariables.FindFsmFloat("Ground Y").Value = obj.transform.GetPositionY() + 0.2f;
        fsm.FsmVariables.FindFsmFloat("Air Y").Value = obj.transform.GetPositionY() + 6.1f;
        
        var shield = obj.transform.Find("Shield Projectile").gameObject;
    }
}