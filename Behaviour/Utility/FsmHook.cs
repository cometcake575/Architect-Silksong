using System;
using System.Linq;
using Architect.Placements;
using Architect.Utils;
using BepInEx;
using FsmMaster;
using HutongGames.PlayMaker;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class FsmHook : PreviewableBehaviour
{
    public string targetId;
    public string fsmName;
    public string stateName;
    public int index;
    public bool inject;

    public string fsmMasterData;
    
    private PlayMakerFSM _fsm;
    
    private bool _setup;

    private string _state = string.Empty;
    private float _time;

    private FsmState _stateTarget;
    private FsmStateAction _action;
    
    public string GetState()
    {
        return _state;
    }

    public void SendEvent(string eventName)
    {
        Setup();
        if (_fsm) _fsm.SendEvent(eventName);
    }

    public void DisableAction(int i)
    {
        if (_fsm)
        {
            var state = _fsm.GetState(stateName);
            if (state == null) return;
            if (state.actions.Length <= i) return;
            state.actions[i].enabled = false;
        }
    }

    public void EnableAction(int i)
    {
        if (_fsm)
        {
            var state = _fsm.GetState(stateName);
            if (state == null) return;
            if (state.actions.Length <= i) return;
            state.actions[i].enabled = true;
        }
    }

    public float GetTime()
    {
        return Time.time - _time;
    }

    public void ClearEvents(bool all = false)
    {
        Setup();
        if (!_fsm) return;
        foreach (var state in _fsm.FsmStates.Where(state => state.Name == stateName || all)) state.transitions = [];
    }

    public void SetState()
    {
        Setup();
        if (!_fsm) return;
        _fsm.SetState(stateName);
    }

    private void Setup()
    {
        if (isAPreview) return;
        
        if (_fsm) return;
        _setup = true;
        if (!PlacementManager.TryGetValue(targetId, out var target) && !targetId.StartsWith("Hero_Hornet"))
        {
            var o = ObjectUtils.FindGameObject(targetId, index);
            if (!o) return;
            target = o;
        }

        if (!target) target = HeroController.instance.gameObject;
        
        _time = Time.time;
        _fsm = target.GetComponentsInChildren<PlayMakerFSM>().FirstOrDefault(o => o.FsmName == fsmName);

        if (_fsm)
        {
            if (inject)
            {
                _stateTarget = _fsm.GetState(stateName);
                if (_stateTarget == null) return;
                _stateTarget.AddAction(OnTarget, 0);
                _action = _stateTarget.actions[0];
            }
            
            try
            {
                SetupFsmChanges();
            }
            catch
            {
                // Data likely formatted wrong
            }
        }
    }

    private void OnDestroy()
    {
        if (_stateTarget != null)
        {
            var actions = _stateTarget.actions.ToList();
            actions.Remove(_action);
            _stateTarget.actions = actions.ToArray();
        }
    }

    public void OnTarget() => gameObject.BroadcastEvent("OnTarget");

    private void Update()
    {
        if (!_setup) Setup();
        if (_fsm)
        {
            if (_state != _fsm.ActiveStateName)
            {
                _state = _fsm.ActiveStateName;
                _time = Time.time;
                gameObject.BroadcastEvent("OnChange");
                if (_state == stateName && !inject) OnTarget();
            }
        }
    }

    public static FsmMasterPlugin FsmMaster;
    private static FsmEditManager _editManager;
    
    public static void Init()
    {
        typeof(FsmMasterPlugin).Hook("Awake",
            (Action<FsmMasterPlugin> orig, FsmMasterPlugin self) =>
            {
                orig(self);
                FsmMaster = self;
                _editManager = self._editManager;
            });
        FsmGraphOverlay.ShowEditIndicator = false;
    }

    public void SetupFsmChanges()
    {
        if (fsmMasterData.IsNullOrWhiteSpace()) return;
        
        var editSet = FsmSaveDataStore.FromWire(JsonUtility.FromJson<FsmSaveDataStore.FsmEditSetWire>(fsmMasterData));
        var fsm = _fsm.fsm;
        
        foreach (var variableOverride in editSet.VariableOverrides)
            _editManager.ApplyVariableOverride(editSet.FsmKey, fsm, variableOverride);
        foreach (var actionFieldOverride in editSet.ActionFieldOverrides)
            _editManager.ApplyActionFieldOverride(editSet.FsmKey, fsm, actionFieldOverride);
        foreach (var disabledState in editSet.DisabledStates)
            _editManager.DisableState(editSet.FsmKey, fsm, disabledState);
        foreach (var transitionRetarget in editSet.TransitionRetargets)
            _editManager.ApplyTransitionRetarget(editSet.FsmKey, fsm, transitionRetarget);
        foreach (var sequencerOverride in editSet.SequencerOverrides)
            _editManager.InstallSequencer(editSet.FsmKey, fsm, sequencerOverride);
    }
}