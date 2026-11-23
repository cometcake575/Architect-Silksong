using System;
using System.Linq;
using HutongGames.PlayMaker;

namespace Architect.Utils;

public static class FsmUtils
{
    public static void AddState(this PlayMakerFSM fsm, string state)
    {
        var oldStates = fsm.fsm.States;
        
        var newStates = new FsmState[oldStates.Length + 1];
        for (var i = 0; i < oldStates.Length; i++)
        {
            newStates[i] = oldStates[i];
        }
        var newState = new FsmState(fsm.fsm)
        {
            Name = state
        };
        
        newStates[oldStates.Length] = newState;
        fsm.fsm.States = newStates;
    }
    
    public static FsmState GetState(this PlayMakerFSM fsm, string state)
    {
        return fsm.Fsm.GetState(state);
    }
    
    public static void DisableAction(this FsmState state, int index)
    {
        state.Actions[index].Enabled = false;
    }
    
    public static void DisableActions(this FsmState state, params int[] indexes)
    {
        foreach (var index in indexes) state.Actions[index].Enabled = false;
    }

    public static void AddEvent(this FsmState state, string eve, int index = 0)
    {
        state.AddAction(() => state.fsm.FsmComponent.SendEvent(eve), index);
    }
    
    public static void AddTransition(this FsmState state, string onEventName, string toStateName)
    {
        var transitions = state.Transitions;
        var fsmTransitionArray = new FsmTransition[transitions.Length + 1];
        var fsmTransition = new FsmTransition
        {
            ToState = toStateName,
            ToFsmState = state.Fsm.GetState(toStateName),
            FsmEvent = FsmEvent.GetFsmEvent(onEventName)
        };
        int index;
        for (index = 0; index < transitions.Length; ++index)
            fsmTransitionArray[index] = transitions[index];
        fsmTransitionArray[index] = fsmTransition;
        state.Transitions = fsmTransitionArray;
    }
    
    public static FsmTransition GetTransition(this FsmState state, string onEventName)
    {
        var transitions = state.Transitions;
        return transitions.FirstOrDefault(t => t.EventName == onEventName);
    }

    public static void ChangeTransition(this FsmState state, string eve, string toState)
    {
        var transition = state.GetTransition(eve);
        transition.ToState = toState;
        transition.ToFsmState = state.Fsm.GetState(toState);
    }

    public static T GetAction<T>(this FsmState state, int index) where T : FsmStateAction
    {
        return state.Actions[index] as T;
    }
    
    public static T[] GetActionsOfType<T>(this FsmState state) where T : FsmStateAction
    {
        return state.Actions.Where(fsmStateAction => fsmStateAction.GetType() == typeof(T)).Cast<T>().ToArray();
    }

    public static void AddAction(this FsmState state, Action action, int index = -1, bool everyFrame = false)
    {
        var customAction = new CustomFsmAction(action)
        {
            EveryFrame = everyFrame
        };
        AddAction(state, customAction, index);
    }
    
    public static void AddAction(this FsmState state, FsmStateAction customAction, int index = -1)
    {
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