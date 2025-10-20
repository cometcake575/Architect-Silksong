using System;
using System.Collections.Generic;
using Architect.Behaviour.Abilities;
using Architect.Behaviour.Custom;
using Architect.Behaviour.Fixers;
using Architect.Behaviour.Utility;
using Architect.Config;
using Architect.Config.Types;
using Architect.Editor;
using Architect.Storage;
using Architect.Utils;
using HutongGames.PlayMaker.Actions;
using MonoMod.RuntimeDetour;
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
                if (!value.GetValue())
                    foreach (var renderer in o.GetComponentsInChildren<Renderer>())
                        renderer.enabled = false;
            }))
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

    public static readonly List<ConfigType> Npcs = GroupUtils.Merge(Visible, [
        ConfigurationManager.RegisterConfigType(
            new StringConfigType("Dialogue", "shakra_text", (o, value) =>
            {
                o.GetComponent<MiscFixers.Npc>().text = value.GetValue();
            }).WithDefaultValue("Sample Text").WithPriority(-1))
    ]);

    public static readonly List<ConfigType> Shakra = GroupUtils.Merge(Npcs, [
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Attack Enemies", "shakra_attack", (o, value) =>
            {
                if (value.GetValue()) return;
                UnityEngine.Object.Destroy(o.LocateMyFSM("Attack Enemies"));
            }).WithDefaultValue(true))
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

    public static readonly List<ConfigType> Breakable = GroupUtils.Merge(Visible, [
        ConfigurationManager.RegisterConfigType(MakePersistenceConfigType("Stay Broken", "breakable_stay"))
    ]);

    public static readonly List<ConfigType> Unbreakable = GroupUtils.Merge(Breakable, [
        ConfigurationManager.RegisterConfigType(new BoolConfigType("Breakable", "breakable_on", (o, value) =>
        {
            if (!value.GetValue()) o.RemoveComponentsInChildren<Breakable>();
        }).WithDefaultValue(true))
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
            }).WithDefaultValue(true).WithPriority(-1))
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
            new StringConfigType("Object ID", "duplicator_id", (o, value) =>
            {
                o.GetComponent<ObjectDuplicator>().id = value.GetValue();
            }))
    ]);
    
    public static readonly List<ConfigType> Zaprock =  GroupUtils.Merge(Generic, [
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Cooldown", "zap_cooldown", (o, value) =>
            {
                var wait = (WaitRandom)o.LocateMyFSM("Control").GetState("Zap Pause").Actions[1];
                wait.timeMin = value.GetValue();
                wait.timeMax = value.GetValue();
            }).WithDefaultValue(1))
    ]);

    public static readonly List<ConfigType> Levers = GroupUtils.Merge(Visible, [
        ConfigurationManager.RegisterConfigType(MakePersistenceConfigType("Stay Pulled", "lever_stay_pulled"))
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

    public static readonly List<ConfigType> Decorations = GroupUtils.Merge(Visible, [
        ConfigurationManager.RegisterConfigType(
            new IntConfigType("Render Layer", "obj_layer",
                (o, value) => { o.GetComponent<SpriteRenderer>().sortingOrder = value.GetValue(); },
                (o, value, _) => { o.GetComponent<SpriteRenderer>().sortingOrder = value.GetValue(); })),
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
                }))
    ]);

    private static readonly int Terrain = LayerMask.NameToLayer("Terrain");
    private static readonly int Default = LayerMask.NameToLayer("Default");

    public static readonly List<ConfigType> Colliders = GroupUtils.Merge(Decorations, [
        ConfigurationManager.RegisterConfigType(
            new ChoiceConfigType("Collision Type", "collider_type", (o, value) =>
                {
                    switch (value.GetValue())
                    {
                        case 0:
                            o.RemoveComponent<Collider2D>();
                            break;
                        case 1:
                            o.layer = Default;
                            o.AddComponent<CustomDamager>().damageAmount = 1;
                            o.GetComponent<Collider2D>().isTrigger = true;
                            break;
                        case 2:
                            o.GetComponent<Collider2D>().isTrigger = false;
                            o.layer = Terrain;
                            break;
                        case 3:
                            o.layer = Terrain;
                            o.AddComponent<PlatformEffector2D>().surfaceArc = 120;
                            var col = o.GetComponent<Collider2D>();
                            col.isTrigger = false;
                            col.usedByEffector = true;
                            break;
                        case 4:
                            o.layer = Default;
                            o.GetComponent<Collider2D>().isTrigger = false;
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
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Colour A", "colour_alpha", (o, value) =>
            {
                var sr = o.GetComponent<SpriteRenderer>();
                var color = sr.color;
                color.a = value.GetValue();
                sr.color = color;
            }).WithDefaultValue(1))
    ]));

    public static readonly List<ConfigType> Gravity = GroupUtils.Merge(Visible, [
            ConfigurationManager.RegisterConfigType(new FloatConfigType("Gravity Scale", "gravity_scale", 
                (o, value) => 
                {
                    var body = o.GetOrAddComponent<Rigidbody2D>();
                    body.gravityScale = value.GetValue();
                    body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                }
            ))
    ]);

    public static readonly List<ConfigType> ObjectSpinner = GroupUtils.Merge(Generic, [
        ConfigurationManager.RegisterConfigType(new StringConfigType("Object ID", "spinner_target", 
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
            ConfigurationManager.RegisterConfigType(new StringConfigType("Object ID", "anchor_target", 
                (o, value) => 
                {
                    o.GetComponent<ObjectAnchor>().targetId = value.GetValue();
                }
            )),
            ConfigurationManager.RegisterConfigType(new FloatConfigType("Track Length", "anchor_dist", 
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
            ).WithDefaultValue(5)),
            ConfigurationManager.RegisterConfigType(new FloatConfigType("Track Rotation", "anchor_rot", 
                (o, value) =>
                {
                    o.GetComponent<ObjectAnchor>().startRotation = value.GetValue();
                }
            ).WithDefaultValue(0)),
            ConfigurationManager.RegisterConfigType(new FloatConfigType("Track Rotation over Time", "anchor_rot_speed", 
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

    public static readonly List<ConfigType> Enemies = GroupUtils.Merge(Visible, [
        ConfigurationManager.RegisterConfigType(
            new IntConfigType("Health", "enemy_hp",
                (o, value) => { o.GetComponent<HealthManager>().hp = value.GetValue(); })),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Enable Health Scaling", "enemy_hp_scale",
                (o, value) =>
                {
                    if (value.GetValue()) return;
                    o.AddComponent<EnemyFixers.DisableHealthScaling>();
                }).WithDefaultValue(true)),
        ConfigurationManager.RegisterConfigType(
            new IntConfigType("Shell Shard Drops", "shell_shard",
                (o, value) => { o.GetComponent<HealthManager>().SetShellShards(value.GetValue()); })),
        ConfigurationManager.RegisterConfigType(
            new IntConfigType("Small Rosary Drops", "small_money",
                (o, value) => { o.GetComponent<HealthManager>().SetGeoSmall(value.GetValue()); })),
        ConfigurationManager.RegisterConfigType(
            new IntConfigType("Medium Rosary Drops", "med_money",
                (o, value) => { o.GetComponent<HealthManager>().SetGeoMedium(value.GetValue()); })),
        ConfigurationManager.RegisterConfigType(
            new IntConfigType("Large Rosary Drops", "large_money",
                (o, value) => { o.GetComponent<HealthManager>().SetGeoLarge(value.GetValue()); })),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Invincible", "invulnerable",
                (o, value) =>
                {
                    if (!value.GetValue()) return;
                    o.AddComponent<EnemyInvulnerabilityMarker>();
                })),
        ConfigurationManager.RegisterConfigType(MakePersistenceConfigType("Stay Dead", "enemy_stay_dead",
            (o, item) =>
            {
                item.OnSetSaveState += b => { o.GetComponent<HealthManager>().isDead = b; };
                item.OnGetSaveState += (out bool b) => { b = o.GetComponent<HealthManager>().isDead; };
            }))
    ]);

    static ConfigGroup()
    {
        typeof(HealthManager).Hook(nameof(HealthManager.IsBlockingByDirection),
            (Func<HealthManager, int, AttackTypes, SpecialTypes, bool> orig, HealthManager self, int cardinalDirection,
                AttackTypes attackType, SpecialTypes specialType) => self.GetComponent<EnemyInvulnerabilityMarker>() || 
                                                                     orig(self, cardinalDirection, 
                                                                         attackType, specialType));
    }

    private class EnemyInvulnerabilityMarker : MonoBehaviour;

    public static readonly List<ConfigType> Mossgrub = GroupUtils.Merge(Enemies, [
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Start Walking", "mossgrub_walk", (o, value) =>
            {
                if (!value.GetValue()) return;
                o.LocateMyFSM("Noise Reaction").SendEvent("WAKE");
            }))
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

    public static readonly List<ConfigType> Choice = GroupUtils.Merge(Generic, [
        ConfigurationManager.RegisterConfigType(
            new StringConfigType("Text", "choice_text", (o, value) =>
            {
                o.GetComponent<ChoiceDisplay>().text = value.GetValue();
            }).WithDefaultValue("Sample Text")),
        ConfigurationManager.RegisterConfigType(
            new ChoiceConfigType("Currency Type", "choice_currency", (o, value) =>
            {
                var val = value.GetValue();
                var choice = o.GetComponent<ChoiceDisplay>();
                if (val == 0) choice.cost = 0;
                choice.currencyType = val == 1 ? CurrencyType.Money : CurrencyType.Shard;
            }).WithOptions("None", "Rosaries", "Shell Shards").WithDefaultValue(0).WithPriority(1)),
        ConfigurationManager.RegisterConfigType(
            new IntConfigType("Cost", "choice_cost", (o, value) =>
            {
                o.GetComponent<ChoiceDisplay>().cost = value.GetValue();
            }).WithDefaultValue(0))
    ]);

    public static readonly List<ConfigType> Png = GroupUtils.Merge(Decorations, [
        ConfigurationManager.RegisterConfigType(
            new StringConfigType("PNG URL", "png_url",
                (o, value) => { o.GetComponentInChildren<PngObject>().url = value.GetValue(); }, (o, value, _) =>
                {
                    var prev = o.GetOrAddComponent<PngPreview>();
                    var point = (prev?.point).GetValueOrDefault(true);
                    var ppu = (prev?.ppu).GetValueOrDefault(100);
                    CustomAssetManager.DoLoadSprite(o, value.GetValue(), point, ppu);
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
                .WithPriority(-2))
    ]);

    private static readonly ConfigType WavUrl = ConfigurationManager.RegisterConfigType(
        new StringConfigType("WAV URL", "wav_url",
            (o, value) => { o.GetComponentInChildren<WavObject>().url = value.GetValue(); }).WithPriority(-1));
    
    public static readonly List<ConfigType> Wav = GroupUtils.Merge(Generic, [
        WavUrl,
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Volume", "wav_volume",
                (o, value) => { o.GetComponent<WavObject>().volume = value.GetValue(); })
                .WithDefaultValue(1)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Pitch", "wav_pitch",
                (o, value) => { o.GetComponent<WavObject>().pitch = value.GetValue(); })
                .WithDefaultValue(1)),
        ConfigurationManager.RegisterConfigType(
            new ChoiceConfigType("Sound Type", "wav_mode",
                (o, value) => { o.GetComponent<WavObject>().globalSound = value.GetValue() == 1; })
                .WithOptions("Local", "Global").WithDefaultValue(1))
    ]);

    public static readonly List<ConfigType> Mp4 = GroupUtils.Merge(Visible, [
        ConfigurationManager.RegisterConfigType(
            new StringConfigType("MP4 URL", "mp4_url",
                (o, value) => { o.GetOrAddComponent<Mp4Object>().url = value.GetValue(); }, (o, value, context) =>
                {
                    var player = o.GetOrAddComponent<VideoPlayer>();
                    player.playbackSpeed = 0;
                    CustomAssetManager.DoLoadVideo(player,
                        context == ConfigurationManager.PreviewContext.Cursor ? null : o.transform.GetScaleX(),
                        value.GetValue());
                }).WithPriority(-1)),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Play on Start", "mp4_start_playing",
                (o, value) => { o.GetOrAddComponent<Mp4Object>().playOnStart = value.GetValue(); }).WithPriority(-1)),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Loop Video", "mp4_loop",
                (o, value) => { o.GetComponent<VideoPlayer>().isLooping = value.GetValue(); })),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Playback Speed", "mp4_speed",
                (o, value) => { o.GetComponent<VideoPlayer>().playbackSpeed = value.GetValue(); }))
    ]);

    private static readonly int ActiveRegion = LayerMask.NameToLayer("ActiveRegion");
    private static readonly int SoftTerrain = LayerMask.NameToLayer("Soft Terrain");

    public static readonly List<ConfigType> TriggerZone = GroupUtils.Merge(Stretchable, [
        ConfigurationManager.RegisterConfigType(
            new ChoiceConfigType("Trigger Type", "trigger_type",
                    (o, value) =>
                    {
                        var val = value.GetValue();
                        o.GetComponent<TriggerZone>().mode = val;

                        o.layer = val == 3 ? ActiveRegion : SoftTerrain;
                    })
                .WithOptions("Player", "Nail Swing", "Enemy", "Other Zone", "Kratt", "Beastling").WithDefaultValue(0)),
        ConfigurationManager.RegisterConfigType(
            new IntConfigType("Trigger Layer", "trigger_layer",
                (o, value) => { o.GetOrAddComponent<TriggerZone>().layer = value.GetValue(); }))
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

    public static readonly List<ConfigType> HazardRespawn = GroupUtils.Merge(Generic, [
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Collision Trigger", "hrp_collision", (o, value) =>
            {
                if (value.GetValue()) return;
                o.RemoveComponent<Collider2D>();
            }).WithDefaultValue(true))
    ]);

    public static readonly List<ConfigType> PoleRing = GroupUtils.Merge(Visible, [
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Height", "ring_height", (o, value) =>
            {
                o.transform.position -= o.transform.rotation * new Vector3(value.GetValue(), 0);
                o.transform.GetChild(0).GetChild(0).Translate(0, value.GetValue(), 0);
            }).WithDefaultValue(5).WithPriority(-1))
    ]);

    public static readonly List<ConfigType> MapperRing = GroupUtils.Merge(Gravity, [
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
                var fsm = o.LocateMyFSM("Control");
                fsm.GetState("Init").AddAction(() => fsm.SendEvent("FINISHED"), 0);
            }).WithDefaultValue(true))
    ]);

    public static readonly List<ConfigType> Judge = GroupUtils.Merge(Enemies, [
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Start Awake", "judge_awake", (o, value) =>
            {
                if (!value.GetValue()) return;
                var fsm = o.LocateMyFSM("Control");
                fsm.GetState("Init").AddAction(() => fsm.SendEvent("PATROL"), 2);
            }).WithDefaultValue(true))
    ]);

    public static readonly List<ConfigType> Bindings = GroupUtils.Merge(Mutable, [
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Binding Active", "binding_active",
                (o, value) => { o.GetComponent<Binding>().active = value.GetValue(); }).WithDefaultValue(true)),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Reversible", "binding_toggle",
                (o, value) => { o.GetComponent<Binding>().reversible = value.GetValue(); }).WithDefaultValue(false))
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

    public static readonly List<ConfigType> RoomClearer = GroupUtils.Merge(Generic, [
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

    public static readonly List<ConfigType> Hazards = GroupUtils.Merge(Decorations, [
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Damages Enemies", "damages_enemies",
                (o, value) =>
                {
                    if (value.GetValue()) return;
                    o.RemoveComponentsInChildren<DamageEnemies>();
                })
        )
    ]);

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

    private static ConfigType MakePersistenceConfigType(string name, string id,
        Action<GameObject, PersistentBoolItem> action = null)
    {
        return new ChoiceConfigType(name, id, (o, value) =>
        {
            var val = value.GetValue();

            if (val == 0)
            {
                o.RemoveComponent<PersistentBoolItem>();
            }
            else
            {
                var item = o.GetComponent<PersistentBoolItem>();
                if (!item)
                {
                    var it = o.AddComponent<PersistentBoolItem>();
                    action?.Invoke(o, it);
                }

                if (val == 1) o.AddComponent<SemiPersistentBool>();
            }
        }).WithOptions("False", "Bench", "True").WithPriority(-1);
    }
}