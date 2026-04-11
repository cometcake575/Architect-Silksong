using System.Collections;
using System.Collections.Generic;
using Architect.Behaviour.Utility;
using Architect.Utils;
using UnityEngine;

namespace Architect.Events.Blocks.Outputs;

public class TransitionBlock : ScriptBlock
{
    protected override IEnumerable<string> Inputs => ["Transition"];
    protected override IEnumerable<(string, string)> OutputVars => [("InScene", "Boolean")];

    
    
    protected override string Name => "Scene Transition";

    public string Scene = "Tut_01";
    public string Door = "placeholder";

    protected override object GetValue(string id) => Scene == GameManager.instance.sceneName;

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
