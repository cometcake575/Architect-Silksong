using Architect.Content.Preloads;
using Architect.Utils;
using UnityEngine;
using UnityEngine.Animations;
using Object = UnityEngine.Object;

namespace Architect.Behaviour.Fixers;

public static class HazardFixers
{
    private static GameObject _cogDamager;
    private static GameObject _junkFall;
    private static GameObject _lavaBox;
    private static GameObject _coalRegion;
    private static GameObject _tendrilDamager;
    private static GameObject _cradleSpikes;
    private static GameObject _voltHazard;

    private static float _lanternTime = 1;
        
    public static void Init()
    {
        PreloadManager.RegisterPreload(new BasicPreload("Cog_04", "Spike Cog collider Round/Cog Damager", 
            o =>
            {
                var cmh = o.GetComponentInChildren<CogMultiHitter>();
                cmh.useSelfForAngle = true;
                _cogDamager = o;
            }));
        
        PreloadManager.RegisterPreload(new BasicPreload("Under_06", "junk_chute_ manual (1)", 
            o => _junkFall = o));
        
        PreloadManager.RegisterPreload(new BasicPreload("Bone_East_09", "Lava Box", 
            o => _lavaBox = o));
        
        PreloadManager.RegisterPreload(new BasicPreload("Abyss_07", "Abyss Tendril Hero Damager", 
            o => _tendrilDamager = o));
        
        PreloadManager.RegisterPreload(new BasicPreload("Bone_East_03", "Coal Region", 
            o => _coalRegion = o));
        
        PreloadManager.RegisterPreload(new BasicPreload("Cradle_03", "cradle_spike_plat (10)", 
            o => _cradleSpikes = o.transform.GetChild(8).gameObject));
        
        PreloadManager.RegisterPreload(new BasicPreload("Coral_29", "Zap Hazard Parent", 
            o =>
            {
                var b = false;
                foreach (var h in o.GetComponentsInChildren<PolygonCollider2D>())
                {
                    if (!b)
                    {
                        b = true;
                        continue;
                    }
                    Object.Destroy(h.gameObject);
                }
                _voltHazard = o;
            }));

        HookUtils.OnFsmAwake += fsm =>
        {
            if (fsm.FsmName == "Control" && fsm.gameObject.name.Contains("Wisp Fireball"))
            {
                var o = fsm.FsmVariables.FindFsmGameObject("Wisp Fireball Master");
                if (o == null) return;
                o.Value = null;
                fsm.GetState("Request Attack").AddAction(() =>
                {
                    if (_lanternTime <= 0)
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
        cog.transform.SetPositionZ(0.01f);
    }

    public static void FixLargeCogAfterSpawn(GameObject cog)
    {
        var dmg = Object.Instantiate(_cogDamager, null);
        dmg.transform.parent = cog.transform;
        var cmh = dmg.GetComponent<CogMultiHitter>();
        cmh.heroGrindEffect = Object.Instantiate(cmh.heroGrindEffect.gameObject, dmg.transform).transform;
        cmh.heroGrindEffect.parent = null;
        cmh.heroGrindEffect.localScale = Vector3.one;
        dmg.transform.localPosition = Vector3.zero;
        dmg.SetActive(true);
    }
    
    public static void FixUnderworksCog(GameObject cog)
    {
        cog.transform.SetPositionZ(0.01f);
    }

    public static void FixCog(GameObject cog)
    {
        cog.RemoveComponent<CurveOffsetAnimation>();
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
        fsm.GetState("Follow Start").DisableAction(1);
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
    }

    public static void FixJunkPipePostSpawn(GameObject obj)
    {
        var junkFall = Object.Instantiate(_junkFall, null);
        junkFall.transform.parent = obj.transform;
        junkFall.transform.SetLocalPosition2D(new Vector2(2.5f, -6.5f));
        junkFall.SetActive(true);
        junkFall.transform.Find("Spike Collider").gameObject.LocateMyFSM("Shift Hero").enabled = false;

        junkFall.AddComponent<JunkPipe>();
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

    public static void FixCoal(GameObject obj)
    {
        var cr = Object.Instantiate(_coalRegion, obj.transform);
        cr.SetActive(true);
        cr.transform.localPosition = Vector3.zero;

        var bc2d = cr.GetComponent<BoxCollider2D>();
        bc2d.size = new Vector2(2.75f, 1);
        bc2d.offset = new Vector2(0, 0.25f);
    }

    public static void FixVoltgrass(GameObject obj)
    {
        var vh = Object.Instantiate(_voltHazard, obj.transform);
        vh.SetActive(true);
        vh.transform.localPosition = Vector3.zero;
        var pc = vh.GetComponentInChildren<PolygonCollider2D>();
        pc.points =
        [
            new Vector2(-1, -3),
            new Vector2(1, -4),
            new Vector2(2.2f, -4),
            new Vector2(3, 0.4f),
            new Vector2(2, 1),
            new Vector2(-0.6f, 1)
        ];
        pc.transform.GetChild(0).localPosition = new Vector3(0.6198f, -1.1504f, 0);
        pc.transform.localPosition = Vector3.zero;
    }

    private static GameObject _currentTendrilDamager;

    public static void FixTendrils(GameObject obj)
    {
        if (_currentTendrilDamager) return;
        _currentTendrilDamager = Object.Instantiate(_tendrilDamager, obj.transform);
        _currentTendrilDamager.SetActive(true);
    }

    public static void FixCradleSpikes(GameObject obj)
    {
        Object.Instantiate(
            _cradleSpikes,
            obj.transform.position,
            default,
            obj.transform
        ).GetComponent<TinkEffect>().overrideCamShake = true;
    }
}