using System;
using System.Collections.Generic;
using System.Linq;
using Architect.Behaviour.Custom;
using Architect.Behaviour.Utility;
using Architect.Content.Preloads;
using Architect.Objects.Placeable;
using Architect.Utils;
using GlobalEnums;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using JetBrains.Annotations;
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
    
    // Trobbio
    private static GameObject _flares;
    private static GameObject _floor;
    private static GameObject _bursts;
    
    // Tormented Trobbio
    private static GameObject _tflares;
    private static GameObject _tfloor;
    private static GameObject _tbursts;
    
    // Khann
    private static readonly List<string> Spears = [
        "Long Spear",
        "Air Spear",
        "Uppercut Spear",
        "Roar Spikes",
        "Cross Spears",
        "Cross Followup Spears",
        "Shoot Spikes"
    ];
    private static readonly Dictionary<string, GameObject> SpearObjects = [];
    
    // Karmelita
    private static GameObject _grindSpikesL;
    private static GameObject _grindSpikesR;
    
    // Fourth Chorus
    private static GameObject _lavaPlats;
    private static GameObject _lavaRocks;

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
                if (self.fsmComponent.GetComponentInParent<DisableBossTitle>()) self.bossTitle.value = "";
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
        PreloadManager.RegisterPreload(new BasicPreload("Library_13", 
            "Grand Stage Scene/Boss Scene Trobbio/Flare Glitter",
            o => _flares = o));
        PreloadManager.RegisterPreload(new BasicPreload("Library_13", 
            "Grand Stage Scene/Boss Scene Trobbio/Floor Tiles",
            o => _floor = o));
        PreloadManager.RegisterPreload(new BasicPreload("Library_13", 
            "Grand Stage Scene/Boss Scene Trobbio/Trapdoor Bursts",
            o => _bursts = o));
        
        PreloadManager.RegisterPreload(new BasicPreload("Library_13", 
            "Grand Stage Scene/Boss Scene TormentedTrobbio/Flare Glitter",
            o => _tflares = o));
        PreloadManager.RegisterPreload(new BasicPreload("Library_13", 
            "Grand Stage Scene/Boss Scene TormentedTrobbio/Floor Tiles",
            o => _tfloor = o));
        PreloadManager.RegisterPreload(new BasicPreload("Library_13", 
            "Grand Stage Scene/Boss Scene TormentedTrobbio/Trapdoor Bursts",
            o => _tbursts = o));

        foreach (var spear in Spears)
        {
            PreloadManager.RegisterPreload(new BasicPreload("Memory_Coral_Tower", $"Boss Scene/{spear}",
                o => SpearObjects[spear] = o));
        }
        
        PreloadManager.RegisterPreload(new BasicPreload("Memory_Ant_Queen", 
            "Boss Scene/Grind Spikes R",
            o =>
            {
                o.RemoveComponentsInChildren<CheckOutOfBoundsX>();
                _grindSpikesR = o;
            }));
        
        PreloadManager.RegisterPreload(new BasicPreload("Memory_Ant_Queen", 
            "Boss Scene/Grind Spikes L",
            o =>
            {
                o.RemoveComponentsInChildren<CheckOutOfBoundsX>();
                _grindSpikesL = o;
            }));
        
        PreloadManager.RegisterPreload(new BasicPreload("Bone_East_08", 
            "Boss Scene/Lava Plats",
            o => _lavaPlats = o));
        
        PreloadManager.RegisterPreload(new BasicPreload("Bone_East_08", 
            "Boss Scene/Lava Rocks",
            o =>
            {
                KeepActive(o);
                _lavaRocks = o;
            }));

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
        corpseFsm.GetState("Death Start").DisableAction(3);
        
        corpseFsm.GetState("Break 4").AddAction(() => fsm.SendEvent("CANCEL"), 5);

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
        obj.AddComponent<Teleplane>();
    }

    public class Teleplane : MonoBehaviour
    {
        public float width = 5;
        public float height = 1;

        private void Start()
        {
            var fsm = gameObject.GetComponent<PlayMakerFSM>();
            var teleplane = new GameObject(name + " Teleplane")
            {
                transform = { position = transform.position },
                tag = "Teleplane"
            };
            var col = teleplane.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(width, height);

            var st = fsm.GetState("Get Teleplane");
            if (st != null) ((FindClosest)st.actions[0]).name = teleplane.name;

            fsm.FsmVariables.FindFsmGameObject("Teleplane").Value = teleplane;
        }
    }

    public static void FixDregwheel(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Control");
        
        fsm.GetState("Dormant").AddAction(() => fsm.SendEvent("SPAWN M"), 0);
        ((SetFloatValue)fsm.GetState("Spawn Mid").actions[0]).floatValue.value = obj.transform.position.x;
        fsm.FsmVariables.FindFsmFloat("Ground Y").value = obj.transform.position.y;

        var clamp = fsm.GetState("Spawn Roll Clamp");
        clamp?.DisableAction(2);

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
        obj.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
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
        
        fsm.GetState("Do Roar?").AddAction(() => fsm.SendEvent("SKIP"), 0);
        fsm.GetState("Quick Roar").DisableAction(5);
        var qre = fsm.GetState("Quick Roar End");
        qre.DisableAction(1);
        qre.DisableAction(2);
        qre.DisableAction(3);

        var ede = obj.GetComponent<EnemyDeathEffects>();
        ede.setPlayerDataBool = "";
        
        ede.PreInstantiate();
        var corpseFsm = ede.GetInstantiatedCorpse(AttackTypes.Generic).LocateMyFSM("Death");
        
        corpseFsm.GetState("Stagger").DisableAction(2);
        corpseFsm.GetState("Blow").DisableAction(1);
        corpseFsm.GetState("Splash").DisableAction(0);
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

        if (!HeroController.instance.TryFindGroundPoint(out var pos,
                obj.transform.position,
                true)) pos = obj.transform.position;
        
        fsm.GetState("Get Node").AddAction(() => obj.transform.position = pos, 0);
        fsm.GetState("Dig Out G 1").AddAction(() => obj.BroadcastEvent("OnAmbush"), 0);
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
        
        if (!HeroController.instance.TryFindGroundPoint(out var pos,
                obj.transform.position,
                true)) pos = obj.transform.position;
        
        fsm.GetState("Get Node").AddAction(() => obj.transform.position = pos, 0);
        fsm.GetState("Dig Out G 1").AddAction(() => obj.BroadcastEvent("OnAmbush"), 0);
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
        
        // Disable death bool
        fsm.GetState("End").DisableAction(4);
        
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

        // Floor stomp
        fsm.GetState("Stomp").DisableAction(9);

        var floorSlam = fsm.GetState("Floor Slam");
        floorSlam.DisableAction(0);
        floorSlam.AddAction(() => obj.BroadcastEvent("FloorSlam"), 0);

        fsm.GetState("Wall Slam").AddAction(() => obj.BroadcastEvent("WallSlam"), 0);
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

    private static readonly int HeroDetector = LayerMask.NameToLayer("Hero Detector");
    public static void FixWatcher(GameObject obj)
    {
        obj.AddComponent<Watcher>();
        
        var fsm = obj.LocateMyFSM("Control");
        
        fsm.GetState("Start State").AddAction(() => fsm.SendEvent("SLEEP"), 0);
        fsm.GetState("Away").DisableAction(2);
        
        fsm.GetState("Die").DisableAction(0);

        ((StartRoarEmitter)fsm.GetState("Wake Roar 2").actions[4]).stunHero = false;

        fsm.FsmVariables.FindFsmFloat("Dig Min X").Value = obj.transform.GetPositionX() - 12.5f;
        fsm.FsmVariables.FindFsmFloat("Dig Max X").Value = obj.transform.GetPositionX() + 12.5f;
        fsm.FsmVariables.FindFsmFloat("Start Dig X Max").Value = 99999;

        fsm.FsmVariables.FindFsmFloat("Cliff Y").value = float.MaxValue;

        var br = new GameObject("Battle Range")
        {
            transform =
            {
                parent = obj.transform,
                localPosition = Vector3.zero
            },
            layer = HeroDetector
        };
        var bc = br.AddComponent<BoxCollider2D>();
        bc.isTrigger = true;
        bc.size = new Vector2(50, 16);
        var alertRange = br.AddComponent<AlertRange>();
        fsm.FsmVariables.FindFsmObject("Battle Range").Value = alertRange;

        fsm.GetState("Uppercut Launch").DisableAction(4);
        
        var idle = fsm.GetState("Idle");
        idle.transitions = idle.transitions
            .Where(trans => trans.EventName != "HORNET DEAD").ToArray();
        
        var rc = fsm.GetState("Range Check");
        rc.transitions = rc.transitions
            .Where(trans => trans.EventName != "HORNET DEAD").ToArray();

        obj.LocateMyFSM("Battle Music").enabled = false;
    }

    public class Watcher : Wakeable
    {
        public override void DoWake()
        {
            var fsm = gameObject.LocateMyFSM("Control");
            fsm.GetState("Sleep").AddAction(() => fsm.SendEvent("WAKE"), 0);
            fsm.SendEvent("WAKE");
        }
    }

    public static void FixZango(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Control");
        fsm.GetState("Init").DisableAction(3);
        
        fsm.GetState("Extract Kill").AddAction(() => obj.BroadcastEvent("OnDeath"), 0);

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
        var anim = obj.GetComponent<tk2dSpriteAnimator>();
        anim.defaultClipId = anim.GetClipIdByName("Roar");
        obj.transform.SetPositionZ(0.006f);
    }

    public static void FixKarmelita(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Control");

        var spikesL = Object.Instantiate(_grindSpikesL);
        spikesL.name = obj.name + " Spikes L";
        var spikesR = Object.Instantiate(_grindSpikesR);
        spikesR.name = obj.name + " Spikes R";

        fsm.FsmVariables.FindFsmGameObject("Grind Spikes L").Value = spikesL;
        fsm.FsmVariables.FindFsmGameObject("Grind Spikes R").Value = spikesR;
        
        var btd = obj.AddComponent<BlackThreader.BlackThreadData>();
        btd.SingCheck = () => fsm.ActiveStateName.Contains("Movement");
        btd.OnBlackThread = () =>
        {
            foreach (var sr in spikesL.GetComponentsInChildren<DamageHero>(true))
            {
                sr.damagePropertyFlags |= DamagePropertyFlags.Void;
            }
            foreach (var sr in spikesR.GetComponentsInChildren<DamageHero>(true))
            {
                sr.damagePropertyFlags |= DamagePropertyFlags.Void;
            }
        };
        
        fsm.fsm.startState = "Roar Antic";

        var hm = obj.GetComponent<HealthManager>();
        fsm.GetState("Roar Antic").AddAction(() =>
        {
            fsm.FsmVariables.FindFsmInt("P2 HP").Value = (int)(hm.hp * 0.65f);
            fsm.FsmVariables.FindFsmInt("P3 HP").Value = (int)(hm.hp * 0.35f);
        }, 0);

        obj.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;

        ((StartRoarEmitter)fsm.GetState("Roar").actions[0]).stunHero = false;
        fsm.GetState("Battle Start").DisableAction(0);
        
        var attackChoice = fsm.GetState("Attack Choice");
        attackChoice.transitions = attackChoice.transitions
            .Where(trans => trans.EventName != "HORNET DEAD").ToArray();
        
        fsm.GetState("Spin Aim").DisableAction(5);
        fsm.FsmVariables.FindFsmGameObject("Arena Centre")
            .Value = new GameObject(obj.name + " Arena Centre") 
            { transform = { position = obj.transform.position } };
        
        // Death
        var ede = obj.GetComponent<EnemyDeathEffects>();
        ede.PreInstantiate();
        var corpse = ede.GetInstantiatedCorpse(AttackTypes.Generic);
        RemoveConstrainPosition(corpse);
        
        var deathFsm = corpse.LocateMyFSM("Death");
        var stagger = deathFsm.GetState("Stagger");
        stagger.DisableAction(8);
        stagger.DisableAction(9);
        stagger.transitions[0].toState = "Steam";
        stagger.transitions[0].toFsmState = deathFsm.GetState("Steam");

        var blow = deathFsm.GetState("Blow");
        blow.DisableAction(2);
        blow.transitions = [];
        blow.AddAction(() => Object.Destroy(corpse));

        // Y Positions
        var landY = fsm.FsmVariables.FindFsmFloat("Land Y");
        var otherLandY = ((CheckYPosition)fsm.GetState("Wall Dive").actions[6]).compareTo;
        var spearSlam = fsm.GetState("Spear Slam");
        var spearSlamY = ((SetPosition)spearSlam.actions[7]).y;

        var as1 = fsm.GetState("Air Sickles");
        var as2 = fsm.GetState("Air Sickles 2");
        var airSickles1Y = ((SetPosition2d)as1.actions[1]).y;
        var airSickles2Y = ((SetPosition2d)as2.actions[1]).y;
        
        as1.AddAction(FixSickles, 0);
        as2.AddAction(FixSickles, 0);
        
        fsm.GetState("Spin Attack").AddAction(AdjustPositions, 0, true);
        fsm.GetState("Throw Fall").AddAction(AdjustPositions, 0, true);
        fsm.GetState("Wall Dive").AddAction(AdjustPositions, 0, true);
        
        // Fix stuck stunned issue
        var stunAir = fsm.GetState("Stun Air");
        var time = 0f;
        stunAir.AddAction(() => time = Time.time);
        stunAir.AddAction(() =>
        {
            if (Time.time - time > 5) fsm.SendEvent("LAND");
        }, everyFrame: true);

        // Spear Slam
        var spearSet = fsm.FsmVariables.FindFsmGameObject("Spear Set");
        spearSlam.AddAction(() => spearSet.Value.RemoveComponentsInChildren<CheckOutOfBoundsX>(), 7);
        
        // Dash Grind
        fsm.GetState("Dash Grind").AddAction(FixSpikes, 0);

        var restartSinging = fsm.GetState("Restart Singing");
        var stunStart = fsm.GetState("Stun Start");
        restartSinging.DisableAction(0);
        stunStart.DisableAction(13);
        
        stunStart.AddAction(() => obj.BroadcastEvent("OnStun"), 0);
        restartSinging.AddAction(() => obj.BroadcastEvent("OnRecover"), 0);

        AdjustPositions();
        return;

        void AdjustPositions()
        {
            var ground = HeroController.instance.FindGroundPointY(
                obj.transform.position.x,
                obj.transform.position.y + 0.5f,
                true) + 1.57f;

            landY.Value = ground;
            otherLandY.Value = ground + 0.27f;
            spearSlamY.Value = ground - 2.23f;
        }

        void FixSickles()
        {
            airSickles1Y.Value = obj.transform.GetPositionY() - 3.92f;
            airSickles2Y.Value = obj.transform.GetPositionY() - 3.92f;
        }

        void FixSpikes()
        {
            spikesL.transform.SetPositionY(landY.Value - 21.13f);
            spikesR.transform.SetPositionY(landY.Value - 21.13f);
            spikesL.transform.SetPositionX(obj.transform.GetPositionX()-71);
            spikesR.transform.SetPositionX(obj.transform.GetPositionX()+71);
        }
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
        obj.AddComponent<Teleplane>();
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
        
        fsm.GetState("Dash To").DisableAction(0);

        var rf = (RandomFloat)fsm.GetState("Set Dash Pos").actions[1];
        
        fsm.GetState("Set Dash Pos").AddAction(() =>
        {
            rf.min = HeroController.instance.transform.GetPositionY() - 2.5f;
            rf.max = HeroController.instance.transform.GetPositionY() + 2.5f;
        }, 0);
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

    private class FixedMovement : MonoBehaviour
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

        var entry = fsm.GetState("Entry Antic");
        entry.DisableAction(2);
        ((Wait)entry.actions[3]).time = 0.01f;
        
        fsm.GetState("Stun Start").DisableAction(3);
        
        var checkL = fsm.GetState("Check L");
        checkL.DisableAction(0);
        checkL.DisableAction(1);
        checkL.DisableAction(4);
        fsm.GetState("Check M").DisableAction(2);
        fsm.GetState("Check R").DisableAction(2);
        fsm.GetState("Dive End").DisableAction(3);
        
        fsm.GetState("Swipe 5").AddAction(() => obj.BroadcastEvent("OnLadleSlam"), 0);
        fsm.GetState("Stomp Land").AddAction(() => obj.BroadcastEvent("OnStompLand"), 0);
        fsm.GetState("Butt Land").AddAction(() => obj.BroadcastEvent("OnButtLand"), 0);
        
        var pickPoint = fsm.GetState("Pick Point");
        pickPoint.DisableAction(0);
        pickPoint.DisableAction(1);
        pickPoint.DisableAction(3);
        var targetX = fsm.FsmVariables.FindFsmFloat("Target X");
        pickPoint.AddAction(() =>
        {
            targetX.Value = obj.transform.GetPositionX() + 
                            (Random.value > 0.5f ? 1 : -1) * Random.Range(6, 14);
        }, 3);

        var idleY = fsm.FsmVariables.FindFsmFloat("Idle Y");
        var swipeY = fsm.FsmVariables.FindFsmFloat("Swipe Y");
        var targetY = fsm.FsmVariables.FindFsmFloat("Target Y");
        
        fsm.GetState("Idle").AddAction(FixYValues, 0);
        FixYValues();

        var ede = obj.GetComponent<EnemyDeathEffects>();
        ede.setPlayerDataBool = "";
        ede.PreInstantiate();
        var corpse = ede.GetInstantiatedCorpse(AttackTypes.Generic);
        RemoveConstrainPosition(corpse);
        var corpseFsm = corpse.LocateMyFSM("Death");
        var stagger = corpseFsm.GetState("Stagger");
        stagger.DisableAction(6);
        stagger.DisableAction(7);

        return;

        void FixYValues()
        {
            if (HeroController.instance.TryFindGroundPoint(out var pos,
                    obj.transform.position,
                    true))
            {
                var y = pos.y;
                idleY.Value = y + 2.87f;
                swipeY.Value = y + 1.39f;
                targetY.Value = y - 2;
            }
        }
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
                PlaySound(Clips[Random.RandomRangeInt(0, 4)], 5);
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

    public static void FinishSplinterFix(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Behaviour Base");
        fsm.fsmTemplate = null;
        fsm.GetState("Initial Position").AddAction(() => fsm.SendEvent("FINISHED"), 0);
        fsm.GetState("Reposition").DisableAction(3);
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

        var dh = fsm.GetState("Death Hit");
        dh.DisableAction(15);
        dh.DisableAction(16);
        ((CheckYPosition)fsm.GetState("Death Fly").actions[10]).compareTo.Value = obj.transform.position.y - 1;

        ((StartRoarEmitter)fsm.GetState("Roar").actions[3]).stunHero = false;
        var re = fsm.GetState("Roar End");
        re.DisableAction(9);
        re.DisableAction(10);
        
        fsm.GetState("P2 Roar End").AddAction(() => obj.BroadcastEvent("TrySummon"), 0);
        fsm.GetState("P3 Roar End").AddAction(() => obj.BroadcastEvent("TrySummon"), 0);
        fsm.GetState("P4 Roar End").AddAction(() => obj.BroadcastEvent("TrySummon"), 0);
    }

    public static void FixGron(GameObject obj)
    {
        var fsm = FixForebrother(obj);
        fsm.GetState("Start Pause").DisableAction(0);
        ((CheckYPosition)fsm.GetState("Death Fly").actions[8]).compareTo.Value = obj.transform.position.y - 1;
        
        var init = fsm.GetState("Init");
        init.transitions = [];
        init.AddAction(() => fsm.SetState("Entry Fall"), 0);

        var cbts = (ConvertBoolToString)fsm.GetState("Shout").actions[5];
        cbts.trueString = cbts.falseString;
    }

    private static readonly PhysicsMaterial2D PinMaterial = new()
    {
        friction = 1
    };

    public static void FixPhantom(GameObject obj)
    {
        RemoveConstrainPosition(obj);
        var fsm = obj.LocateMyFSM("Control");
        
        var pin = fsm.FsmVariables.FindFsmGameObject("Pin Projectile").Value;
        var pc = pin.GetComponent<Collider2D>();
        pc.isTrigger = false;
        pc.sharedMaterial = PinMaterial;
        
        var prb2d = pin.GetComponent<Rigidbody2D>();
        prb2d.bodyType = RigidbodyType2D.Dynamic;
        prb2d.gravityScale = 0;

        fsm.GetState("Dormant").AddAction(() => fsm.SendEvent("PHANTOM START"));
        fsm.GetState("Appear").AddAction(() => fsm.SendEvent("FINISHED"));
        fsm.GetState("Start Pause").AddAction(() => fsm.SendEvent("FINISHED"));

        var leftX = fsm.FsmVariables.FindFsmFloat("Left X");
        var rightX = fsm.FsmVariables.FindFsmFloat("Right X");

        var aThrow = (RandomFloat)fsm.GetState("Set A Throw").actions[2];
        var aThrowMin = aThrow.min;
        var aThrowMax = aThrow.max;

        var fogin2Y = ((SetPosition)fsm.GetState("Fog In 2").actions[0]).y;

        var aThrowRequirement = ((FloatTestToBool)fsm.GetState("In Air").actions[7]).float2;

        var pullY = ((FloatCompare)fsm.GetState("A Dash").actions[12]).float2;
        
        var dragoonY = ((FloatCompare)fsm.GetState("Dragoon Launch").actions[8]).float2;

        fsm.GetState("To Idle").AddAction(() =>
        {
            foreach (var col in obj.GetComponentsInChildren<Collider2D>(true))
            {
                Physics2D.IgnoreCollision(pc, col);
            }
        }, 0);
        
        // Adjust positions
        fsm.GetState("Idle").AddAction(FixPositions, 0);
        fsm.GetState("Range Check").AddAction(FixPositions, 0);

        var posState = fsm.GetState("Pos");
        posState.DisableAction(3);

        var gt = fsm.GetState("G Throw");
        gt.DisableAction(15);
        gt.DisableAction(16);
        var gpw = fsm.GetState("G Pull Wait");
        gpw.DisableAction(3);
        gpw.DisableAction(4);
        
        gt.AddAction(Thunk, everyFrame: true);
        gpw.AddAction(Thunk, everyFrame: true);

        var at = fsm.GetState("A Throw");
        at.DisableAction(13);
        at.DisableAction(14);
        at.DisableAction(15);
        ((SetVelocityAsAngle)at.actions[9]).everyFrame = false;
        ((FaceAngle)at.actions[10]).everyFrame = false;

        var apw = fsm.GetState("A Pull Wait");
        apw.DisableAction(3);
        apw.DisableAction(4);
        apw.DisableAction(5);
        apw.AddAction(ThunkAir, everyFrame: true);

        var rb2d = obj.GetComponent<Rigidbody2D>();
        fsm.GetState("Dragoon Down").AddAction(ClearVelocity, 0, true);
        fsm.GetState("Fog In").AddAction(ClearVelocity, 0);
        
        fsm.GetState("Dragoon Blast").AddAction(() => obj.BroadcastEvent("Slam"), 0);

        var dEnd = fsm.GetState("Dragoon End");
        dEnd.AddAction(ClearVelocity, 0);
        
        var hdc = fsm.GetState("Hero Death Check");
        hdc.transitions = hdc.transitions
            .Where(trans => trans.EventName != "HORNET DEAD").ToArray();
        var toIdle = fsm.GetState("To Idle");
        toIdle.transitions = toIdle.transitions
            .Where(trans => trans.EventName != "HORNET DEAD").ToArray();

        fsm.GetState("In Air").AddAction(() => rb2d.gravityScale = 2.5f, 0);
        toIdle.AddAction(() => rb2d.gravityScale = 2.5f, 0);

        var fp = fsm.GetState("Final Parry");
        for (var i = 0; i <= 18; i++) fp.DisableAction(i);
        fp.AddAction(() =>
        {
            obj.GetComponent<HealthManager>().SetDead();
            obj.GetComponent<SpriteFlash>().Flash(new Color(1f, 1f, 1f), 0.8f, 0, 
                0.12f, 0.25f);
            obj.layer = 2;
            rb2d.constraints = RigidbodyConstraints2D.FreezeAll;
            obj.GetComponent<tk2dSpriteAnimator>().Play("Death Stagger");
            fsm.SetState("Blood Stream");
            pin.SetActive(false);
            GameManager.instance.FreezeMoment(FreezeMomentTypes.BossDeathSlow);
        }, 0);
        var de = fsm.GetState("Death Explode");
        de.transitions = [];
        de.AddAction(() => Object.Destroy(obj));

        fsm.FsmGlobalTransitions.First(o => o.EventName == "FINAL BLOCK")
            .fsmEvent = FsmEvent.FindEvent("ZERO HP");
        
        return;

        void FixPositions()
        {
            var xPos = HeroController.instance.transform.GetPositionX();
            leftX.Value = xPos - 8.5f;
            rightX.Value = xPos + 8.5f;
            
            if (HeroController.instance.TryFindGroundPoint(out var pos,
                    obj.transform.position,
                    true))
            {
                aThrowMin.Value = pos.y + 6;
                aThrowMax.Value = pos.y + 8;
                fogin2Y.Value = pos.y + 8;
                dragoonY.Value = pos.y + 20.5f;
                pullY.Value = pos.y + 3;
                aThrowRequirement.Value = pos.y + 6;
            }
        }

        void ClearVelocity()
        {
            rb2d.gravityScale = 0;
            rb2d.bodyType = RigidbodyType2D.Dynamic;
            rb2d.linearVelocityY = 0;
        }

        void Thunk()
        {
            if (prb2d.linearVelocityX == 0) fsm.SendEvent("FINISHED");
            prb2d.linearVelocityY = 0;
        }

        void ThunkAir()
        {
            if (prb2d.linearVelocityX == 0 || prb2d.linearVelocityY == 0)
            {
                fsm.SendEvent("FINISHED");
                prb2d.linearVelocity = Vector2.zero;
            }
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

        bts.useCustomHPMultiplier = true;
        bts.customHPMultiplier = 1;

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
        blow.DisableAction(5);
        blow.transitions = [];
        blow.AddAction(() =>
        {
            corpse.SetActive(false);
        });
        
    }

    public static void FixServitorBoran(GameObject obj)
    {
        RemoveConstrainPosition(obj);

        var head = obj.transform.Find("Legs Container").GetChild(0).GetChild(0);
        head.gameObject.AddComponent<PlaceableObject.SpriteSource>();
        head.GetChild(0).Find("Roll Collider").gameObject.layer = LayerMask.NameToLayer("Enemies");

        obj.AddComponent<Boran>();
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
        fsm.GetState("Roll Check").DisableAction(0);
        fsm.GetState("Floor").DisableAction(8);
    }

    public static void FixGiantFlea(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Control");
        var init = fsm.GetState("Init");
        init.DisableAction(4);

        ((StartRoarEmitter)fsm.GetState("Roar").actions[5]).stunHero = false;
        fsm.GetState("Revisit").DisableAction(1);
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
        fsm.GetState("Charge").DisableAction(12);
        fsm.GetState("Terrain Effects").AddAction(() => obj.BroadcastEvent("OnBounce"), 0);
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

        var fsm = obj.LocateMyFSM("Control");
        
        var emerge = fsm.GetState("Emerge");
        emerge.DisableAction(13);
        emerge.DisableAction(14);
        var death = fsm.GetState("Death");
        death.DisableAction(1);
        death.DisableAction(2);
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

    public class Boran : Wakeable
    {
        public override void DoWake() 
        {
            var fsm = gameObject.LocateMyFSM("Control");
            fsm.GetState("Rest").AddAction(() => fsm.SendEvent("WAKE"), 0);
            fsm.SendEvent("WAKE");
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

        obj.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        obj.layer = EnemiesLayer;
    }

    public static void FixTallcrawJuror(GameObject obj)
    {
        obj.GetComponent<MeshRenderer>().enabled = true;
        obj.GetComponent<tk2dSprite>().color = Color.white;
        
        var fsm = obj.LocateMyFSM("Control");
        fsm.GetState("Roost L").DisableAction(0);
        fsm.GetState("Roost R").DisableAction(0);
        fsm.FsmVariables.FindFsmBool("Spawner").Value = false;
        fsm.FsmVariables.FindFsmBool("z_Summon").Value = false;
        
        obj.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        obj.GetComponent<tk2dSpriteAnimator>().Play("Idle");
    }

    public static void FixTinyCrawJuror(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Behaviour");
        
        var setFg = fsm.GetState("Set FG");
        setFg.DisableAction(1);
        setFg.DisableAction(2);
        
        fsm.GetState("Init").AddAction(() =>
        {
            obj.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
            obj.layer = EnemiesLayer;
            fsm.SendEvent("FG");
        }, 14);

        var flap = fsm.GetState("Flap");
        flap.DisableAction(8);
        ((GetPosition)flap.actions[7]).y = ((RandomFloat)flap.actions[8]).storeResult;

        obj.GetComponent<MeshRenderer>().enabled = true;
        obj.GetComponent<tk2dSprite>().color = Color.white;
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

    public static void FixGargantGloom(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Control");
        fsm.FsmVariables.FindFsmGameObject("Home").Value = new GameObject(obj.name + " Home");
        
        fsm.GetState("Start L").DisableAction(0);
        fsm.GetState("Start R").DisableAction(0);

        fsm.GetState("Dormant").AddAction(() => fsm.SendEvent("WAKE"));
    }

    public static void FixGloomsac(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Control");
        fsm.FsmVariables.FindFsmBool("Hornet Dead").Value = false;
        fsm.GetState("Capture").AddAction(() =>
        {
            obj.BroadcastEvent("OnDeath");
        });
    }

    public static void FixSeth(GameObject obj)
    {
        KeepActiveRemoveConstrainPos(obj);
        
        var fsm = obj.LocateMyFSM("Control");
        
        // Disable music
        fsm.GetState("Stun Start 2").DisableAction(5);
        var rre = fsm.GetState("Rage Roar End");
        rre.DisableAction(1);
        rre.DisableAction(2);
        
        var idle = fsm.GetState("Idle");
        idle.transitions = idle.transitions
            .Where(trans => trans.EventName != "HORNET DEAD").ToArray();
        var attackChoice = fsm.GetState("Attack Choice");
        attackChoice.transitions = attackChoice.transitions
            .Where(trans => trans.EventName != "HORNET DEAD").ToArray();

        var trail = Object.Instantiate(_shieldTrail);
        trail.SetActive(true);
        fsm.FsmVariables.FindFsmGameObject("Pt Shield Trail").Value = trail;

        var tmix = fsm.FsmVariables.FindFsmFloat("Tele Min X");
        var tmax = fsm.FsmVariables.FindFsmFloat("Tele Max X");
        fsm.FsmVariables.FindFsmFloat("Jump X").value = obj.transform.GetPositionX();
        var gy = fsm.FsmVariables.FindFsmFloat("Ground Y");
        var ay = fsm.FsmVariables.FindFsmFloat("Air Y");

        var shield = obj.transform.Find("Shield Projectile");
        var shieldFsm = shield.gameObject.LocateMyFSM("Control");
        var fly = shieldFsm.GetState("Fly");
        for (var i = 2; i <= 8; i++) fly.DisableAction(i);
        fly.AddAction(() =>
        {
            var hit = Physics2D.Raycast(shield.position, Vector2.down, 4);
            if (hit) fsm.SendEvent("FLOOR");
        }, everyFrame: true);
        

        fsm.GetState("Attack Choice").AddAction(AdjustPositions, 0);
        AdjustPositions();
        return;

        void AdjustPositions()
        {
            var hx = HeroController.instance.transform.GetPositionX();
            tmix.Value = hx - 12.5f;
            tmax.Value = hx + 12.5f;

            if (HeroController.instance.TryFindGroundPoint(out var pos,
                    obj.transform.position,
                    true))
            {
                gy.Value = pos.y + 0.2f;
                ay.Value = pos.y + 6.1f;
            }
        }
    }

    public static void FixCorrcrustKaraka(GameObject obj)
    {
        FixSpearSpawned(obj);
        RemoveConstrainPosition(obj);

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
        }, 2);
        
        // Move with Rigidbody2D
        var rb2d = obj.GetComponent<Rigidbody2D>();
        rise.AddAction(() =>
        {
            runningTime += Time.deltaTime;

            var percentage = Mathf.Clamp01(runningTime * 2);
            
            rb2d.MovePosition(new Vector2(
                EaseInOutSine(fromX, targetX.Value, percentage),
                EaseOutCubic(fromY, targetY.Value, percentage))
            );
        }, 3, true);
    }

    private static float EaseOutCubic(float start, float end, float value)
    {
        --value;
        end -= start;
        return end * (float) (value * value * value + 1.0) + start;
    }

    private static float EaseInOutSine(float start, float end, float value)
    {
        end -= start;
        return (float)(-(double)end / 2.0 * (Mathf.Cos((float)(3.1415927410125732 * value / 1.0)) - 1.0)) + start;
    }

    public static void FixCrawfather(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Control");
        fsm.GetState("BG Idle").AddAction(() => fsm.SendEvent("BATTLE START"));
        fsm.GetState("Emerge Announce").AddAction(() => fsm.SendEvent("FINISHED"), 0);
        fsm.GetState("Emerge Antic").AddAction(() => fsm.SendEvent("FINISHED"), 0);
        fsm.GetState("Emerge").AddAction(() => fsm.SendEvent("FINISHED"), 0);

        ((StartRoarEmitter)fsm.GetState("Roar").actions[1]).stunHero = false;
        
        var flapY = fsm.FsmVariables.FindFsmFloat("Flap Y");
        fsm.GetState("Idle").AddAction(AdjustFlapY, 0);
        fsm.GetState("Range Check").AddAction(AdjustFlapY, 0);
        
        fsm.GetState("Call").AddAction(() => obj.BroadcastEvent("TrySummon"), 0);

        var rb2d = obj.GetComponent<Rigidbody2D>();
        var launch = fsm.GetState("Launch");

        // Initial positions
        var runningTime = 0f;
        var fromY = 0f;
        
        // Modify launch to use collision
        launch.DisableAction(3);
        launch.AddAction(() =>
        {
            runningTime = 0;
            fromY = obj.transform.GetPositionY();
        }, 3);
        launch.AddAction(() =>
        {
            runningTime += Time.deltaTime;
            var percentage = Mathf.Clamp01(runningTime * 2.22f);

            rb2d.MovePosition(new Vector2(
                obj.transform.GetPositionX(),
                EaseOutCubic(fromY, flapY.Value, percentage))
            );

            if (percentage >= 1) fsm.SendEvent("FINISHED");
        }, 4, true);
        
        var dive = fsm.GetState("Dive");
        var firstMoveTime = 0f;
        dive.AddAction(() =>
        {
            firstMoveTime = Time.time;
        });
        dive.AddAction(() =>
        {
            if (Time.time - firstMoveTime < 0.1f) return;
            if (Mathf.Abs(rb2d.linearVelocityY) < 0.05f) fsm.SendEvent("LAND");
        }, everyFrame: true);

        var ede = obj.GetComponent<EnemyDeathEffects>();
        ede.PreInstantiate();
        var corpseFsm = ede.GetInstantiatedCorpse(AttackTypes.Generic).LocateMyFSM("Death");
        corpseFsm.GetState("Stagger").DisableAction(5);
        
        AdjustFlapY();
        return;
        
        void AdjustFlapY() 
        {
            if (HeroController.instance.TryFindGroundPoint(out var pos,
                    obj.transform.position,
                    true))
            {
                flapY.Value = pos.y + 6.5f;
            }
        }
    }
    
    public static void FixKhann(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Control");

        var spears = new GameObject(obj.name + " Spears");
        GameObject longSpear = null;
        GameObject shootSpikes = null;
        foreach (var (spear, spearObj) in SpearObjects)
        {
            var so = Object.Instantiate(spearObj, spears.transform);
            fsm.FsmVariables.FindFsmGameObject(spear).Value = so;
            var spearFsm = so.LocateMyFSM("Control");
            if (spearFsm)
            {
                var init = spearFsm.GetState("Init");
                init?.AddAction(() =>
                {
                    var ck = spearFsm.FsmVariables.FindFsmGameObject("Coral King");
                    if (ck != null) ck.Value = obj;
                });
            }

            switch (spear)
            {
                case "Shoot Spikes":
                    ((SendEventByName)fsm.GetState("Ground Hit").actions[0])
                        .eventTarget.fsmComponent = spearFsm;
                    shootSpikes = so;
                    break;
                case "Long Spear":
                    longSpear = so;
                    break;
            }
        }

        var btd = obj.AddComponent<BlackThreader.BlackThreadData>();
        btd.SingCheck = () => fsm.ActiveStateName.Contains("Antic");
        btd.OnBlackThread = () =>
        {
            foreach (var sr in spears.GetComponentsInChildren<DamageHero>(true))
            {
                sr.damagePropertyFlags |= DamagePropertyFlags.Void;
            }
        };

        var startingX = obj.transform.GetPositionX();
        
        fsm.GetState("Hop Away 1").AddAction(() =>
        {
            if (Mathf.Abs(obj.transform.GetPositionX() - startingX) > 12.5f && Random.value > 0.5f)
            {
                fsm.SendEvent("JUMP OVER");
            }
        });
        
        fsm.GetState("Dormant").AddAction(() =>
        {
            obj.GetComponent<MeshRenderer>().enabled = true;
            obj.GetComponent<HealthManager>().IsInvincible = false;
            fsm.SendEvent("BATTLE START");
        });
        fsm.GetState("Start Pos").AddAction(() => fsm.SendEvent("FINISHED"), 0);
        fsm.GetState("Drop In").AddAction(() => fsm.SendEvent("LAND"), 0);
        fsm.GetState("Intro Land").DisableAction(6);
        ((StartRoarEmitter)fsm.GetState("Intro Roar").actions[2]).stunHero = false;
        var ie = fsm.GetState("Intro End");
        ie.DisableAction(1);
        ie.DisableAction(2);

        var pc = fsm.GetState("Phase Check");
        pc.transitions = pc.transitions
            .Where(trans => trans.EventName != "HORNET DEAD").ToArray();
        
        // Death
        var po = fsm.GetState("Pull Out");
        fsm.FsmGlobalTransitions.First(o => o.EventName == "ZERO HP")
            .toFsmState = po;
        po.DisableAction(13);
        po.AddAction(() =>
        {
            obj.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
            obj.layer = 2;
        }, 0);
        po.AddAction(() => fsm.SendEvent("LAND"));
        fsm.GetState("Hornet Land").AddAction(() => fsm.SendEvent("NEXT"), 0);
        fsm.GetState("Seth Check").AddAction(() => fsm.SendEvent("FINISHED"), 0);
        fsm.GetState("Get Item").AddAction(() => fsm.SendEvent("NEXT"), 0);

        var fr = fsm.GetState("Final Rumble");
        fr.DisableAction(0);
        ((Wait)fr.actions[1]).time = 20;

        var e = fsm.GetState("Event");
        e.DisableAction(0);
        e.AddAction(() => Object.Destroy(obj));
        
        obj.LocateMyFSM("Crust Up").GetState("Finish").AddAction(() =>
        {
            if (obj.GetComponent<BlackThreadState>()) 
                obj.GetComponent<MeshRenderer>().enabled = false;
        });
        
        // Positions
        var centreX = fsm.FsmVariables.FindFsmFloat("Centre X");
        var groundY = fsm.FsmVariables.FindFsmFloat("Ground Y");
        var airY = fsm.FsmVariables.FindFsmFloat("Air Jab Pos");
        fsm.GetState("Phase Check").AddAction(FixPositions, 0);
        fsm.GetState("Next Move").AddAction(FixPositions, 0);
        FixPositions();

        return;

        void FixPositions()
        {
            centreX.Value = obj.transform.GetPositionX();
            
            shootSpikes!.transform.SetPositionX(obj.transform.GetPositionX());
            longSpear!.transform.SetPositionX(obj.transform.GetPositionX());
            
            var ground = HeroController.instance.FindGroundPointY(
                obj.transform.position.x,
                obj.transform.position.y,
                true);
            groundY.Value = ground;
            airY.Value = ground + 6.55f;
            
            shootSpikes!.transform.SetPositionY(ground + 10);
            if (!longSpear.transform.GetChild(0).gameObject.activeSelf &&
                !longSpear.transform.GetChild(1).gameObject.activeSelf) 
                longSpear!.transform.SetPositionY(ground + 7);
        }
    }

    public static void FixPinstress(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Control");

        fsm.FsmVariables.FindFsmFloat("Centre X").Value = obj.transform.GetPositionX();
        fsm.FsmVariables.FindFsmFloat("Jump X Min").Value = obj.transform.GetPositionX() - 7.5f;
        fsm.FsmVariables.FindFsmFloat("Jump X Max").Value = obj.transform.GetPositionX() + 7.5f;
        fsm.FsmVariables.FindFsmFloat("Ground Y").Value = obj.transform.GetPositionY() - 1.9273f;
    }

    public static void FixPlasmified(GameObject obj)
    {
        obj.LocateMyFSM("Control").GetState("Extract Kill")
            .AddAction(() => obj.BroadcastEvent("OnDeath"), 0);
    }

    public static void FixChoristor(GameObject obj)
    {
        obj.AddComponent<FakePersistentMarker>();
    }

    public class FakePersistentMarker : MonoBehaviour;

    public static void FixUndercrank(GameObject obj)
    {
        FixUnderworksArenaEnemy(obj);
        obj.AddComponent<FakePersistentMarker>();
    }

    public static void FixSkarr(GameObject obj)
    {
        obj.transform.SetRotation2D(0);
        obj.RemoveComponent<EnemyEdgeControl>();
    }

    public static void FixGnat(GameObject obj)
    {
        var ede = obj.GetComponent<EnemyDeathEffects>();
        ede.PreInstantiate();
        var fsm = ede.GetInstantiatedCorpse(AttackTypes.Generic).LocateMyFSM("Custom Corpse");
        fsm.GetState("Spawn 2").actions[1].enabled = false;
    }

    public static void FixTormentedTrobbio(GameObject obj)
    {
        var fsm = FixTrobbio(obj, _tflares, _tfloor, _tbursts, (y, fsm) =>
        {
            ((SetFsmFloat)fsm.GetState("Tornado Start").actions[9]).setValue = y + 3.21f;
        });
        
        fsm.GetState("Wait").AddAction(() => fsm.SendEvent("ENTER"), 0);
        
        var sp = fsm.GetState("Start Pause");
        sp.DisableAction(0);
        sp.DisableAction(1);
        sp.DisableAction(2);
        sp.DisableAction(3);
        sp.AddAction(() => fsm.SetState("Start Idle"), 0);

        var ede = obj.GetComponent<EnemyDeathEffects>();
        ede.PreInstantiate();
        var corpse = ede.GetInstantiatedCorpse(AttackTypes.Generic);
        var corpseFsm = corpse.LocateMyFSM("Control");
        
        var stagger = corpseFsm.GetState("Stagger");
        stagger.DisableAction(2);
        stagger.DisableAction(3);
        stagger.DisableAction(15);

        var interactable = corpseFsm.GetState("Leave End");
        interactable.transitions = [];
        interactable.AddAction(() => Object.Destroy(corpse));
    }

    public static void FixRegularTrobbio(GameObject obj)
    {
        var fsm = FixTrobbio(obj, _flares, _floor, _bursts);
        
        var wr = fsm.GetState("Wait Refight");
        wr.DisableAction(0);
        wr.DisableAction(1);
        wr.AddAction(() => fsm.SetState("Start Idle"), 0);
        
        fsm.GetState("Death Hit").DisableAction(37);
        fsm.GetState("Death Fling").DisableAction(5);
        fsm.GetState("Death Air").DisableAction(4);
    }

    public static PlayMakerFSM FixTrobbio(GameObject obj, GameObject flareObj, GameObject floorObj, GameObject burstObj,
        [CanBeNull] Action<float, PlayMakerFSM> yPos = null)
    {
        var flares = Object.Instantiate(flareObj);
        flares.name = obj.name + " Flares";
        var floor = Object.Instantiate(floorObj);
        floor.name = obj.name + " Floor";
        var bursts = Object.Instantiate(burstObj);
        bursts.name = obj.name + " Bursts";

        var fsm = obj.LocateMyFSM("Control");

        var tt = fsm.GetState("Tornado Turn");
        tt.AddAction(new Wait
        {
            time = 0.01f
        });
        
        // Tornado range
        fsm.FsmVariables.FindFsmFloat("Tornado X Min").Value = float.NegativeInfinity;
        fsm.FsmVariables.FindFsmFloat("Tornado X Max").Value = float.PositiveInfinity;
        fsm.FsmVariables.FindFsmFloat("Centre X").Value = obj.transform.GetPositionX();
        
        // Startup
        fsm.GetState("State").AddAction(() => fsm.SendEvent("REFIGHT"), 0);
        
        fsm.GetState("Start Idle").AddAction(() =>
        {
            var rb2d = obj.GetComponent<Rigidbody2D>();
            rb2d.gravityScale = 1;
            rb2d.bodyType = RigidbodyType2D.Dynamic;
            
            obj.GetComponent<MeshRenderer>().enabled = true;

            obj.GetComponent<Collider2D>().enabled = true;
            fsm.FsmVariables.FindFsmGameObject("Damage Collider").Value.SetActive(true);
        }, 0);
        
        // Remove clamp
        fsm.GetState("Stun Start").DisableAction(2);
        
        // Flares
        fsm.FsmVariables.FindFsmGameObject("Flare Glitter").Value = flares;
        var flaresFsm = flares.LocateMyFSM("Control");
        var lowMin = flaresFsm.FsmVariables.FindFsmFloat("Low Min Y");
        var lowMax = flaresFsm.FsmVariables.FindFsmFloat("Low Max Y");
        var highMin = flaresFsm.FsmVariables.FindFsmFloat("High Min Y");
        var highMax = flaresFsm.FsmVariables.FindFsmFloat("High Max Y");
        var posY = flaresFsm.FsmVariables.FindFsmFloat("Pos Y");
        var topY = flaresFsm.FsmVariables.FindFsmFloat("Top Y");
        
        var flareOffset = new Vector3(73.8811f, 16.7204f, 0.0065f);
        fsm.GetState("Flash Antic").AddAction(() => 
            flares.transform.position = obj.transform.position - flareOffset, 0);
        
        // Bursts
        fsm.FsmVariables.FindFsmGameObject("Trapdoor Bursts").Value = bursts;
        fsm.GetState("BC Pause").AddAction(AdjustBursts, 0);
        bursts.LocateMyFSM("Control").FsmVariables.FindFsmGameObject("Trobbio").Value = obj;
        
        // Floor
        var exit1 = fsm.GetState("Exit 1");
        var exitFloorPos = ((SetPosition)fsm.GetState("Exit Pause").actions[2]).y;
        exit1.AddAction(AdjustFloor, 0);
        for (var f = 0; f < floor.transform.childCount; f++)
        {
            var floorPiece = floor.transform.GetChild(f).gameObject;
            var floorFsm = floorPiece.LocateMyFSM("Control");
            var idle = floorFsm.GetState("Idle");
            idle.DisableAction(1);
            var bumped = true;
            idle.AddAction(() =>
            {
                if (Mathf.Abs(obj.transform.GetPositionX() - floorPiece.transform.GetPositionX()) < 0.77095f)
                {
                    if (!bumped)
                    {
                        floorFsm.SendEvent("BUMP");
                        bumped = true;
                    }
                }
                else bumped = false;
            }, everyFrame: true);
        }
        
        floor.SetActive(false);
        fsm.GetState("Bomb Flurry?").AddAction(() => floor.SetActive(true), 0);
        fsm.GetState("Enter 1").AddAction(() => floor.SetActive(false), 0);

        var md = (FloatCompare)fsm.GetState("Move Dir").actions[0];
        md.equal = md.lessThan;
        var md2 = (FloatCompare)fsm.GetState("Move Dir 2").actions[0];
        md2.equal = md2.lessThan;
        
        // Adjust height
        var floorY = fsm.FsmVariables.FindFsmFloat("Floor Y");
        var ceilY = fsm.FsmVariables.FindFsmFloat("Max Y");
        fsm.GetState("Choice").AddAction(AdjustHeightFromSelf, 0);
        fsm.GetState("Enter Pause").AddAction(AdjustHeightAboveSelf, 0);
        exit1.AddAction(AdjustHeightFromSelf, 0);

        var entryRf = (RandomFloat)fsm.GetState("Get Entry Point").actions[0];
        fsm.GetState("Get Entry Point").AddAction(() =>
        {
            var ox = obj.transform.GetPositionX();
            var px = HeroController.instance.transform.GetPositionX();
            entryRf.min = Mathf.Max(px, ox) - 12.4f;
            entryRf.max = Mathf.Min(px, ox) + 12.4f;
        }, 0);
        
        AdjustHeightFromSelf();
        AdjustBursts();
        AdjustFloor();
        return fsm;

        void AdjustHeightAboveSelf()
        {
            AdjustHeight(5);
        }

        void AdjustHeightFromSelf()
        {
            AdjustHeight(0);
        }

        void AdjustFloor()
        {
            floor.transform.SetPositionX(obj.transform.GetPositionX() - flareOffset.x);
            for (var f = 0; f < floor.transform.childCount; f++)
            {
                var floorPiece = floor.transform.GetChild(f);
                
                if (HeroController.instance.TryFindGroundPoint(out var pos,
                        floorPiece.position + new Vector3(0, 5),
                        true))
                {
                    floorPiece.transform.SetPositionY(pos.y - 1.2f);
                }
            }
        }

        void AdjustBursts()
        {
            bursts.transform.SetPositionX(obj.transform.GetPositionX() - flareOffset.x);
            for (var b = 0; b < bursts.transform.GetChild(0).childCount; b++)
            {
                var burst = bursts.transform.GetChild(0).GetChild(b);
                
                if (HeroController.instance.TryFindGroundPoint(out var pos,
                        burst.position + new Vector3(0, 5),
                        true))
                {
                    burst.transform.SetPositionY(pos.y - 1.78f);
                }
            }
        }

        void AdjustHeight(float f)
        {
            if (HeroController.instance.TryFindGroundPoint(out var pos,
                    obj.transform.position + new Vector3(0, f),
                    true))
            {
                lowMin.Value = pos.y + 1.46f;
                lowMax.Value = pos.y + 2.46f;
                highMin.Value = pos.y + 6.96f;
                topY.Value = highMax.Value = pos.y + 9.46f;
                posY.Value = pos.y + 1.7f;
                floorY.Value = pos.y + 1.8f;
                ceilY.Value = pos.y + 11.37f;
                exitFloorPos.Value = pos.y - 3.04f;
                yPos?.Invoke(pos.y, fsm);
            }
        }
    }

    public static void FixFourthChorusPreload(GameObject obj)
    {
        var sg = obj.transform.Find("song_golem");
        sg.position = Vector3.zero;
        sg.gameObject.SetActive(true);
        var head = sg.Find("Song_Butt").Find("SG_waist").Find("Torso").Find("SG_head").gameObject;
        head.AddComponent<PlaceableObject.SpriteSource>();
        
        var fc = obj.AddComponent<FourthChorus>();
        fc.parent = obj;
        fc.head = head;
    }

    public class FourthChorus : MonoBehaviour
    {
        public GameObject plats;
        public GameObject parent;
        public GameObject head;
        public bool threaded;
        public bool doPlats;
        private bool _done;
        
        private void Update()
        {
            if (threaded)
            {
                threaded = false;
                foreach (var par in parent.GetComponentsInChildren<tk2dSprite>(true))
                {
                    if (par.gameObject != gameObject) par.color = Color.black;
                }
                foreach (var par in parent.GetComponentsInChildren<SpriteRenderer>(true))
                {
                    if (par.gameObject != gameObject) par.color = Color.black;
                }
            }
            if (_done) return;
            _done = true;
            var pbi = GetComponentInChildren<PersistentBoolItem>(true);
            if (pbi && pbi.itemData.Value) gameObject.SetActive(false);
        }

        private void Start()
        {
            if (!doPlats) plats.SetActive(false);
        }
    }

    public static void FixFourthChorus(GameObject obj)
    {
        var fsm = obj.transform.GetChild(0).gameObject.LocateMyFSM("Control");
        var fc = obj.GetComponent<FourthChorus>();
        
        var item = obj.AddComponent<PersistentBoolItem>();
        item.OnSetSaveState += b =>
        {
            fc.head.GetComponent<HealthManager>().isDead = b;
            if (b)
            {
                obj.SetActive(false);
                item.SetValueOverride(true);
            }
        };
        item.OnGetSaveState += (out bool b) => { b = fc.head.GetComponent<HealthManager>().isDead; };

        var rocks = Object.Instantiate(_lavaRocks, obj.transform);
        rocks.name = obj.name + " Lava Rocks";
        rocks.transform.position = fc.head.transform.position
            .Where(z: rocks.transform.GetPositionZ()) - new Vector3(80.9f, 8.8f);
        rocks.SetActive(true);
        fsm.FsmVariables.FindFsmGameObject("Rocks L").Value = rocks.transform.Find("Rocks L").gameObject;
        fsm.FsmVariables.FindFsmGameObject("Rocks M").Value = rocks.transform.Find("Rocks M").gameObject;
        fsm.FsmVariables.FindFsmGameObject("Rocks R").Value = rocks.transform.Find("Rocks R").gameObject;
        
        var btd = fc.head.AddComponent<BlackThreader.BlackThreadData>();
        var headFsm = fc.head.LocateMyFSM("Phase Control");
        btd.SingCheck = () => headFsm.ActiveStateName.Contains("Phase");
        btd.OnBlackThread = () =>
        {
            foreach (var o in obj.GetComponentsInChildren<DamageHero>(true))
                o.damagePropertyFlags |= DamagePropertyFlags.Void;
        };
            
        var plats = Object.Instantiate(_lavaPlats, obj.transform);
        plats.name = obj.name + " Lava Plats";
        plats.transform.position = fc.head.transform.position
            .Where(z: plats.transform.GetPositionZ()) + new Vector3(4.32f, -5.4f);
        fsm.FsmVariables.FindFsmGameObject("Lava Plats").Value = plats;
        plats.SetActive(true);
        fc.plats = plats;
        
        var ds = fsm.GetState("Death Start");
        ds.DisableAction(0);
        ds.DisableAction(1);

        var init = fsm.GetState("Init");
        init.DisableAction(17);
        init.DisableAction(26);
        init.DisableAction(27);
        init.DisableAction(28);
        
        fsm.GetState("Meet?").AddAction(() => fsm.SendEvent("REMEET"), 0);
        fsm.GetState("Clamp Roar?").AddAction(() => fsm.SendEvent("FINISHED"), 0);
        fsm.GetState("Music").AddAction(() => fsm.SendEvent("FINISHED"), 0);
        fsm.GetState("Punch").DisableAction(1);
        fsm.GetState("Punch Antic").DisableAction(4);
        
        fsm.GetState("Swipe L").DisableAction(5);
        fsm.GetState("Swipe R").DisableAction(5);
        
        fsm.GetState("Remeet Roar").DisableAction(9);
        var rnc = fsm.GetState("Roar No Clamp");
        rnc.DisableAction(0);
        ((StartRoarEmitter)rnc.actions[12]).stunHero = false;
        rnc.AddAction(() => obj.BroadcastEvent("OnRoar"), 13);

        Fix("To HandSlam Antic", 2);
        Fix("Remeet Roar", 10);
        Fix("Punch Antic", 5);
        Fix("Stun Anim", 1);
        Fix("Death Anim", 1);
        FixB("Slam M", 2);
        FixB("Slam M", 5);
        FixB("Slam MR", 1);
        FixB("Slam ML", 1);
        FixB("Slam R", 1);
        FixB("Slam L", 1);

        return;

        void Fix(string stateName, int index)
        {
            var apt = (AnimatePositionTo)fsm.GetState(stateName).actions[index];
            apt.localSpace = true;
            var toValue = apt.toValue;
            toValue.value -= new Vector3(80.9f, 8.8f);
        }

        void FixB(string stateName, int index)
        {
            var toValue = ((SetVector3Value)fsm.GetState(stateName).actions[index]).vector3Value;
            toValue.value -= new Vector3(80.9f, 8.8f);
        }
    }

    public static void FixShakra(GameObject obj)
    {
        RemoveConstrainPosition(obj);

        Object.Destroy(obj.LocateMyFSM("Dialogue"));

        var fsm = obj.LocateMyFSM("Attack Enemies");
        fsm.fsmTemplate = null;
        
        fsm.GetState("Idle").AddAction(() => fsm.SendEvent("BATTLE HERO"), 0);
        fsm.GetState("Start Music?").AddAction(() => fsm.SendEvent("FINISHED"), 0);
        fsm.fsm.globalTransitions = fsm.fsm.globalTransitions
            .Where(o => o.EventName != "START AWAY").ToArray();

        var lookPos = fsm.GetState("Look Pos");
        lookPos.DisableAction(0);
        lookPos.DisableAction(1);

        var lookR = fsm.GetState("Look R");
        lookR.DisableAction(2);
        lookR.DisableAction(3);

        var lookL = fsm.GetState("Look L");
        lookL.DisableAction(2);
        lookL.DisableAction(3);

        var doY = fsm.GetState("Do Y");
        doY.DisableAction(2);
        doY.DisableAction(3);

        var tryStomp = fsm.GetState("Try Stomp");
        tryStomp.DisableAction(5);
        tryStomp.DisableAction(6);
        tryStomp.DisableAction(9);
        tryStomp.DisableAction(10);

        var destinationRetry = fsm.GetState("Destination Retry");
        destinationRetry.DisableAction(2);

        var chargeSide = fsm.GetState("Charge Side");
        chargeSide.DisableAction(0);
        chargeSide.DisableAction(1);

        var chargeTeleIn = fsm.GetState("Charge Tele In");
        chargeTeleIn.DisableAction(3);
        var chargeY = fsm.FsmVariables.FindFsmFloat("Charge Y");
        chargeTeleIn.AddAction(() =>
        {
            var ground = HeroController.instance.FindGroundPointY(
                HeroController.instance.transform.position.x,
                HeroController.instance.transform.position.y + 0.5f,
                true);
            chargeY.Value = ground + 0.717f;
        }, 6);
        
        fsm.GetState("End Battle").AddAction(() => Object.Destroy(obj));
    }

    public static void FixBellBeast(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Control");

        obj.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;

        var cx = obj.transform.GetPositionX();
        var cy = obj.transform.GetPositionY();
        fsm.FsmVariables.FindFsmFloat("Centre X").Value = cx;
        fsm.FsmVariables.FindFsmFloat("Left X").Value = cx - 10.35f;
        fsm.FsmVariables.FindFsmFloat("Right X").Value = cx + 10.35f;

        var groundY = fsm.FsmVariables.FindFsmFloat("Ground Y");
        var leapOutY = fsm.FsmVariables.FindFsmFloat("LeapOut Y");
        var submergeY = fsm.FsmVariables.FindFsmFloat("Submerge Y");
        
        fsm.GetState("Dormant").AddAction(() => fsm.SendEvent("BATTLE START"));
        fsm.GetState("Burst Pos").AddAction(() => UpdatePositions(cx, cy), 0);
        fsm.GetState("Rage Antic").AddAction(() => UpdatePositions(cx, cy), 0);
        
        UpdatePositions(cx, cy);

        var ede = obj.GetComponent<EnemyDeathEffects>();
        ede.PreInstantiate();
        var corpse = ede.GetInstantiatedCorpse(AttackTypes.Generic);
        RemoveConstrainPosition(corpse);
        
        var corpseFsm = corpse.LocateMyFSM("Death");
        corpseFsm.GetState("Stagger").DisableAction(2);
        var blow = corpseFsm.GetState("Blow");
        blow.DisableAction(3);
        blow.DisableAction(4);
        blow.AddAction(() => Object.Destroy(corpse));

        return;

        void UpdatePositions(float xPos, float yPos)
        {
            var ground = HeroController.instance.FindGroundPointY(
                xPos,
                yPos + 1,
                true) + 1.83f;
            groundY.Value = ground;
            leapOutY.Value = ground - 11.52f;
            submergeY.Value = ground - 18;
        }
    }

    public static void FixGrom(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Control");

        var startPos = obj.transform.position;
        fsm.GetState("Dormant").AddAction(() =>
        {
            obj.transform.position = startPos;
            fsm.SendEvent("SPAWN");
        });
        fsm.GetState("Position").AddAction(() => fsm.SendEvent("FINISHED"), 0);
        
        var groundY = fsm.FsmVariables.FindFsmFloat("Ground Y");
        fsm.GetState("Drop").AddAction(() =>
        {
            var ground = HeroController.instance.FindGroundPointY(
                obj.transform.position.x,
                obj.transform.position.y + 1,
                true);
            groundY.Value = ground;
        }, 2, true);

        ((AddHP)fsm.GetState("Heal").actions[0]).healToMax = true;
    }

    public class DeathMarker : MonoBehaviour
    {
        public float time = 0;
    }
}