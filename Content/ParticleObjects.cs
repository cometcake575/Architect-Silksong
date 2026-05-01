using System.Collections.Generic;
using Architect.Behaviour.Custom;
using Architect.Behaviour.Fixers;
using Architect.Config;
using Architect.Config.Types;
using Architect.Content.Preloads;
using Architect.Objects.Categories;
using Architect.Objects.Groups;
using Architect.Objects.Placeable;
using Architect.Utils;
using UnityEngine;

namespace Architect.Content;

public static class ParticleObjects
{
    public static void Init()
    {
        Categories.Effects.Add(new PreloadObject("Confetti Burst", "confetti_burst",
            ("Aqueduct_05_festival", "Caravan_States/Flea_Games_Start_effect/confetti_burst (1)"),
            description:"Appears when the 'Burst' trigger is run.",
            sprite: ResourceUtils.LoadSpriteResource("confetti_burst", ppu:1500),
            preloadAction: MiscFixers.FixConfetti)
            .WithReceiverGroup(ReceiverGroup.Particles)
            .WithInputGroup(InputGroup.Particles)
            .WithConfigGroup(Particle)
            .WithRotationGroup(RotationGroup.All));

        Categories.Effects.Add(new PreloadObject("Fly Swarm Effect", "fly_swarm_effect",
            ("Arborium_02", "ant_tiny_white_bug_swarm"), preloadAction: o =>
            {
                o.AddComponent<ParticleObject>();
                o.transform.GetChild(0).localPosition = Vector3.zero;
                o.transform.GetChild(1).localPosition = Vector3.zero;
                o.transform.GetChild(2).gameObject.SetActive(false);
                o.transform.GetChild(3).gameObject.SetActive(false);
            }, sprite: ResourceUtils.LoadSpriteResource("fly_swarm", ppu:62.5f))
            .WithConfigGroup(Particle));

        Categories.Effects.Add(new PreloadObject("Fish Effect", "fish_effect",
                ("Memory_Coral_Tower", "Fish/Pt Exit"),
                preloadAction: MiscFixers.FixDecoration,
                sprite: ResourceUtils.LoadSpriteResource("fish", ppu: 377.5f)))
            .WithScaleAction((o, f) => { o.transform.SetScale2D(new Vector2(f, f)); })
            .WithInputGroup(InputGroup.Particles)
            .WithReceiverGroup(ReceiverGroup.Particles)
            .WithConfigGroup(Fish);

        Categories.Effects.Add(new PreloadObject("Grass Effect", "grass_effect",
                ("Tut_02", "green_grass_tri (6)/Green Grass A"),
                sprite: ResourceUtils.LoadSpriteResource("grass_burst", ppu:62.5f),
                description: "Appears when the 'Burst' trigger is run.",
                preloadAction: o =>
                {
                    o.transform.localScale = Vector3.one * 4;
                    o.AddComponent<ParticleObject>();
                    o.RemoveComponent<ParticleSystemAutoRecycle>();

                    var em = o.GetComponent<ParticleSystem>().emission;
                    em.enabled = false;
                })
            .WithInputGroup(InputGroup.Particles)
            .WithConfigGroup(Particle)
            .WithReceiverGroup(ReceiverGroup.Grass)).DoIgnoreScale();
        
        Categories.Effects.Add(new PreloadObject("Maggot Effect", "maggot_effect",
                ("localpoolprefabs_assets_shared.bundle", "Assets/Prefabs/Effects/hero_maggoted_effect.prefab"), 
                description:"Appears when the 'Burst' trigger is run.",
                notSceneBundle: true, preloadAction: o =>
                {
                    o.AddComponent<ParticleObject>();
                    o.RemoveComponent<PlayParticleEffects>();
                    o.RemoveComponent<ParticleSystemAutoDisable>();
                    o.transform.GetChild(1).gameObject.SetActive(false);
                    o.transform.GetChild(2).gameObject.SetActive(false);
                }, sprite: ResourceUtils.LoadSpriteResource("maggot_burst", ppu:62.5f)
                )
            .WithReceiverGroup(ReceiverGroup.Particles)
            .WithInputGroup(InputGroup.Particles)
            .WithConfigGroup(Particle));

        Categories.Effects.Add(new PreloadObject("Splash Effect", "water_effect",
            ("Hang_09", "coral_river_chunk/particle_barrel_splash"),
            preloadAction: MiscFixers.AddComponent<ParticleObject>,
            sprite: ResourceUtils.LoadSpriteResource("water_effect", ppu:68.75f))
            .DoIgnoreScale()
            .WithFlipAction((o, f) =>
            {
                if (!f) return;
                var ps = o.GetComponent<ParticleSystem>();
                var vol = ps.velocityOverLifetime;
                vol.xMultiplier *= -1;
            })
            .WithConfigGroup(Particle)
            .WithInputGroup(InputGroup.Particles)
            .WithReceiverGroup(ReceiverGroup.Particles));

        Categories.Effects.Add(new PreloadObject("Feather Effect", "feather_effect",
                ("Peak_08b", "DJ Get Sequence/Fayforn Ground Sit NPC"),
                preloadAction: MiscFixers.AddComponent<ParticleObject>,
                postSpawnAction: o =>
                {
                    o.RemoveComponent<PlayMakerFSM>();
                    o.RemoveComponent<AnimatorLookAnimNPC>();
                    o.RemoveComponent<NoiseResponder>();
                    o.RemoveComponent<AudioSource>();
                    foreach (var i in (int[]) [0, 1, 2, 3, 4, 6, 7])
                        o.transform.GetChild(i).gameObject.SetActive(false);
                },
                sprite: ResourceUtils.LoadSpriteResource("feather_effect", ppu: 68.75f))
            .WithConfigGroup(Particle)
            .WithInputGroup(InputGroup.Particles)
            .WithReceiverGroup(ReceiverGroup.Particles));
        
        Categories.Effects.Add(new PreloadObject("Steam Effect", "steam_effect",
                ("Song_10", "Spa Region (1)/Spa Steam (1)"), 
                sprite: ResourceUtils.LoadSpriteResource("steam_effect", FilterMode.Point, ppu:75.5f),
                preloadAction: o =>
                {
                    o.AddComponent<ParticleObject>();
                    o.transform.SetScale2D(new Vector2(1, 1));
                })
            .WithRotationGroup(RotationGroup.All)
            .WithConfigGroup(Particle)
            .WithInputGroup(InputGroup.Particles)
            .WithReceiverGroup(ReceiverGroup.Particles));

        Categories.Effects.Add(new PreloadObject("Bubble Lantern Effect", "bubble_lantern_effect",
                ("Memory_Coral_Tower", "Group (38)/Coral_lamp_hang_single (1)/crystals_immediate_BG (4)"),
                preloadAction: MiscFixers.AddComponent<ParticleObject>,
                sprite: ResourceUtils.LoadSpriteResource("bubble_effect", ppu: 137.5f))
            .WithConfigGroup(Particle)
            .WithInputGroup(InputGroup.Particles)
            .WithReceiverGroup(ReceiverGroup.Particles)).DoIgnoreScale();

        Categories.Effects.Add(new PreloadObject("Bubble Brazier Effect", "bubble_brazier_effect",
                ("Memory_Coral_Tower", "big_brazier (2)/CT_big_jar (2)/crystals_immediate_BG (10)"),
                preloadAction: MiscFixers.AddComponent<ParticleObject>,
                sprite: ResourceUtils.LoadSpriteResource("bubble_effect", ppu: 68.75f))
            .WithConfigGroup(Particle)
            .WithInputGroup(InputGroup.Particles)
            .WithReceiverGroup(ReceiverGroup.Particles));

        Categories.Effects.Add(new PreloadObject("Rain Dots Effect", "rain_dots_effect",
                ("Memory_Coral_Tower", "rain_dots (26)"),
                preloadAction: o =>
                {
                    o.AddComponent<ParticleObject>();
                    o.transform.localScale = Vector3.one;
                },
                sprite: ResourceUtils.LoadSpriteResource("rain_dots", ppu: 137.5f))
            .WithConfigGroup(Particle)
            .WithInputGroup(InputGroup.Particles)
            .WithReceiverGroup(ReceiverGroup.Particles));

        Categories.Effects.Add(new PreloadObject("Snow Effect", "snow_effect",
                ("Peak_05", "peak_storm_set_mid_strength"),
                description: "Affects the whole room.\n" +
                             "Rotate the object to rotate the direction of the storm.",
                preloadAction: MiscFixers.FixDecoration,
                postSpawnAction: MiscFixers.FixSnow,
                sprite: ResourceUtils.LoadSpriteResource("snow", ppu: 377.5f)))
            .WithScaleAction((o, f) => { o.transform.SetScale2D(new Vector2(f, f)); })
            .WithConfigGroup(Particle)
            .WithInputGroup(InputGroup.Particles)
            .WithReceiverGroup(ReceiverGroup.Particles)
            .WithRotationGroup(RotationGroup.All);
        
        Categories.Effects.Add(new PreloadObject("Ducts Effect", "wet_particles",
                ("Aqueduct_03", "waterways_particles (1)"), description: "Affects the whole room.",
                preloadAction: MiscFixers.FixDecoration,
                sprite: ResourceUtils.LoadSpriteResource("drip", ppu: 377.5f)))
            .WithScaleAction((o, f) =>
            {
                o.transform.SetScale2D(new Vector2(f, f));
            })
            .WithConfigGroup(Particle)
            .WithInputGroup(InputGroup.Particles)
            .WithReceiverGroup(ReceiverGroup.Particles);

        Categories.Effects.Add(new PreloadObject("Surface Dust Effect", "surface_dust",
                ("Abandoned_town", "collid"),
                preloadAction: MiscFixers.FixDecoration,
                sprite: ResourceUtils.LoadSpriteResource("surface_dust", ppu: 377.5f)))
            .WithScaleAction((o, f) =>
            {
                o.transform.SetScale2D(new Vector2(f, f));
            })
            .WithConfigGroup(Particle)
            .WithInputGroup(InputGroup.Particles)
            .WithReceiverGroup(ReceiverGroup.Particles);
        
        PreloadManager.RegisterPreload(new BasicPreload(
            "localpoolprefabs_assets_shared.bundle", 
            "Assets/Prefabs/Effects/Particle System/Knight Particles_follow.prefab", o =>
            {
                Categories.Effects.Add(CreateParticleObject(o, "Bellhart Effect", "default_particles", "default_particles"));
                Categories.Effects.Add(CreateParticleObject(o, "Citadel Effect", "song_city_default", "song_city_default"));
                Categories.Effects.Add(CreateParticleObject(o, "Moss Effect", "moss_particles", "moss_particles"));
                Categories.Effects.Add(CreateParticleObject(o, "Marrow Effect", "marrow_particles", "bone_forest_particles"));
                Categories.Effects.Add(CreateParticleObject(o, "Wormways Effect", "crawl_particles", "crawl_particles"));
                Categories.Effects.Add(CreateParticleObject(o, "Hunter's March Effect", "march_particles", "Hunters_March_particles"));
                Categories.Effects.Add(CreateParticleObject(o, "Shellwood Effect", "shellwood_particles", "shellwood_particles"));
                Categories.Effects.Add(CreateParticleObject(o, "Greymoor Effect", "greymoor_particles", "greymoor_particles"));
                Categories.Effects.Add(CreateParticleObject(o, "Smog Effect", "smog_rise_particles", "smog_rise_particles"));
                Categories.Effects.Add(CreateParticleObject(o, "Dust Effect", "dust_particles", "dust_particles"));
                Categories.Effects.Add(CreateParticleObject(o, "Sand Effect", "blown_sand_particles", "blown_sand_particles"));
                Categories.Effects.Add(CreateParticleObject(o, "Sinner's Road Effect", "dustpen_particles", "dustpen_particles"));
                Categories.Effects.Add(CreateParticleObject(o, "Fog Canyon Effect", "fog_canyon_particles", "fog_canyon_particles"));
                Categories.Effects.Add(CreateParticleObject(o, "Waterways Effect", "waterways_particles", "waterways_particles"));
                Categories.Effects.Add(CreateParticleObject(o, "Honey Effect", "honey_particles", "hive_drip_particles"));
                Categories.Effects.Add(CreateParticleObject(o, "Deepnest Effect", "deepnest_particles", "Deepnest Particles"));
                Categories.Effects.Add(CreateParticleObject(o, "Whiteward Effect", "ward_particles", "ward_particles"));
                Categories.Effects.Add(CreateParticleObject(o, "Wisp Effect", "wisp_particles", "wisp_particles"));
                Categories.Effects.Add(CreateParticleObject(o, "Coral Gorge Effect", "gorge_particles", "coral_gorge_particles"));
                Categories.Effects.Add(CreateParticleObject(o, "Abyss Effect", "abyss_particles", "abyss particles"));
            }, notSceneBundle: true));
    }

    private static PlaceableObject CreateParticleObject(GameObject particles, string name, string id, string path)
    {
        var o = Object.Instantiate(particles.transform.Find(path).gameObject);
        o.SetActive(false);
        Object.DontDestroyOnLoad(o);

        var fc = o.AddComponent<FollowCamera>();
        fc.followX = true;
        fc.followY = true;

        o.AddComponent<ParticleObject>();

        var co = new CustomObject(name, id, o,
            sprite: ResourceUtils.LoadSpriteResource(id, FilterMode.Point, ppu: 75.5f),
            description: "Affects the whole room.")
        {
            ParentScale = Vector3.one,
            LossyScale = Vector3.one,
            Offset = new Vector3(0, 0, -o.transform.GetPositionZ())
        };
        return co
            .WithConfigGroup(Particle)
            .WithInputGroup(InputGroup.Particles)
            .WithReceiverGroup(ReceiverGroup.Particles);
    }
    
    public static readonly List<ConfigType> Particle = GroupUtils.Merge(ConfigGroup.Stretchable, [
        ConfigGroup.ZOffset,
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Play on Start", "particles_play_on_awake",
                    (o, value) =>
                    {
                        o.ApplyToAllComponents<ParticleSystem>(ps =>
                        {
                            var main = ps.main;
                            main.playOnAwake = value.GetValue();
                        });
                    }).WithPriority(-1)),
        ConfigurationManager.RegisterConfigType(
            new IntConfigType("Emission Rate", "particles_rate",
                    (o, value) =>
                    {
                        o.ApplyToAllComponents<ParticleSystem>(ps =>
                        {
                            var emission = ps.emission;
                            emission.rateOverTime = value.GetValue();
                        });
                    }).WithPriority(-1)),
        ConfigurationManager.RegisterConfigType(
            new IntConfigType("Max Particles", "particles_max",
                    (o, value) =>
                    {
                        o.ApplyToAllComponents<ParticleSystem>(ps =>
                        {
                            var main = ps.main;
                            main.maxParticles = value.GetValue();
                        });
                    }).WithPriority(-1)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Playback Speed", "particles_speed",
                    (o, value) =>
                    {
                        o.ApplyToAllComponents<ParticleSystem>(ps =>
                        {
                            var main = ps.main;
                            main.simulationSpeed = value.GetValue();
                        });
                    }).WithPriority(-1)),
        ConfigurationManager.RegisterConfigType(
            new Vector3ConfigType("Velocity", "particles_velocity",
                    (o, value) =>
                    {
                        var val = value.GetValue();
                        
                        o.ApplyToAllComponents<ParticleSystem>(ps =>
                        {
                            var vol = ps.velocityOverLifetime;
                            vol.enabled = true;
                            
                            vol.x = val.x;
                            vol.y = val.y;
                            vol.z = val.z;

                            var fol = ps.forceOverLifetime;
                            fol.enabled = false;
                        });
                    }).WithPriority(-1)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Gravity", "particles_velocity_gravity",
                    (o, value) =>
                    {
                        o.ApplyToAllComponents<ParticleSystem>(ps =>
                        {
                            var main = ps.main;
                            main.gravityModifier = value.GetValue();
                        });
                    }).WithPriority(-1)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Size Multiplier", "particles_size_mul",
                    (o, value) =>
                    {
                        o.ApplyToAllComponents<ParticleSystem>(ps =>
                        {
                            var sol = ps.sizeOverLifetime;
                            sol.enabled = true;
                            sol.sizeMultiplier *= value.GetValue();
                        });
                    }).WithPriority(-1)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType("Lifetime", "particles_lifetime",
                    (o, value) =>
                    {
                        o.ApplyToAllComponents<ParticleSystem>(ps =>
                        {
                            var main = ps.main;
                            main.startLifetime = value.GetValue();
                        });
                    }).WithPriority(-1)),
        ConfigurationManager.RegisterConfigType(
            new ColourConfigType("Colour", "particles_colour",
                    (o, value) =>
                    {
                        o.ApplyToAllComponents<ParticleSystem>(ps =>
                        {
                            var main = ps.main;
                            main.startColor = value.GetValue();
                            
                            var cbs = ps.colorBySpeed;
                            cbs.enabled = false;

                            var col = ps.colorOverLifetime;
                            col.enabled = false;
                        });
                    }, true).WithPriority(-1)),
        ConfigGroup.PngUrl,
        ConfigGroup.Aa,
        ConfigurationManager.RegisterConfigType(
            new DoubleIntConfigType("Frame Counts", "particles_allframecount",
                    (o, value) =>
                    {
                        var val = value.GetValue();
                        o.ApplyToAllComponents<ParticleSystem>(ps =>
                        {
                            var tsa = ps.textureSheetAnimation;
                            tsa.enabled = true;

                            tsa.numTilesX = val.Item1;
                            tsa.numTilesY = val.Item2;
                        });
                    })
                .WithPriority(-1)),
        ConfigurationManager.RegisterConfigType(
            new IntConfigType("Frame Cycles", "particles_frame_cycles",
                    (o, value) =>
                    {
                        o.ApplyToAllComponents<ParticleSystem>(ps =>
                        {
                            var tsa = ps.textureSheetAnimation;
                            tsa.enabled = true;

                            tsa.cycleCount = value.GetValue();
                        });
                    })
                .WithPriority(-1))
    ]);

    public static readonly List<ConfigType> Fish = GroupUtils.Merge(Particle, [
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Foreground", "fish_fore",
                    (o, value) =>
                    {
                        if (!value.GetValue()) o.transform.GetChild(1).gameObject.SetActive(false);
                    })
                .WithDefaultValue(true)),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType("Background", "fish_back",
                    (o, value) =>
                    {
                        if (!value.GetValue()) o.transform.GetChild(0).gameObject.SetActive(false);
                    })
                .WithDefaultValue(true))
    ]);
}