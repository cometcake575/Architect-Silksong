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
    
    public static readonly List<EventReceiverType> Activatable = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("grow_pod", "Grow", o =>
        {
            o.GetComponent<ActivatingBase>().SetActive(true);
        })),
        EventManager.RegisterReceiverType(new EventReceiverType("shrink_pod", "Shrink", o =>
        {
            o.GetComponent<ActivatingBase>().SetActive(false);
        }))
    ]);
    
    public static readonly List<EventReceiverType> RuneBomb = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("bomb_trigger", "Activate", o =>
        {
            var obj = Object.Instantiate(o.transform.GetChild(0).gameObject, o.transform.position,
                o.transform.localRotation);
            var ls = obj.transform.localScale;
            ls.x *= o.transform.localScale.x;
            ls.y *= o.transform.localScale.x;
            obj.transform.localScale = ls;
            obj.SetActive(true);
        }))
    ]);
    
    public static readonly List<EventReceiverType> BlackThreader = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("do_black_thread", "Activate", o =>
        {
            o.GetComponent<BlackThreader>().BlackThread();
        }))
    ]);
    
    public static readonly List<EventReceiverType> Roar = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("do_roar", "Roar", o =>
        {
            o.GetComponent<RoarEffect>().DoRoar();
        }))
    ]);
    
    public static readonly List<EventReceiverType> CoralSpike = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("do_rise", "Activate", o =>
        {
            o.GetComponent<PlayMakerFSM>().SendEvent("SPIKE UP");
        }))
    ]);
    
    public static readonly List<EventReceiverType> Plasmifier = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("do_plasmify", "Activate", o =>
        {
            o.GetComponent<Plasmifier>().Plasmify();
        }))
    ]);
    
    public static readonly List<EventReceiverType> Voltring = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("do_zap", "Zap", o =>
        {
            o.transform.GetChild(0).gameObject.SetActive(true);
        }))
    ]);
    
    public static readonly List<EventReceiverType> Dropper = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("boulder_drop", "Drop", o =>
        {
            o.LocateMyFSM("Control").SendEvent("DROP");
        }))
    ]);
    
    public static readonly List<EventReceiverType> Pilby = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("pilby_die", "Die", o =>
        {
            var detector = o.transform.Find("Skull King Detector");
            detector.GetComponent<TriggerEvent>().OnTriggerEnter2D(detector.GetComponent<BoxCollider2D>());
        }))
    ]);
    
    public static readonly List<EventReceiverType> CrankPlatform = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("crank_plat_reset", "Reset", o =>
        {
            var plat = o.GetComponent<CrankPlat>();
            plat.isComplete = false;
            plat.posT = 0;
            plat.SetCrankEnabled(true);
            plat.UpdatePos();
        })),
        EventManager.RegisterReceiverType(new EventReceiverType("crank_plat_hit", "Move", o =>
        {
            var plat = o.GetComponent<CrankPlat>();
            var force = plat.hitForce;
            plat.hitForce = 999999;
            plat.DoHit();
            plat.hitForce = force;
        }))
    ]);
    
    public static readonly List<EventReceiverType> PlayerDataSetter = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("pd_set", "SetValue", o =>
        {
            o.GetComponent<PlayerDataSetter>().SetValue();
        })),
        EventManager.RegisterReceiverType(new EventReceiverType("pd_check", "Call", o =>
        {
            o.GetComponent<PlayerDataSetter>().Relay();
        }))
    ]);
    
    public static readonly List<EventReceiverType> Bumpers = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("bouncer_evil", "SetFire", o =>
        {
            o.GetComponent<Bumper>().SetEvil(true);
        })),
        EventManager.RegisterReceiverType(new EventReceiverType("bouncer_normal", "SetNormal", o =>
        {
            o.GetComponent<Bumper>().SetEvil(false);
        }))
    ]);
    
    public static readonly List<EventReceiverType> Item = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("item_pickup", "PickUp", o =>
        {
            var ip = o.GetComponentInChildren<CollectableItemPickup>();
            ip.canPickupTime = 0;
            ip.DoPickupInstant();
        }))
    ]);
    
    public static readonly List<EventReceiverType> LifebloodCocoons = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("lifeblood_get", "Collect", o =>
        {
            foreach (var _ in o.GetComponentsInChildren<HealthFlyer>(true)) GameManager.instance.AddBlueHealthQueued();
        }))
    ]);
    
    public static readonly List<EventReceiverType> CameraShaker = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("camera_shake", "Shake", o =>
        {
            o.GetComponent<CameraShaker>().Shake();
        }))
    ]);
    
    public static readonly List<EventReceiverType> TimeSlower = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("do_timechange", "SlowTime", o =>
        {
            o.GetComponent<TimeSlower>().SlowTime();
        }))
    ]);
    
    public static readonly List<EventReceiverType> ObjectSpinner = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("spinner_reverse", "Reverse", o =>
        {
            o.GetComponent<ObjectSpinner>().speed *= -1;
        }))
    ]);
    
    public static readonly List<EventReceiverType> WalkTarget = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("start_walk", "Start", o =>
        {
            o.GetComponent<WalkTarget>().StartWalk();
        })),
        EventManager.RegisterReceiverType(new EventReceiverType("end_walk", "Cancel", o =>
        {
            o.GetComponent<WalkTarget>().StopWalk();
        }))
    ]);
    
    public static readonly List<EventReceiverType> Trap = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("activate_trap", "Activate", o =>
        {
            var fsm = o.LocateMyFSM("Control") ?? o.LocateMyFSM("FSM");
            fsm.SendEvent("ACTIVATE");
            fsm.SendEvent("TRAP");
            fsm.SendEvent("ATTACK");
        }))
    ]);
    
    public static readonly List<EventReceiverType> Gates = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("gate_open", "Open", o =>
        {
            o.GetComponent<Gate>().Open();
        }))
    ]);
    
    public static readonly List<EventReceiverType> AnimPlayer = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("play_anim", "Play", o =>
        {
            o.GetComponent<AnimPlayer>().Play();
        })),
        EventManager.RegisterReceiverType(new EventReceiverType("stop_anim", "Stop", o =>
        {
            o.GetComponent<AnimPlayer>().Stop();
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
            o.GetComponentInChildren<ParticleSystem>().Emit(200);
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
        })),
        EventManager.RegisterReceiverType(new EventReceiverType("reset_score", "Reset", o =>
        {
            o.GetComponent<MiscFixers.CustomFleaCounter>().Reset();
        }))
    ]);
    
    public static readonly List<EventReceiverType> Playable = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("start_play", "Play", o =>
        {
            o.GetComponent<IPlayable>().Play();
        })),
        EventManager.RegisterReceiverType(new EventReceiverType("stop_play", "Pause", o =>
        {
            o.GetComponent<IPlayable>().Pause();
        })),
        EventManager.RegisterReceiverType(new EventReceiverType("reset_play", "Reset", o =>
        {
            o.GetComponent<IPlayable>().Reset();
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
        EventManager.RegisterReceiverType(new EventReceiverType("anchor_reset", "Reset", o =>
        {
            o.GetComponent<ObjectAnchor>().Reset();
        })),
        EventManager.RegisterReceiverType(new EventReceiverType("anchor_stop", "StopMoving", o =>
        {
            o.GetComponent<ObjectAnchor>().moving = false;
        })),
        EventManager.RegisterReceiverType(new EventReceiverType("anchor_start", "StartMoving", o =>
        {
            o.GetComponent<ObjectAnchor>().moving = true;
        }))
    ]);
    
    public static readonly List<EventReceiverType> ObjectMover = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("mover_move", "Move", o =>
        {
            o.GetComponent<ObjectMover>().Move();
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
    
    public static readonly List<EventReceiverType> Garpid = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("garpid_attack", "Attack", o =>
        {
            o.LocateMyFSM("Control").SendEvent("ALERT");
        }))
    ]);
    
    public static readonly List<EventReceiverType> Wakeable = GroupUtils.Merge(Enemies, [
        EventManager.RegisterReceiverType(new EventReceiverType("enemy_wake", "Wake", o =>
        {
            o.GetComponent<EnemyFixers.Wakeable>().DoWake();
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
    
    public static readonly List<EventReceiverType> Colourer = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("do_colour", "ApplyColour", o =>
        {
            o.GetComponent<ObjectColourer>().Apply();
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