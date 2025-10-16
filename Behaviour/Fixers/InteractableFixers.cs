using Architect.Events;
using Architect.Utils;
using UnityEngine;

namespace Architect.Behaviour.Fixers;

public static class InteractableFixers
{
    public static void FixMarchPlate(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Control");

        var activated = fsm.GetState("Activate Device");
        activated.DisableAction(0);
        
        activated.AddAction(() => obj.BroadcastEvent("OnActivate"), 0);
    }
    
    public static void FixSlabPlate(GameObject obj)
    {
        var plate = obj.GetComponent<TrapPressurePlate>();
        plate.OnPressed.AddListener(() => obj.BroadcastEvent("OnActivate"));
    }

    public static void FixLever(GameObject obj)
    {
        obj.GetComponent<PersistentBoolItem>().OnSetSaveState += value =>
        {
            if (!value) return;
            EventManager.BroadcastEvent(obj, "OnPull");
            EventManager.BroadcastEvent(obj, "LoadedPulled");
        };
    }

    public static void FixTrapWire(GameObject obj)
    {
        obj.LocateMyFSM("Control").GetState("Snap").AddAction(() => obj.BroadcastEvent("OnActivate"), 0);
    }

    public static void FixCogLever(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Control");
        fsm.GetState("Activate").AddAction(() =>
        {
            obj.BroadcastEvent("OnPull");
            obj.BroadcastEvent("LoadedPulled");
        });
        fsm.GetState("Open Mechanism").AddAction(() =>
        {
            obj.BroadcastEvent("OnPull");
            obj.BroadcastEvent("FirstPulled");
        });
    }

    public static void FixCogLeverPreload(GameObject obj)
    {
        obj.transform.GetChild(3).SetAsFirstSibling();
        obj.transform.GetChild(2).gameObject.SetActive(false);
        obj.transform.GetChild(4).gameObject.SetActive(false);
    }

    public static void FixButtonPreload(GameObject obj)
    {
        obj.transform.GetChild(0).gameObject.SetActive(false);
    }

    public static void FixButton(GameObject obj)
    {
        var plate = obj.GetComponent<PersistentPressurePlate>();
        
        plate.OnActivate.AddListener(() =>
        {
            obj.BroadcastEvent("OnPress");
            obj.BroadcastEvent("FirstPress");
        });
        
        plate.OnActivated.AddListener(() =>
        {
            obj.BroadcastEvent("OnPress");
            obj.BroadcastEvent("LoadedPressed");
        });
    }

    public static void FixActivator(GameObject obj)
    {
        var activator = obj.GetComponent<TimedActivator>();
        activator.OnActivated.AddListener(() => obj.BroadcastEvent("OnActivate"));
        activator.OnDeactivate.AddListener(() => obj.BroadcastEvent("OnDeactivate"));
    }

    public static void FixCloverStatue(GameObject obj)
    {
        obj.LocateMyFSM("Control").GetState("Break").AddAction(() => obj.BroadcastEvent("OnActivate"), 0);
    }

    public static void FixSlabBlade(GameObject obj)
    {
        obj.transform.GetChild(2).gameObject.SetActive(false);
    }

    public static void FixSpikeBall(GameObject obj)
    {
        obj.transform.GetChild(1).GetChild(1).SetAsFirstSibling();
    }
}