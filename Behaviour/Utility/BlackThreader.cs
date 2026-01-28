using System;
using System.Collections.Generic;
using System.Linq;
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
    public bool blockAttacks;
    public bool requireAct3;
    public float hpMultiplier = 1;

    public float[] chances = [0, 0, 0, 0];

    private int _attackIndex;
    private bool _blackThreaded;

    public static void Init()
    {
        typeof(BlackThreadState).Hook(nameof(BlackThreadState.DoAttack),
            (Action<BlackThreadState, BlackThreadAttack> orig, BlackThreadState self, BlackThreadAttack attack) =>
            {
                if (self is CustomBlackThreadState bts)
                {
                    if (bts.blockingAttack) return;
                    if (bts.blockAttacks) bts.blockingAttack = true;
                    bts.source.BroadcastEvent("OnAttack");
                }
                orig(self, attack);
            });
    }

    private CustomBlackThreadState _bts;

    public void ForceAttack()
    {
        if (!_bts) return;
        _bts.blockingAttack = false;
        _bts.queuedForceAttack = true;
    }

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

        if (!target) return;
        var dupe = target.GetComponent<ObjectDuplicator>();
        if (dupe) dupe.blackThreader = this;
        else BlackThread(target);
    }

    public void BlackThread(GameObject target)
    {
        var hm = target.GetComponent<HealthManager>();
        if (!hm) return;

        _bts = target.AddComponent<CustomBlackThreadState>();
        _bts.source = gameObject;

        _bts.customAttack = Effects.BlackThreadAttacksDefault[_attackIndex];
        
        _bts.extraSpriteRenderers = target.GetComponentsInChildren<SpriteRenderer>(true);
        _bts.extraMeshRenderers = target.GetComponentsInChildren<MeshRenderer>(true);

        _bts.useCustomHPMultiplier = true;
        _bts.customHPMultiplier = 1;

        _bts.blockAttacks = blockAttacks;
        
        hm.hp = Mathf.FloorToInt(hm.hp * hpMultiplier);
        
        hm.blackThreadState = _bts;
        hm.hasBlackThreadState = true;
    }

    private void Update()
    {
        if (mode == 0 && !_blackThreaded) BlackThread();
    }

    public class CustomBlackThreadState : BlackThreadState
    {
        public bool blockAttacks;
        public bool blockingAttack;
        
        public BlackThreadAttack customAttack;
        public GameObject source;

        private new void Start()
        {
            if (!CustomStart())
            {
                enabled = false;
                return;
            }

            blockingAttack = blockAttacks;
            
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

            return true;
        }
    }

    public class SingPatcherData : MonoBehaviour
    {
        public Func<bool> Check;
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

        private Func<bool> _check;

        private void Start()
        {
            var spd = GetComponent<SingPatcherData>();
            if (spd) _check = spd.Check;
        }

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
                    !state.Contains("Chase") && !(_check?.Invoke() ?? false)) return;
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