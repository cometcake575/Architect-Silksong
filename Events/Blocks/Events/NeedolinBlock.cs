using System.Collections.Generic;
using System.Linq;
using Architect.Utils;
using HutongGames.PlayMaker.Actions;
using UnityEngine;

namespace Architect.Events.Blocks.Events;

public class NeedolinBlock : ScriptBlock
{
    private static readonly List<NeedolinBlockRef> Refs = [];
    
    public int DetectionType;
    
    private float _timeStarted;
    private bool _playing;
    
    public static void Init()
    {
        HookUtils.OnFsmAwake += fsm =>
        {
            if (fsm.FsmName is "Silk Specials" or "Bench Control")
            {
                var state = fsm.GetState("Needolin Sub");
                var needolinFsm = ((RunFSM)state?
                    .actions[fsm.FsmName == "Bench Control" ? 0 : 2])?.fsmTemplateControl.RunFsm;
                if (needolinFsm == null) return;
                
                needolinFsm.GetState("End Needolin").AddAction(Stop, 0);
                needolinFsm.GetState("Needolin FT Out").AddAction(StopSpecial, 0);
                needolinFsm.GetState("Needolin Mem Out").AddAction(StopSpecial, 0);
                needolinFsm.GetState("Take Control").AddAction(() =>
                {
                    Start(0);
                }, 0);
                needolinFsm.GetState("Needolin FT In").AddAction(() =>
                {
                    Start(1);
                }, 0);
                needolinFsm.GetState("Needolin Mem In").AddAction(() =>
                {
                    Start(2);
                }, 0);
            }
        };
    }

    private static void Start(int type)
    {
        Refs.RemoveAll(r => !r);
        foreach (var r in Refs.Where(r =>
                     !r.Block._playing && 
                     (r.Block.DetectionType == 0 || r.Block.DetectionType == type)))
        {
            r.Block._playing = true;
            r.Block._timeStarted = Time.time;
            r.Block.Event("OnStart");
        }
    }
    
    private static void Stop()
    {
        Refs.RemoveAll(r => !r);
        foreach (var r in Refs.Where(r => r.Block._playing))
        {
            r.Block._playing = false;
            r.Block.Event("OnStop");
        }
    }
    
    private static void StopSpecial()
    {
        Refs.RemoveAll(r => !r);
        foreach (var r in Refs.Where(r => r.Block._playing &&
                                          r.Block.DetectionType != 0))
        {
            r.Block._playing = false;
            r.Block.Event("OnStop");
        }
    }
    
    protected override string Name => "Needolin Control";

    protected override IEnumerable<string> Outputs => [
        "OnStart",
        "OnStop"
    ];

    protected override IEnumerable<(string, string)> OutputVars => [
        ("Playing", "Boolean"),
        ("Duration", "Number")
    ];

    public override object GetValue(string id)
    {
        if (_playing)
        {
            if (id == "Playing") return true;
            return Time.time - _timeStarted;
        }

        return id == "Playing" ? false : 0f;
    }

    public override void SetupReference()
    {
        new GameObject("[Architect] Needolin Block").AddComponent<NeedolinBlockRef>().Block = this;
    }

    public class NeedolinBlockRef : MonoBehaviour
    {
        public NeedolinBlock Block;
        
        private void OnEnable()
        {
            Refs.Add(this);
        }

        private void OnDisable()
        {
            Refs.Remove(this);
        }
    }
}