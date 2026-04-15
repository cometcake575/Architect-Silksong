using System.Collections.Generic;
using Architect.Behaviour.Utility;
using UnityEngine;

namespace Architect.Events.Blocks.Outputs;

public class AnimatorBlock : ScriptBlock
{
    protected override IEnumerable<string> Inputs => ["Start", "Stop"];
    protected override IEnumerable<string> Outputs => ["Stop"];
    protected override IEnumerable<(string, string)> InputVars => [("Target", "Object")];
    protected override IEnumerable<(string, string)> OutputVars => [("Current", "Text")];
    
    protected override string Name => "Animator Controller";
    
    public string ClipName;

    public bool TakeCtrl;
    
    public bool OverrideAnimTime;
    public float AnimTime;

    private IAnimPlayer _player;

    public override void SetupReference()
    {
        var target = GetVariable<GameObject>("Target");
        if (target && target != HeroController.instance.gameObject)
        {
            var player = target.AddComponent<AnimPlayer>();
            player.animator = target.GetComponent<tk2dSpriteAnimator>();
            if (!player.animator) return;
            player.clip = player.animator.GetClipByName(ClipName);
            player.overrideAnimTime = OverrideAnimTime;
            player.animTime = AnimTime;
            _player = player;
        }
        else
        {
            var player = new GameObject("[Architect] Anim Player Block").AddComponent<PlayerAnimPlayer>();
            player.Block = this;

            player.clipName = ClipName;
            player.takeCtrl = TakeCtrl;
            player.overrideAnimTime = OverrideAnimTime;
            player.animTime = AnimTime;

            _player = player;
        }
    }

    public override object GetValue(string id)
    {
        return _player.GetClip();
    }

    protected override void Trigger(string id)
    {
        if (id == "Start") _player.Play();
        else _player.Stop();
    }
}
