using System.Linq;
using Architect.Placements;
using Architect.Utils;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class FsmHook : MonoBehaviour
{
    public string targetId;
    public string fsmName;
    public string stateName;
    public int index;
    
    private PlayMakerFSM _fsm;
    
    private bool _setup;

    private string _state = string.Empty;
    private float _time;
    
    public string GetState()
    {
        return _state;
    }

    public void SendEvent(string eventName)
    {
        Setup();
        if (_fsm) _fsm.SendEvent(eventName);
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
        if (_fsm) return;
        _setup = true;
        if (!PlacementManager.Objects.TryGetValue(targetId, out var target) && !targetId.StartsWith("Hero_Hornet"))
        {
            var o = ObjectUtils.FindGameObject(targetId, index);
            if (!o) return;
            target = o;
        }

        if (!target) target = HeroController.instance.gameObject;
        
        _time = Time.time;
        _fsm = target.GetComponentsInChildren<PlayMakerFSM>().FirstOrDefault(o => o.FsmName == fsmName);
    }

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
                if (_state == stateName)
                {
                    gameObject.BroadcastEvent("OnTarget");
                }
            }
        }
    }
}