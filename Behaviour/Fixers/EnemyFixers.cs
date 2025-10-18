using System;
using System.Reflection;
using Architect.Utils;
using HutongGames.PlayMaker.Actions;
using MonoMod.RuntimeDetour;
using UnityEngine;

namespace Architect.Behaviour.Fixers;

public static class EnemyFixers
{
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
    }
    
    public static void ApplyGravity(GameObject obj)
    {
        obj.GetOrAddComponent<Rigidbody2D>().gravityScale = 1;
    }
    
    public static void FixAknid(GameObject obj)
    {
        RemoveConstrainPosition(obj);
        obj.LocateMyFSM("Control").GetState("Travel To").DisableAction(0);
    }

    public static void KeepActive(GameObject obj)
    {
        obj.RemoveComponent<DeactivateIfPlayerdataTrue>();
        obj.RemoveComponent<DeactivateIfPlayerdataFalse>();
        obj.RemoveComponent<DeactivateIfPlayerdataFalseDelayed>();
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

        var ground = obj.transform.position.y;
        
        fsm.FsmVariables.FindFsmFloat("Ground Y").Value = ground;
        fsm.FsmVariables.FindFsmFloat("Jump Y").Value = ground + 5.78f;
        fsm.FsmVariables.FindFsmFloat("Censer Bot Y").Value = ground - 3.64f;
        fsm.FsmVariables.FindFsmFloat("Censer Top Y").Value = ground + 7.18f;
        fsm.FsmVariables.FindFsmFloat("Stomp Y").Value = ground + 4.58f;

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

        ((CheckYPosition)corpseFsm.GetState("Fall").Actions[2]).compareTo = ground;
        ((SetPosition)corpseFsm.GetState("Land").Actions[0]).y = ground;
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
        fsm.GetState("Patrol").AddAction(() =>
        {
            var hPos = HeroController.instance.transform.position;
            var oPos = obj.transform.position;
            if (Mathf.Abs(hPos.x - oPos.x) < 4 && Mathf.Abs(hPos.y - oPos.y) < 1) fsm.SendEvent("ATTACK");
        }, 0, true);
    }

    public class DisableHealthScaling : MonoBehaviour;

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
}