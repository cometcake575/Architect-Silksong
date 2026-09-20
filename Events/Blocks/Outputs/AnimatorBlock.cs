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
    public bool ClearXVel = true;
    public bool ClearYVel = false;
    
    public bool OverrideAnimTime;
    public float AnimTime;

    private IAnimPlayer _player;

    private bool _setup;

    public override void Reset()
    {
        _setup = false;
    }

    private void DoSetup()
    {
        if (_setup) return;
        _setup = true;
        var target = GetVariable<GameObject>("Target");
        if (target && (!HeroController.instance || target != HeroController.instance.gameObject))
        {
            var player = target.AddComponent<AnimPlayer>();
            _player = player;
            player.animator = target.GetComponentInChildren<tk2dSpriteAnimator>(true);
            if (!player.animator)
            {
                _setup = false;
                return;
            }
            try
            {
                player.clip = player.animator.GetClipByName(ClipName);
            }
            catch
            {
                //
            }

            player.overrideAnimTime = OverrideAnimTime;
            player.animTime = AnimTime;
        }
        else
        {
            var player = new GameObject("[Architect] Anim Player Block").AddComponent<PlayerAnimPlayer>();
            player.Block = this;

            player.clipName = ClipName;
            player.takeCtrl = TakeCtrl;
            player.clearXVel = ClearXVel;
            player.clearYVel = ClearYVel;
            player.overrideAnimTime = OverrideAnimTime;
            player.animTime = AnimTime;

            _player = player;
        }
    }

    public override object GetValue(string id)
    {
        DoSetup();
        return _setup ? _player.GetClip() : null;
    }

    protected override void Trigger(string id)
    {
        DoSetup();
        if (!_setup) return;
        if (id == "Start") _player.Play();
        else _player.Stop();
    }
}
