using System;
using System.Collections.Generic;
using Architect.Behaviour.Utility;
using UnityEngine;

namespace Architect.Events.Blocks.Events;

public class PlayerBlock : ToggleableBlock
{
    protected override IEnumerable<string> Outputs => [
        "FaceLeft",
        "FaceRight",
        "Jump",
        "WallJump",
        "DoubleJump",
        "Land",
        "HardLand",
        "Dash",
        "Attack",
        "OnHazardRespawn",
        "OnDamage",
        "OnDeath",
        "OnHeal",
        "OnHealFail",
        "NeedolinStart",
        "NeedolinStop"
    ];

    protected override IEnumerable<(string, string)> OutputVars => [
        ("X", "Number"),
        ("Y", "Number"),
        ("Ground", "Boolean"),
        ("Left", "Boolean"),
        ("Right", "Boolean"),
        ("Up", "Boolean"),
        ("Down", "Boolean"),
        ("Self", "Object")
    ];
    protected override string Name => "Player Listener";

    public override void SetupReference()
    {
        var te = new GameObject("[Architect] Player Block").AddComponent<PlayerEvent>();
        te.Block = this;
    }

    protected override object GetValue(string id)
    {
        return id switch
        {
            "Ground" => !HeroController.instance.cState.onGround,
            "Left" => !HeroController.instance.cState.facingRight,
            "Right" => HeroController.instance.cState.facingRight,
            "Up" => HeroController.instance.cState.lookingUp,
            "Down" => HeroController.instance.cState.lookingDown,
            "Benched" => PlayerData.instance.atBench,
            "Sprinting" => HeroController.instance.sprintFSM.ActiveStateName.Contains("Sprint"),
            "Floating" => HeroController.instance.umbrellaFSM.ActiveStateName == "Float Idle",
            "Clawlining" => HeroController.instance.harpoonDashFSM.ActiveStateName == "Catch" 
                            || HeroController.instance.harpoonDashFSM.ActiveStateName == "Dash",
            "X" => HeroController.instance.transform.GetPositionX(),
            "Y" => HeroController.instance.transform.GetPositionY(),
            "Self" => HeroController.instance.gameObject,
            _ => throw new ArgumentOutOfRangeException(nameof(id), id, null)
        };
    }

    public class PlayerEvent : MonoBehaviour
    {
        public ScriptBlock Block;
    
        private void OnEnable()
        {
            PlayerHook.PlayerListenerBlocks.Add(this);
        }

        private void OnDisable()
        {
            PlayerHook.PlayerListenerBlocks.Remove(this);
        }
    }
}

public class StateBlock : PlayerBlock
{
    protected override string Name => "Player State";
    
    protected override IEnumerable<string> Outputs => [
        "OnFaceLeft",
        "OnFaceRight",
        "OnLand",
        "OnHardLand",
        "OnBench",
        "OnUnbench"
    ];

    protected override IEnumerable<(string, string)> OutputVars => [
        ("Ground", "Boolean"),
        ("Left", "Boolean"),
        ("Right", "Boolean"),
        ("Up", "Boolean"),
        ("Down", "Boolean"),
        ("Benched", "Boolean"),
        ("Self", "Object")
    ];
}

public class ActionBlock : PlayerBlock
{
    protected override string Name => "Player Movement";
    
    protected override IEnumerable<string> Outputs => [
        "OnJump",
        "OnWallJump",
        "OnDoubleJump",
        "OnInflate",
        "OnDownDash",
        "OnDash",
        "OnClawline"
    ];

    protected override IEnumerable<(string, string)> OutputVars => [
        ("X", "Number"),
        ("Y", "Number"),
        ("Sprinting", "Boolean"),
        ("Floating", "Boolean"),
        ("Clawlining", "Boolean"),
        ("Self", "Object")
    ];
}

public class AttackBlock : PlayerBlock
{
    protected override string Name => "Player Attack";
    
    protected override IEnumerable<string> Outputs => [
        "OnAttack",
        "OnNormalAttack",
        "OnUpAttack",
        "OnDownAttack",
        "OnDashAttack"
    ];

    protected override IEnumerable<(string, string)> OutputVars => [("Self", "Object")];
}
