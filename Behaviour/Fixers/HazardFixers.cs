using System;
using System.Reflection;
using Architect.Content.Preloads;
using Architect.Utils;
using MonoMod.RuntimeDetour;
using UnityEngine;
using UnityEngine.Animations;
using Object = UnityEngine.Object;

namespace Architect.Behaviour.Fixers;

public static class HazardFixers
{
    private static Transform _heroGrind;
    private static GameObject _cogDamager;

    private static float _lanternTime = 1;
        
    public static void Init()
    {
        PreloadManager.RegisterPreload(new BasicPreload("Cog_04", "Pt Hero Grind", 
            o => _heroGrind = o.transform));
        
        PreloadManager.RegisterPreload(new BasicPreload("Cog_04", "Spike Cog collider Round/Cog Damager", 
            o =>
            {
                var cmh = o.GetComponentInChildren<CogMultiHitter>();
                cmh.heroGrindEffect = _heroGrind;
                cmh.useSelfForAngle = true;
                _cogDamager = o;
            }));

        HookUtils.OnFsmAwake += fsm =>
        {
            
            if (fsm.FsmName == "Control" && fsm.gameObject.name == "Wisp Fireball(Clone)")
            {
                var obj = fsm.FsmVariables.FindFsmGameObject("Wisp Fireball Master");
                fsm.GetState("Request Attack").AddAction(() =>
                {
                    if (!obj.Value && _lanternTime <= 0)
                    {
                        _lanternTime = 1;
                        fsm.SendEvent("APPROVED");
                    }
                }, 2);
            }
        };
    }

    public static void Update() => _lanternTime -= Time.deltaTime;
    
    public static void FixFan(GameObject fan)
    {
        var trans = fan.transform;
        trans.GetChild(0).gameObject.SetActive(false);
        trans.GetChild(1).gameObject.SetActive(false);
        trans.GetChild(3).GetChild(2).gameObject.SetActive(false);
        trans.GetChild(4).gameObject.SetActive(false);
    }

    public static void FixLargeCog(GameObject cog)
    {
        cog.transform.SetParent(null);
        cog.transform.SetPositionZ(0.01f);
        Object.Instantiate(_cogDamager, cog.transform).SetActive(true);
    }

    public static void FixCog(GameObject cog)
    {
        cog.RemoveComponent<CurveOffsetAnimation>();
        cog.GetComponentInChildren<CogMultiHitter>().heroGrindEffect = _heroGrind;
        cog.transform.GetChild(3).SetAsFirstSibling();
    }

    public static void FixMillTrap(GameObject obj)
    {
        obj.transform.GetChild(0).gameObject.SetActive(false);
        obj.transform.localScale *= 2;
    }

    public static void FixZaprockPreload(GameObject obj)
    {
        obj.transform.SetRotation2D(0);
    }

    public static void FixZaprock(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Control");
        fsm.GetState("Idle").AddAction(() => fsm.SendEvent("ENTER"), 0);
    }

    public static void FixWispLantern(GameObject obj)
    {
        obj.GetComponentInChildren<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;

        var source = new ConstraintSource
        {
            sourceTransform = obj.transform.GetChild(3).GetChild(0),
            weight = 1
        };

        AddConstraint(1);
        AddConstraint(2);
        AddConstraint(4);
        AddConstraint(5);
        
        return;
        
        void AddConstraint(int num)
        {
            var constraint = obj.transform.GetChild(num).gameObject.AddComponent<PositionConstraint>();
            constraint.AddSource(source);
            constraint.constraintActive = true;
        }
    }
}