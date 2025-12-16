using System;
using System.Collections.Generic;
using Architect.Utils;
using HutongGames.PlayMaker;
using UnityEngine;

namespace Architect.Behaviour.Custom;

public class Darkness : MonoBehaviour
{
    private static readonly List<Darkness> DarknessObjects = [];

    private static PlayMakerFSM _fsm;
    private static FsmInt _value;

    public static void Init()
    {
        HookUtils.OnFsmAwake += fsm =>
        {
            if (fsm.FsmName == "Darkness Control")
            {
                _fsm = fsm;
                _value = fsm.FsmVariables.FindFsmInt("Darkness Level");
            }
        };
    }

    private void OnEnable()
    {
        DarknessObjects.Add(this);
        Refresh();
    }

    private void OnDisable()
    {
        DarknessObjects.Remove(this);
        Refresh();
    }

    private void Update()
    {
        if (DarknessObjects.Count > 0) Refresh();
    }

    private static void Refresh()
    {
        GameManager.instance.sm.darknessLevel = Mathf.Min(DarknessObjects.Count, 2);
        _value.Value = GameManager.instance.sm.darknessLevel;
        _fsm.SendEvent("RESET");
    }
}