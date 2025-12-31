using Architect.Placements;
using Architect.Utils;
using GlobalSettings;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Architect.Behaviour.Utility;

public class BlackThreader : MonoBehaviour
{
    public string id;
    public int mode;
    public bool requireAct3;
    public float hpMultiplier = 1;

    public float[] chances = [0, 0, 0, 0];

    private int _attackIndex;
    private bool _blackThreaded;

    private CustomBlackThreadState _bts;

    private void Start()
    {
        var total = 0f;
        foreach (var f in chances) total += f;

        var rand = Random.Range(0, total);
        for (var i = 0; i < 4; i++)
        {
            rand -= chances[i];
            if (rand <= 0)
            {
                _attackIndex = i;
                return;
            }
        }
    }

    public void BlackThread()
    {
        if (_blackThreaded) return;
        if (requireAct3 && !PlayerData.instance.GetBool("blackThreadWorld")) return;

        _blackThreaded = true;
        
        if (!PlacementManager.Objects.TryGetValue(id, out var target)) return;
        var hm = target.GetComponent<HealthManager>();
        if (!hm) return;

        _bts = target.AddComponent<CustomBlackThreadState>();

        _bts.customAttack = Effects.BlackThreadAttacksDefault[_attackIndex];
        
        _bts.extraSpriteRenderers = target.GetComponentsInChildren<SpriteRenderer>(true);
        _bts.extraMeshRenderers = target.GetComponentsInChildren<MeshRenderer>(true);

        _bts.useCustomHPMultiplier = true;
        _bts.customHPMultiplier = hpMultiplier;
        
        hm.blackThreadState = _bts;
        hm.hasBlackThreadState = true;
    }

    private void Update()
    {
        if (mode == 0 && !_blackThreaded) BlackThread();
    }

    public class CustomBlackThreadState : BlackThreadState
    {
        public BlackThreadAttack customAttack;

        private new void Start()
        {
            if (!CustomStart())
            {
                enabled = false;
                return;
            }
            attacks = [customAttack];
            SetupThreaded(true);
        }

        private bool CustomStart()
        {
            GetSingFsm();
            if (!singFsm)
            {
                var anim = GetComponent<tk2dSpriteAnimator>();
                var clipName = "";
                if (anim)
                {
                    clipName = (anim.GetClipByName("Sing") ??
                                    anim.GetClipByName("Idle") ??
                                    anim.currentClip ??
                                    anim.DefaultClip).name;
                }

                var fsm = GetComponent<PlayMakerFSM>();
                if (!fsm) return false;
                
                fsm.AddState("Sing");
                GetSingFsm();
                if (!singFsm) return false;
                
                var patch = gameObject.AddComponent<SingPatcher>();
                patch.fsm = fsm;
                patch.bts = this;
                patch.animator = anim;
                patch.animClip = clipName;

                centreOffset = Vector2.zero;
            }
            
            if (hasStarted) return false;
            hasStarted = true;
            
            OnAwake();
            
            isBlackThreadWorld = true;
            
            var go = Instantiate(customAttack.Prefab, transform);
            go.transform.localPosition = Vector3.zero;
            go.SetActive(false);
            spawnedAttackObjs[customAttack] = go;
            
            chosenAttack = customAttack;
            hasChosenAttack = true;

            PreSpawnBlackThreadEffects();
            FirstThreadedSetUp();
            
            BecomeThreadedNoSing();

            healthManager.hp = Mathf.FloorToInt(healthManager.hp * customHPMultiplier);

            return true;
        }
    }

    private class SingPatcher : MonoBehaviour
    {
        public BlackThreadState bts;
        public PlayMakerFSM fsm;
        public tk2dSpriteAnimator animator;
        public string animClip;

        private bool _forcedSing;
        private string _lastStateName;
        private string _lastClip;
        
        private void Update()
        {
            var fs = bts.IsInForcedSing;
            if (fs == _forcedSing) return;

            if (fs)
            {
                var state = fsm.ActiveStateName;
                if (!state.Contains("Idle") && 
                    !state.Contains("Walk") && 
                    !state.Contains("Stationary") && 
                    !state.Contains("Chase")) return;
                _lastStateName = fsm.ActiveStateName;
                fsm.SetState("Sing");
                if (animator)
                {
                    _lastClip = animator.currentClip.name;
                    animator.Play(animClip);
                }
            }
            else
            {
                fsm.SetState(_lastStateName);
                if (animator) animator.Play(_lastClip);
            }
            
            _forcedSing = fs;
        }
    }
}