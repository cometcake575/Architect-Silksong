using System.Collections;
using System.Collections.Generic;
using Architect.Behaviour.Utility;
using Architect.Utils;
using UnityEngine;

namespace Architect.Events.Blocks.Outputs;

public class TransitionBlock : ScriptBlock
{
    protected override IEnumerable<string> Inputs => ["Transition"];

    private static readonly Color DefaultColor = new(0.2f, 0.4f, 0.8f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Scene Transition";

    public string Scene = "";
    public string Door = "";

    protected override void Trigger(string trigger)
    {
        ArchitectPlugin.Instance.StartCoroutine(Coroutine());
    }

    private IEnumerator Coroutine()
    {
        yield return HeroController.instance.FreeControl();
        
        GameManager.instance.BeginSceneTransition(new GameManager.SceneLoadInfo
        {
            SceneName = Scene,
            EntryGateName = Door,
            EntryDelay = 0,
            Visualization = GameManager.SceneLoadVisualizations.Default,
            PreventCameraFadeOut = true,
            WaitForSceneTransitionCameraFade = false
        });
    }
}
