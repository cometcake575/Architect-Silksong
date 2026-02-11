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

    private PlayMakerFSM _fsm;
    
    private bool _setup;

    private string _state = string.Empty;

    public string GetState()
    {
        return _state;
    }

    public void SetState()
    {
        if (!_fsm) return;
        _fsm.SetState(stateName);
    }

    private void Setup()
    {
        _setup = true;
        if (!PlacementManager.Objects.TryGetValue(targetId, out var target))
        {
            var o = ObjectUtils.FindGameObject(targetId);
            if (!o) return;
            target = o;
        }
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
                gameObject.BroadcastEvent("OnChange");
                if (_state == stateName)
                {
                    gameObject.BroadcastEvent("OnTarget");
                }
            }
        }
    }
}