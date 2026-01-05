using System;
using System.Collections.Generic;
using System.Linq;
using Architect.Behaviour.Abilities;
using Architect.Behaviour.Custom;
using Architect.Behaviour.Fixers;
using Architect.Behaviour.Utility;
using Architect.Config;
using Architect.Config.Types;
using Architect.Content.Custom;
using Architect.Editor;
using Architect.Events;
using Architect.Storage;
using Architect.Utils;
using GlobalEnums;
using HutongGames.PlayMaker.Actions;
using TeamCherry.Localization;
using UnityEngine;
using UnityEngine.Video;

namespace Architect.Objects.Groups;

public static class ConfigGroup
{
    public static readonly List<ConfigType> Generic =
    [
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Active", "start_active", (o, value) =>
            {
                if (!value.GetValue()) o.SetActive(false);
            }))
    ];

    public static readonly List<ConfigType> Visible = GroupUtils.Merge(Generic, [
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Visible", "is_visible", (o, value) =>
            {
                if (value.GetValue()) return;
                
                foreach (var renderer in o.GetComponentsInChildren<tk2dSprite>())
                {
                    var col = renderer.color;
                    col.a = 0;
                    renderer.color = col;
                }
                foreach (var renderer in o.GetComponentsInChildren<SpriteRenderer>())
                {
                    var col = renderer.color;
                    col.a = 0;
                    renderer.color = col;
                }

                o.AddComponent<MiscFixers.ColorLock>();
            }))
    ]);

    public static readonly List<ConfigType> Item = GroupUtils.Merge(Generic, [
        ConfigurationManager.RegisterConfigType(
            new StringConfigType("Item ID", "item_id", (o, value) =>
            {
                o.GetComponent<CustomPickup>().item = value.GetValue();
            }).WithDefaultValue("Rosary_Set_Small")),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Ignore Limit", "item_ignore_obtained", (o, value) =>
            {
                o.GetComponent<CustomPickup>().ignoreObtained = value.GetValue();
            }).WithDefaultValue(true)),
        ConfigurationManager.RegisterConfigType(MakePersistenceConfigType("Stay Collected", "item_stay"))
    ]);

    public static readonly List<ConfigType> LoreTablets = GroupUtils.Merge(Generic, [
        ConfigurationManager.RegisterConfigType(
            new StringConfigType("Text", "tablet_text", (o, value) =>
            {
                o.GetComponentInChildren<BasicNPC>(true).talkText = [
                    new LocalisedString("ArchitectMod", value.GetValue())
                ];
            }).WithDefaultValue("Sample Text"))
    ]);

    public static readonly List<ConfigType> JellyEgg = GroupUtils.Merge(Visible, [
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Regen Time", "egg_regen", (o, value) =>
            {
                o.GetComponent<Behaviour.Custom.JellyEgg>().regenTime = value.GetValue();
            }).WithDefaultValue(-1))
    ]);

    public static readonly List<ConfigType> BlackThreader = GroupUtils.Merge(Generic, [
        ConfigurationManager.RegisterConfigType(
            new IdConfigType("Object ID", "voider_id", (o, value) =>
            {
                o.GetComponent<BlackThreader>().id = value.GetValue();
            })),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Void Circle Chance", "voider_ball", (o, value) =>
            {
                o.GetComponent<BlackThreader>().chances[0] = Mathf.Abs(value.GetValue());
            }).WithDefaultValue(1)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Void Shot Chance", "voider_shot", (o, value) =>
            {
                o.GetComponent<BlackThreader>().chances[1] = Mathf.Abs(value.GetValue());
            }).WithDefaultValue(1)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Void Spew Chance", "voider_vomit", (o, value) =>
            {
                o.GetComponent<BlackThreader>().chances[2] = Mathf.Abs(value.GetValue());
            }).WithDefaultValue(1)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Void Lash Chance", "voider_whip", (o, value) =>
            {
                o.GetComponent<BlackThreader>().chances[3] = Mathf.Abs(value.GetValue());
            }).WithDefaultValue(1)),
        ConfigurationManager.RegisterConfigType(
            new ChoiceConfigType("Trigger Mode", "voider_mode", (o, value) =>
            {
                o.GetComponent<BlackThreader>().mode = value.GetValue();
            }).WithOptions("From Start", "Event Only").WithDefaultValue(0)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Health Multiplier", "voider_hp_mul", (o, value) =>
            {
                o.GetComponent<BlackThreader>().hpMultiplier = value.GetValue();
            }).WithDefaultValue(1)),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Require Act 3", "voider_do_check", (o, value) =>
            {
                o.GetComponent<BlackThreader>().requireAct3 = value.GetValue();
            }).WithDefaultValue(false))
    ]);

    public static readonly List<ConfigType> Plasmifier = GroupUtils.Merge(Generic, [
        ConfigurationManager.RegisterConfigType(
            new IdConfigType("Object ID", "plasmifier_id", (o, value) =>
            {
                o.GetComponent<Plasmifier>().id = value.GetValue();
            })),
        ConfigurationManager.RegisterConfigType(
            new ChoiceConfigType("Trigger Mode", "plasmifier_mode", (o, value) =>
            {
                o.GetComponent<Plasmifier>().mode = value.GetValue();
            }).WithOptions("From Start", "Event Only").WithDefaultValue(0)),
        ConfigurationManager.RegisterConfigType(
            new IntConfigType("Heal Amount", "plasmifier_heal", (o, value) =>
            {
                o.GetComponent<Plasmifier>().heal = value.GetValue();
            }).WithDefaultValue(5))
    ]);

    public static readonly List<ConfigType> AnimPlayer = GroupUtils.Merge(Generic, [
        ConfigurationManager.RegisterConfigType(
            new StringConfigType("Clip Name", "anim_clip", (o, value) =>
            {
                o.GetComponent<AnimPlayer>().clipName = value.GetValue();
            })),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Duration Override", "anim_duration", (o, value) =>
            {
                var ap = o.GetComponent<AnimPlayer>();
                ap.overrideAnimTime = true;
                ap.animTime = value.GetValue();
            })),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Take Control", "anim_take_ctrl", (o, value) =>
            {
                o.GetComponent<AnimPlayer>().takeCtrl = value.GetValue();
            }).WithDefaultValue(true))
    ]);

    public static readonly List<ConfigType> TimeSlower = GroupUtils.Merge(Generic, [
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Time Scale", "time_scale", (o, value) =>
            {
                o.GetComponent<TimeSlower>().targetSpeed = value.GetValue();
            }).WithDefaultValue(0.25f)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Change Time", "time_change", (o, value) =>
            {
                o.GetComponent<TimeSlower>().changeTime = value.GetValue();
            }).WithDefaultValue(0.1f)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Wait Time", "time_wait", (o, value) =>
            {
                o.GetComponent<TimeSlower>().waitTime = value.GetValue();
            }).WithDefaultValue(1)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Return Time", "time_return", (o, value) =>
            {
                o.GetComponent<TimeSlower>().returnTime = value.GetValue();
            }).WithDefaultValue(0.75f)),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Prevent Pausing Game", "time_prevent_pausing", (o, value) =>
            {
                o.GetComponent<TimeSlower>().noPause = value.GetValue();
            }).WithDefaultValue(true))
    ]);

    public static readonly List<ConfigType> Toll = GroupUtils.Merge(Visible, [
        ConfigurationManager.RegisterConfigType(
            new IntConfigType("Cost", "toll_cost", (o, value) =>
            {
                o.LocateMyFSM("Behaviour (special)").FsmVariables.FindFsmInt("Cost")
                    .value = Math.Max(1, value.GetValue());
            }).WithDefaultValue(100)),
        ConfigurationManager.RegisterConfigType(
            new StringConfigType("Text", "toll_text", (o, value) =>
            {
                o.GetComponent<MiscFixers.Toll>().text = value.GetValue();
            }).WithDefaultValue("Sample Text"))
    ]);

    public static readonly List<ConfigType> Npcs = GroupUtils.Merge(Visible, [
        ConfigurationManager.RegisterConfigType(
            new StringConfigType("Dialogue", "shakra_text", (o, value) =>
            {
                o.GetComponent<MiscFixers.Npc>().text = value.GetValue();
            }).WithDefaultValue("Sample Text").WithPriority(-1)),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Needolin Dialogue", "needolin_on", (o, value) =>
            {
                if (value.GetValue()) return;
                o.RemoveComponentsInChildren<NeedolinTextOwner>();
            }).WithDefaultValue(true))
    ]);

    public static readonly List<ConfigType> Shakra = GroupUtils.Merge(Npcs, [
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Attack Enemies", "shakra_attack", (o, value) =>
            {
                if (value.GetValue()) return;
                UnityEngine.Object.Destroy(o.LocateMyFSM("Attack Enemies"));
            }).WithDefaultValue(true))
    ]);

    public static readonly List<ConfigType> Caretaker = GroupUtils.Merge(Npcs, [
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Call Out", "caretaker_call", (o, value) =>
            {
                o.GetComponent<MiscFixers.Caretaker>().hail = value.GetValue();
            }).WithDefaultValue(false))
    ]);

    public static readonly List<ConfigType> MapStateHook = GroupUtils.Merge(Generic, [
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Memory", "msh_state", (o, value) =>
            {
                o.GetComponent<MapStateHook>().memory = value.GetValue();
            }).WithDefaultValue(false))
    ]);

    public static readonly List<ConfigType> CameraBorder = GroupUtils.Merge(Generic, [
        ConfigurationManager.RegisterConfigType(
            new ChoiceConfigType("Border Type", "camera_border_type", (o, value) =>
            {
                o.GetComponent<CameraBorder>().type = value.GetValue();
            }).WithOptions("Left", "Right", "Top", "Bottom").WithDefaultValue(0)),
        ConfigurationManager.RegisterConfigType(
            new ChoiceConfigType("Active Mode", "camera_border_mode", (o, value) =>
            {
                o.GetComponent<CameraBorder>().activeType = value.GetValue();
            }).WithOptions("Both", "Gameplay", "Binoculars").WithDefaultValue(0))
    ]);

    public static readonly List<ConfigType> Wisp = GroupUtils.Merge(Visible, [
        ConfigurationManager.RegisterConfigType(new IntConfigType("Range Multiplier", "wisp_range", (o, value) =>
        {
            var col = o.transform.GetChild(0).GetComponent<CircleCollider2D>();
            if (value.GetValue() == 0) col.enabled = false;
            else col.radius *= value.GetValue();
        }).WithDefaultValue(1).WithPriority(-1))
    ]);

    public static readonly List<ConfigType> Roar = GroupUtils.Merge(Generic, [
        ConfigurationManager.RegisterConfigType(new FloatConfigType("Time", "roar_time", (o, value) =>
        {
            o.GetComponent<RoarEffect>().time = value.GetValue();
        }).WithDefaultValue(1)),
        ConfigurationManager.RegisterConfigType(new ChoiceConfigType("Size", "roar_size", (o, value) =>
        {
            o.GetComponent<RoarEffect>().small = value.GetValue() == 1;
        }).WithOptions("Large", "Small").WithDefaultValue(0))
    ]);

    public static readonly List<ConfigType> CloseableGates = GroupUtils.Merge(Visible, [
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Start Open", "gate_start_open", (o, value) =>
            {
                if (!value.GetValue()) return;
                o.GetComponent<Gate>().Opened();
            }).WithDefaultValue(false))
    ]);

    public static readonly List<ConfigType> Relay = [
        ConfigurationManager.RegisterConfigType(
            new StringConfigType("Relay ID", "relay_id", (o, value) =>
            {
                o.GetComponent<Relay>().id = value.GetValue();
            }).WithPriority(-1)),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Reset on Bench", "relay_bench_reset", (o, value) =>
            {
                o.GetComponent<Relay>().semiPersistent = value.GetValue();
            }).WithPriority(-1)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Delay", "relay_delay", (o, value) =>
            {
                o.GetComponent<Relay>().delay = value.GetValue();
            }).WithDefaultValue(0).WithPriority(-1)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Relay Chance", "relay_chance", (o, value) =>
            {
                o.GetComponent<Relay>().relayChance = value.GetValue();
            }).WithDefaultValue(1).WithPriority(-1)),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Trigger on Load", "relay_load_trigger", (o, value) =>
            {
                o.GetComponent<Relay>().broadcastImmediately = value.GetValue();
            }).WithDefaultValue(false).WithPriority(-1)),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Start Enabled", "relay_start_active", (o, value) =>
            {
                o.GetComponent<Relay>().startActivated = value.GetValue();
            }).WithDefaultValue(true).WithPriority(-1)),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("[Deprecated]", "relay_multiplayer", (_, _) => { }))
    ];
    
    public static readonly List<ConfigType> Timer =  GroupUtils.Merge(Generic, [
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Start Delay", "timer_start_delay", (o, value) =>
            {
                o.GetComponent<Timer>().startDelay = value.GetValue();
            }).WithDefaultValue(0)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Repeat Delay", "timer_repeat_delay", (o, value) =>
            {
                o.GetComponent<Timer>().repeatDelay = value.GetValue();
            }).WithDefaultValue(1)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Random Delay", "timer_rand_delay", (o, value) =>
            {
                o.GetComponent<Timer>().randDelay = value.GetValue();
            }).WithDefaultValue(0)),
        ConfigurationManager.RegisterConfigType(
            new IntConfigType("Max Calls", "timer_limit", (o, value) =>
            {
                o.GetComponent<Timer>().maxCalls = value.GetValue();
            }))
    ]);
    
    public static readonly List<ConfigType> Mutable =  GroupUtils.Merge(Visible, [
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Mute Sound", "mutable_muted", (o, value) =>
            {
                o.GetComponentInChildren<SoundMaker>().muted = value.GetValue();
            }).WithDefaultValue(false).WithPriority(-1))
    ]);
    
    public static readonly List<ConfigType> Coral =  GroupUtils.Merge(Visible, [
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Start Grown", "coral_start_up", (o, value) =>
            {
                o.GetComponent<ActivatingBase>().startActive = value.GetValue();
            }).WithDefaultValue(true))
    ]);
    
    public static readonly List<ConfigType> AbilityCrystal =  GroupUtils.Merge(Mutable, [
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Regen Time", "crystal_regen_time", (o, value) =>
            {
                o.GetComponent<AbilityCrystal>().regenTime = value.GetValue();
            }).WithDefaultValue(2.5f)),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Single Use", "crystal_single_use", (o, value) =>
            {
                o.GetComponent<AbilityCrystal>().singleUse = value.GetValue();
            }).WithDefaultValue(false).WithPriority(-1))
    ]);
    
    public static readonly List<ConfigType> Duplicator =  GroupUtils.Merge(Generic, [
        ConfigurationManager.RegisterConfigType(
            new IdConfigType("Object ID", "duplicator_id", (o, value) =>
            {
                o.GetComponent<ObjectDuplicator>().id = value.GetValue();
            }))
    ]);
    
    public static readonly List<ConfigType> Zaprock =  GroupUtils.Merge(Visible, [
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Start Delay", "zap_delay", (o, value) =>
            {
                ((Wait)o.LocateMyFSM("Control").GetState("Start Pause").Actions[0]).time = value.GetValue();
            }).WithDefaultValue(0.25f)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Cooldown", "zap_cooldown", (o, value) =>
            {
                var wait = (WaitRandom)o.LocateMyFSM("Control").GetState("Zap Pause").Actions[1];
                wait.timeMin = wait.timeMax = value.GetValue();
            }).WithDefaultValue(1))
    ]);

    public static readonly List<ConfigType> Levers = GroupUtils.Merge(Visible, [
        ConfigurationManager.RegisterConfigType(MakePersistenceConfigType("Stay Pulled", "lever_stay_pulled"))
    ]);
    
    public static readonly List<ConfigType> Buttons = GroupUtils.Merge(Visible, [
        ConfigurationManager.RegisterConfigType(MakePersistenceConfigType("Stay Pushed", "button_stay_pushed"))
    ]);

    public static readonly List<ConfigType> Frost = GroupUtils.Merge(Generic, [
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Freeze Speed", "frost_speed", (o, value) =>
            {
                o.GetComponent<FrostMarker>().frostSpeed = value.GetValue();
            }).WithDefaultValue(10)
        )
    ]);

    public static readonly List<ConfigType> Stretchable = GroupUtils.Merge(Generic, [
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Width", "size_width",
                (o, value) => { o.transform.SetScaleX(o.transform.GetScaleX() * value.GetValue()); },
                (o, value, _) => { o.transform.SetScaleX(o.transform.GetScaleX() * value.GetValue()); })),
        ConfigurationManager.RegisterConfigType(new FloatConfigType("Height", "size_height",
            (o, value) => { o.transform.SetScaleY(o.transform.GetScaleY() * value.GetValue()); },
            (o, value, _) => { o.transform.SetScaleY(o.transform.GetScaleY() * value.GetValue()); }))
    ]);

    public static readonly List<ConfigType> Updraft = GroupUtils.Merge(Visible, [
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Width", "updraft_width",
                (o, value) => { o.transform.SetScaleX(o.transform.GetScaleX() * value.GetValue()); },
                (o, value, _) => { o.transform.SetScaleX(o.transform.GetScaleX() * value.GetValue()); })
                .WithDefaultValue(1.5f).WithPriority(-1)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Height", "updraft_height",
                (o, value) => { o.transform.SetScaleY(o.transform.GetScaleY() * value.GetValue() * 2); },
                (o, value, _) => { o.transform.SetScaleY(o.transform.GetScaleY() * value.GetValue()); })
                .WithDefaultValue(5).WithPriority(-1))
    ]);
    
    public static readonly List<ConfigType> Wind =  GroupUtils.Merge(Stretchable, [
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Show Particles", "wind_particles", (o, value) =>
            {
                if (!value.GetValue()) return;
                o.GetComponent<Wind>().SetupParticles();
            }).WithDefaultValue(true)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Wind Speed", "wind_speed", (o, value) =>
            {
                o.GetComponent<Wind>().speed = value.GetValue() * 10;
            }).WithDefaultValue(3).WithPriority(-1)),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Affects Player", "wind_affects_player", (o, value) =>
            {
                o.GetComponent<Wind>().affectsPlayer = value.GetValue();
            }).WithDefaultValue(true)),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Affects Enemies", "wind_affects_enemies", (o, value) =>
            {
                o.GetComponent<Wind>().affectsEnemies = value.GetValue();
            }).WithDefaultValue(true)),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Affects Projectiles", "wind_affects_projectiles", (o, value) =>
            {
                o.GetComponent<Wind>().affectsProjectiles = value.GetValue();
            }).WithDefaultValue(true)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Particle R", "wind_colour_r", (o, value) =>
            {
                o.GetComponent<Wind>().r = value.GetValue();
            }, (o, value, arg3) =>
            {
                if (arg3 == ConfigurationManager.PreviewContext.Cursor) return;
                var sr = o.GetComponent<SpriteRenderer>();
                var color = sr.color;
                color.r = value.GetValue();
                sr.color = color;
            }).WithDefaultValue(1).WithPriority(-1)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Particle G", "wind_colour_g", (o, value) =>
            {
                o.GetComponent<Wind>().g = value.GetValue();
            }, (o, value, arg3) =>
            {
                if (arg3 == ConfigurationManager.PreviewContext.Cursor) return;
                var sr = o.GetComponent<SpriteRenderer>();
                var color = sr.color;
                color.g = value.GetValue();
                sr.color = color;
            }).WithDefaultValue(1).WithPriority(-1)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Particle B", "wind_colour_b", (o, value) =>
            {
                o.GetComponent<Wind>().b = value.GetValue();
            }, (o, value, arg3) =>
            {
                if (arg3 == ConfigurationManager.PreviewContext.Cursor) return;
                var sr = o.GetComponent<SpriteRenderer>();
                var color = sr.color;
                color.b = value.GetValue();
                sr.color = color;
            }).WithDefaultValue(1).WithPriority(-1)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Particle A", "wind_colour_a", (o, value) =>
            {
                o.GetComponent<Wind>().a = value.GetValue();
            }).WithDefaultValue(1).WithPriority(-1))
    ]);

    public static readonly List<ConfigType> Slope = GroupUtils.Merge(Stretchable, [
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Slide Speed", "slope_speed",
                (o, value) =>
                {
                    var val = value.GetValue();
                    o.GetComponent<SlideSurface>().shallowSlideSpeed *= val;
                    o.GetComponent<SlideSurface>().steepSlideSpeed *= val;
                }).WithDefaultValue(1)),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Show Particles", "slope_particles",
                (o, value) =>
                {
                    if (value.GetValue()) return;
                    o.GetComponent<SlideSurface>().slideParticles = [null, null, null];
                }).WithDefaultValue(true))
    ]);

    public static readonly List<ConfigType> Water = GroupUtils.Merge(Stretchable, [
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Maggoted", "water_maggot",
                (o, value) =>
                {
                    o.GetComponent<MiscFixers.Water>().maggot = value.GetValue();
                }).WithDefaultValue(false).WithPriority(-1)),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Voided", "water_void",
                (o, value) =>
                {
                    o.GetComponent<MiscFixers.Water>().abyss = value.GetValue();
                    o.transform.GetChild(0).gameObject.LocateMyFSM("Volt Travel").enabled = false;
                }).WithDefaultValue(false).WithPriority(-1)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Flow Speed", "water_flow_speed",
                (o, value) =>
                {
                    o.GetComponent<SurfaceWaterRegion>().flowSpeed = value.GetValue();
                }).WithDefaultValue(0).WithPriority(-1))
    ]);

    public static readonly List<ConfigType> DreamBlock = GroupUtils.Merge(Visible, [
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Show Particles", "show_particles_dream",
                (o, value) =>
                {
                    if (!value.GetValue()) return;
                    o.GetComponent<DreamBlock>().SetupParticles();
                }).WithDefaultValue(true))
    ]);

    public static readonly List<ConfigType> CradlePlat = GroupUtils.Merge(Visible, [
        ConfigurationManager.RegisterConfigType(MakePersistenceConfigType("Stay Moved", "cradle_plat_stay")
            .WithDefaultValue(0)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Move Distance (Y)", "cradle_plat_dist_y",
                (o, value) =>
                {
                    o.GetComponent<CrankPlat>().endPoint.transform.SetLocalPositionY(value.GetValue());
                }).WithPriority(-1)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Move Distance (X)", "cradle_plat_dist_x",
                (o, value) =>
                {
                    o.GetComponent<CrankPlat>().endPoint.transform.SetLocalPositionX(value.GetValue());
                }).WithPriority(-1)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Hit Force", "cradle_plat_force",
                (o, value) =>
                {
                    o.GetComponent<CrankPlat>().hitForce = value.GetValue();
                }).WithPriority(-1)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Max Hit Speed", "cradle_max_speed",
                (o, value) =>
                {
                    o.GetComponent<CrankPlat>().maxHitSpeed = value.GetValue();
                }).WithPriority(-1)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Max Return Speed", "cradle_max_return",
                (o, value) =>
                {
                    o.GetComponent<CrankPlat>().maxReturnSpeed = value.GetValue();
                }).WithPriority(-1))
    ]);

    private static readonly string[] CrumbleHazardStates = ["Drop Antic", "Drop", "Return Antic"];
    public static readonly List<ConfigType> CrumblePlat = GroupUtils.Merge(Visible, [
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Respawn on Hazard", "crumble_hazard_respawn",
                (o, value) =>
                {
                    if (value.GetValue()) return;
                    var fsm = o.LocateMyFSM("Control");

                    foreach (var s in CrumbleHazardStates)
                    {
                        var drop = fsm.GetState(s);
                        drop.transitions = drop.transitions
                            .Where(trans => trans.EventName != "HAZARD RESPAWNED").ToArray();
                    }
                }).WithDefaultValue(true)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Respawn Time", "crumble_respawn_time",
                (o, value) =>
                {
                    o.LocateMyFSM("Control").FsmVariables.FindFsmFloat("Return Time").Value = value.GetValue();
                }).WithDefaultValue(3))
    ]);

    public static readonly List<ConfigType> SilkSpool = GroupUtils.Merge(Visible, [
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Stay Used", "spool_used",
                    (o, value) =>
                    {
                        if (value.GetValue()) return;
                        o.RemoveComponent<PersistentIntItem>();
                    })
                .WithDefaultValue(false))
    ]);

    public static readonly List<ConfigType> SilkLever = GroupUtils.Merge(Visible, [
        ConfigurationManager.RegisterConfigType(MakePersistenceConfigType("Stay Pulled", "silk_lever_stay", (o, item) =>
        {
            o.GetComponent<Lever>().persistent = item;
            
            item.OnSetSaveState += value =>
            {
                if (!value) return;
                EventManager.BroadcastEvent(o, "OnPull");
                EventManager.BroadcastEvent(o, "LoadedPulled");
            };
        }))
    ]);

    private static readonly ConfigType ZOffset =
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Z Offset", "obj_z",
                (o, value) => { o.transform.SetPositionZ(o.transform.GetPositionZ() + value.GetValue()); },
                (o, value, arg3) =>
                {
                    if (arg3 == ConfigurationManager.PreviewContext.Cursor)
                    {
                        CursorManager.Offset.z += value.GetValue();
                    }
                    else o.transform.SetPositionZ(o.transform.GetPositionZ() + value.GetValue());
                }).WithDefaultValue(0));

    public static readonly List<ConfigType> Decorations = GroupUtils.Merge(Visible, [
        ConfigurationManager.RegisterConfigType(
            new IntConfigType("Render Layer", "obj_layer",
                (o, value) =>
                {
                    foreach (var comp in o.GetComponentsInChildren<Renderer>())
                        comp.sortingOrder = value.GetValue();
                },
                (o, value, _) => { o.GetComponent<SpriteRenderer>().sortingOrder = value.GetValue(); })
                .WithDefaultValue(0)),
        ZOffset
    ]);

    public static readonly List<ConfigType> PersistentBreakable = GroupUtils.Merge(Visible, [
        ConfigurationManager.RegisterConfigType(MakePersistenceConfigType("Stay Broken", "breakable_stay"))
    ]);

    private static readonly ConfigType IsBreakable = ConfigurationManager.RegisterConfigType(new BoolConfigType(
        "Breakable", "breakable_on", (o, value) =>
        {
            if (!value.GetValue()) o.RemoveComponentsInChildren<Breakable>();
        }).WithDefaultValue(true));
    
    public static readonly List<ConfigType> BreakableDecor = GroupUtils.Merge(Decorations, [IsBreakable]);

    public static readonly List<ConfigType> WispLanterns = GroupUtils.Merge(PersistentBreakable, [IsBreakable]);

    public static readonly List<ConfigType> BreakableWall = GroupUtils.Merge(PersistentBreakable, [
        ConfigurationManager.RegisterConfigType(new IntConfigType("Required Hits", "breakable_hits", (o, value) =>
        {
            o.LocateMyFSM("breakable_wall_v2").FsmVariables.FindFsmInt("Hits").Value = value.GetValue();
        }).WithDefaultValue(4))
    ]);

    public static readonly List<ConfigType> LifebloodCocoons = GroupUtils.Merge(PersistentBreakable, [
        ConfigurationManager.RegisterConfigType(new IntConfigType("Lifeseed Count", "lifeblood_count", (o, value) =>
        {
            var fling = o.GetComponent<HealthCocoon>().flingPrefabs[2];
            fling.MinAmount = fling.MaxAmount = value.GetValue();
        }).WithDefaultValue(2).WithPriority(-1))
    ]);

    public static readonly List<ConfigType> TrackPoint = [
        ConfigurationManager.RegisterConfigType(
            new StringConfigType("Track ID", "track_id", (o, value) =>
            {
                o.GetComponent<SplineObjects.SplinePoint>().id = value.GetValue();
            }).WithDefaultValue("1").WithPriority(-1))
    ];

    public static readonly List<ConfigType> TrackStartPoint = GroupUtils.Merge(Visible, GroupUtils.Merge(TrackPoint, [
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Speed", "track_speed", (o, value) =>
            {
                o.GetComponent<SplineObjects.Spline>().speed = value.GetValue();
            }).WithDefaultValue(10)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Colour R", "track_r", (o, value) =>
            {
                o.GetComponent<SplineObjects.Spline>().r = value.GetValue();
            }).WithDefaultValue(1)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Colour G", "track_g", (o, value) =>
            {
                o.GetComponent<SplineObjects.Spline>().g = value.GetValue();
            }).WithDefaultValue(1)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Colour B", "track_b", (o, value) =>
            {
                o.GetComponent<SplineObjects.Spline>().b = value.GetValue();
            }).WithDefaultValue(1)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Colour A", "track_a", (o, value) =>
            {
                o.GetComponent<SplineObjects.Spline>().a = value.GetValue();
            }).WithDefaultValue(0.1f)),
        ZOffset
    ]));

    private static readonly int Terrain = LayerMask.NameToLayer("Terrain");
    private static readonly int Default = LayerMask.NameToLayer("Default");

    public static readonly List<ConfigType> Colliders = GroupUtils.Merge(Decorations, [
        ConfigurationManager.RegisterConfigType(
            new ChoiceConfigType("Collision Type", "collider_type", (o, value) =>
                {
                    switch (value.GetValue())
                    {
                        case 0:
                            o.RemoveComponentsInChildren<Collider2D>();
                            break;
                        case 1:
                            o.layer = Default;
                            var collider = o.GetComponentInChildren<Collider2D>();
                            collider.isTrigger = true;
                            collider.gameObject.AddComponent<CustomDamager>().damageAmount = 1;
                            break;
                        case 2:
                            o.GetComponentInChildren<Collider2D>().isTrigger = false;
                            o.layer = Terrain;
                            break;
                        case 3:
                            o.layer = Terrain;
                            var col = o.GetComponentInChildren<Collider2D>();
                            col.gameObject.AddComponent<PlatformEffector2D>().surfaceArc = 120;
                            col.isTrigger = false;
                            col.usedByEffector = true;
                            break;
                        case 4:
                            o.layer = Default;
                            o.GetComponentInChildren<Collider2D>().isTrigger = false;
                            break;
                    }
                }
            ).WithOptions("None", "Hazard", "Solid", "Semi-Solid", "Barrier")),
        
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Fallthrough Time", "collider_fallthrough",
                (o, value) =>
                {
                    o.AddComponent<Fallthrough>().fallthroughTime = value.GetValue();
                }
            ))
    ]);

    private static readonly ConfigType AlphaColour = ConfigurationManager.RegisterConfigType(
        new FloatConfigType("Colour A", "colour_alpha", (o, value) =>
        {
            var sr = o.GetComponent<SpriteRenderer>();
            var color = sr.color;
            color.a = value.GetValue();
            sr.color = color;
        }).WithDefaultValue(1));
    public static readonly List<ConfigType> Colours = GroupUtils.Merge(Stretchable, GroupUtils.Merge(Colliders, [
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Colour R", "colour_red", (o, value) =>
            {
                var sr = o.GetComponent<SpriteRenderer>();
                var color = sr.color;
                color.r = value.GetValue();
                sr.color = color;
            }, (o, value, arg3) =>
            {
                if (arg3 == ConfigurationManager.PreviewContext.Cursor) return;
                var sr = o.GetComponent<SpriteRenderer>();
                var color = sr.color;
                color.r = value.GetValue();
                sr.color = color;
            }).WithDefaultValue(1)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Colour G", "colour_green", (o, value) =>
            {
                var sr = o.GetComponent<SpriteRenderer>();
                var color = sr.color;
                color.g = value.GetValue();
                sr.color = color;
            }, (o, value, arg3) =>
            {
                if (arg3 == ConfigurationManager.PreviewContext.Cursor) return;
                var sr = o.GetComponent<SpriteRenderer>();
                var color = sr.color;
                color.g = value.GetValue();
                sr.color = color;
            }).WithDefaultValue(1)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Colour B", "colour_blue", (o, value) =>
            {
                var sr = o.GetComponent<SpriteRenderer>();
                var color = sr.color;
                color.b = value.GetValue();
                sr.color = color;
            }, (o, value, arg3) =>
            {
                if (arg3 == ConfigurationManager.PreviewContext.Cursor) return;
                var sr = o.GetComponent<SpriteRenderer>();
                var color = sr.color;
                color.b = value.GetValue();
                sr.color = color;
            }).WithDefaultValue(1)),
        AlphaColour
    ]));
    
    public static readonly List<ConfigType> Line = GroupUtils.Merge(Colliders, [
        ConfigurationManager.RegisterConfigType(
            new StringConfigType("Point Set ID", "point_id", (o, value) =>
            {
                o.GetComponent<LineObject>().id = value.GetValue();
            }).WithDefaultValue("1").WithPriority(-1)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Visual Width", "line_width", (o, value) =>
            {
                o.GetComponent<LineObject>().width = value.GetValue();
            }).WithDefaultValue(0.2f)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Colour R", "line_red", (o, value) =>
            {
                o.GetComponent<LineObject>().r = value.GetValue();
            }).WithDefaultValue(1)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Colour G", "line_green", (o, value) =>
            {
                o.GetComponent<LineObject>().g = value.GetValue();
            }).WithDefaultValue(1)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Colour B", "line_blue", (o, value) =>
            {
                o.GetComponent<LineObject>().b = value.GetValue();
            }).WithDefaultValue(1)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Colour A", "line_alpha", (o, value) =>
            {
                o.GetComponent<LineObject>().a = value.GetValue();
            }).WithDefaultValue(1))
    ]);
    
    public static readonly List<ConfigType> Colourer = [
        ConfigurationManager.RegisterConfigType(
            new IdConfigType("Object ID", "colourer_target", (o, value) =>
            {
                o.GetComponent<ObjectColourer>().targetId = value.GetValue();
            })),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Start Applied", "colourer_active", (o, value) =>
            {
                o.GetComponent<ObjectColourer>().startApplied = value.GetValue();
            }).WithDefaultValue(true)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Colour R", "colourer_red", (o, value) =>
            {
                o.GetComponent<ObjectColourer>().r = value.GetValue();
            }).WithDefaultValue(1)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Colour G", "colourer_green", (o, value) =>
            {
                o.GetComponent<ObjectColourer>().g = value.GetValue();
            }).WithDefaultValue(1)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Colour B", "colourer_blue", (o, value) =>
            {
                o.GetComponent<ObjectColourer>().b = value.GetValue();
            }).WithDefaultValue(1)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Colour A", "colourer_alpha", (o, value) =>
            {
                var oc = o.GetComponent<ObjectColourer>();
                oc.useAlpha = true;
                oc.a = value.GetValue();
            })),
        ConfigurationManager.RegisterConfigType(
            new ChoiceConfigType("Tint Mode", "colourer_mode", (o, value) =>
            {
                o.GetComponent<ObjectColourer>().directSet = value.GetValue() == 1;
            }).WithOptions("Multiply", "Set").WithDefaultValue(0))
    ];

    public static readonly List<ConfigType> Gravity = GroupUtils.Merge(Visible, [
        ConfigurationManager.RegisterConfigType(new FloatConfigType("Gravity Scale", "gravity_scale",
            (o, value) =>
            {
                o.AddComponent<GravityLock>().level = value.GetValue();
            }
        ))
    ]);

    public class GravityLock : MonoBehaviour
    {
        public float level;
        public Rigidbody2D rb2d;

        private void Start()
        {
            rb2d = gameObject.GetOrAddComponent<Rigidbody2D>();
        }

        private void Update()
        {
            rb2d.gravityScale = level;
            rb2d.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    public static readonly List<ConfigType> TriggerActivator = GroupUtils.Merge(Gravity, [
        ConfigurationManager.RegisterConfigType(new IntConfigType("Trigger Layer", "activator_layer",
            (o, value) =>
            {
                o.GetComponent<MiscFixers.TriggerActivator>().layer = value.GetValue();
            }
        ))
    ]);

    public static readonly List<ConfigType> FakePerformance = GroupUtils.Merge(Generic, [
            ConfigurationManager.RegisterConfigType(new FloatConfigType("Range Multiplier", "perform_range",
                (o, value) =>
                {
                    o.GetComponent<FakePerformanceRegion>().rangeMult = value.GetValue();
                }
            ).WithDefaultValue(1))
    ]);

    public static readonly List<ConfigType> PlayerDataSetter = GroupUtils.Merge(Generic, [
            ConfigurationManager.RegisterConfigType(new StringConfigType("Data ID", "pd_id",
                (o, value) =>
                {
                    o.GetComponent<PlayerDataSetter>().dataName = value.GetValue();
                }
            )),
            ConfigurationManager.RegisterConfigType(new BoolConfigType("Set/Compare Value", "pd_value",
                (o, value) =>
                {
                    o.GetComponent<PlayerDataSetter>().value = value.GetValue();
                }
            ))
    ]);

    public static readonly List<ConfigType> Fleas = GroupUtils.Merge(Decorations, [
            ConfigurationManager.RegisterConfigType(new BoolConfigType("Fly Away", "flea_flee",
                (o, value) =>
                {
                    if (value.GetValue()) return;
                    var leave = o.LocateMyFSM("Call Out").GetState("Leave Antic");
                    leave.transitions = [];
                    ((Tk2dPlayAnimationWithEvents)leave.actions[2]).clipName = "RescueToFly";
                }
            ).WithDefaultValue(true))
    ]);

    public static readonly List<ConfigType> Benches = GroupUtils.Merge(Visible, [
        ConfigurationManager.RegisterConfigType(new BoolConfigType("Save Respawn", "bench_spawn",
            (o, value) =>
            {
                if (value.GetValue()) return;
                var burst = o.GetComponentsInChildren<PlayMakerFSM>()
                    .First(fsm => fsm.FsmName == "Bench Control").GetState("Rest Burst");
                burst.DisableAction(9);
                burst.DisableAction(10);
                burst.DisableAction(11);
            }
        ).WithDefaultValue(true))
    ]);

    public static readonly List<ConfigType> WalkTarget = GroupUtils.Merge(Generic, [
        ConfigurationManager.RegisterConfigType(new FloatConfigType("Speed Override", "walk_speed", 
                (o, value) =>
                {
                    o.GetComponent<WalkTarget>().speed = Mathf.Abs(value.GetValue());
                }
            ).WithPriority(1)),
        ConfigurationManager.RegisterConfigType(new ChoiceConfigType("Mode", "walk_anim", 
                (o, value) =>
                {
                    var wt = o.GetComponent<WalkTarget>();
                    wt.anim = value.GetStringValue();

                    wt.speed = value.GetValue() switch
                    {
                        1 => 8.5f,
                        2 => 20,
                        _ => 6
                    };
                }
            ).WithDefaultValue(0).WithOptions("Walk", "Run", "Sprint"))
    ]);

    public static readonly List<ConfigType> ObjectSpinner = GroupUtils.Merge(Generic, [
        ConfigurationManager.RegisterConfigType(new IdConfigType("Object ID", "spinner_target", 
                (o, value) => 
                {
                    o.GetComponent<ObjectSpinner>().targetId = value.GetValue(); 
                }
            )),
            ConfigurationManager.RegisterConfigType(new FloatConfigType("Rotation Speed", "spinner_speed", 
                (o, value) =>
                {
                    o.GetComponent<ObjectSpinner>().speed = value.GetValue();
                }
            ).WithDefaultValue(100))
    ]);

    public static readonly List<ConfigType> ObjectAnchor = GroupUtils.Merge(Generic, [
            ConfigurationManager.RegisterConfigType(new IdConfigType("Object ID", "anchor_target", 
                (o, value) => 
                {
                    o.GetComponent<ObjectAnchor>().targetId = value.GetValue();
                }
            )),
            ConfigurationManager.RegisterConfigType(new IdConfigType("Parent ID (Optional)", "anchor_parent", 
                (o, value) => 
                {
                    o.GetComponent<ObjectAnchor>().parentId = value.GetValue();
                }
            )),
            ConfigurationManager.RegisterConfigType(new FloatConfigType("Path Distance", "anchor_dist", 
                (o, value) => 
                {
                    o.GetComponent<ObjectAnchor>().trackDistance = value.GetValue();
                }
            ).WithDefaultValue(10)),
            ConfigurationManager.RegisterConfigType(new FloatConfigType("Start Distance", "anchor_start_dist", 
                (o, value) =>
                {
                    o.GetComponent<ObjectAnchor>().startOffset = value.GetValue();
                }
            ).WithDefaultValue(0)),
            ConfigurationManager.RegisterConfigType(new FloatConfigType("Move Speed", "anchor_speed", 
                (o, value) =>
                {
                    o.GetComponent<ObjectAnchor>().speed = value.GetValue();
                }
            ).WithDefaultValue(0)),
            ConfigurationManager.RegisterConfigType(new FloatConfigType("Path Rotation", "anchor_rot", 
                (o, value) =>
                {
                    o.GetComponent<ObjectAnchor>().startRotation = value.GetValue();
                }
            ).WithDefaultValue(0)),
            ConfigurationManager.RegisterConfigType(new FloatConfigType("Path Rotation over Time", "anchor_rot_speed", 
                (o, value) =>
                {
                    o.GetComponent<ObjectAnchor>().rotationSpeed = value.GetValue();
                }
            ).WithDefaultValue(0)),
            ConfigurationManager.RegisterConfigType(new FloatConfigType("Smoothing", "anchor_smoothing", 
                (o, value) =>
                {
                    o.GetComponent<ObjectAnchor>().smoothing = value.GetValue();
                }
            ).WithDefaultValue(0.5f)),
            ConfigurationManager.RegisterConfigType(new FloatConfigType("Pause Time", "anchor_pause_time", 
                (o, value) =>
                {
                    o.GetComponent<ObjectAnchor>().pauseTime = value.GetValue();
                }
            ).WithDefaultValue(0)),
            ConfigurationManager.RegisterConfigType(new BoolConfigType("Stick Player", "anchor_stick", 
                (o, value) =>
                {
                    o.GetComponent<ObjectAnchor>().stickPlayer = value.GetValue();
                }
            )),
            ConfigurationManager.RegisterConfigType(new BoolConfigType("Start Moving", "anchor_moving", 
                (o, value) =>
                {
                    o.GetComponent<ObjectAnchor>().moving = value.GetValue();
                }
            ).WithDefaultValue(true))
    ]);

    public static readonly List<ConfigType> ObjectMover = GroupUtils.Merge(Generic, [
            ConfigurationManager.RegisterConfigType(new IdConfigType("Object ID", "mover_target", 
                (o, value) => 
                {
                    o.GetComponent<ObjectMover>().targetId = value.GetValue();
                }
            )),
            ConfigurationManager.RegisterConfigType(new FloatConfigType("X Offset", "mover_x_offset", 
                (o, value) => 
                {
                    o.GetComponent<ObjectMover>().xOffset = value.GetValue();
                }
            ).WithDefaultValue(0)),
            ConfigurationManager.RegisterConfigType(new FloatConfigType("Y Offset", "mover_y_offset", 
                (o, value) => 
                {
                    o.GetComponent<ObjectMover>().yOffset = value.GetValue();
                }
            ).WithDefaultValue(0)),
            ConfigurationManager.RegisterConfigType(new FloatConfigType("Rotation", "mover_rot", 
                (o, value) => 
                {
                    o.GetComponent<ObjectMover>().rotation = value.GetValue();
                }
            ).WithDefaultValue(0)),
            ConfigurationManager.RegisterConfigType(new BoolConfigType("Clear Velocity", "mover_clear_vel", 
                (o, value) => 
                {
                    o.GetComponent<ObjectMover>().clearVelocity = value.GetValue();
                }
            ).WithDefaultValue(true)),
            ConfigurationManager.RegisterConfigType(new ChoiceConfigType("Position Source", "mover_mode", 
                (o, value) => 
                {
                    o.GetComponent<ObjectMover>().moveMode = value.GetValue();
                }
            ).WithOptions("Mover", "Self", "Player").WithDefaultValue(0)),
            ConfigurationManager.RegisterConfigType(new IdConfigType("Position Source ID", "mover_mode_2", 
                (o, value) =>
                {
                    o.GetComponent<ObjectMover>().moveTarget = value.GetValue();
                }
            ).WithPriority(1))
    ]);

    private static readonly ConfigType Invincible =
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Invincible", "invulnerable",
                (o, value) =>
                {
                    if (!value.GetValue()) return;
                    o.AddComponent<EnemyInvulnerabilityMarker>();
                }));

    public static readonly List<ConfigType> SimpleEnemies = GroupUtils.Merge(Visible, [
        ConfigurationManager.RegisterConfigType(
            new IntConfigType("Shell Shard Drops", "shell_shard",
                (o, value) => { o.GetComponentInChildren<HealthManager>(true).SetShellShards(value.GetValue()); })),
        ConfigurationManager.RegisterConfigType(
            new IntConfigType("Small Rosary Drops", "small_money",
                (o, value) => { o.GetComponentInChildren<HealthManager>(true).SetGeoSmall(value.GetValue()); })),
        ConfigurationManager.RegisterConfigType(
            new IntConfigType("Medium Rosary Drops", "med_money",
                (o, value) => { o.GetComponentInChildren<HealthManager>(true).SetGeoMedium(value.GetValue()); })),
        ConfigurationManager.RegisterConfigType(
            new IntConfigType("Large Rosary Drops", "large_money",
                (o, value) => { o.GetComponentInChildren<HealthManager>(true).SetGeoLarge(value.GetValue()); })),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Give Silk", "give_silk",
                (o, value) => { o.GetComponentInChildren<HealthManager>(true).doNotGiveSilk = !value.GetValue(); })),
        Invincible
    ]);

    public static readonly List<ConfigType> Garpid = GroupUtils.Merge(Visible, [
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Loop", "garpid_loop",
                (o, value) =>
                {
                    if (!value.GetValue()) return;
                    var fsm = o.LocateMyFSM("Control");
                    fsm.GetState("Dormant").AddAction(() => fsm.SendEvent("ALERT"));
                }).WithDefaultValue((true))),
        Invincible
    ]);

    public static readonly List<ConfigType> NonPersistentEnemies = GroupUtils.Merge(SimpleEnemies, [
        ConfigurationManager.RegisterConfigType(
            new IntConfigType("Health", "enemy_hp",
                (o, value) =>
                {
                    o.GetComponentInChildren<HealthManager>(true).hp = value.GetValue();
                }).WithPriority(-1)),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Enable Health Scaling", "enemy_hp_scale",
                (o, value) =>
                {
                    if (value.GetValue()) return;
                    o.AddComponent<EnemyFixers.DisableHealthScaling>();
                }).WithDefaultValue(true))
    ]);

    public static readonly List<ConfigType> Enemies = GroupUtils.Merge(NonPersistentEnemies, [
        ConfigurationManager.RegisterConfigType(MakePersistenceConfigType("Stay Dead", "enemy_stay_dead", 
            (o, item) => {
                item.OnSetSaveState += b =>
                {
                    o.GetComponent<HealthManager>().isDead = b;
                    if (b) item.SetValueOverride(true);
                };
                item.OnGetSaveState += (out bool b) => { b = o.GetComponent<HealthManager>().isDead; };
            }))
    ]);

    public static readonly List<ConfigType> Lilypad = GroupUtils.Merge(NonPersistentEnemies, [
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Is Trap", "nuphar_mode", (o, value) =>
            {
                o.LocateMyFSM("Control").FsmVariables.FindFsmBool("Is Enemy").value = value.GetValue();
            }).WithDefaultValue(false))
    ]);

    public static readonly List<ConfigType> Furm = GroupUtils.Merge(Enemies, [
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Silver Chance", "furm_silver", (o, value) =>
            {
                var silver = o.LocateMyFSM("Control").GetState("Silver?");
                silver.DisableAction(1);
                var eve = (SendRandomEventV4)silver.actions[2];
                eve.enabled = true;
                var val = Mathf.Clamp(value.GetValue(), 0, 1);
                eve.weights[0] = 1 - val;
                eve.weights[1] = val;
            }).WithDefaultValue(0))
    ]);

    public static readonly List<ConfigType> WingedFurm = GroupUtils.Merge(Enemies, [
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Target X Offset", "winged_furm_x", (o, value) =>
            {
                var mp = o.LocateMyFSM("Tween").FsmVariables.FindFsmVector3("Move Pos");
                var mpPos = mp.Value;
                mpPos.x = o.transform.GetPositionX() + value.GetValue();
                mp.Value = mpPos;
            }).WithDefaultValue(0)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Target Y Offset", "winged_furm_y", (o, value) =>
            {
                var mp = o.LocateMyFSM("Tween").FsmVariables.FindFsmVector3("Move Pos");
                var mpPos = mp.Value;
                mpPos.y = o.transform.GetPositionY() + value.GetValue();
                mp.Value = mpPos;
            }).WithDefaultValue(5))
    ]);

    public static readonly List<ConfigType> YPatroller = GroupUtils.Merge(Enemies, [
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Target Y Offset", "patroller_y", (o, value) =>
            {
                o.GetComponent<EnemyFixers.PatrollerFix>().yOffset = value.GetValue();
            }).WithDefaultValue(0).WithPriority(-1))
    ]);

    public static readonly List<ConfigType> Patroller = GroupUtils.Merge(YPatroller, [
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Target X Offset", "patroller_x", (o, value) =>
            {
                o.GetComponent<EnemyFixers.PatrollerFix>().xOffset = value.GetValue();
            }).WithDefaultValue(5).WithPriority(-1))
    ]);
    
    public static readonly List<ConfigType> HugeFlea = GroupUtils.Merge(Enemies, [
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Count as Flea", "huge_flea_count",
                (o, value) =>
                {
                    if (value.GetValue()) return;
                    o.LocateMyFSM("Control").GetState("Stun").DisableAction(0);
                }).WithDefaultValue(true))
    ]);
    
    public static readonly List<ConfigType> Bosses = GroupUtils.Merge(Enemies, [
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Show Boss Title", "boss_title",
                (o, value) =>
                {
                    if (value.GetValue()) return;
                    o.GetOrAddComponent<EnemyFixers.DisableBossTitle>();
                }).WithDefaultValue(true))
    ]);

    private static readonly ConfigType DamagesEnemies = ConfigurationManager.RegisterConfigType(
        new BoolConfigType("Damages Enemies", "damages_enemies",
            (o, value) =>
            {
                if (value.GetValue()) return;
                o.RemoveComponentsInChildren<DamageEnemies>();
            })
    );

    public static readonly List<ConfigType> GarmondBoss = GroupUtils.Merge(Enemies, [
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Lethal Damage", "garmond_lethal",
                (o, value) =>
                {
                    if (!value.GetValue()) return;
                    foreach (var comp in o.GetComponentsInChildren<DamageHero>(true))
                        comp.damagePropertyFlags &= ~DamagePropertyFlags.NonLethal;
                }).WithDefaultValue(true)),
        DamagesEnemies
    ]);

    public static readonly List<ConfigType> Moorwing = GroupUtils.Merge(Bosses, [DamagesEnemies]);
    
    public static readonly List<ConfigType> Boran = GroupUtils.Merge(Enemies, [DamagesEnemies]);

    public static readonly List<ConfigType> LeafRoller = GroupUtils.Merge(Enemies, [
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Hidden", "hidden_leaf_roller",
                (o, value) =>
                {
                    if (!value.GetValue()) return;
                    var anim = o.GetComponent<tk2dSpriteAnimator>();
                    anim.defaultClipId = anim.GetClipIdByName("Hidden");
                }).WithDefaultValue(false))
    ]);

    public static readonly List<ConfigType> CameraShaker = GroupUtils.Merge(Generic, [
        ConfigurationManager.RegisterConfigType(
            new ChoiceConfigType("Shake Type", "camera_shake_type",
                (o, value) =>
                {
                    o.GetComponent<CameraShaker>().shakeType = value.GetValue();
                }).WithOptions("Tiny", "Small", "Medium", "Large").WithDefaultValue(2)
        )
    ]);

    public static readonly List<ConfigType> SavageBeastfly = GroupUtils.Merge(Bosses, [
        ConfigurationManager.RegisterConfigType(
            new ChoiceConfigType("Start Phase", "beastfly_phase",
                (o, value) =>
                {
                    var val = value.GetValue();
                    if (val == 0) return;
                    
                    var setHp = o.LocateMyFSM("Control").GetState("Set HP");
                    setHp.DisableAction(1);
                    if (val == 2) setHp.DisableAction(3);
                }).WithOptions("Phase 1", "Phase 2", "Phase 3").WithDefaultValue(0).WithPriority(-1))
    ]);

    public static readonly List<ConfigType> MossMother = GroupUtils.Merge(Bosses, [
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Follow Player", "moss_mother_align",
                (o, value) =>
                {
                    if (value.GetValue()) return;
                    o.LocateMyFSM("Control").GetState("Idle").DisableAction(0);
                }).WithDefaultValue(true))
    ]);

    public static readonly List<ConfigType> LastJudge = GroupUtils.Merge(Bosses, [
        ConfigurationManager.RegisterConfigType(
            new ChoiceConfigType("Start Phase", "lj_phase",
                (o, value) =>
                {
                    var val = value.GetValue();
                    if (val == 0) return;
                    var fsm = o.LocateMyFSM("Control");
                    var init = fsm.GetState("Init");
                    init.DisableAction(12);
                    init.DisableAction(13);
                    fsm.FsmVariables.FindFsmInt("HP P2").Value = int.MaxValue;
                    if (val == 2) fsm.FsmVariables.FindFsmInt("HP P3").Value = int.MaxValue;
                }).WithOptions("Phase 1", "Phase 2", "Phase 3").WithDefaultValue(0))
    ]);

    static ConfigGroup()
    {
        typeof(HealthManager).Hook(nameof(HealthManager.IsBlockingByDirection),
            (Func<HealthManager, int, AttackTypes, SpecialTypes, bool> orig, HealthManager self, int cardinalDirection,
                    AttackTypes attackType, SpecialTypes specialType) =>
                self.GetComponentInParent<EnemyInvulnerabilityMarker>() ||
                orig(self, cardinalDirection,
                    attackType, specialType));
    }

    private class EnemyInvulnerabilityMarker : MonoBehaviour;

    public static readonly List<ConfigType> Wakeable = GroupUtils.Merge(Enemies, [
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Start Awake", "mossgrub_walk", (o, value) =>
            {
                if (!value.GetValue()) return;
                o.GetComponent<EnemyFixers.Wakeable>().DoWake();
            }).WithDefaultValue(true))
    ]);

    public static readonly List<ConfigType> Velocity = GroupUtils.Merge(Gravity, [
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("X Velocity", "velocity_apply_x", (o, value) =>
            {
                o.GetOrAddComponent<VelocityApplier>().x = value.GetValue();
            }).WithDefaultValue(10)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Y Velocity", "velocity_apply_y", (o, value) =>
            {
                o.GetOrAddComponent<VelocityApplier>().y = value.GetValue();
            }).WithDefaultValue(0))
    ]);
    
    public static readonly List<ConfigType> Watcher = GroupUtils.Merge(Wakeable, GroupUtils.Merge(Bosses, [
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Nook Y Offset", "watcher_nook_y", (o, value) =>
            {
                o.LocateMyFSM("Control").FsmVariables.FindFsmFloat("Cliff Y")
                    .value = o.transform.GetPositionY() + value.GetValue();
            }).WithDefaultValue(1000))
    ]));

    public static readonly List<ConfigType> BurningBug = GroupUtils.Merge(Enemies, [
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Immolator", "burning_bug_explode", (o, value) =>
            {
                var fsm = o.LocateMyFSM("Control");
                if (value.GetValue())
                {
                    o.GetComponent<tk2dSpriteAnimator>().Play("Immolater Idle");
                    var hm = o.GetComponent<HealthManager>();
                    hm.hasSpecialDeath = true;
                    hm.enemyDeathEffects.doNotSpawnCorpse = true;
                    fsm.GetState("State").AddAction(() => fsm.SendEvent("IMMOLATER"), 3);
                }
                else fsm.GetState("Patrol Wait").AddAction(() => fsm.SendEvent("WAKE"));
            }).WithDefaultValue(false))
    ]);

    public static readonly List<ConfigType> Surgeon = GroupUtils.Merge(Enemies, [
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Dig Range", "surgeon_range", (o, value) =>
            {
                o.AddComponent<EnemyFixers.Teleplane>().width = value.GetValue();
            }).WithDefaultValue(5)),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Invisible Body", "surgeon_no_body", (o, value) =>
            {
                if (value.GetValue()) o.GetComponent<tk2dSprite>().scale = Vector3.zero;
            }).WithDefaultValue(false))
    ]);

    public static readonly List<ConfigType> Jailer = GroupUtils.Merge(Enemies, [
        ConfigurationManager.RegisterConfigType(
            new StringConfigType("Captured Scene", "jailer_scene", (o, value) =>
            {
                o.GetOrAddComponent<EnemyFixers.CustomJailer>().targetScene = value.GetValue();
            }).WithDefaultValue("Slab_03").WithPriority(-1)),
        ConfigurationManager.RegisterConfigType(
            new StringConfigType("Captured Door ID", "jailer_door", (o, value) =>
            {
                o.GetOrAddComponent<EnemyFixers.CustomJailer>().targetDoor = value.GetValue();
            }).WithDefaultValue("door_slabCaged").WithPriority(-1))
    ]);

    public static readonly List<ConfigType> Hoker = GroupUtils.Merge(Enemies, [
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Roam Around", "hoker_wander", (o, value) =>
            {
                if (value.GetValue()) return;
                var fsm = o.LocateMyFSM("Control");
                fsm.GetState("Peaceful").DisableAction(0);
                fsm.GetState("Idle").DisableAction(0);
                fsm.GetState("Fire").DisableAction(5);
            }).WithDefaultValue(true))
    ]);

    public static readonly List<ConfigType> Metronome = GroupUtils.Merge(Visible, [
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Tick Time", "metronome_time", (o, value) =>
            {
                MiscFixers.SetMetronomeTime(o, value.GetValue());
            }).WithDefaultValue(1)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Start Delay", "metronome_delay", (o, value) =>
            {
                MiscFixers.SetMetronomeDelay(o, value.GetValue());
            }).WithDefaultValue(0))
    ]);

    public static readonly List<ConfigType> TextDisplay = GroupUtils.Merge(Generic, [
        ConfigurationManager.RegisterConfigType(
            new StringConfigType("Text", "display_text", (o, value) =>
            {
                o.GetComponent<TextDisplay>().text = value.GetValue();
            }).WithDefaultValue("Sample Text").WithPriority(-1)),
        ConfigurationManager.RegisterConfigType(
            new ChoiceConfigType("Vertical Alignment", "display_align_v", (o, value) =>
            {
                o.GetComponent<TextDisplay>().verticalAlignment = value.GetValue();
            }).WithOptions("Top", "Middle", "Bottom").WithDefaultValue(0).WithPriority(-1)),
        ConfigurationManager.RegisterConfigType(
            new ChoiceConfigType("Horizontal Alignment", "display_align_h", (o, value) =>
            {
                o.GetComponent<TextDisplay>().horizontalAlignment = value.GetValue();
            }).WithOptions("Left", "Middle", "Right").WithDefaultValue(0).WithPriority(-1)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Textbox Y Offset", "display_y_offset", (o, value) =>
            {
                o.GetComponent<TextDisplay>().offsetY = value.GetValue();
            }).WithDefaultValue(0).WithPriority(-1))
    ]);

    public static readonly List<ConfigType> TitleDisplay = GroupUtils.Merge(Generic, [
        ConfigurationManager.RegisterConfigType(
            new StringConfigType("Header", "title_header", (o, value) =>
            {
                o.GetComponent<TitleDisplay>().header = value.GetValue();
            })),
        ConfigurationManager.RegisterConfigType(
            new StringConfigType("Body", "title_body", (o, value) =>
            {
                o.GetComponent<TitleDisplay>().body = value.GetValue();
            }).WithDefaultValue("Sample Text")),
        ConfigurationManager.RegisterConfigType(
            new StringConfigType("Footer", "title_footer", (o, value) =>
            {
                o.GetComponent<TitleDisplay>().footer = value.GetValue();
            })),
        ConfigurationManager.RegisterConfigType(
            new ChoiceConfigType("Mode", "title_type", (o, value) =>
            {
                o.GetComponent<TitleDisplay>().type = value.GetValue();
            }).WithOptions("Large", "Left", "Right").WithDefaultValue(0))
    ]);

    public static readonly List<ConfigType> Choice = GroupUtils.Merge(Generic, [
        ConfigurationManager.RegisterConfigType(
            new StringConfigType("Text", "choice_text", (o, value) =>
            {
                o.GetComponent<ChoiceDisplay>().text = value.GetValue();
            }).WithDefaultValue("Sample Text")),
        ConfigurationManager.RegisterConfigType(
            new ChoiceConfigType("Requirement", "choice_currency", (o, value) =>
            {
                var val = value.GetValue();
                var choice = o.GetComponent<ChoiceDisplay>();
                switch (val)
                {
                    case 0:
                        choice.cost = 0;
                        break;
                    case 3:
                        choice.useItem = true;
                        break;
                }
                choice.currencyType = val == 1 ? CurrencyType.Money : CurrencyType.Shard;
            }).WithOptions("None", "Rosaries", "Shell Shards", "Item").WithDefaultValue(0).WithPriority(1)),
        ConfigurationManager.RegisterConfigType(
            new IntConfigType("Required Amount", "choice_cost", (o, value) =>
            {
                o.GetComponent<ChoiceDisplay>().cost = value.GetValue();
            }).WithDefaultValue(0)),
        ConfigurationManager.RegisterConfigType(
            new StringConfigType("Item ID", "choice_item_id", (o, value) =>
            {
                o.GetComponent<ChoiceDisplay>().item = value.GetValue();
            }).WithDefaultValue("Simple Key")),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Consume Item", "choice_take_item", (o, value) =>
            {
                o.GetComponent<ChoiceDisplay>().takeItem = value.GetValue();
            }).WithDefaultValue(true))
    ]);

    public static readonly List<ConfigType> Png = GroupUtils.Merge(Decorations, [
        ConfigurationManager.RegisterConfigType(
            new StringConfigType("PNG URL", "png_url",
                (o, value) => { o.GetComponentInChildren<PngObject>().url = value.GetValue(); }, (o, value, _) =>
                {
                    var prev = o.GetOrAddComponent<PngPreview>();
                    var point = (prev?.point).GetValueOrDefault(true);
                    var ppu = (prev?.ppu).GetValueOrDefault(100);
                    var vcount = (prev?.vcount).GetValueOrDefault(1);
                    var hcount = (prev?.hcount).GetValueOrDefault(1);
                    CustomAssetManager.DoLoadSprite(value.GetValue(), point, ppu, hcount, vcount,
                        sprites =>
                        {
                            if (o) o.GetComponent<SpriteRenderer>().sprite = sprites[0];
                        });
                }).WithPriority(-1)),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Anti Aliasing", "png_antialias",
                    (o, value) => { o.GetComponentInChildren<PngObject>().point = !value.GetValue(); },
                    (o, value, _) => { o.GetOrAddComponent<PngPreview>().point = !value.GetValue(); })
                .WithDefaultValue(true).WithPriority(-2)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Pixels Per Unit", "png_ppu",
                    (o, value) => { o.GetComponentInChildren<PngObject>().ppu = value.GetValue(); },
                    (o, value, _) => { o.GetOrAddComponent<PngPreview>().ppu = value.GetValue(); })
                .WithDefaultValue(100)
                .WithPriority(-2)),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Light Reflection", "png_glow",
                    (o, value) => { o.GetComponentInChildren<PngObject>().glow = value.GetValue(); })
                .WithDefaultValue(true)
                .WithPriority(-2)),
        ConfigurationManager.RegisterConfigType(
            new IntConfigType("Vertical Frame Count", "png_framecount",
                    (o, value) => { o.GetComponentInChildren<PngObject>().vcount = value.GetValue(); },
                    (o, value, _) => { o.GetOrAddComponent<PngPreview>().vcount = value.GetValue(); })
                .WithDefaultValue(1)
                .WithPriority(-2)),
        ConfigurationManager.RegisterConfigType(
            new IntConfigType("Horizontal Frame Count", "png_hframecount",
                    (o, value) => { o.GetComponentInChildren<PngObject>().hcount = value.GetValue(); },
                    (o, value, _) => { o.GetOrAddComponent<PngPreview>().hcount = value.GetValue(); })
                .WithDefaultValue(1)
                .WithPriority(-2)),
        ConfigurationManager.RegisterConfigType(
            new IntConfigType("Empty Frame Count", "png_eframecount",
                    (o, value) => { o.GetComponentInChildren<PngObject>().dummy = value.GetValue(); })
                .WithDefaultValue(0)
                .WithPriority(-2)),
        ConfigurationManager.RegisterConfigType(
            new IntConfigType("Start Frame", "png_sframe",
                    (o, value) =>
                    {
                        o.GetComponentInChildren<PngObject>().frame = Math.Max(0, value.GetValue());
                    })
                .WithDefaultValue(0)
                .WithPriority(-2)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Frames per Second", "png_frametime",
                    (o, value) =>
                    {
                        var png = o.GetComponentInChildren<PngObject>();
                        if (value.GetValue() == 0) png.frameTime = 0;
                        else png.frameTime = 1 / Mathf.Max(0.01f, value.GetValue());
                    })
                .WithDefaultValue(10)
                .WithPriority(-2)),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Play on Start", "png_start_playing",
                (o, value) =>
                {
                    o.GetComponentInChildren<PngObject>().playing = value.GetValue();
                }).WithDefaultValue(true).WithPriority(-2))
    ]);

    private static readonly ConfigType WavUrl = ConfigurationManager.RegisterConfigType(
        new StringConfigType("WAV URL", "wav_url",
            (o, value) => { o.GetComponentInChildren<WavObject>().url = value.GetValue(); }).WithPriority(-1));
    
    public static readonly List<ConfigType> Wav = GroupUtils.Merge(Generic, [
        WavUrl,
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Volume", "wav_volume",
                (o, value) => { o.GetComponent<WavObject>().Volume = value.GetValue(); })
                .WithDefaultValue(1)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Pitch", "wav_pitch",
                (o, value) => { o.GetComponent<WavObject>().pitch = value.GetValue(); })
                .WithDefaultValue(1)),
        ConfigurationManager.RegisterConfigType(
            new ChoiceConfigType("Sound Type", "wav_mode",
                (o, value) => { o.GetComponent<WavObject>().globalSound = value.GetValue() == 1; })
                .WithOptions("Local", "Global").WithDefaultValue(1)),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Loop Sound", "wav_loop",
                (o, value) => { o.GetComponent<WavObject>().loop = value.GetValue(); })
                .WithDefaultValue(false)),
        ConfigurationManager.RegisterConfigType(
            new StringConfigType("Sync ID", "wav_sync_id",
                (o, value) => { o.GetComponent<WavObject>().syncId = value.GetValue(); }))
    ]);

    public static readonly List<ConfigType> Mp4 = GroupUtils.Merge(Decorations, [
        ConfigurationManager.RegisterConfigType(
            new StringConfigType("MP4 URL", "mp4_url",
                (o, value) => { o.GetOrAddComponent<Mp4Object>().url = value.GetValue(); }, (o, value, context) =>
                {
                    var player = o.GetOrAddComponent<VideoPlayer>();
                    if (player.playbackSpeed > 0) player.playbackSpeed = 0;
                    CustomAssetManager.DoLoadVideo(player,
                        context == ConfigurationManager.PreviewContext.Cursor ? null : o.transform.GetScaleX(),
                        value.GetValue());
                }).WithPriority(-1)),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Play on Start", "mp4_start_playing",
                (o, value) =>
                {
                    o.GetOrAddComponent<Mp4Object>().playOnStart = value.GetValue();
                }).WithDefaultValue(true).WithPriority(-1)),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Loop Video", "mp4_loop",
                    (o, value) =>
                    {
                        if (!value.GetValue()) return;
                        o.GetComponent<VideoPlayer>().isLooping = value.GetValue();
                    })
                .WithDefaultValue(false)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Playback Speed", "mp4_speed",
                (o, value) => { o.GetComponent<VideoPlayer>().playbackSpeed = value.GetValue(); })
                .WithDefaultValue(1)),
        AlphaColour
    ]);

    private static readonly int ActiveRegion = LayerMask.NameToLayer("ActiveRegion");
    private static readonly int SoftTerrain = LayerMask.NameToLayer("Soft Terrain");

    public static readonly List<ConfigType> TriggerZones = GroupUtils.Merge(Stretchable, [
        ConfigurationManager.RegisterConfigType(
            new ChoiceConfigType("Trigger Type", "trigger_type",
                    (o, value) =>
                    {
                        var val = value.GetValue();
                        o.GetComponent<TriggerZone>().mode = val;

                        if (val == 3)
                        {
                            o.layer = ActiveRegion;
                            o.AddComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
                        }
                        else o.layer = SoftTerrain;
                    })
                .WithOptions("Player", "Nail Swing", "Enemy", "Other Zone", "Activator").WithDefaultValue(0)),
        ConfigurationManager.RegisterConfigType(
            new ChoiceConfigType("Shape", "trigger_shape",
                    (o, value) =>
                    {
                        if (value.GetValue() == 0) return;
                        o.GetComponent<PolygonCollider2D>().enabled = true;
                        o.GetComponent<BoxCollider2D>().enabled = false;
                    }, (o, value, _) =>
                    {
                        o.GetComponent<SpriteRenderer>().sprite =
                            value.GetValue() == 0 ? TriggerZone.SquareZone : TriggerZone.CircleZone;
                    })
                .WithOptions("Square", "Circle").WithDefaultValue(0)),
        ConfigurationManager.RegisterConfigType(
            new IntConfigType("Trigger Layer", "trigger_layer",
                (o, value) =>
                {
                    var zone = o.GetComponent<TriggerZone>();
                    zone.layer = value.GetValue();
                    zone.usingLayer = true;
                }))
    ]);

    public static readonly List<ConfigType> FleaCounter = GroupUtils.Merge(Png, [
        ConfigurationManager.RegisterConfigType(
            new IntConfigType("Start Score", "flea_counter_start", (o, value) =>
        {
            o.GetComponent<MiscFixers.CustomFleaCounter>().currentCount = value.GetValue();
        }).WithDefaultValue(0).WithPriority(-1)),
        ConfigurationManager.RegisterConfigType(
            new IntConfigType("Bronze Score", "flea_counter_first", (o, value) =>
        {
            o.GetComponent<MiscFixers.CustomFleaCounter>().bronze = value.GetValue();
        }).WithDefaultValue(5).WithPriority(-1)),
        ConfigurationManager.RegisterConfigType(
            new IntConfigType("Silver Score", "flea_counter_second", (o, value) =>
        {
            o.GetComponent<MiscFixers.CustomFleaCounter>().silver = value.GetValue();
        }).WithDefaultValue(10).WithPriority(-1)),
        ConfigurationManager.RegisterConfigType(
            new IntConfigType("Gold Score", "flea_counter_third", (o, value) =>
        {
            o.GetComponent<MiscFixers.CustomFleaCounter>().gold = value.GetValue();
        }).WithDefaultValue(15).WithPriority(-1)),
        ConfigurationManager.RegisterConfigType(
            new ChoiceConfigType("Mode", "flea_counter_mode", (o, value) =>
        {
            o.GetComponent<MiscFixers.CustomFleaCounter>().high = value.GetValue() == 0;
        }).WithOptions("Highest", "Lowest").WithDefaultValue(0).WithPriority(-1)),
        ConfigurationManager.RegisterConfigType(
            new StringConfigType("Title Header", "flea_counter_header", (o, value) =>
            {
                o.GetComponent<MiscFixers.CustomFleaCounter>().header = value.GetValue();
            }).WithPriority(-1)),
        ConfigurationManager.RegisterConfigType(
            new StringConfigType("Title Footer", "flea_counter_footer", (o, value) =>
            {
                o.GetComponent<MiscFixers.CustomFleaCounter>().footer = value.GetValue();
            }).WithPriority(-1))
    ]);
        
    public static readonly List<ConfigType> Interaction = GroupUtils.Merge(Stretchable, [
        ConfigurationManager.RegisterConfigType(
            new ChoiceConfigType("Label", "interaction_label",
                    (o, value) =>
                    {
                        if (!Enum.TryParse<InteractableBase.PromptLabels>(CustomInteraction.Labels[value.GetValue()],
                                out var result)) return; 
                        o.GetComponent<CustomInteraction>().interactLabel = result;
                    }).WithOptions(CustomInteraction.Labels).WithDefaultValue(0)),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Hide on Interact", "interaction_hide",
                    (o, value) =>
                    {
                        o.GetComponent<CustomInteraction>().hideOnInteract = value.GetValue();
                    }).WithDefaultValue(true)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("X Offset", "interaction_offset_x",
                    (o, value) =>
                    {
                        o.GetComponent<CustomInteraction>().xOffset = value.GetValue();
                    }).WithDefaultValue(0)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Y Offset", "interaction_offset_y",
                    (o, value) =>
                    {
                        o.GetComponent<CustomInteraction>().yOffset = value.GetValue();
                    }).WithDefaultValue(2.5f))
    ]);

    public static readonly List<ConfigType> RosaryBead = GroupUtils.Merge(Gravity, GroupUtils.Merge(Png, [
        ConfigurationManager.RegisterConfigType(new IntConfigType("Rosary Worth", "bead_worth",
            (o, value) =>
            {
                var gc = o.GetComponent<GeoControl>();
                if (!gc) return;
                var costRef = ScriptableObject.CreateInstance<CostReference>();
                costRef.value = value.GetValue();
                gc.valueReference = costRef;
            }
        )),
        ConfigurationManager.RegisterConfigType(new FloatConfigType("Delete after Time", "delete_after_time",
            (o, value) =>
            {
                o.AddComponent<DeleteAfterTime>().value = value.GetValue();
            }
        )),
        ConfigurationManager.RegisterConfigType(new BoolConfigType("Collectable", "bead_collectable",
            (o, value) =>
            {
                if (!value.GetValue())
                {
                    o.RemoveComponent<EventRegister>();
                    o.RemoveComponent<GeoControl>();
                }
            }
        ).WithDefaultValue(true)),
        ConfigurationManager.RegisterConfigType(MakePersistenceConfigType("Stay Collected", "rosary_stay")
            .WithDefaultValue(0))
    ]));

    public class DeleteAfterTime : MonoBehaviour
    {
        public float value;

        private void Update()
        {
            value -= Time.deltaTime;
            if (value <= 0) Destroy(gameObject);
        }
    }

    public static readonly List<ConfigType> HazardRespawn = GroupUtils.Merge(Generic, [
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Collision Trigger", "hrp_collision", (o, value) =>
            {
                if (value.GetValue()) return;
                o.RemoveComponent<Collider2D>();
            }).WithDefaultValue(true))
    ]);

    public static readonly List<ConfigType> Mirror = GroupUtils.Merge(Stretchable, GroupUtils.Merge(Png, [
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Alpha Colour", "mirror_alpha", (o, value) =>
            {
                o.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, value.GetValue() * 1.75f);
            }).WithDefaultValue(0.75f))
    ]));

    public static readonly List<ConfigType> PoleRing = GroupUtils.Merge(Visible, [
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Height", "ring_height", (o, value) =>
            {
                o.transform.position -= o.transform.rotation * new Vector3(value.GetValue(), 0);
                o.transform.GetChild(0).GetChild(0).Translate(0, value.GetValue(), 0);
            }).WithDefaultValue(5).WithPriority(-1))
    ]);

    public static readonly List<ConfigType> MapperRing = GroupUtils.Merge(TriggerActivator, [
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Fall Over", "ring_fall", (o, value) =>
            {
                if (value.GetValue()) return;
                o.RemoveComponent<CogRollThenFallOver>();
            }).WithDefaultValue(false))
    ]);

    public static readonly List<ConfigType> Transitions = GroupUtils.Merge(Stretchable, [
        ConfigurationManager.RegisterConfigType(
            new ChoiceConfigType("Gate Type", "trans_type",
                    (o, value) =>
                    {
                        o.GetComponent<CustomTransitionPoint>().pointType = value.GetValue();
                    })
                .WithOptions("Door", "Left", "Right", "Top", "Bottom").WithDefaultValue(0).WithPriority(-1)),
        ConfigurationManager.RegisterConfigType(
            new StringConfigType("Door ID", "trans_id", (o, value) =>
            {
                o.name = value.GetValue();
            }).WithPriority(-1)),
        ConfigurationManager.RegisterConfigType(
            new StringConfigType("Target Door ID", "trans_other_id",
                (o, value) =>
                {
                    o.GetComponent<TransitionPoint>().entryPoint = value.GetValue();
                }).WithPriority(-1)),
        ConfigurationManager.RegisterConfigType(
            new StringConfigType("Target Scene", "trans_other_scene",
                (o, value) =>
                {
                    o.GetComponent<TransitionPoint>().targetScene = value.GetValue();
                }).WithPriority(-1)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Entry Delay", "trans_delay",
                (o, value) =>
                {
                    o.GetComponent<TransitionPoint>().entryDelay = value.GetValue();
                }).WithPriority(-1)),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Collision Trigger", "trans_collide",
                (o, value) =>
                {
                    o.GetComponent<TransitionPoint>().isADoor = !value.GetValue();
                }).WithPriority(-1))
    ]);

    public static readonly List<ConfigType> Skarrwing = GroupUtils.Merge(Enemies, [
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Start on Ceiling", "buzzer_ceiling", (o, value) =>
            {
                if (value.GetValue()) return;
                var fsm = o.LocateMyFSM("Control");
                fsm.GetState("Initiate")
                    .AddAction(() => { fsm.FsmVariables.FindFsmBool("Start Alert").Value = true; }, 0);
            }).WithDefaultValue(false))
    ]);

    public static readonly List<ConfigType> SpearSkarr = GroupUtils.Merge(Enemies, [
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Start Awake", "spear_awake", (o, value) =>
            {
                if (!value.GetValue()) return;
                o.GetComponent<EnemyFixers.Wakeable>().DoWake();
            }).WithDefaultValue(true))
    ]);

    public static readonly List<ConfigType> Judge = GroupUtils.Merge(Enemies, [
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Start Awake", "judge_awake", (o, value) =>
            {
                if (!value.GetValue()) return;
                o.GetComponent<EnemyFixers.Wakeable>().DoWake();
            }).WithDefaultValue(true))
    ]);

    public static readonly List<ConfigType> Bindings = GroupUtils.Merge(Mutable, [
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Binding Active", "binding_active",
                (o, value) => { o.GetComponent<Binding>().active = value.GetValue(); }).WithDefaultValue(true)),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Reversible", "binding_toggle",
                (o, value) => { o.GetComponent<Binding>().reversible = value.GetValue(); }).WithDefaultValue(false)),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Appear in UI", "binding_ui",
                (o, value) => { o.GetComponent<Binding>().uiVisible = value.GetValue(); }).WithDefaultValue(true))
    ]);

    public static readonly List<ConfigType> KeyListener = GroupUtils.Merge(Generic, [
        ConfigurationManager.RegisterConfigType(
            new StringConfigType("Keybind", "key_listener_key",
                (o, value) =>
                {
                    if (!Enum.TryParse<KeyCode>(value.GetValue(), true, out var key)) return;
                    o.GetComponent<KeyListener>().key = key;
                }))
    ]);

    public static readonly List<ConfigType> Bumpers = GroupUtils.Merge(Visible, [
        ConfigurationManager.RegisterConfigType(
            new ChoiceConfigType("Mode", "bumper_mode",
                (o, value) =>
                {
                    o.GetComponent<Bumper>().SetEvil(value.GetValue() == 1);
                },
                (o, value, _) =>
                {
                    o.GetComponent<SpriteRenderer>().sprite =
                        value.GetValue() == 0 ? Bumper.NormalIcon : Bumper.EvilIcon;
                }).WithOptions("Regular", "Fire").WithDefaultValue(0))
    ]);

    public static readonly List<ConfigType> Remover = GroupUtils.Merge(Generic, [
        ConfigurationManager.RegisterConfigType(
            new StringConfigType("Filter", "remover_filter",
                (o, value) =>
                {
                    o.GetComponent<ObjectRemover>().filter = value.GetValue();
                }))
    ]);

    public static readonly List<ConfigType> RoomClearer = GroupUtils.Merge(Remover, [
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Remove Transitions", "remove_transitions",
                    (o, value) => { o.GetOrAddComponent<RoomClearerConfig>().removeTransitions = value.GetValue(); })
                .WithDefaultValue(false).WithPriority(-1)
        ),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Remove Benches", "remove_benches",
                    (o, value) => { o.GetOrAddComponent<RoomClearerConfig>().removeBenches = value.GetValue(); })
                .WithDefaultValue(true).WithPriority(-1)
        ),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Remove Blur", "remove_blur",
                    (o, value) => { o.GetOrAddComponent<RoomClearerConfig>().removeBlur = value.GetValue(); })
                .WithDefaultValue(true).WithPriority(-1)
        ),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Remove Music", "remove_music",
                    (o, value) => { o.GetOrAddComponent<RoomClearerConfig>().removeMusic = value.GetValue(); })
                .WithDefaultValue(true).WithPriority(-1)
        ),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Remove Other", "remove_other",
                    (o, value) => { o.GetOrAddComponent<RoomClearerConfig>().removeOther = value.GetValue(); })
                .WithDefaultValue(true).WithPriority(-1)
        )
    ]);

    public static readonly List<ConfigType> ObjectRemover = GroupUtils.Merge(Generic, [
        ConfigurationManager.RegisterConfigType(
            new StringConfigType("Path", "remover_path",
                (o, value) => { o.AddComponent<ObjectRemoverConfig>().objectPath = value.GetValue(); }).WithPriority(-1)
        )
    ]);

    public static readonly List<ConfigType> ObjectEnabler = GroupUtils.Merge(Generic, [
        ConfigurationManager.RegisterConfigType(
            new StringConfigType("Path", "enabler_path",
                (o, value) => { o.GetComponent<ObjectEnabler>().objectPath = value.GetValue(); })
                .WithPriority(-1)
        )
    ]);
    
    public static readonly List<ConfigType> Hazards = GroupUtils.Merge(Decorations, [
        DamagesEnemies,
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Damages Player", "damages_player",
                (o, value) =>
                {
                    if (value.GetValue()) return;
                    o.RemoveComponentsInChildren<DamageHero>();
                    o.RemoveComponentsInChildren<CogMultiHitter>();
                })
        )
    ]);
    
    public static readonly List<ConfigType> StretchableHazards = GroupUtils.Merge(Stretchable, 
        GroupUtils.Merge(Hazards, []));

    public static readonly List<ConfigType> WhiteSpikes = GroupUtils.Merge(Mutable, GroupUtils.Merge(Hazards, [
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Start Up", "wp_spikes_up",
                (o, value) =>
                {
                    o.GetComponentInChildren<WhiteSpikes>().up = value.GetValue(); 
                }).WithDefaultValue(true)
        ),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Start Delay", "wp_spikes_delay",
                (o, value) =>
                {
                    o.GetComponentInChildren<WhiteSpikes>().shiftDelay = value.GetValue();
                }).WithDefaultValue(0)
        ),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Speed", "wp_spikes_speed",
                (o, value) =>
                {
                    o.GetComponentInChildren<WhiteSpikes>().speed = value.GetValue();
                }).WithDefaultValue(1)
        )
    ]));

    public static readonly List<ConfigType> Cogs = GroupUtils.Merge(Hazards, [
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Rotation Speed", "cog_rot_speed",
                (o, value) =>
                {
                    o.GetComponentInChildren<CurveRotationAnimation>().OffsetZ = value.GetValue();
                })
        )
    ]);

    public static readonly List<ConfigType> Binoculars = GroupUtils.Merge(Visible, [
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Camera Speed", "freecam_speed",
                (o, value) => { o.GetComponent<Binoculars>().speed = value.GetValue() * 10; }).WithDefaultValue(2)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Start Zoom", "freecam_start_zoom",
                (o, value) => { o.GetComponent<Binoculars>().startZoom = value.GetValue(); })),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Zoom Minimum", "freecam_min_zoom",
                (o, value) => { o.GetComponent<Binoculars>().minZoom = value.GetValue(); })),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Zoom Maximum", "freecam_max_zoom",
                (o, value) => { o.GetComponent<Binoculars>().maxZoom = value.GetValue(); })),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Start Offset X", "freecam_offset_x",
                (o, value) => { o.GetComponent<Binoculars>().startOffset.x = value.GetValue(); })),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Start Offset Y", "freecam_offset_y",
                (o, value) => { o.GetComponent<Binoculars>().startOffset.y = value.GetValue(); }))
    ]);

    public static readonly List<ConfigType> CloverPod = GroupUtils.Merge(Visible, [
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Start Grown", "start_grown",
                    (o, value) =>
                    {
                        o.GetComponent<BouncePod>().startActive = value.GetValue();
                    }).WithDefaultValue(false))
    ]);

    public static readonly List<ConfigType> FallingBell = GroupUtils.Merge(Hazards, [
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Reset Time", "bell_reset",
                    (o, value) =>
                    {
                        ((Wait)o.LocateMyFSM("Control").GetState("Reset Pause").actions[0]).time = value.GetValue();
                    }).WithDefaultValue(1))
    ]);

    public static readonly List<ConfigType> GrindPlat = GroupUtils.Merge(Visible, [
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Fall Distance", "grind_plat_dist",
                    (o, value) =>
                    {
                        o.LocateMyFSM("Control").FsmVariables.FindFsmFloat("Drop Y").Value = 
                            o.transform.GetPositionY() - value.GetValue();
                    }).WithDefaultValue(5)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Stay Time", "grind_plat_time",
                    (o, value) =>
                    {
                        ((Wait)o.LocateMyFSM("Control").GetState("Land Grind").actions[4]).time = value.GetValue();
                    }).WithDefaultValue(1.5f))
    ]);

    private static ChoiceConfigType MakePersistenceConfigType(string name, string id,
        Action<GameObject, PersistentBoolItem> action = null)
    {
        var cc = new ChoiceConfigType(name, id, (o, value) =>
        {
            var val = value.GetValue();

            if (val == 0)
            {
                o.RemoveComponentsInChildren<PersistentBoolItem>();
                o.RemoveComponentsInChildren<PersistentIntItem>();
            }
            else
            {
                var item1 = o.GetComponentInChildren<PersistentBoolItem>();
                var item2 = o.GetComponentInChildren<PersistentIntItem>();
                if (!item1 && !item2)
                {
                    var it = o.AddComponent<PersistentBoolItem>();
                    it.itemData = new PersistentBoolItem.PersistentBoolData
                    {
                        ID = o.name,
                        SceneName = o.scene.name
                    };
                    action?.Invoke(o, it);
                }

                o.AddComponent<SemiPersistentBool>().semiPersistent = val == 1;
            }
        }).WithOptions("False", "Bench", "True");
        cc.WithPriority(-1);
        return cc;
    }
}