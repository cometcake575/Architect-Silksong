using System;
using Architect.Content.Preloads;
using Architect.Prefabs;
using Architect.Utils;
using GlobalEnums;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class CustomTransitionPoint : PreviewableBehaviour
{
    public int pointType;
    public bool applyInEditMode = true;

    private static GameObject _wakeDoor;

    public static void Init()
    {
        typeof(TransitionPoint).Hook(nameof(TransitionPoint.GetGatePosition),
            (Func<TransitionPoint, GatePosition> orig, TransitionPoint self) =>
            {
                if (!self) return GatePosition.unknown;
                var ctp = self.GetComponentInParent<CustomTransitionPoint>();
                return ctp ? ctp.GetGatePosition() : orig(self);
            });
        
        typeof(TransitionPoint).Hook(nameof(TransitionPoint.PrepareEntry),
            (Action<TransitionPoint> orig, TransitionPoint self) =>
            {
                orig(self);
                var ctp = self.GetComponentInParent<CustomTransitionPoint>();
                if (ctp) ctp.gameObject.BroadcastEvent("OnExit");
            });
        
        PreloadManager.RegisterPreload(new BasicPreload("Memory_Coral_Tower", "Door Get Up/door_wakeInMemory",
            o =>
            {
                o.transform.GetChild(0).gameObject.SetActive(false);
                o.transform.GetChild(1).gameObject.SetActive(false);

                _wakeDoor = o;
            }));
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

        if (pointType == 5)
        {
            if (GameManager.instance.entryGateName != name) return;
            var wd = Instantiate(_wakeDoor, transform);
            wd.transform.localPosition = Vector3.zero;
            var gateName = name;
            wd.transform.name = gateName;
            name += "_parent";
        }
    }

    public GatePosition GetGatePosition()
    {
        return pointType switch
        {
            1 => GatePosition.left,
            2 => GatePosition.right,
            3 => GatePosition.top,
            4 => GatePosition.bottom,
            _ => GatePosition.door
        };
    }
}