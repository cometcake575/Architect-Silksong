using System.Collections.Generic;
using Architect.Behaviour.Utility;
using UnityEngine;

namespace Architect.Events.Blocks.Outputs;

public class AnimatorBlock : ScriptBlock
{
    protected override IEnumerable<string> Inputs => ["Start", "Stop"];
    protected override IEnumerable<string> Outputs => ["Stop"];

    private static readonly Color DefaultColor = new(0.2f, 0.2f, 0.8f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Animator Controller";
    
    public string ClipName;

    public bool TakeCtrl;
    
    public bool OverrideAnimTime;
    public float AnimTime;

    private AnimPlayer _player;

    public override void SetupReference()
    {
        _player = new GameObject("[Architect] Anim Player Block").AddComponent<AnimPlayer>();
        _player.Block = this;
        
        _player.clipName = ClipName;
        _player.takeCtrl = TakeCtrl;
        _player.overrideAnimTime = OverrideAnimTime;
        _player.animTime = AnimTime;
    }

    protected override void Trigger(string id)
    {
        if (id == "Start") _player.Play();
        else _player.Stop();
    }
}
