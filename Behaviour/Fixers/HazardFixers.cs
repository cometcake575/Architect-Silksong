using Architect.Content.Preloads;
using Architect.Utils;
using UnityEngine;
using UnityEngine.Animations;
using Object = UnityEngine.Object;

namespace Architect.Behaviour.Fixers;

public static class HazardFixers
{
    private static Transform _heroGrind;
    private static GameObject _cogDamager;
    private static GameObject _junkFall;
    private static GameObject _lavaBox;

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
        
        PreloadManager.RegisterPreload(new BasicPreload("Under_06", "junk_chute_ manual (1)", 
            o => _junkFall = o));
        
        PreloadManager.RegisterPreload(new BasicPreload("Bone_East_09", "Lava Box", 
            o => _lavaBox = o));

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

    public static void UpdateLanterns() => _lanternTime -= Time.deltaTime;
    
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

    public static void FixUnderworksCog(GameObject cog)
    {
        cog.transform.SetParent(null);
        cog.transform.SetPositionZ(0.01f);
        cog.GetComponentInChildren<CogMultiHitter>().heroGrindEffect = _heroGrind;
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

    public static void FixWisp(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Control");
        
        var master = fsm.FsmVariables.FindFsmGameObject("Wisp Fireball Master");
        fsm.GetState("Request Attack").AddAction(() =>
        {
            if (!master.Value && _lanternTime <= 0)
            {
                _lanternTime = 1;
                fsm.SendEvent("APPROVED");
            }
        }, 2);
    }

    public static void FixSpikeBall(GameObject obj)
    {
        obj.transform.GetChild(1).SetAsFirstSibling();
    }

    public static void FixCoralSpike(GameObject obj)
    {
        var beenDormant = false;
        obj.LocateMyFSM("Control").GetState("Dormant").AddAction(() =>
        {
            if (!beenDormant)
            {
                beenDormant = true;
                return;
            }
            obj.BroadcastEvent("OnBreak");
        }, 0);
    }

    public static void FixJunkPipe(GameObject obj)
    {
        obj.transform.GetChild(2).SetAsFirstSibling();
        var junkFall = Object.Instantiate(_junkFall, obj.transform);
        junkFall.transform.SetLocalPosition2D(new Vector2(2.5f, -6.5f));
        junkFall.SetActive(true);
        junkFall.transform.Find("Spike Collider").gameObject.LocateMyFSM("Shift Hero").enabled = false;

        junkFall.AddComponent<JunkPipe>();
        
        junkFall.GetComponent<PlayMakerFSM>().GetState("Pause").DisableAction(2);
    }

    public class JunkPipe : MonoBehaviour
    {
        private PlayMakerFSM _fsm;
        
        private void Awake()
        {
            _fsm = GetComponent<PlayMakerFSM>();
            _fsm.GetState("Pause").DisableAction(2);
            _fsm.GetState("Flow").AddAction(() => transform.GetChild(2).gameObject.SetActive(true));
        }
        
        private void OnDisable()
        {
            transform.GetChild(2).gameObject.SetActive(false);
        }
        
        private void OnEnable()
        {
            transform.GetChild(2).gameObject.SetActive(false);
        }
    }

    public static void FixMaggotBlob(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Control");
        fsm.InitTemplate();
        fsm.fsmTemplate = null;

        obj.AddComponent<MaggotBlob>();
        obj.RemoveComponent<RandomScale>();
    }

    private class MaggotBlob : MonoBehaviour
    {
        private bool _setup;
        
        private void Update()
        {
            if (_setup) return;
            _setup = true;
            
            var fsm = gameObject.LocateMyFSM("Control");
            fsm.SendEvent("FINISHED");
            var launch = fsm.GetState("Launch");
            launch.DisableAction(4);
            launch.DisableAction(7);
            launch.DisableAction(8);
        }
    }

    public static void FixLava(GameObject obj)
    {
        var lb = Object.Instantiate(_lavaBox, obj.transform);
        lb.SetActive(true);

        lb.transform.localPosition = Vector3.zero;
        lb.transform.localScale = Vector3.one;
        
        var bc2 = lb.GetComponent<BoxCollider2D>();
        bc2.size = Vector2.one;
        bc2.offset = Vector2.zero;
    }
}