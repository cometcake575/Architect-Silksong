using System.Linq;
using Architect.Placements;
using Architect.Utils;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class FsmHook : MonoBehaviour
{
    public string targetId;
    public string fsmName;

    private PlayMakerFSM _fsm;
    
    private bool _setup;

    public string GetState()
    {
        return _fsm ? _fsm.ActiveStateName : "";
    }

    public void SetState(string state)
    {
        if (!_fsm) return;
        _fsm.SetState(state);
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
    }
}