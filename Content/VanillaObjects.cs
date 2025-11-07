using System;
using Architect.Behaviour.Fixers;
using Architect.Content.Custom;
using Architect.Content.Preloads;
using Architect.Objects.Categories;
using Architect.Objects.Groups;
using Architect.Objects.Placeable;
using Architect.Utils;
using HutongGames.PlayMaker.Actions;
using JetBrains.Annotations;
using UnityEngine;

namespace Architect.Content;

public static class VanillaObjects
{
    public static void Init()
    {
        AddMossObjects();
        AddMarrowObjects();
        AddDocksObjects();
        AddFieldsObjects();
        AddWormwaysObjects();
        AddMarchObjects();
        AddMoorObjects();
        AddRoadObjects();
        AddWispObjects();
        AddShellwoodObjects();
        AddStepsObjects();
        AddVoltObjects();
        AddSandsObjects();
        AddMemoriumObjects();
        AddCogworksObjects();
        AddCitadelObjects();
        AddSlabObjects();
        AddPeakObjects();
        AddBileObjects();
        AddMistObjects();
        AddDuctObjects();
        AddAbyssObjects();
        AddMemoryObjects();
        AddFleaObjects();
        AddSurfaceObjects();
        AddMiscObjects();
    }

    private static void AddVoltObjects()
    {
        Categories.Hazards.Add(new PreloadObject("Voltbeam", "coral_lightning_rock",
                ("Coral_29", "Zap Worm Lightning (2)"), sprite: ResourceUtils.LoadSpriteResource("glow", ppu: 10.75f),
                preloadAction: HazardFixers.FixZaprockPreload,
                postSpawnAction: HazardFixers.FixZaprock,
                description: "Works best in small spaces, hitbox is very tall and pink glow extends much further.")
            .WithConfigGroup(ConfigGroup.Zaprock)
            .WithRotationGroup(RotationGroup.All));
    }

    private static void AddSandsObjects()
    {
        AddEnemy("Coral Furm", "coral_spike_goomba", ("Coral_24", "Coral Spike Goomba"));
        AddEnemy("Driznarga", "coral_conch_shooter_heavy", ("Coral_24", "Coral Conch Shooter Heavy (1)"));

        AddEnemy("Conchfly", "coral_conch_driller",
            ("Memory_Coral_Tower", "Battle Scenes/Battle Scene Chamber 2/Wave 9/Coral Conch Driller"),
            preloadAction: EnemyFixers.ClearRotation,
            postSpawnAction: EnemyFixers.FixDriller);

        AddEnemy("Crustcrawler A", "coral_goomba_m",
            ("Memory_Coral_Tower", "Enemy Activator Groups/Enemy Activator Low/Enemy Folder/Coral Goomba M (2)"));
        AddEnemy("Crustcrawler B", "coral_goomba_l",
            ("Memory_Coral_Tower", "Enemy Activator Groups/Enemy Activator Low/Enemy Folder/Coral Goomba L"));

        AddEnemy("Kai", "coral_swimmer_fat",
            ("Memory_Coral_Tower", "Battle Scenes/Battle Scene Chamber 3/Wave 5 - fish1/Coral Swimmer Fat (1)"),
            postSpawnAction: EnemyFixers.FixSpearSpawned);
        AddEnemy("Spinebeak Kai", "coral_poke_swimmer",
            ("Memory_Coral_Tower", "Battle Scenes/Battle Scene Chamber 3/Wave 5 - fish1/Coral Poke Swimmer"),
            postSpawnAction: EnemyFixers.FixSpearSpawned);
        AddEnemy("Steelspine Kai", "coral_spike_swimmer",
            ("Memory_Coral_Tower", "Battle Scenes/Battle Scene Chamber 2/Wave 7b - Fish/Coral Spike Swimmer (1)"),
            postSpawnAction: EnemyFixers.FixSpearSpawned);

        AddEnemy("Karaka", "coral_warrior",
            ("Memory_Coral_Tower", "Battle Scenes/Battle Scene Chamber 2/Wave 2/Coral Warrior (1)"),
            postSpawnAction: EnemyFixers.FixKaraka);

        AddEnemy("Yuma", "coral_swimmer_small",
            ("Memory_Coral_Tower", "Enemy Activator Groups/Enemy Activator Low/Enemy Folder/Coral Swimmer Small"));
        AddEnemy("Yumama", "coral_big_jellyfish",
            ("Memory_Coral_Tower",
                "Battle Scenes/Battle Scene Chamber 3/Wave 15b - double jellyfish/Coral Big Jellyfish"),
            postSpawnAction: EnemyFixers.FixYumama).DoFlipX();

        AddEnemy("Kakri", "coral_flyer",
            ("Memory_Coral_Tower", "Battle Scenes/Battle Scene Chamber 1/Wave 5/Coral Flyer"),
            postSpawnAction: EnemyFixers.FixSpearSpawned);
        AddEnemy("Yago", "coral_flyer_throw",
            ("Memory_Coral_Tower", "Battle Scenes/Battle Scene Chamber 3/Wave 2b/Coral Flyer Throw"),
            postSpawnAction: EnemyFixers.FixSpearSpawned);

        Categories.Interactable.Add(new PreloadObject("Coral Nut", "coral_crust_tree_activator",
                ("Coral_24", "coral_crust_tree (5)/Coral Crust Tree Activator"),
                postSpawnAction: InteractableFixers.FixActivator)
            .WithBroadcasterGroup(BroadcasterGroup.ActiveDeactivatable)
            .WithRotationGroup(RotationGroup.Eight));

        Categories.Platforming.Add(new PreloadObject("Red Coral Spike", "red_coral_spike",
                ("Coral_24", "coral_crust_tree (5)/Interactive Activate Parent/Branch 1/Coral Crust Tree Spike Red"),
                preloadAction: MiscFixers.FixCoral).WithConfigGroup(ConfigGroup.Coral))
            .WithReceiverGroup(ReceiverGroup.Activatable).WithRotationGroup(RotationGroup.All);
        Categories.Platforming.Add(new PreloadObject("Grey Coral Spike", "grey_coral_spike",
                ("Coral_24", "coral_crust_tree (10)/Interactive Activate Parent/Branch 1/Coral Crust Tree Spike Grey"),
                preloadAction: MiscFixers.FixCoral).WithConfigGroup(ConfigGroup.Coral)
            .WithReceiverGroup(ReceiverGroup.Activatable).WithRotationGroup(RotationGroup.All));
        Categories.Platforming.Add(new PreloadObject("Growing Coral Platform 1", "small_grey_coral_plat",
                ("Coral_24",
                    "coral_crust_tree (5)/Interactive Activate Parent/Branch 1/Coral Crust Tree Plat Small Grey"),
                preloadAction: MiscFixers.FixCoral).WithConfigGroup(ConfigGroup.Coral)
            .WithReceiverGroup(ReceiverGroup.Activatable).WithRotationGroup(RotationGroup.Four));
        Categories.Platforming.Add(new PreloadObject("Growing Coral Platform 2", "small_red_coral_plat",
                ("Coral_24",
                    "coral_crust_tree (7)/Interactive Activate Parent/Branch 1/Coral Crust Tree Plat Small Red"),
                preloadAction: MiscFixers.FixCoral).WithConfigGroup(ConfigGroup.Coral)
            .WithReceiverGroup(ReceiverGroup.Activatable).WithRotationGroup(RotationGroup.Four));
        Categories.Platforming.Add(new PreloadObject("Growing Coral Platform 3", "mid_red_coral_plat",
                ("Coral_24", "coral_crust_tree (5)/Interactive Activate Parent/Branch 1/Coral Crust Tree Plat Mid Red"),
                preloadAction: MiscFixers.FixCoral).WithConfigGroup(ConfigGroup.Coral)
            .WithReceiverGroup(ReceiverGroup.Activatable).WithRotationGroup(RotationGroup.Four));
        
        AddSolid("Coral Platform 1", "coral_plat_float", ("Coral_24", "Coral_plat_float_green_medium (1)"));
    }

    private static void AddRoadObjects()
    {
        AddEnemy("Muckroach", "dustroach", ("Dust_05", "Dustroach"));
        AddEnemy("Roachcatcher", "roachcatcher", ("Dust_02", "Roachfeeder Short"));
        AddEnemy("Roachfeeder", "roachfeeder",
            ("Dust_02", "Black Thread States Thread Only Variant/Normal World/Roachfeeder Tall")).DoFlipX();

        Categories.Interactable.Add(new PreloadObject("Temporary Gate", "greymoor_flip_bridge",
                ("Dust_02", "greymoor_flip_bridge (1)"))
            .WithRotationGroup(RotationGroup.Four));
    }

    private static void AddCitadelObjects()
    {
        AddEnemy("Choir Pouncer", "pilgrim_01_song",
            ("Song_11", "Black Thread States Thread Only Variant/Normal World/Pilgrim 01 Song"));
        AddEnemy("Choir Hornhead", "pilgrim_02_song", ("Song_11", "Pilgrim 02 Song"));
        AddEnemy("Choristor", "choristor", ("Hang_04_boss", "Battle Scene/Wave 1/Song Pilgrim 03"));
        
        AddEnemy("Choir Bellbearer", "pilgrim_03_song", ("Hang_04_boss", "Battle Scene/Wave 3/Pilgrim 03 Song"),
            postSpawnAction: EnemyFixers.FixForumEnemy);
        
        AddEnemy("Choir Clapper", "heavy_sentry", 
            ("Hang_04_boss", "Battle Scene/Wave 8 - Heavy Sentry/Song Heavy Sentry"),
            postSpawnAction: MiscFixers.FixChoirClapper).DoFlipX();
        
        AddEnemy("Minister", "song_admin", 
            ("Hang_04_boss", "Battle Scene/Wave 5 - Song Admins/Song Administrator"), 
            postSpawnAction:EnemyFixers.FixMinister);
        
        AddEnemy("Maestro", "song_maestro", 
            ("Hang_04_boss", "Battle Scene/Wave 13 - Maestro/Song Pilgrim Maestro"),
            postSpawnAction:EnemyFixers.FixMaestro).DoFlipX();
        
        AddEnemy("Choir Elder", "pilgrim_stomper_song", ("Song_11", "Pilgrim Stomper Song"));

        AddEnemy("Reed", "song_reed",
            ("Hang_04_boss", "Battle Scene/Wave 1/Song Reed"),
            postSpawnAction: EnemyFixers.FixForumEnemy).DoFlipX();
        AddEnemy("Grand Reed", "song_reed_grand",
            ("Hang_07", "Black Thread States/Normal World/Unscaler/Song Reed Grand (1)")).DoFlipX();

        Categories.Interactable.Add(new PreloadObject("Citadel Button", "citadel_button",
                ("Song_09", "Hornet_pressure_plate_small_persistent"),
                preloadAction: InteractableFixers.FixButtonPreload,
                postSpawnAction: InteractableFixers.FixButton)
            .WithBroadcasterGroup(BroadcasterGroup.Buttons)
            .WithConfigGroup(ConfigGroup.Buttons));
        Categories.Interactable.Add(new PreloadObject("Citadel Gate", "citadel_gate",
                ("Song_09", "Citadel Switch Gate"))
            .WithReceiverGroup(ReceiverGroup.Gates));

        Categories.Platforming.Add(new PreloadObject("Metronome Platform", "metronome_plat",
                ("Song_11", "metronome_plat (11)"),
                preloadAction: MiscFixers.FixMetronome)
            .WithConfigGroup(ConfigGroup.Metronome));
    }

    private static void AddSurfaceObjects()
    {
        Categories.Platforming.Add(new PreloadObject("Cradle Nut", "cradle_nut",
            ("Cradle_Destroyed_Challenge_01", "Cradle Challenge Pea Break")));
        Categories.Hazards.Add(MiscObjects.CreateCustomHazard("Brown Vines", "brown_vines",
        [
            new Vector2(-3.672f, -1.265f),
            new Vector2(-2.4f, 0),
            new Vector2(-1.089f, 0.077f),
            new Vector2(0.874f, 1.282f),
            new Vector2(2.353f, 0.813f),
            new Vector2(3.454f, -1.574f)
        ]));
        
        AddSolid("Surface Platform", "plat_float_06", ("Abandoned_town", "plat_float_06"));
        
        AddEnemy("Skrill", "surface_scuttler", ("Abandoned_town", "Surface Scuttler"),
            postSpawnAction: EnemyFixers.FixSkrill).WithConfigGroup(ConfigGroup.SimpleEnemies);
    }

    private static void AddDuctObjects()
    {
        AddEnemy("Spit Squit", "swamp_mosquito_skinny", ("Aqueduct_03", "Swamp Mosquito Skinny"));
        AddEnemy("Barnak", "swamp_barnacle", ("Aqueduct_03", "Swamp Barnacle (1)")).DoFlipX();
        AddEnemy("Ductsucker", "swamp_ductsucker", ("Aqueduct_03", "Swamp Ductsucker"),
            postSpawnAction: EnemyFixers.FixDuctsucker);

        Categories.Misc.Add(new PreloadObject("Kratt", "caravan_lech_wounded",
                ("Aqueduct_05_caravan", "Caravan_States/Fleatopia/Caravan Lech/Caravan Lech Wounded"),
                postSpawnAction: MiscFixers.FixKratt)
            .WithBroadcasterGroup(BroadcasterGroup.Hittable)
            .WithConfigGroup(ConfigGroup.TriggerActivator));

        Categories.Platforming.Add(new PreloadObject("Trapped Wardenfly", "swamp_barnacle_slab_fly",
            ("Aqueduct_04", "Swamp Barnacle Slab Fly")));
    }

    private static void AddAbyssObjects()
    {
        AddEnemy("Shadow Creeper", "shadow_creeper", ("Abyss_05", "Abyss Crawler (2)"))
            .WithRotationGroup(RotationGroup.Four);
        AddEnemy("Shadow Charger", "shadow_charger", ("Abyss_05", "Abyss Crawler Large (1)")).DoFlipX()
            .WithRotationGroup(RotationGroup.Four);

        AddSolid("Abyss Platform 1", "abyss_plat_mid",
            ("Abyss_05", "abyss_plat_mid"));
        AddSolid("Abyss Platform 2", "abyss_plat_wide",
            ("Abyss_05", "abyss_plat_wide"), preloadAction: o => o.transform.GetChild(1).SetAsFirstSibling());

        Categories.Platforming.Add(new PreloadObject("Abyss Pod", "abyss_pod",
            ("Abyss_05", "Abyss Bounce Pod")));
    }

    private static void AddMemoryObjects()
    {
        Categories.Enemies.Add(new PreloadObject("Wingmould", "white_palace_fly",
            ("Memory_Red", "Scenery Groups/End Scenery/White Palace Fly Red Memory (1)")).DoFlipX())
            .WithConfigGroup(ConfigGroup.SimpleEnemies);

        Categories.Interactable.Add(new PreloadObject("Reusable Lever", "reusable_lever",
                ("Memory_Red", "Scenery Groups/Deepnest Scenery/Control Lever"),
                description:"Can be pulled multiple times and does not stay pulled.",
                preloadAction:o => o.transform.SetRotation2D(180),
                postSpawnAction: InteractableFixers.FixReusableLever)
            .WithBroadcasterGroup(BroadcasterGroup.Activatable)
            .WithRotationGroup(RotationGroup.Eight));

        Categories.Interactable.Add(new PreloadObject("Clover Plant", "clover_pod_activator",
                ("Clover_02c", "grove_pod (1)/Clover Bounce Pod Activator"),
                postSpawnAction: InteractableFixers.FixActivator)
            .WithBroadcasterGroup(BroadcasterGroup.ActiveDeactivatable));

        Categories.Platforming.Add(new PreloadObject("Clover Pod", "clover_pod",
                ("Clover_02c", "grove_pod (1)/Interactive Parent/Clover Bounce Pod (3)"))
            .WithReceiverGroup(ReceiverGroup.Activatable)
            .WithConfigGroup(ConfigGroup.CloverPod));

        Categories.Platforming.Add(new PreloadObject("Memory Platform", "memory_ground_plat",
            ("Memory_Red", "Scenery Groups/Entry Scenery/memory_ground_plat (6)"),
            preloadAction: MiscFixers.FixMemoryPlat));

        Categories.Platforming.Add(new PreloadObject("Silk Pod", "silk_pod",
            ("Memory_Red", "Scenery Groups/Entry Scenery/red_memory_silk_pod0007 (15)")));

        Categories.Platforming.Add(new PreloadObject("Honey Pod", "hive_pod",
            ("Memory_Red", "Scenery Groups/Hive Scenery/Hive_Break_01")));


        AddSolid("Deepnest Platform 1", "deepnest_platform_01",
            ("Memory_Red", "Scenery Groups/Deepnest Scenery/plat_float_07"));
        AddSolid("Deepnest Platform 2", "deepnest_platform_02",
            ("Memory_Red", "Scenery Groups/Deepnest Scenery/deepnest_platform_05"));
        AddSolid("Deepnest Platform 3", "deepnest_platform_03",
            ("Memory_Red", "Scenery Groups/Deepnest Scenery/deepnest_platform_03"));
        AddSolid("Deepnest Platform 4", "deepnest_platform_04",
            ("Memory_Red", "Scenery Groups/Deepnest Scenery/deepnest_platform_04"));
        AddSolid("Deepnest Platform 5", "deepnest_platform_05",
            ("Memory_Red", "Scenery Groups/Deepnest Scenery/deepnest_platform_01"));

        AddSolid("Hive Platform 1", "hive_platform_03",
            ("Memory_Red", "Scenery Groups/Hive Scenery/hive_plat_04 (1)"));
        AddSolid("Hive Platform 2", "hive_platform_02",
            ("Memory_Red", "Scenery Groups/Hive Scenery/hive_plat_02"));
        AddSolid("Hive Platform 3", "hive_platform_01",
            ("Memory_Red", "Scenery Groups/Hive Scenery/hive_plat_01"));

        Categories.Interactable.Add(new PreloadObject("Clover Statue", "clover_statue",
                ("Clover_02c", "Memory Orb Group/Clover_Statue_Break Orb"),
                postSpawnAction: InteractableFixers.FixCloverStatue)
            .WithBroadcasterGroup(BroadcasterGroup.Activatable));

        Categories.Misc.Add(new PreloadObject("Green Prince NPC", "green_prince",
            ("Song_04", "Black Thread States/Normal World/Scene States/Green Prince Stand Song_04"), 
            postSpawnAction: MiscFixers.FixGreenPrince)
            .WithConfigGroup(ConfigGroup.Npcs));
    }

    private static void AddMoorObjects()
    {
        AddEnemy("Silk Snipper", "silk_snipper", ("Greymoor_06", "Farmer Scissors"));
        AddEnemy("Mite", "mite", ("Greymoor_06", "Mite"));
        AddEnemy("Fluttermite", "mitefly", ("Greymoor_03", "Mitefly (1)"),
            postSpawnAction: EnemyFixers.FixFluttermite);

        Categories.Hazards.Add(new PreloadObject("Mill Trap", "mill_trap",
                ("Greymoor_06", "Greymoor_windmill_cog (1)/GameObject/dustpen_trap_shine0000"),
                preloadAction: HazardFixers.FixMillTrap).WithConfigGroup(ConfigGroup.Hazards)
            .WithRotationGroup(RotationGroup.All));

        Categories.Platforming.Add(new PreloadObject("Scarecraw", "scarecraw",
            ("Greymoor_03", "Weathervane States/Post Quest/Scarecraw")));

        Categories.Platforming.Add(new PreloadObject("Rag Balloon S", "greymoor_balloon_small",
            ("Greymoor_03", "greymoor_balloon_small (2)")));
        Categories.Platforming.Add(new PreloadObject("Rag Balloon M", "greymoor_balloon_mid",
            ("Greymoor_03", "greymoor_balloon_mid (1)")));
        Categories.Platforming.Add(new PreloadObject("Rag Balloon L", "greymoor_balloon_large",
            ("Greymoor_03", "greymoor_balloon_large")));

        Categories.Interactable.Add(new PreloadObject("Grey Gate", "grey_lever_gate",
                ("Greymoor_07", "grey_lever_gate (1)"))
            .WithReceiverGroup(ReceiverGroup.CloseableGates)
            .WithConfigGroup(ConfigGroup.CloseableGates));
    }

    private static void AddSlabObjects()
    {
        AddEnemy("Wardenfly Jailer", "slab_jailer",
                ("Bone_East_04c", "Scene Control/Slab Jailer Scene/Slab Fly Large Cage"),
                preloadAction: EnemyFixers.KeepActive, postSpawnAction: EnemyFixers.FixJailer)
            .WithConfigGroup(ConfigGroup.Jailer).DoFlipX();

        Categories.Interactable.Add(new PreloadObject("Slab Pressure Plate", "slab_pressure_plate",
                ("Slab_05", "spike_trap_slab_jail/pressure_plate"), postSpawnAction: InteractableFixers.FixSlabPlate)
            .WithBroadcasterGroup(BroadcasterGroup.Activatable));

        Categories.Interactable.Add(new PreloadObject("Slab Gate", "jail_gate_door",
                ("Slab_05", "Jail Gate Door (2)"))
            .WithReceiverGroup(ReceiverGroup.Gates));

        Categories.Hazards.Add(new PreloadObject("Slab Spike Ball", "slab_spike_ball",
            ("Slab_21", "slab_spike_ball"), description:"Hangs from a chain.",
            preloadAction: InteractableFixers.FixSpikeBall));
        Categories.Hazards.Add(new PreloadObject("Slab Spinning Blade", "slab_prob_blade",
            ("Slab_21", "trap_spinning_blade_S_bend_slab_jail (4)/prop_blade"),
            preloadAction: InteractableFixers.FixSlabBlade));
    }

    private static void AddPeakObjects()
    {
        AddEnemy("Mnemonid", "crystal_drifter", ("Peak_06", "Crystal Drifter"));
        AddEnemy("Mnemonord", "crystal_drifter_giant", ("Peak_06", "Crystal Drifter Giant"));
        AddEnemy("Driftlin", "peaks_drifter", ("Peak_05", "Peaks Drifter"));

        Categories.Platforming.Add(new PreloadObject("Gold Ring", "harpoon_ring",
                ("Peak_05", "chair_lift_ring/Harpoon Ring Citadel"), postSpawnAction: MiscFixers.FixRing).DoFlipX()
            .WithBroadcasterGroup(BroadcasterGroup.HarpoonRings));

        Categories.Misc.Add(new PreloadObject("Weaver Heat Lamp", "weaver_heat_lamp",
                ("Peak_05", "weaver_heat_lamp (2)/Lamp"), preloadAction: MiscFixers.FixLamp)
            .WithRotationGroup(RotationGroup.Four));
        Categories.Misc.Add(new PreloadObject("Coal Lamp", "coal_lamp",
            ("Peak_05", "coal_lantern_jail_wall_mount/string_cap")));
    }

    private static void AddBileObjects()
    {
        AddEnemy("Bloatroach", "bloat_roach", ("Shadow_02", "Bloat Roach"),
            postSpawnAction: EnemyFixers.FixBloatroach);
        AddEnemy("Miremite", "swamp_goomba",
            ("Shadow_02", "Black Thread States Thread Only Variant/Normal World/Swamp Goomba")).DoFlipX();
        
        AddSolid("Bilewater Platform 1", "swamp_plat_1",
            ("Shadow_02", "plank_plat (4)"), preloadAction:MiscFixers.FixBilePlat);
        AddSolid("Bilewater Platform 2", "swamp_plat_2",
            ("Shadow_26", "gloom_lift_destroy/gloom_lift_set/gloom_plat_lift destroy"));

        Categories.Platforming.Add(new PreloadObject("Muck Pod", "swap_bounce_pod",
            ("Shadow_02", "Swamp Bounce Pod")).DoFlipX());
        Categories.Platforming.Add(new PreloadObject("Crumbling Moss", "moss_crumble_plat",
            ("Shadow_02", "moss_crumble_plat")));
        
        Categories.Hazards.Add(new PreloadObject("Stake Trap", "bilewater_trap",
            ("Shadow_10", "Swamp Stake Shooter Folder (1)/Swamp Stake Shooter"),
            preloadAction: o => o.transform.SetPositionZ(0.006f),
            postSpawnAction: o =>
            {
                var fsm = o.LocateMyFSM("Control");
                fsm.GetState("Fire").AddAction(() => fsm.SetState("Idle"));
            }).DoFlipX().WithRotationGroup(RotationGroup.All)
            .WithReceiverGroup(ReceiverGroup.Trap));
        
        Categories.Hazards.Add(new PreloadObject("Spike Ball", "swing_trap_small",
            ("Shadow_10", "Spike Ball Folder/stake_trap_swing"), preloadAction: o =>
            {
                o.transform.SetPositionZ(3.65f);
                o.transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
                o.transform.GetChild(1).GetChild(0).GetChild(1).gameObject.SetActive(false);
                o.transform.GetChild(1).GetChild(0).GetChild(2).gameObject.SetActive(false);
                o.transform.GetChild(1).GetChild(1).GetChild(1).gameObject.SetActive(false);
                o.transform.GetChild(1).GetChild(1).GetChild(2).gameObject.SetActive(false);
                o.transform.GetChild(1).GetChild(1).gameObject.AddComponent<PlaceableObject.SpriteSource>();
            }).WithRotationGroup(RotationGroup.All).WithReceiverGroup(ReceiverGroup.Trap));
        
        Categories.Hazards.Add(new PreloadObject("Groal's Spike Ball", "swing_trap_spike",
            ("Shadow_18", "Battle Scene/stake_trap_swing_repeater"), description:"Only has collision when swinging.", 
            preloadAction:HazardFixers.FixSpikeBall)
            .WithReceiverGroup(ReceiverGroup.SpikeBall));
    }

    private static void AddMistObjects()
    {
        AddEnemy("Wraith", "wraith", ("Dust_Maze_01", "Wraith"));

        Categories.Hazards.Add(new PreloadObject("Pressure Plate Trap", "dust_trap_spike_plate",
            ("Dust_Maze_01", "Mist Maze Controller/Trap Sets/Trap Set/Dust Trap Spike Plate")));
        Categories.Hazards.Add(new PreloadObject("Falling Spike Trap", "dust_trap_spike_dropper",
            ("Dust_Maze_01", "Mist Maze Controller/Trap Sets/Trap Set/Dust Trap Spike Dropper")).DoFlipX());
        Categories.Hazards.Add(new PreloadObject("Mite Trap", "mite_trap",
            ("Dust_Maze_01", "Mist Maze Controller/Trap Sets/Trap Set/Mite Trap")).DoFlipX());
    }

    private static void AddStepsObjects()
    {
        Categories.Platforming.Add(new PreloadObject("Grey Ring", "harpoon_ring_pinstress",
                ("Coral_34", "Harpoon Ring Pinstress Rope (4)"), postSpawnAction: MiscFixers.FixRing).DoFlipX()
            .WithBroadcasterGroup(BroadcasterGroup.HarpoonRings));

        AddEnemy("Judge", "judge", ("Coral_32", "Black Thread States/Normal World/Coral Judge (3)"))
            .WithConfigGroup(ConfigGroup.Judge);

        Categories.Platforming.Add(new PreloadObject("Bell of Judgement", "hang_bell",
            ("Coral_32", "shell_plat_hang_bell (4)"), preloadAction: MiscFixers.FixBellSprite));

        /*AddEnemy("Last Judge", "last_judge", ("Coral_Judge_Arena", "Boss Scene/Last Judge"),
            postSpawnAction:EnemyFixers.FixLastJudge);*/
    }

    private static void AddMiscObjects()
    {
        Categories.Misc.Add(new PreloadObject("Bell Bench", "bell_bench",
            ("Bone_East_15", "bell_bench/RestBench"),
            preloadAction: MiscFixers.FixBench, preview: true)
            .WithConfigGroup(ConfigGroup.Benches));

        Categories.Misc.Add(new PreloadObject("Garmond and Zaza NPC (Ally)", "garmond_zaza",
                ("Song_17", "Garmond Fight Scene/Garmond Fighter"),
                postSpawnAction: MiscFixers.FixGarmond)
            .WithConfigGroup(ConfigGroup.Npcs));

        Categories.Misc.Add(new PreloadObject("Shakra NPC (Ally)", "shakra",
                ("Shellwood_01",
                    "Black Thread States/Black Thread World/Shakra Guard Scene/Scene Folder/Mapper StandGuard NPC"),
                postSpawnAction: MiscFixers.FixShakra)
            .WithConfigGroup(ConfigGroup.Shakra));
        
        Categories.Misc.Add(new PreloadObject("Second Sentinel NPC (Ally)", "second_sentinel_ally", 
            ("Song_25", "Song Knight Control/Song Knight Present/Song Knight BattleEncounter"),
            postSpawnAction: MiscFixers.FixSecondSentinel));

        Categories.Misc.Add(new PreloadObject("Twelfth Architect NPC", "twelfth_architect",
                ("Under_17", "Architect Scene/Chair/pillar E/pillar D/pillar C/pillar B/pillar A/seat/Architect NPC"),
                preloadAction: MiscFixers.PreFixArchitect, postSpawnAction: MiscFixers.FixArchitect)
            .WithConfigGroup(ConfigGroup.Npcs));

        Categories.Misc.Add(new PreloadObject("Caretaker NPC", "caretaker",
                ("Song_Enclave",
                    "Black Thread States/Normal World/Enclave States/States/Level 1/Enclave Caretaker"),
                postSpawnAction: MiscFixers.FixCaretaker)
            .WithConfigGroup(ConfigGroup.Npcs).DoFlipX());

        Categories.Misc.Add(new PreloadObject("Sherma NPC", "sherma_1",
                ("Song_Enclave",
                    "Black Thread States/Normal World/Enclave States/States/Level 2/Sherma Enclave NPC"),
                postSpawnAction: MiscFixers.FixSherma)
            .WithConfigGroup(ConfigGroup.Npcs));

        Categories.Misc.Add(new PreloadObject("Caretaker Sherma NPC", "sherma_2",
                ("Song_Enclave",
                    "Black Thread States/Black Thread World/Enclave Act 3/Sherma Caretaker"),
                postSpawnAction: MiscFixers.FixShermaCaretaker)
            .WithConfigGroup(ConfigGroup.Npcs));

        Categories.Misc.Add(new PreloadObject("Shakra Ring", "mapper_ring",
                ("Shadow_02",
                    "Mapper/Mapper_ambient_rings/rings/mapper extra rings/mapper rings/Mapper_Ring_world (3)"),
                preloadAction: MiscFixers.MarkRing)
            .WithConfigGroup(ConfigGroup.MapperRing)
            .WithBroadcasterGroup(BroadcasterGroup.MapperRing));

        PreloadManager.RegisterPreload(new BasicPreload("Bellway_01_boss", "Bellbeast Children",
            o =>
            {
                var obj = ((CreateObject)o.LocateMyFSM("bellbeast_children_control").GetState("Do Spawn")
                    .Actions[1]).gameObject.Value;
                Categories.Misc.Add(new CustomObject("Beastling", "bellbeast_child", obj,
                        postSpawnAction: MiscFixers.FixBellBaby)
                    .WithConfigGroup(ConfigGroup.TriggerActivator)
                    .WithBroadcasterGroup(BroadcasterGroup.Hittable));
            }));

        Categories.Platforming.Add(new PreloadObject("Updraft", "updraft_region",
                ("Ant_19", "Updraft Region (1)"),
                preloadAction: MiscFixers.FixUpdraft,
                sprite: ResourceUtils.LoadSpriteResource("updraft", ppu: 112),
                uiSprite: ResourceUtils.LoadSpriteResource("updraft_ui"))
            .WithConfigGroup(ConfigGroup.Updraft)
            .WithRotateAction(MiscFixers.DelayRotation));
    }

    private static void AddMemoriumObjects()
    {
        AddSolid("Memorium Platform 1", "memorium_plat_2", 
            ("Arborium_03", "hanging_gardens_plat_float_metal_small (3)"));
        AddSolid("Memorium Platform 2", "memorium_plat_1", ("Arborium_03", "Arborium Plat Mid"));
    }
    
    private static void AddCogworksObjects()
    {
        Categories.Hazards.Add(new PreloadObject("Steam Vent", "steam_vent",
                ("Under_07", "steam_vent_short (3)"))
            .WithRotationGroup(RotationGroup.Four));

        Categories.Hazards.Add(new PreloadObject("Fan", "fan_hazard", ("Under_03", "fan_hazard"), 
                preloadAction: HazardFixers.FixFan).WithConfigGroup(ConfigGroup.Hazards));

        Categories.Hazards.Add(new PreloadObject("Small Cog", "spike_cog_1",
                ("Cog_04", "cog_rail_hazard (9)/cog"), preloadAction: HazardFixers.FixCog)
            .WithConfigGroup(ConfigGroup.Cogs));

        Categories.Hazards.Add(new PreloadObject("Large Grey Cog", "spike_cog_2",
                ("Cog_04", "Spike_cog_set_core (5)/Spike Cog 2"), preloadAction: HazardFixers.FixLargeCog)
            .WithConfigGroup(ConfigGroup.Cogs));

        Categories.Hazards.Add(new PreloadObject("Large Gold Cog", "spike_cog_3",
                ("Cog_04", "Spike_cog_set_core (5)/Spike Cog 3 (1)"), preloadAction: HazardFixers.FixLargeCog)
            .WithConfigGroup(ConfigGroup.Cogs));

        Categories.Interactable.Add(new PreloadObject("Cog Lever", "harpoon_ring_citadel",
                ("Cog_04", "cog_lever"), preloadAction: InteractableFixers.FixCogLeverPreload,
                postSpawnAction: InteractableFixers.FixCogLever)
            .WithBroadcasterGroup(BroadcasterGroup.Levers)
            .WithConfigGroup(ConfigGroup.Levers));

        AddEnemy("Cogwork Choirbug", "song_auto_1",
            ("Cog_04", "Black Thread States Thread Only Variant/Normal World/Song Automaton 01")).DoFlipX();
        AddEnemy("Cogwork Cleanser", "song_auto_2",
            ("Cog_04", "Black Thread States Thread Only Variant/Black Thread World/Group (1)/Song Automaton 02"));
    }

    private static void AddWispObjects()
    {
        Categories.Platforming.Add(new PreloadObject("Wisp Bounce Pod", "wisp_bounce_pod",
            ("Wisp_02", "Wisp Bounce Pod")));

        Categories.Hazards.Add(new PreloadObject("Wispfire Lantern", "wisp_flame_lantern",
                ("Wisp_02", "Wisp Flame Lantern"),
                preloadAction: HazardFixers.FixWispLantern)
            .WithConfigGroup(ConfigGroup.Unbreakable));
    }

    private static void AddShellwoodObjects()
    {
        Categories.Hazards.Add(MiscObjects.CreateCustomHazard("Shellwood Thorns", "shellwood_thorns",
        [
            new Vector2(-3.272f, -1.265f),
            new Vector2(-3.011f, 0.066f),
            new Vector2(-1.269f, 0.777f),
            new Vector2(0.474f, 1.182f),
            new Vector2(2.553f, 1.113f),
            new Vector2(3.554f, -0.694f),
            new Vector2(0.474f, -1.382f),
            new Vector2(-1.399f, -0.882f),
            new Vector2(-3.272f, -1.265f)
        ]));

        Categories.Platforming.Add(new PreloadObject("Bouncebloom", "bounce_bloom",
            ("Arborium_05", "Shellwood Bounce Bloom")));

        Categories.Misc.Add(new PreloadObject("Harp Tablet", "weaver_harp_sign",
                ("Shellwood_10", "weaver_harp_sign"),
                description: "Use <br> for a new line, <page> for a new page,\n" +
                             "and <hpage> for one where Hornet speaks.\n\n" +
                             "Use <color>, <b>, <i>, <s> and <u> to format text.\n" +
                             "For example: '<b><color=#FF0000>YOU</color></b>'",
                preloadAction: o =>
                    o.transform.GetChild(1).gameObject.AddComponent<PlaceableObject.SpriteSource>())
            .WithConfigGroup(ConfigGroup.LoreTablets));

        AddEnemy("Pond Skipper", "pond_skater", ("Arborium_05", "Pond Skater"));
        AddEnemy("Pondcatcher", "pilgrim_fisher",
            ("Shellwood_01", "Black Thread States/Normal World/Pilgrim Fisher Enemy (1)"));
        AddEnemy("Gahlia", "bloom_puncher", ("Arborium_05", "Bloom Puncher"))
            .WithRotationGroup(RotationGroup.Four);
        AddEnemy("Phacia", "flower_drifter", ("Shellwood_10", "Flower Drifter"));
        AddEnemy("Disguised Phacia", "flower_drifter_hidden", ("Arborium_03", "Flower Drifter (3)"));
        AddEnemy("Pollenica", "bloom_shooter", ("Arborium_03", "Bloom Shooter"))
            .WithRotationGroup(RotationGroup.Eight);
    }

    private static void AddMarchObjects()
    {
        AddEnemy("Skarrlid", "bone_hunter_tiny",
            ("Ant_04", "Black Thread States Thread Only Variant/Normal World/Bone Hunter Tiny"));

        AddEnemy("Skarrwing", "bone_hunter_buzzer",
                ("Ant_04", "Black Thread States Thread Only Variant/Normal World/Bone Hunter Buzzer"))
            .WithConfigGroup(ConfigGroup.Skarrwing);

        AddEnemy("Skarr Scout", "bone_hunter_child",
            ("Ant_04", "Black Thread States Thread Only Variant/Normal World/Bone Hunter Child"));

        AddEnemy("Skarr Stalker", "bone_hunter",
            ("Ant_04", "Black Thread States Thread Only Variant/Normal World/Bone Hunter"));

        AddEnemy("Spear Skarr", "bone_hunter_fly",
                ("Ant_21", "Enemy Control/Ant Merchant Killed/Big Guard Dead/Bone Hunter Fly"))
            .WithConfigGroup(ConfigGroup.SpearSkarr).DoFlipX();

        AddEnemy("Skarrgard", "bone_hunter_throw",
            ("Ant_21", "Enemy Control/Normal/Bone Hunter Throw"),
            preloadAction: EnemyFixers.RemoveConstrainPosition);

        Categories.Misc.Add(new PreloadObject("Silkcatcher", "silkcatcher_plant",
            ("Ant_04", "Silkcatcher Plant")));

        Categories.Platforming.Add(new PreloadObject("Hunterfruit", "march_pogo",
            ("Ant_04", "White Palace Fly")));

        Categories.Platforming.Add(new PreloadObject("Bending Pole Ring", "march_ring",
                ("Ant_09", "Fields Harpoon Ring Pole"), preloadAction: MiscFixers.FixPoleRing)
            .WithConfigGroup(ConfigGroup.PoleRing));

        Categories.Hazards.Add(new PreloadObject("Sickle Trap", "hunter_sickle_trap",
                ("Ant_04", "Hunter Sickle Trap"))
            .WithReceiverGroup(ReceiverGroup.Trap));

        Categories.Interactable.Add(new PreloadObject("Hunter's March Pressure Plate", "hunter_trap_plate",
                ("Ant_04", "Hunter Trap Plate"), postSpawnAction: InteractableFixers.FixMarchPlate)
            .WithBroadcasterGroup(BroadcasterGroup.Activatable)).DoFlipX();
    }

    private static void AddMarrowObjects()
    {
        AddEnemy("Skull Scuttler", "bone_goomba",
            ("Bone_East_03", "Black Thread States Thread Only Variant/Normal World/Bone Goomba"));

        Categories.Misc.Add(new PreloadObject("Skull", "bone_goomba_skull",
            ("Bone_East_03", "bone_goomba_skull_break")));

        AddEnemy("Skullwing", "bone_goomba_bounce_fly",
            ("Weave_05b", "Bone Goomba Bounce Fly (11)"));

        AddEnemy("Skull Brute", "bone_goomba_l",
            ("Bone_East_03", "Black Thread States Thread Only Variant/Normal World/Bone Goomba Large"));

        Categories.Misc.Add(new PreloadObject("Large Skull", "bone_goomba_skull_large",
            ("Bone_East_03", "bone_goomba_skull_break_large)")));

        AddEnemy("Caranid", "bone_circler",
            ("Bone_East_03", "Black Thread States Thread Only Variant/Normal World/Hunting PreScene/Bone Circler"));

        AddEnemy("Vicious Caranid", "bone_circler_v",
            ("Bone_East_03", "Black Thread States Thread Only Variant/Black Thread World/Bone Circler Vicious (1)"));

        AddEnemy("Kilik", "bone_crawler", ("Ant_19", "Bone Crawler (2)"),
            preloadAction: EnemyFixers.RemoveConstrainPosition);

        AddEnemy("Mawling", "bone_roller", ("Arborium_03", "Bone Roller"));

        AddEnemy("Marrowmaw", "bone_thumper", ("Arborium_04", "Enemy Respawner/Source Folder/Bone Thumper"),
            preloadAction: EnemyFixers.ApplyGravity);

        Categories.Platforming.Add(new PreloadObject("Magnetite Platform 1", "bone_plat_crumble_1",
            ("Bone_East_LavaChallenge", "bone_plat_01_crumble_small (2)")));

        Categories.Platforming.Add(new PreloadObject("Magnetite Platform 2", "bone_plat_crumble_2",
            ("Bone_East_LavaChallenge", "bone_plat_01_crumble (2)")));

        Categories.Platforming.Add(new PreloadObject("Magnetite Platform 3", "bone_plat_crumble_3",
            ("Bone_East_LavaChallenge", "bone_plat_02_crumble (1)")));

        Categories.Platforming.Add(new PreloadObject("Magnetite Platform 4", "bone_plat_crumble_4",
            ("Bone_East_LavaChallenge", "bone_plat_crumble_tall (4)")));

        Categories.Hazards.Add(new PreloadObject("Bone Boulder", "bone_boulder",
            ("Bone_East_03", "Black Thread States Thread Only Variant/Normal World/Bone_Boulder")));

        Categories.Interactable.Add(new PreloadObject("Bone Lever", "bone_lever",
                ("Mosstown_01", "Bone Lever"), postSpawnAction: InteractableFixers.FixLever)
            .WithBroadcasterGroup(BroadcasterGroup.Levers)
            .WithConfigGroup(ConfigGroup.Levers)
            .WithRotationGroup(RotationGroup.Eight));

        AddSolid("Bone Platform 1", "marrow_plat_01", ("Bone_East_03", "bone_plat_02 (2)"));
        AddSolid("Bone Platform 2", "bone_plat_03", ("Bone_East_03", "bone_plat_03 (2)"));
        AddSolid("Bone Platform 3", "marrow_plat_02", ("Bone_East_03", "bone_plat_03 (6)"));
    }

    private static void AddDocksObjects()
    {
        AddEnemy("Smelt Shoveller", "dock_worker", ("Bone_East_03", "Dock Worker"));
        AddEnemy("Flintstone Flyer", "dock_flyer",
            ("Bone_East_01", "Black Thread States Thread Only Variant/Normal World/Dock Flyer"),
            postSpawnAction: EnemyFixers.FixFlintFlyer);

        AddEnemy("Lavalug", "tar_slug", ("Dock_02", "Tar Slug")).DoFlipX();
        AddEnemy("Flintflame Flyer", "dock_bomber", ("Dock_02", "Dock Bomber"),
            postSpawnAction: EnemyFixers.FixFlintFlyer);
        AddEnemy("Smokerock Sifter", "shield_dockworker",
            ("Dock_02", "Shield Dockworker Spawn/Shield Dockworker (2)"));
        AddEnemy("Deep Diver", "dock_charger", ("Dock_02b", "Dock Charger")).DoFlipX();

        Categories.Interactable.Add(new PreloadObject("Deep Docks Gate", "song_gate_small",
                ("Bone_East_15", "Song_Gate_small (3)"))
            .WithReceiverGroup(ReceiverGroup.Gates)
            .WithRotationGroup(RotationGroup.Eight));
        Categories.Interactable.Add(new PreloadObject("Deep Docks Lever", "song_lever_side",
                ("Bone_East_15", "Song_lever_side"), postSpawnAction: InteractableFixers.FixLever)
            .WithBroadcasterGroup(BroadcasterGroup.Levers)
            .WithConfigGroup(ConfigGroup.Levers)
            .WithRotationGroup(RotationGroup.Eight).DoFlipX());

        AddSolid("Docks Platform 1", "dock_plat_01", ("Bone_East_01", "dock_plat_float_01 (1)"));
        AddSolid("Docks Platform 2", "dock_plat_02", ("Bone_East_01", "dock_plat_float_01 (9)"));

        Categories.Platforming.Add(new PreloadObject("Crumbling Rocks", "lava_crumble_plat",
            ("Dock_02b", "lava_crumble_plat (5)")));
    }

    private static void AddFieldsObjects()
    {
        AddSolid("Far Fields Platform 1", "fung_plat_float_01", ("Bone_East_15", "fung_plat_float_06"));
        AddSolid("Far Fields Platform 2", "fung_plat_float_02", ("Bone_East_14", "bone_plat_03"));

        AddEnemy("Hoker", "spine_floater", ("Bone_East_14", "Spine Floater (9)"),
                postSpawnAction: MiscFixers.FixHoker)
            .WithConfigGroup(ConfigGroup.Hoker).DoFlipX();
    }

    private static void AddWormwaysObjects()
    {
        Categories.Misc.Add(new PreloadObject("Lifeblood Cocoon", "health_cocoon",
                ("Crawl_09", "Area_States/Infected/Health Cocoon"))
            .WithConfigGroup(ConfigGroup.Breakable));
    }

    private static void AddFleaObjects()
    {
        Categories.Misc.AddStart(new PreloadObject("Confetti Burst", "confetti_burst",
            ("Aqueduct_05_festival", "Caravan_States/Flea_Games_Start_effect/confetti_burst (1)"),
            description:"Appears when the 'Burst' trigger is run.",
            sprite: ResourceUtils.LoadSpriteResource("confetti_burst", ppu:1500),
            preloadAction: MiscFixers.FixConfetti)
            .WithReceiverGroup(ReceiverGroup.Confetti)
            .WithRotationGroup(RotationGroup.All));
        
        Categories.Misc.AddStart(new PreloadObject("Score Counter", "flea_counter",
            ("Aqueduct_05_festival", "Flea Games Counter"), preloadAction: MiscFixers.FixFleaCounter, 
            description:"If the mode is 'Highest', the score changes colour above each milestone.\n" +
                        "If the mode is 'Lowest', the score changes colour below each milestone.",
            sprite: ResourceUtils.LoadSpriteResource("flea_counter", ppu:64))
            .WithConfigGroup(ConfigGroup.FleaCounter)
            .WithReceiverGroup(ReceiverGroup.FleaCounter)
            .WithBroadcasterGroup(BroadcasterGroup.FleaCounter));
        
        Categories.Misc.AddStart(new PreloadObject("Fleamaster NPC", "fleamaster_npc",
            ("Aqueduct_05_festival", "Caravan_States/Flea Festival/Flea Game - Juggling/Flea Games Host NPC"),
            preloadAction: MiscFixers.FixFleamaster)
            .WithConfigGroup(ConfigGroup.Npcs));

        /*
        Categories.Platforming.Add(new PreloadObject("Flea Dodge Platform", "dodge_plat",
            ("Aqueduct_05_festival",
                "Caravan_States/Flea Festival/Flea Game - Dodging/Active While Playing/Dodge Plat L")));*/
    }

    private static void AddMossObjects()
    {
        AddEnemy("Mossgrub", "mossbone_crawler", ("Arborium_09", "MossBone Crawler (1)")).DoFlipX()
            .WithRotationGroup(RotationGroup.Four)
            .WithConfigGroup(ConfigGroup.Mossgrub);

        AddEnemy("Massive Mossgrub", "mossbone_crawler_fat",
            ("Arborium_09", "MossBone Crawler Fat"));

        AddEnemy("Mossmir", "mossbone_fly",
            ("Arborium_04", "MossBone Fly (1)"));

        AddEnemy("Aknid", "aspid_collector",
            ("Mosstown_01", "Black Thread States Thread Only Variant/Black Thread World/Aspid Collector"),
            postSpawnAction: EnemyFixers.FixAknid);

        AddEnemy("Overgrown Pilgrim", "pilgrim_moss_spitter",
            ("Mosstown_01", "Black Thread States Thread Only Variant/Normal World/Pilgrim Moss Spitter"));

        AddEnemy("Pilgrim Groveller", "pilgrim_03",
            ("Mosstown_01", "Pilgrim 03 (1)")).DoFlipX();

        AddEnemy("Pilgrim Pouncer", "pilgrim_01",
            ("Mosstown_01", "Pilgrim 01")).DoFlipX();

        AddEnemy("Winged Pilgrim", "pilgrim_fly",
            ("Coral_32", "Black Thread States/Black Thread World/Black_Thread_Core/Enemy Group/Pilgrim Fly")).DoFlipX();

        AddEnemy("Pilgrim Hiker", "pilgrim_hiker",
            ("Coral_32", "Black Thread States/Black Thread World/Black_Thread_Core/Enemy Group/Pilgrim Hiker"));

        AddSolid("Moss Grotto Platform 1", "bone_plat_01",
            ("Tut_02", "bone_plat_01"));
        AddSolid("Moss Grotto Platform 2", "bone_plat_02",
            ("Tut_02", "bone_plat_02"));

        Categories.Interactable.Add(new PreloadObject("Pilgrim Trap Wire", "pilgrim_trap_wire",
                ("Mosstown_02", "Pilgrim Trap Wire"), postSpawnAction: InteractableFixers.FixTrapWire).DoFlipX()
            .WithBroadcasterGroup(BroadcasterGroup.Activatable)
            .WithRotationGroup(RotationGroup.All));

        Categories.Hazards.Add(new PreloadObject("Pilgrim Trap Spike", "pilgrim_trap_spike",
                ("Mosstown_02", "traps_left/Pilgrim Trap Spike"),
                description: "This spike starts hidden, the 'Activate' trigger will\n" +
                             "cause the spike to come out of the ground.")
            .WithReceiverGroup(ReceiverGroup.Trap));
    }

    private static PlaceableObject AddEnemy(string name, string id, (string, string) path,
        [CanBeNull] Action<GameObject> preloadAction = null,
        [CanBeNull] Action<GameObject> postSpawnAction = null)
    {
        return Categories.Enemies.Add(new PreloadObject(name, id,
                path,
                preloadAction: preloadAction,
                postSpawnAction: postSpawnAction)
            .WithReceiverGroup(ReceiverGroup.Enemies)
            .WithBroadcasterGroup(BroadcasterGroup.Enemies)
            .WithConfigGroup(ConfigGroup.Enemies));
    }

    private static void AddSolid(string name, string id, (string, string) path,
        [CanBeNull] Action<GameObject> preloadAction = null)
    {
        Categories.Solids.Add(new PreloadObject(name, id, path, preloadAction: preloadAction))
            .WithRotationGroup(RotationGroup.Four)
            .WithConfigGroup(ConfigGroup.Colliders);
    }
}