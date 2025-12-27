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
        "NeedolinStart",
        "NeedolinStop"
    ];

    protected override IEnumerable<(string, string)> OutputVars => [
        ("X", "Number"),
        ("Y", "Number"),
        ("Left", "Boolean"),
        ("Right", "Boolean")
    ];
    protected override Color Color => Color.green;
    protected override string Name => "Player Listener";

    protected override void SetupReference()
    {
        var te = new GameObject("[Architect] Player Block").AddComponent<PlayerEvent>();
        te.Block = this;
    }

    protected override object GetValue(string id)
    {
        return id switch
        {
            "Left" => !HeroController.instance.cState.facingRight,
            "Right" => !HeroController.instance.cState.facingRight,
            "X" => HeroController.instance.transform.GetPositionX(),
            "Y" => HeroController.instance.transform.GetPositionY(),
            _ => throw new ArgumentOutOfRangeException(nameof(id), id, null)
        };
    }

    public class PlayerEvent : MonoBehaviour
    {
        public PlayerBlock Block;
    
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