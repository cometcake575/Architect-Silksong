using System.Collections;
using System.Collections.Generic;
using Architect.Behaviour.Custom;
using Architect.Behaviour.Fixers;
using Architect.Behaviour.Utility;
using Architect.Content.Custom;
using Architect.Events;
using Architect.Utils;
using HutongGames.PlayMaker.Actions;
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
    
    public static readonly List<EventReceiverType> Prefab = [
        EventManager.RegisterReceiverType(new EventReceiverType("prefab_start", "Activate", o =>
        {
            o.SetActive(true);
        }, true))
    ];
    
    public static readonly List<EventReceiverType> Wisp = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("wisp_go", "Fire", o =>
        {
            o.LocateMyFSM("Control").SetState("Fire Antic");
        }))
    ]);
    
    public static readonly List<EventReceiverType> ThreadEffect = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("do_thread", "Activate", o =>
        {
            var sp = o.GetComponent<MiscFixers.ThreadEffect>().sp;
            sp.gameObject.SetActive(true);
            sp.PlayPossess();
        }))
    ]);
    
    public static readonly List<EventReceiverType> AbilityCrystal = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("crystal_clear", "ClearAll", o =>
        {
            AbilityObjects.ActiveCrystals.Clear();
            AbilityObjects.RefreshCrystalUI();
        }))
    ]);
    
    public static readonly List<EventReceiverType> MagmaRocks = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("magma_go", "Away", o =>
        {
            o.LocateMyFSM("Control").SendEvent("AWAY");
        })),
        EventManager.RegisterReceiverType(new EventReceiverType("magma_back", "Return", o =>
        {
            o.LocateMyFSM("Control").SendEvent("RETURN");
        }))
    ]);
    
    public static readonly List<EventReceiverType> Blast = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("servitor_blast", "Fire", o =>
        {
            o.LocateMyFSM("Control").SetState("Shoot");
        }))
    ]);
    
    public static readonly List<EventReceiverType> TriggerZone = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("disable_tz", "DisableImmediate", o =>
        {
            o.GetComponent<TriggerZone>().block = true;
            o.SetActive(false);
        }))
    ]);
    
    public static readonly List<EventReceiverType> Dust = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("dust_on", "Emit", o =>
        {
            ArchitectPlugin.Instance.StartCoroutine(EmitDust(o));
        }))
    ]);

    private static IEnumerator EmitDust(GameObject o)
    {
        foreach (var s in o.GetComponentsInChildren<ParticleSystem>()) s.Play();
        yield return new WaitForSeconds(o.GetComponent<MiscFixers.Dust>().time);
        foreach (var s in o.GetComponentsInChildren<ParticleSystem>()) s.Stop();
        o.BroadcastEvent("OnFinish");
    }
    
    public static readonly List<EventReceiverType> Velocity = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("set_velocity", "SetVelocity", (o, b) =>
        {
            if (b == null) return;
            o.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(
                b.GetVariable<float>("New X"),
                b.GetVariable<float>("New Y")
            );
        }))
    ]);
    
    public static readonly List<EventReceiverType> MapperRing = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("set_velocity_ring", "SetVelocity", (o, b) =>
        {
            if (b == null) return;
            var rb2d = o.GetComponent<Rigidbody2D>();
            o.transform.GetChild(0).gameObject.SetActive(true);
            rb2d.bodyType = RigidbodyType2D.Dynamic;
            rb2d.linearVelocity = new Vector2(
                b.GetVariable<float>("New X"),
                b.GetVariable<float>("New Y")
            );
        }))
    ]);
    
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
    
    public static readonly List<EventReceiverType> MaskAndSpool = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("collect_mask", "Collect", o =>
        {
            o.GetComponent<PlayMakerFSM>().SendEvent("GET");
        }))
    ]);
    
    public static readonly List<EventReceiverType> Spine = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("spine_go", "Shoot", o =>
        {
            o.LocateMyFSM("Control").SendEvent("ANTIC");
        }))
    ]);
    
    public static readonly List<EventReceiverType> DialDoor = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("dial_rotate_l", "RotateLeft", o =>
        {
            var ddb = o.GetComponent<DialDoorBridge>();
            ddb.StartCoroutine(ddb.MoveRotate(-1));
            ddb.isRotated = !ddb.isRotated;
        })),
        EventManager.RegisterReceiverType(new EventReceiverType("dial_rotate_r", "RotateRight", o =>
        {
            var ddb = o.GetComponent<DialDoorBridge>();
            ddb.StartCoroutine(ddb.MoveRotate(1));
            ddb.isRotated = !ddb.isRotated;
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
    
    public static readonly List<EventReceiverType> VoidBullet = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("vb_trigger", "Activate", o =>
        {
            var obj = Object.Instantiate(o.transform.GetChild(0).gameObject, o.transform.position,
                o.transform.localRotation);
            ((FlingObjectsFromGlobalPool)obj.LocateMyFSM("Control").GetState("Fire").actions[0]).gameObject = 
                o.transform.GetChild(1).gameObject;
            obj.SetActive(true);
        }))
    ]);
    
    public static readonly List<EventReceiverType> SilkAcid = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("silk_acid_trigger", "Activate", o =>
        {
            var obj = Object.Instantiate(o.transform.GetChild(0).gameObject, o.transform.position,
                o.transform.localRotation);
            obj.LocateMyFSM("Control").GetState("Idle").DisableAction(5);
            obj.SetActive(true);
        }))
    ]);
    
    public static readonly List<EventReceiverType> BlackThreader = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("do_black_thread", "Activate", o =>
        {
            o.GetComponent<BlackThreader>().BlackThread();
        })),
        EventManager.RegisterReceiverType(new EventReceiverType("queue_bts", "Queue Attack", o =>
        {
            o.GetComponent<BlackThreader>().ForceAttack();
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
    
    public static readonly List<EventReceiverType> Burst = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("do_zap", "Activate", o =>
        {
            var c = o.transform.GetChild(0);
            c.gameObject.SetActive(true);
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
            o.GetComponent<Gate>()?.Open();
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
    
    public static readonly List<EventReceiverType> BattleGate = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("bone_gate_close", "Close", o =>
        {
            o.LocateMyFSM("BG Control").SendEvent("BG CLOSE");
        })),
        EventManager.RegisterReceiverType(new EventReceiverType("bone_gate_open", "Open", o =>
        {
            o.LocateMyFSM("BG Control").SendEvent("BG OPEN");
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
        })),
        EventManager.RegisterReceiverType(new EventReceiverType("set_score", "Set", (o, b) =>
        {
            if (b == null) return;
            o.GetComponent<MiscFixers.CustomFleaCounter>().SetValue(b.GetVariable<float>("New Value"));
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
    
    public static readonly List<EventReceiverType> Png = GroupUtils.Merge(Playable, [
        EventManager.RegisterReceiverType(new EventReceiverType("png_flip_x", "FlipX", o =>
        {
            o.transform.SetScaleX(-o.transform.GetScaleX());
        })),
        EventManager.RegisterReceiverType(new EventReceiverType("png_flip_y", "FlipY", o =>
        {
            o.transform.SetScaleY(-o.transform.GetScaleY());
        })),
        EventManager.RegisterReceiverType(new EventReceiverType("png_set_width", "SetWidth", (o, b) =>
        {
            if (b == null) return;
            o.transform.SetScaleX(b.GetVariable<float>("New Width", 1));
        })),
        EventManager.RegisterReceiverType(new EventReceiverType("png_set_height", "SetHeight", (o, b) =>
        {
            if (b == null) return;
            o.transform.SetScaleY(b.GetVariable<float>("New Height", 1));
        })),
        EventManager.RegisterReceiverType(new EventReceiverType("png_set_fps", "SetFPS", (o, b) =>
        {
            if (b == null) return;
            var png = o.GetComponentInChildren<PngObject>();
            var val = b.GetVariable<float>("New FPS", 1);
            if (val == 0) png.frameTime = 0;
            else png.frameTime = 1 / Mathf.Max(0.01f, val);
        }))
    ]);
    
    public static readonly List<EventReceiverType> Wav = GroupUtils.Merge(Playable, [
        EventManager.RegisterReceiverType(new EventReceiverType("set_volume", "SetVolume", (o, b) =>
        {
            if (b == null) return;
            o.GetComponent<WavObject>().Volume = b.GetVariable<float>("New Volume");
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
    
    public static readonly List<EventReceiverType> Respawn = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("set_bench_spawn", "SetSpawn", o =>
        {
            PlayerData.instance.SetBenchRespawn(
                o.GetComponent<RespawnMarker>(), 
                GameManager.instance.sceneName, 
                0);
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
        EventManager.RegisterReceiverType(new EventReceiverType("mover_move", "Move", (o, b) =>
        {
            o.GetComponent<ObjectMover>().Move(
                b?.GetVariable<float>("Extra X") ?? 0,
                b?.GetVariable<float>("Extra Y") ?? 0,
                b?.GetVariable<float>("Extra Rot") ?? 0);
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
            var hm = o.GetComponentInChildren<HealthManager>();
            if (!hm) return;
            var dm = hm.gameObject.GetOrAddComponent<EnemyFixers.DeathMarker>();
            if (Time.time - dm.time < 0.1f) return;
            dm.time = Time.time;
            hm.TakeDamage(new HitInstance
            {
                Source = o,
                AttackType = AttackTypes.Generic,
                NailElement = NailElements.None,
                DamageDealt = 999999999,
                ToolDamageFlags = ToolDamageFlags.None,
                SpecialType = SpecialTypes.None,
                SlashEffectOverrides = [],
                IgnoreInvulnerable = true,
                HitEffectsType = EnemyHitEffectsProfile.EffectsTypes.Minimal,
                SilkGeneration = HitSilkGeneration.None,
                Multiplier = 1
            });
        }))
    ]);
    
    public static readonly List<EventReceiverType> FourthChorus = GroupUtils.Merge(Enemies, [
        EventManager.RegisterReceiverType(new EventReceiverType("disable_chorus_plats", "RemovePlats", o =>
        {
            var pl = o.GetComponent<EnemyFixers.FourthChorus>().plats;
            if (pl) pl.SetActive(false);
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
        EventManager.RegisterReceiverType(new EventReceiverType("do_colour", "Colour", (o, b) =>
        {
            o.GetComponent<ObjectColourer>().Apply(Mathf.Max(0, b?.GetVariable<float>("Fade Time") ?? 0));
        })),
        EventManager.RegisterReceiverType(new EventReceiverType("do_colour_dynamic", "DynamicColour", (o, b) =>
        {
            o.GetComponent<ObjectColourer>().Apply(
                Mathf.Max(0, b.GetVariable<float>("Fade Time")),
                new Color(
                    b.GetVariable<float>("R", 1), 
                    b.GetVariable<float>("G", 1), 
                    b.GetVariable<float>("B", 1), 
                    b.GetVariable<float>("A", 1))
            );
        }))
    ]);
    
    public static readonly List<EventReceiverType> Transitions = GroupUtils.Merge(Generic, [
        EventManager.RegisterReceiverType(new EventReceiverType("transition", "Transition", o =>
        {
            ArchitectPlugin.Instance.StartCoroutine(Transition(o));
        }))
    ]);

    private static IEnumerator Transition(GameObject o)
    {
        var tp = o.GetComponent<TransitionPoint>();

        var wasADoor = tp.isADoor;
        tp.isADoor = false;
        yield return HeroController.instance.FreeControl();
        tp.OnTriggerEnter2D(HeroController.instance.GetComponent<Collider2D>());
        if (wasADoor) tp.isADoor = true;
    }
}