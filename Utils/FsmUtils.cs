using System;
using HutongGames.PlayMaker;

namespace Architect.Utils;

public static class FsmUtils
{
    public static FsmState GetState(this PlayMakerFSM fsm, string state)
    {
        return fsm.Fsm.GetState(state);
    }
    
    public static void DisableAction(this FsmState state, int index)
    {
        state.Actions[index].Enabled = false;
    }
    
    public static void AddAction(this FsmState state, Action action, int index = -1, bool everyFrame = false)
    {
        var customAction = new CustomFsmAction(action)
        {
            EveryFrame = everyFrame
        };

        var actions = state.Actions;

        if (index == -1) index = actions.Length;
        
        var fsmStateActionArray = new FsmStateAction[actions.Length + 1];
        var index1 = 0;
        var index2 = 0;
        while (index1 < fsmStateActionArray.Length)
        {
            if (index1 == index)
            {
                fsmStateActionArray[index1] = customAction;
                ++index1;
            }
            if (index2 < actions.Length)
                fsmStateActionArray[index1] = actions[index2];
            ++index1;
            ++index2;
        }
        state.Actions = fsmStateActionArray;
        customAction.Init(state);
    }
    
    public class CustomFsmAction(Action method) : FsmStateAction
    {
        public bool EveryFrame;
        
        private Action _method = method;

        public override void Reset()
        {
            _method = null;
            base.Reset();
        }

        public override void OnEnter()
        {
            _method();
            if (!EveryFrame) Finish();
        }

        public override void OnUpdate()
        {
            if (!EveryFrame) return;
            _method();
        }
    }
}