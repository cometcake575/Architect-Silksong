using System;
using System.Linq;
using Architect.Content.Preloads;
using Architect.Utils;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Architect.Behaviour.Fixers;

public static class EnemyFixers
{
    private static GameObject _bouldersPrefab;
    
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
                self.GetComponent<DisableHealthScaling>() ? hit : orig(self, hit));
        
        typeof(DisplayBossTitle).Hook(nameof(DisplayBossTitle.OnEnter),
            (Action<DisplayBossTitle> orig, DisplayBossTitle self) =>
            {
                if (self.fsmComponent.GetComponent<DisableBossTitle>()) self.bossTitle.value = "";
                orig(self);
            });
        
        PreloadManager.RegisterPreload(new BasicPreload("Bonetown_boss", "Boss Scene/Boulders Battle", 
            o => _bouldersPrefab = o));
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

    public class CustomBehaviourMarker : MonoBehaviour;

    public static void FixLastJudge(GameObject obj)
    {
        RemoveConstrainPosition(obj);
        obj.AddComponent<LastJudgeFixer>();

        var fsm = obj.LocateMyFSM("Control");
        fsm.GetState("Dormant").AddAction(() => fsm.SetState("Idle"));

        var groundY = fsm.FsmVariables.FindFsmFloat("Ground Y");
        var jumpY = fsm.FsmVariables.FindFsmFloat("Jump Y");
        var censerBotY = fsm.FsmVariables.FindFsmFloat("Censer Bot Y");
        var censerTopY = fsm.FsmVariables.FindFsmFloat("Censer Top Y");
        var stompY = fsm.FsmVariables.FindFsmFloat("Stomp Y");

        var centre = obj.transform.position.x;
        fsm.FsmVariables.FindFsmFloat("Centre X").Value = centre;
        
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
        
        fsm.GetState("Dash Land").AddAction(() => fsm.SetState("Idle"), 4);

        obj.GetComponent<HealthManager>().IsInvincible = false;

        var ede = obj.GetComponent<EnemyDeathEffects>();
        
        ede.PreInstantiate();
        
        var corpseFsm = ede.GetInstantiatedCorpse(AttackTypes.Nail).LocateMyFSM("Control");

        var fall = (CheckYPosition)corpseFsm.GetState("Fall").Actions[2];
        var land = (SetPosition)corpseFsm.GetState("Land").Actions[0];
        
        AdjustGroundPos();
        return;
        
        void AdjustGroundPos()
        {
            if (HeroController.instance.TryFindGroundPoint(out var pos, obj.transform.position, 
                    true))
            {
                var ground = pos.y;
                groundY.Value = ground;
                jumpY.Value = ground + 5.78f;
                censerBotY.Value = ground - 3.64f;
                censerTopY.Value = ground + 7.18f;
                stompY.Value = ground + 4.58f;
                
                fall.compareTo = ground;
                land.y = ground;
            }
        }
    }

    public static void RemoveConstrainPosition(GameObject obj)
    {
        obj.RemoveComponent<ConstrainPosition>();
    }

    private class LastJudgeFixer : MonoBehaviour;

    public static void FixBloatroach(GameObject obj)
    {
        RemoveConstrainPosition(obj);
        var fsm = obj.LocateMyFSM("Control");
        var val = fsm.FsmVariables.FindFsmBool("In Attack Range");
        fsm.GetState("Idle").AddAction(() =>
        {
            var hPos = HeroController.instance.transform.position;
            var oPos = obj.transform.position;
            val.Value = Mathf.Abs(hPos.x - oPos.x) < 15 && Mathf.Abs(hPos.y - oPos.y) < 2;
        }, 4, true);
    }

    public static void FixFluttermite(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Control");
        fsm.FsmVariables.FindFsmGameObject("Start Point").value = new GameObject("Start Point")
        {
            transform = { position = obj.transform.position }
        };
        fsm.FsmVariables.FindFsmGameObject("Patrol Point").value = new GameObject("Patrol Point")
        {
            transform = { position = obj.transform.position }
        };
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

        // Stops attacks upon hitting a transition gate too
        ((RayCast2dV2)fsm.GetState("Charge").actions[4]).layerMask = [8, LayerMask.NameToLayer("Enemy Detector")];
        ((RayCast2dV2)fsm.GetState("Dthrust").actions[8]).layerMask = [8, LayerMask.NameToLayer("Enemy Detector")];

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
}