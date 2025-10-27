using System.Collections.Generic;
using Architect.Behaviour.Custom;
using Architect.Behaviour.Fixers;
using Architect.Behaviour.Utility;
using Architect.Events;
using UnityEngine;

namespace Architect.Objects.Groups;

public static class ReceiverGroup
{
    public static readonly List<EventReceiverType> Generic = [
        EventManager.RegisterReceiverType(new EventReceiverType("disable", "Disable", o =>
        {
            o.SetActive(false);
        })),
        EventManager.RegisterReceiverType(new EventReceiverType("enable", "Enable", o =>
        {
            o.SetActive(true);
        }, true))
    ];
    
    public static readonly List<EventReceiverType> CloverPod = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("grow_pod", "Grow", o =>
        {
            o.GetComponent<BouncePod>().SetActive(true);
        })),
        EventManager.RegisterReceiverType(new EventReceiverType("shrink_pod", "Shrink", o =>
        {
            o.GetComponent<BouncePod>().SetActive(false);
        }))
    ]);
    
    public static readonly List<EventReceiverType> ObjectSpinner = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("spinner_reverse", "Reverse", o =>
        {
            o.GetComponent<ObjectSpinner>().speed *= -1;
        }))
    ]);
    
    public static readonly List<EventReceiverType> Trap = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("activate_trap", "Activate", o =>
        {
            o.LocateMyFSM("Control").SendEvent("ACTIVATE");
            o.LocateMyFSM("Control").SendEvent("TRAP");
        }))
    ]);
    
    public static readonly List<EventReceiverType> Gates = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("gate_open", "Open", o =>
        {
            o.GetComponent<Gate>().Open();
        }))
    ]);
    
    public static readonly List<EventReceiverType> TeleportPoint = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("do_tp", "Teleport", o =>
        {
            HeroController.instance.transform.position = o.transform.position;
        }))
    ]);
    
    public static readonly List<EventReceiverType> CloseableGates = GroupUtils.Merge(Gates, [
        EventManager.RegisterReceiverType(new EventReceiverType("gate_close", "Close", o =>
        {
            o.GetComponent<Gate>().Close();
        }))
    ]);
    
    public static readonly List<EventReceiverType> Confetti = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("confetti_burst", "Burst", o =>
        {
            o.GetComponent<ParticleSystem>().Emit(200);
        }))
    ]);
    
    public static readonly List<EventReceiverType> Displayable = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("display_text", "Show", o =>
        {
            o.GetComponent<IDisplayable>().Display();
        }))
    ]);
    
    public static readonly List<EventReceiverType> FleaCounter = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("increment_score", "Increment", o =>
        {
            o.GetComponent<MiscFixers.CustomFleaCounter>().Increment();
        })),
        EventManager.RegisterReceiverType(new EventReceiverType("decrement_score", "Decrement", o =>
        {
            o.GetComponent<MiscFixers.CustomFleaCounter>().Decrement();
        })),
        EventManager.RegisterReceiverType(new EventReceiverType("show_score", "ShowTitle", o =>
        {
            o.GetComponent<MiscFixers.CustomFleaCounter>().Announce();
        }))
    ]);
    
    public static readonly List<EventReceiverType> Playable = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("start_play", "Play", o =>
        {
            o.GetComponent<IPlayable>().Play();
        }))
    ]);
    
    public static readonly List<EventReceiverType> Mp4 = GroupUtils.Merge(Playable, [
        EventManager.RegisterReceiverType(new EventReceiverType("stop_play", "Pause", o =>
        {
            o.GetComponent<Mp4Object>().Pause();
        }))
    ]);
    
    public static readonly List<EventReceiverType> PlayerHooks = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("damage_player", "Damage", PlayerHook.DamagePlayer)),
        EventManager.RegisterReceiverType(new EventReceiverType("hazard_player", "HazardDamage", PlayerHook.HazardDamagePlayer)),
        EventManager.RegisterReceiverType(new EventReceiverType("kill_player", "Kill", PlayerHook.KillPlayer)),
        EventManager.RegisterReceiverType(new EventReceiverType("silk_player", "GiveSilk", _ =>
        {
            PlayerHook.GiveSilk();
        })),
        EventManager.RegisterReceiverType(new EventReceiverType("full_silk_player", "MaxSilk", _ =>
        {
            PlayerHook.MaxSilk();
        })),
        EventManager.RegisterReceiverType(new EventReceiverType("unsilk_player", "TakeSilk", _ =>
        {
            PlayerHook.TakeSilk();
        })),
        EventManager.RegisterReceiverType(new EventReceiverType("no_silk_player", "ClearSilk", _ =>
        {
            PlayerHook.ClearSilk();
        })),
        EventManager.RegisterReceiverType(new EventReceiverType("heal_player", "Heal", _ =>
        {
            PlayerHook.HealPlayer();
        })),
        EventManager.RegisterReceiverType(new EventReceiverType("full_heal_player", "FullHeal", _ =>
        {
            PlayerHook.FullHealPlayer();
        })),
        EventManager.RegisterReceiverType(new EventReceiverType("disable_vignette", "VignetteOff", _ =>
        {
            PlayerHook.DisableVignette();
        })),
        EventManager.RegisterReceiverType(new EventReceiverType("enable_vignette", "VignetteOn", _ =>
        {
            PlayerHook.EnableVignette();
        }))
    ]);
    
    public static readonly List<EventReceiverType> Duplicator = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("duplicate", "SpawnObject", o =>
        {
            o.GetComponent<ObjectDuplicator>().Duplicate();
        }))
    ]);
    
    public static readonly List<EventReceiverType> HazardRespawn = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("set_spawn", "SetSpawn", o =>
        {
            PlayerData.instance.SetHazardRespawn(o.GetComponent<HazardRespawnMarker>());
        }))
    ]);
    
    public static readonly List<EventReceiverType> ObjectAnchor = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("anchor_stop", "StopMoving", o =>
        {
            o.GetComponent<ObjectAnchor>().moving = false;
        })),
        EventManager.RegisterReceiverType(new EventReceiverType("anchor_start", "StartMoving", o =>
        {
            o.GetComponent<ObjectAnchor>().moving = true;
        }))
    ]);
    
    public static readonly List<EventReceiverType> Relay = [
        EventManager.RegisterReceiverType(new EventReceiverType("do_relay", "Call", o =>
        {
            o.GetComponent<Relay>().DoRelay();
        })),
        EventManager.RegisterReceiverType(new EventReceiverType("relay_off", "Disable", o =>
        {
            o.GetComponent<Relay>().DisableRelay();
        })),
        EventManager.RegisterReceiverType(new EventReceiverType("relay_on", "Enable", o =>
        {
            o.GetComponent<Relay>().EnableRelay();
        }))
    ];
    
    public static readonly List<EventReceiverType> Enemies = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("enemy_die", "Die", o =>
        {
            o.GetComponent<HealthManager>().Die(null, AttackTypes.Generic, true);
        }))
    ]);
    
    public static readonly List<EventReceiverType> Binoculars = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("start_using", "StartUsing", o =>
        {
            o.GetComponent<Binoculars>().StartUsing();
        }))
    ]);
    
    public static readonly List<EventReceiverType> SpikeBall = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("swing_1", "Swing", o =>
        {
            o.LocateMyFSM("Control").SendEvent("TRAP");
        })),
        EventManager.RegisterReceiverType(new EventReceiverType("swing_2", "SwingTwice", o =>
        {
            o.LocateMyFSM("Control").SendEvent("TRAP DOUBLE");
        }))
    ]);
    
    public static readonly List<EventReceiverType> Transitions = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("transition", "Transition", o =>
        {
            var tp = o.GetComponent<TransitionPoint>();

            var wasADoor = tp.isADoor;
            tp.isADoor = false;
            tp.OnTriggerEnter2D(HeroController.instance.GetComponent<Collider2D>());
            if (wasADoor) tp.isADoor = true;
        }))
    ]);
}