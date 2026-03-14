using System;
using Architect.Prefabs;
using Architect.Utils;
using GlobalEnums;

namespace Architect.Behaviour.Utility;

public class CustomTransitionPoint : PreviewableBehaviour
{
    public int pointType;
    public bool applyInEditMode = true;

    public static void Init()
    {
        typeof(TransitionPoint).Hook(nameof(TransitionPoint.GetGatePosition),
            (Func<TransitionPoint, GatePosition> orig, TransitionPoint self) =>
            {
                var ctp = self.GetComponent<CustomTransitionPoint>();
                return ctp ? ctp.GetGatePosition() : orig(self);
            });
        
        typeof(TransitionPoint).Hook(nameof(TransitionPoint.PrepareEntry),
            (Action<TransitionPoint> orig, TransitionPoint self) =>
            {
                orig(self);
                if (self.GetComponent<CustomTransitionPoint>()) self.gameObject.BroadcastEvent("OnExit");
            });
    }

    private void Start()
    {
        if (isAPreview && (PrefabManager.InPrefabScene || !applyInEditMode))
        {
            gameObject.SetActive(false);
            return;
        }
        
        var tp = GetComponent<TransitionPoint>();
        
        SceneTeleportMap.AddTransitionGate(tp.targetScene, tp.entryPoint);

        tp.InteractLabel = InteractableBase.PromptLabels.Enter;
    }

    public GatePosition GetGatePosition()
    {
        return pointType switch
        {
            0 => GatePosition.door,
            1 => GatePosition.left,
            2 => GatePosition.right,
            3 => GatePosition.top,
            _ => GatePosition.bottom
        };
    }
}