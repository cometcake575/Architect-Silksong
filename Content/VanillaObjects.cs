using System;
using System.Linq;
using Architect.Behaviour.Custom;
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
using Object = UnityEngine.Object;

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
        AddBellObjects();
        AddShellwoodObjects();
        AddStepsObjects();
        AddVoltObjects();
        AddSandsObjects();
        AddMemoriumObjects();
        AddCogworksObjects();
        AddUnderworksObjects();
        AddVaultsObjects();
        AddCitadelObjects();
        AddWhitewardObjects();
        AddSlabObjects();
        AddPeakObjects();
        AddBileObjects();
        AddMistObjects();
        AddDuctObjects();
        AddAbyssObjects();
        AddMemoryObjects();
        AddFleaObjects();
        AddCradleObjects();
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

        var sphere = new GameObject("Voltring");
        sphere.SetActive(false);
        Object.DontDestroyOnLoad(sphere);
        PreloadManager.RegisterPreload(new BasicPreload("Coral_29", "Boss Scene/Zap Clusters/Cluster 1/Mega Jelly Zap",
            o =>
            {
                o.transform.parent = sphere.transform;
                o.transform.localPosition = Vector3.zero;
            }));
        Categories.Hazards.Add(new CustomObject("Voltring", "coral_lightning_orb", sphere,
            sprite: ResourceUtils.LoadSpriteResource("voltring", ppu:64))
            .WithReceiverGroup(ReceiverGroup.Voltring));
        
        Categories.Hazards.Add(new PreloadObject("Voltgrass", "voltgrass",
            ("Coral_29", "coral_zap_mounds_shortest (10)"),
            sprite: ResourceUtils.LoadSpriteResource("voltgrass", ppu:64),
            postSpawnAction: HazardFixers.FixVoltgrass)
            .WithRotationGroup(RotationGroup.All)
            .WithFlipAction((o, f) =>
            {
                if (!f) return;
                o.transform.SetScaleY(-o.transform.GetScaleY());
                o.transform.SetRotation2D(180 - o.transform.GetRotation2D());
            })
            .WithConfigGroup(ConfigGroup.Decorations)).Offset = new Vector3(0.5723f, 1);

        Categories.Hazards.Add(new PreloadObject("Voltbola", "voltvessel_ball",
                ("Arborium_03", "Lightning Bola Ball Enemy"),
                description:"Usually already landed by the time the room finishes loading.\n" +
                            "Best used with the Object Spawner.",
                hideAndDontSave: true)
            .WithConfigGroup(ConfigGroup.Velocity));
    }

    private static void AddSandsObjects()
    {
        AddEnemy("Coral Furm", "coral_spike_goomba", ("Coral_24", "Coral Spike Goomba"));
        AddEnemy("Driznit", "coral_conch_shooter", ("Coral_32", "Coral Conch Shooter (1)"),
            preloadAction: EnemyFixers.FixDriznit)
            .WithConfigGroup(ConfigGroup.Wakeable)
            .WithReceiverGroup(ReceiverGroup.Wakeable);
        AddEnemy("Driznarga", "coral_conch_shooter_heavy", ("Coral_24", "Coral Conch Shooter Heavy (1)"));
        
        AddEnemy("Pokenabbin", "pokenabbin", ("Coral_24", "Coral Conch Stabber (1)"),
            preloadAction: EnemyFixers.FixPatroller)
            .WithConfigGroup(ConfigGroup.Patroller);

        AddEnemy("Conchfly", "coral_conch_driller",
            ("Memory_Coral_Tower", "Battle Scenes/Battle Scene Chamber 2/Wave 9/Coral Conch Driller"),
            preloadAction: EnemyFixers.ClearRotation,
            postSpawnAction: EnemyFixers.FixDriller);

        AddEnemy("Crustcrawler A", "coral_goomba_m",
            ("Memory_Coral_Tower", "Enemy Activator Groups/Enemy Activator Low/Enemy Folder/Coral Goomba M (2)"),
            preloadAction: EnemyFixers.RemoveConstrainPosition);
        AddEnemy("Crustcrawler B", "coral_goomba_l",
            ("Memory_Coral_Tower", "Enemy Activator Groups/Enemy Activator Low/Enemy Folder/Coral Goomba L"),
            preloadAction: EnemyFixers.RemoveConstrainPosition);
        AddEnemy("Crustcrag", "coral_goomba_xl",
            ("Arborium_06", "Coral Goomba Large (1)"), postSpawnAction: o =>
            {
                var fsm = o.LocateMyFSM("Behaviour");
                var patrol = fsm.FsmVariables.FindFsmBool("In Patrol Range");
                fsm.GetState("Walk").AddAction(() =>
                {
                    patrol.value = true;
                }, 5);
            }).DoFlipX();

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

        AddEnemy("Corrcrust Karaka", "corrcrust_karaka",
            ("Memory_Coral_Tower", "Battle Scenes/Battle Scene Chamber 4/Wave 3/Coral Bubble Brute"),
            postSpawnAction: EnemyFixers.FixCorrcrustKaraka);

        AddEnemy("Karak Gor", "karak_gor",
            ("Memory_Coral_Tower", "Battle Scenes/Battle Scene Chamber 2/Wave 10/Coral Brawler (1)"),
            preloadAction: EnemyFixers.FixKarakGor).DoFlipX();
        
        AddEnemy("Alita", "alita",
            ("Memory_Coral_Tower", "Battle Scenes/Battle Scene Chamber 2/Wave 1/Coral Hunter"),
            postSpawnAction: EnemyFixers.FixAlita);

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
                preloadAction: InteractableFixers.FixCoralNut,
                postSpawnAction: InteractableFixers.FixActivator)
            .WithBroadcasterGroup(BroadcasterGroup.ActiveDeactivatable)
            .WithRotationGroup(RotationGroup.Eight));

        Categories.Hazards.Add(new PreloadObject("Coral Spike S", "stomp_spire",
                ("Memory_Coral_Tower", "Battle Scenes/Battle Scene Chamber 2/Wave 10/Coral Brawler (1)/Stomp Spire L"),
                description: "This spike starts hidden, the 'Activate' trigger will\n" +
                             "cause the spike to come out of the ground.",
                postSpawnAction: HazardFixers.FixCoralSpike)
            .WithReceiverGroup(ReceiverGroup.CoralSpike)
            .WithBroadcasterGroup(BroadcasterGroup.Breakable)
            .WithRotationGroup(RotationGroup.Eight));

        Categories.Hazards.Add(new PreloadObject("Coral Spike L", "coral_spike",
                ("Memory_Coral_Tower", "Boss Scene/Roar Spikes/Spike Holder 1/Coral Spike"),
                description: "This spike starts hidden, the 'Activate' trigger will\n" +
                             "cause the spike to come out of the ground.",
                postSpawnAction: HazardFixers.FixCoralSpike)
            .WithReceiverGroup(ReceiverGroup.CoralSpike)
            .WithBroadcasterGroup(BroadcasterGroup.Breakable)
            .WithRotationGroup(RotationGroup.Eight).DoFlipX());

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
        
        AddEnemy("Watcher at the Edge", "watcher", ("Coral_39", "Coral Warrior Grey"),
            postSpawnAction: EnemyFixers.FixWatcher)
            .WithConfigGroup(ConfigGroup.Watcher)
            .WithReceiverGroup(ReceiverGroup.Wakeable)
            .WithBroadcasterGroup(BroadcasterGroup.Bosses).DoFlipX();
        
        /*
        AddEnemy("Crust King Kahnn", "crust_king", ("Memory_Coral_Tower", "Boss Scene/Coral King"),
            postSpawnAction: EnemyFixers.FixKahnn);*/
    }

    private static void AddRoadObjects()
    {
        AddEnemy("Muckroach", "dustroach", ("Dust_05", "Dustroach"));
        AddEnemy("Roachcatcher", "roachcatcher", ("Dust_02", "Roachfeeder Short"));
        AddEnemy("Roachfeeder", "roachfeeder",
            ("Dust_02", "Black Thread States Thread Only Variant/Normal World/Roachfeeder Tall")).DoFlipX();
        AddEnemy("Roachkeeper", "roachkeeper",
            ("Dust_05", "Roachkeeper")).DoFlipX();

        Categories.Interactable.Add(new PreloadObject("Temporary Gate", "greymoor_flip_bridge",
                ("Dust_02", "greymoor_flip_bridge (1)"))
            .WithRotationGroup(RotationGroup.Four));

        Categories.Interactable.Add(new PreloadObject("Gong", "gong",
                ("Dust_Chef", "kitchen_string"), postSpawnAction: InteractableFixers.FixGong)
            .WithBroadcasterGroup(BroadcasterGroup.Activatable));
        
        AddEnemy("Roachserver", "roachserver",
            ("Dust_Chef", "Battle Parent/Battle Scene/Wave 1/Roachkeeper Chef Tiny"),
            preloadAction: o => o.transform.SetPositionZ(0.006f),
            postSpawnAction: EnemyFixers.FixRoachserver);

        Categories.Hazards.Add(new PreloadObject("Maggot Blob", "chef_blob",
                ("Dust_Chef", "Chef Maggot Blob"),
                description:"Usually already landed by the time the room finishes loading.\n" +
                            "Best used with the Object Spawner.",
                hideAndDontSave: true, postSpawnAction: HazardFixers.FixMaggotBlob)
            .WithConfigGroup(ConfigGroup.Velocity));

        Categories.Misc.Add(new PreloadObject("Silkeater Cocoon", "silkeater",
            ("Dust_11", "Steel Soul States/Regular/NPC Control/Large Cocoon 1")));

        /*AddEnemy("Disgraced Chef Lugoli", "disgraced_chef",
            ("Dust_Chef", "Battle Parent/Battle Scene/Wave 2/Roachkeeper Chef (1)"),
            postSpawnAction: EnemyFixers.FixLugoli)
            .WithBroadcasterGroup(BroadcasterGroup.Bosses)
            .WithConfigGroup(ConfigGroup.Bosses);*/
    }

    private static void AddUnderworksObjects()
    {
        Categories.Misc.Add(new PreloadObject("Twelfth Architect NPC", "twelfth_architect",
                ("Under_17", "Architect Scene/Chair/pillar E/pillar D/pillar C/pillar B/pillar A/seat/Architect NPC"),
                preloadAction: MiscFixers.PreFixArchitect, postSpawnAction: MiscFixers.FixArchitect)
            .WithConfigGroup(ConfigGroup.Npcs));
        
        Categories.Platforming.Add(new PreloadObject("Crumbling Rocks 2", "lava_crumble_plat_b",
            ("Under_19", "lava_crumble_plat"))
            .WithConfigGroup(ConfigGroup.CrumblePlat));

        AddEnemy("Cogwork Underfly", "understore_auto", ("Under_19", "Understore Automaton"));
        AddEnemy("Cogwork Hauler", "understore_auto_ex", 
            ("Under_19", "Understore Automaton EX (9)")).DoFlipX();
        
        AddEnemy("Undersweep", "undersweep", ("Under_19", "Pilgrim Staff Understore")).DoFlipX();
        AddEnemy("Underscrub", "underscrub", ("Under_19", "Pilgrim 03 Understore (1)"));
        AddEnemy("Underloft", "undercrank", ("Under_19b", "Understore Thrower")).DoFlipX();
        
        AddEnemy("Underworker", "underworker", ("Under_10", "Battle Scene/Wave 1/Understore Small"));
        
        AddEnemy("Underpoke", "underpoke", ("Under_10", "Battle Scene/Wave 2/Understore Poker"),
            preloadAction: EnemyFixers.FixUnderworksArenaEnemy).DoFlipX();
        AddEnemy("Undercrank", "understore_heavy", ("Under_10", "Battle Scene/Wave 6/Understore Heavy (1)"),
            preloadAction: EnemyFixers.FixUnderworksArenaEnemy).DoFlipX();

        AddEnemy("Drapemite", "drapemite", ("Slab_15", "Mite Heavy (1)"));
        AddEnemy("Giant Drapemite", "giant_drapemite", ("Hang_10", "Understore Mite Giant"),
            preloadAction: EnemyFixers.RemoveConstrainPosition);
        
        AddEnemy("Drapefly", "drapefly", ("Hang_10", "Citadel Bat"));
        AddEnemy("Drapelord", "drapelord",
            ("Arborium_11", "Merchant Quest Parent/Quest Active/Battle Scene/Wave 1/Citadel Bat Large"));

        Categories.Hazards.Add(new PreloadObject("Spiked Grey Cog", "spike_cog_4",
                ("Under_05", "cog_05_shortcut/before/blocking cogs/Spike Cog 3"), 
                preloadAction: HazardFixers.FixUnderworksCog)
            .WithConfigGroup(ConfigGroup.Cogs));
        Categories.Hazards.Add(new PreloadObject("Spiked Gold Cog", "spike_cog_5", 
                ("Under_05", "cog_05_shortcut/before/blocking cogs/Spike Cog 2"), 
                preloadAction: HazardFixers.FixUnderworksCog)
            .WithConfigGroup(ConfigGroup.Cogs));

        Categories.Platforming.Add(new PreloadObject("Grind Platform", "grind_plat",
            ("Under_06", "Grind Plat Control/Understore Grind Plat (1)"),
            postSpawnAction: MiscFixers.FixGrindPlat)
            .WithConfigGroup(ConfigGroup.GrindPlat));

        Categories.Hazards.Add(new PreloadObject("Junk Pipe", "junk_pipe",
            ("Under_06", "understore_junk_pipe"),
            preloadAction: HazardFixers.FixJunkPipe)
            .WithRotationGroup(RotationGroup.All));

        Categories.Misc.Add(new PreloadObject("Loam NPC", "loam_npc",
            ("Under_03d", "Black Thread States/Normal World/Understore Large Worker"),
            preloadAction: MiscFixers.PreFixLoam,
            postSpawnAction: MiscFixers.FixLoam)
            .WithConfigGroup(ConfigGroup.Npcs));
        
        AddSolid("Underworks Platform 1", "under_plat_1", ("Under_05", "dock_metal_grate_floor_set (1)"),
            preloadAction: MiscFixers.FocusFirstChild);
    }
    
    private static void AddCitadelObjects()
    {
        AddEnemy("Choir Pouncer", "pilgrim_01_song",
            ("Song_11", "Black Thread States Thread Only Variant/Normal World/Pilgrim 01 Song"));
        AddEnemy("Choir Hornhead", "pilgrim_02_song", ("Song_11", "Pilgrim 02 Song"));
        AddEnemy("Choristor", "choristor", ("Hang_04_boss", "Battle Scene/Wave 1/Song Pilgrim 03"));

        AddEnemy("Envoy", "envoy", ("Song_17", "March Group Control/March Group R/Song Pilgrim 01"));
        AddEnemy("Choir Flyer", "choir_flyer", ("Song_11", "Pilgrim 04 Song (2)"),
                preloadAction:EnemyFixers.FixPatroller)
            .WithConfigGroup(ConfigGroup.Patroller);
        
        AddEnemy("Choir Bellbearer", "pilgrim_03_song", ("Hang_04_boss", "Battle Scene/Wave 3/Pilgrim 03 Song"),
            postSpawnAction: EnemyFixers.FixForumEnemy);
        
        AddEnemy("Choir Clapper", "heavy_sentry", 
            ("Hang_04_boss", "Battle Scene/Wave 8 - Heavy Sentry/Song Heavy Sentry"),
            postSpawnAction: MiscFixers.FixChoirClapper).DoFlipX();

        var choirBombS = new GameObject("[Architect] Choir Bomb S");
        choirBombS.SetActive(false);
        Object.DontDestroyOnLoad(choirBombS);
        var bombS = Categories.Hazards.Add(new CustomObject("Rune Bomb S", "choir_bomb_s", choirBombS,
                description:"Appears when the 'Activate' trigger is run.",
            sprite: ResourceUtils.LoadSpriteResource("rune_bomb_small", ppu:64))
            .WithReceiverGroup(ReceiverGroup.RuneBomb).WithRotationGroup(RotationGroup.All));

        var choirBombL = new GameObject("[Architect] Choir Bomb L");
        choirBombL.SetActive(false);
        Object.DontDestroyOnLoad(choirBombL);
        var bombL = Categories.Hazards.Add(new CustomObject("Rune Bomb L", "choir_bomb_l", choirBombL,
                description:"Appears when the 'Activate' trigger is run.",
            sprite: ResourceUtils.LoadSpriteResource("rune_slam_large", ppu:50))
            .WithReceiverGroup(ReceiverGroup.RuneBomb).WithRotationGroup(RotationGroup.All));
        
        PreloadManager.RegisterPreload(new BasicPreload("Hang_04_boss", "rune bomb small", o =>
        {
            o.transform.parent = choirBombS.transform;
            bombS.Offset = o.transform.GetChild(1).transform.localPosition;
        }, hads: true));
        PreloadManager.RegisterPreload(new BasicPreload("Hang_04_boss", "rune slam", o =>
        {
            o.transform.parent = choirBombL.transform;
            bombL.Offset = o.transform.GetChild(1).transform.localPosition;
        }, hads: true));

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

        AddEnemy("Clawmaiden", "clawmaiden", ("Hang_04_boss", "Battle Scene/Wave 4/Song Handmaiden"),
            preloadAction: EnemyFixers.FixClawmaiden);

        Categories.Interactable.Add(new PreloadObject("Dial Door", "dial_door",
                ("Song_20b", "Dial Door Bridge"),
                sprite: ResourceUtils.LoadSpriteResource("cog_door", ppu:64),
                preloadAction: InteractableFixers.FixDialDoor)
            .WithRotateAction(InteractableFixers.FixDialDoorRot)
            .WithReceiverGroup(ReceiverGroup.DialDoor)
            .WithRotationGroup(RotationGroup.Four));

        Categories.Interactable.Add(new PreloadObject("Citadel Button S", "citadel_button",
                ("Song_09", "Hornet_pressure_plate_small_persistent"),
                preloadAction: InteractableFixers.FixButtonPreload,
                postSpawnAction: InteractableFixers.FixButton)
            .WithBroadcasterGroup(BroadcasterGroup.Buttons)
            .WithConfigGroup(ConfigGroup.Buttons));

        Categories.Interactable.Add(new PreloadObject("Citadel Button L", "citadel_button_big",
                ("Coral_10", "Hornet_pressure_plate/Plate"),
                postSpawnAction: InteractableFixers.FixButton)
            .WithBroadcasterGroup(BroadcasterGroup.Buttons)
            .WithConfigGroup(ConfigGroup.Buttons));
        
        Categories.Interactable.Add(new PreloadObject("Citadel Gate S", "citadel_gate",
                ("Song_09", "Citadel Switch Gate"))
            .WithReceiverGroup(ReceiverGroup.Gates));
        Categories.Interactable.Add(new PreloadObject("Citadel Gate L", "citadel_gate_big",
                ("Coral_10", "Song Gate Entrance Right"),
                preloadAction: o => o.transform.GetChild(2).gameObject.SetActive(false))
            .WithReceiverGroup(ReceiverGroup.Gates));

        Categories.Platforming.Add(new PreloadObject("Metronome Platform", "metronome_plat",
                ("Song_11", "metronome_plat (11)"),
                preloadAction: MiscFixers.FixMetronome)
            .WithConfigGroup(ConfigGroup.Metronome));
        
        AddSolid("Citadel Platform 1", "citadel_plat_1", ("Song_Enclave", "sc_plat_float_fat"));
        AddSolid("Citadel Platform 2", "citadel_plat_2", ("Song_11", "sc_plat_float_mid (5)"));
        AddSolid("Citadel Platform 3", "citadel_plat_3",
            ("Song_12", "Black Thread States/Normal World/sc_plat_float_fat (1)"));
        AddSolid("Citadel Platform 4", "citadel_plat_4",
            ("Song_01", "sc_plat_float_tall"));
    }

    private static void AddVaultsObjects()
    {
        AddEnemy("Vaultborn", "vaultborn", ("Library_04", "Acolyte Control/Song Scholar Acolyte"),
            preloadAction: EnemyFixers.FixVaultborn);
        AddEnemy("Lampbearer", "lampbearer", ("Library_04", "Black Thread States/Normal World/Lightbearer (3)"),
                preloadAction: EnemyFixers.FixPatroller)
            .WithConfigGroup(ConfigGroup.YPatroller).DoFlipX();
        AddEnemy("Scrollreader", "scrollreader", ("Library_04", "Black Thread States/Normal World/Scrollkeeper"));
        AddEnemy("Vaultkeeper", "vaultkeeper", ("Library_04", "Scholar"));
    }

    private static void AddCradleObjects()
    {
        Categories.Hazards.Add(new PreloadObject("Rubble Field", "rubble_field",
                ("Cradle_03", "Boss Scene/Rubble Fields/Rubble Field M"),
                postSpawnAction: o => o.transform.GetChild(5).gameObject.SetActive(false),
                sprite: ResourceUtils.LoadSpriteResource("rubble", FilterMode.Point, ppu:10.24f))
            .WithConfigGroup(ConfigGroup.Stretchable)
            .WithReceiverGroup(ReceiverGroup.Trap)).Offset = new Vector3(0, -10);
        
        Categories.Platforming.Add(new PreloadObject("Moving Cradle Platform", "cradle_plat",
                ("Cradle_03", "cradle_plat (7)"))
            .WithConfigGroup(ConfigGroup.CradlePlat)
            .WithReceiverGroup(ReceiverGroup.CrankPlatform)
            .WithRotationGroup(RotationGroup.Four));
        
        Categories.Platforming.Add(new PreloadObject("Spiked Moving Cradle Platform", "cradle_spiked_plat",
            ("Cradle_03", "cradle_spike_plat (10)"))
            .WithConfigGroup(ConfigGroup.CradlePlat)
            .WithReceiverGroup(ReceiverGroup.CrankPlatform));
        
        Categories.Hazards.Add(new PreloadObject("Cradle Spikes", "cradle_spikes",
            ("Cradle_03", "cradle_spike_plat (10)/art/Cradle__0004_moving_plat (9)"),
            preloadAction: o =>
            {
                Object.Instantiate(
                    o.transform.GetRoot().GetChild(8).gameObject, 
                    o.transform.position, 
                    default,
                    o.transform
                ).GetComponent<TinkEffect>().overrideCamShake = true;
            }));

        /*
        AddEnemy("Grand Mother Silk", "gms_boss", ("Cradle_03", "Boss Scene/Silk Boss"),
            postSpawnAction: EnemyFixers.FixGms);*/
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
        
        AddEnemy("Pharlid", "pharlid", ("Cradle_Destroyed_Challenge_02", "Blade Spider")).DoFlipX();
        AddEnemy("Pharlid Diver", "pharlid_diver", ("Cradle_Destroyed_Challenge_02", "Blade Spider Hang"),
            postSpawnAction: EnemyFixers.FixPharlidDiver);
        
        Categories.Enemies.Add(new PreloadObject("Garpid", "garpid",
                ("Cradle_Destroyed_Challenge_01", "Centipede Trap Control/Centipede Trap"),
                preloadAction: o =>
                {
                    var anim = o.GetComponent<tk2dSpriteAnimator>();
                    anim.defaultClipId = anim.GetClipIdByName("Damage Hero");
                }))
            .WithConfigGroup(ConfigGroup.Garpid)
            .WithRotateAction((o, rot) =>
            {
                o.LocateMyFSM("Control").FsmVariables.FindFsmFloat("Rotation").Value = rot;
            })
            .WithRotationGroup(RotationGroup.All)
            .WithReceiverGroup(ReceiverGroup.Garpid)
            .WithBroadcasterGroup(BroadcasterGroup.Damageable);
        
        AddEnemy("Imoba", "imoba", ("Cradle_Destroyed_Challenge_01", "Spike Lazy Flyer"),
            preloadAction: EnemyFixers.FixPatroller).WithConfigGroup(ConfigGroup.Patroller);
        AddEnemy("Skrill", "surface_scuttler", ("Abandoned_town", "Surface Scuttler"),
            postSpawnAction: EnemyFixers.FixSkrill).WithConfigGroup(ConfigGroup.SimpleEnemies);
    }

    private static void AddDuctObjects()
    {
        AddEnemy("Spit Squit", "swamp_mosquito_skinny", ("Aqueduct_03", "Swamp Mosquito Skinny"));
        Categories.Hazards.Add(new PreloadObject("Squit Bullet", "muck_bullet",
            ("Aqueduct_03", "Skinny Mosquito Bullet"),
            description:"Usually already landed by the time the room finishes loading.\n" +
                        "Best used with the Object Spawner.",
            hideAndDontSave: true)
            .WithConfigGroup(ConfigGroup.Velocity));
        
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
        
        Categories.Effects.Add(new PreloadObject("Ducts Effect", "wet_particles",
                ("Aqueduct_03", "waterways_particles (1)"), description: "Affects the whole room.",
                preloadAction: MiscFixers.FixDecoration,
                sprite: ResourceUtils.LoadSpriteResource("drip", ppu: 377.5f)))
            .WithScaleAction((o, f) =>
            {
                o.transform.SetScale2D(new Vector2(f, f));
            })
            .WithConfigGroup(ConfigGroup.Decorations);
    }

    private static void AddAbyssObjects()
    {
        AddEnemy("Shadow Creeper", "shadow_creeper", ("Abyss_05", "Abyss Crawler (2)"))
            .WithRotationGroup(RotationGroup.Four);
        AddEnemy("Shadow Charger", "shadow_charger", ("Abyss_05", "Abyss Crawler Large (1)")).DoFlipX()
            .WithRotationGroup(RotationGroup.Four);
        
        AddEnemy("Gloomsac", "gloomsac", ("Abyss_02b", "Gloomfly"), hideAndDontSave: true,
            postSpawnAction: EnemyFixers.FixGloomsac);
        
        AddEnemy("Gargant Gloom", "gargant_gloom", ("Abyss_02b", "Gloom Beast"),
            preloadAction: EnemyFixers.FixGargantGloomPreload,
            postSpawnAction: EnemyFixers.FixGargantGloom);

        AddSolid("Abyss Platform 1", "abyss_plat_mid",
            ("Abyss_05", "abyss_plat_mid"));
        AddSolid("Abyss Platform 2", "abyss_plat_wide",
            ("Abyss_05", "abyss_plat_wide"), preloadAction: MiscFixers.FocusFirstChild);

        Categories.Hazards.Add(new PreloadObject("Void Tendrils", "abyss_tendrils",
            ("Abyss_07", "Abyss Tendrils (16)"),
            preloadAction: o => o.transform.localScale = new Vector3(1.5f, 1.5f),
            postSpawnAction: HazardFixers.FixTendrils));

        Categories.Platforming.Add(new PreloadObject("Abyss Pod", "abyss_pod",
            ("Abyss_05", "Abyss Bounce Pod")));
    }

    private static void AddMemoryObjects()
    {
        Categories.Enemies.Add(new PreloadObject("Wingmould", "white_palace_fly",
            ("Memory_Red", "Scenery Groups/End Scenery/White Palace Fly Red Memory (1)")).DoFlipX())
            .WithConfigGroup(ConfigGroup.SimpleEnemies)
            .WithBroadcasterGroup(BroadcasterGroup.Damageable);

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

        Categories.Effects.Add(new PreloadObject("Web Effect", "web_effect",
                ("Memory_Red", "thread_memory_region/web_particles (1)"), description: "Affects the whole room.",
                preloadAction: MiscFixers.FixWebDecoration,
                sprite: ResourceUtils.LoadSpriteResource("web", ppu: 377.5f)))
            .WithScaleAction((o, f) =>
            {
                o.transform.SetScale2D(new Vector2(f, f));
            })
            .WithConfigGroup(ConfigGroup.Decorations);

        Categories.Platforming.Add(new PreloadObject("Silk Pod", "silk_pod",
            ("Memory_Red", "Scenery Groups/Entry Scenery/red_memory_silk_pod0007 (15)")));

        Categories.Platforming.Add(new PreloadObject("Honey Pod", "hive_pod",
            ("Memory_Red", "Scenery Groups/Hive Scenery/Hive_Break_01")));

        Categories.Misc.Add(new PreloadObject("Shaman Shell S", "shell_small",
            ("Tut_04", "States/Outro Scene/Snail_Shell_Small"), 
            preloadAction: MiscFixers.FixShamanShell));
        Categories.Misc.Add(new PreloadObject("Shaman Shell M", "shell_mid",
            ("Tut_04", "States/Outro Scene/Snail_Shell_Mid"),
            preloadAction: MiscFixers.FixShamanShell));
        Categories.Misc.Add(new PreloadObject("Shaman Shell L", "shell_large",
            ("Tut_04", "States/Outro Scene/Snail_Shell_Large"),
            preloadAction: MiscFixers.FixShamanShell));


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

        Categories.Misc.Add(new PreloadObject("Lily Pad / Nuphar", "lilypad",
            ("Clover_04b", "Lilypad Trap Setter/Lilypad Plat (1)"),
            postSpawnAction: o =>
            {
                var fsm = o.LocateMyFSM("Control");
                fsm.GetState("Trap Antic 1").AddAction(() => o.BroadcastEvent("OnActivate"), 0);
                fsm.GetState("Activate").AddAction(() => o.BroadcastEvent("OnDeactivate"));
            })
            .WithConfigGroup(ConfigGroup.Lilypad)
            .WithBroadcasterGroup(BroadcasterGroup.ActiveDeactivatable));

        AddEnemy("Leaf Roller", "leaf_roller", ("Clover_02c", "Grass Goomba"))
            .WithConfigGroup(ConfigGroup.LeafRoller);
        AddEnemy("Leaf Glider", "leaf_glider", ("Clover_02c", "Lilypad Plat/Lilypad Fly")).DoFlipX();

        AddEnemy("Cloverstag", "cloverstag", ("Clover_06", "Cloverstag (2)"));

        AddEnemy("Kindanir", "kindanir", ("Clover_04b", "Battle Scene/Wave 3/Grasshopper Child (1)"),
            preloadAction: EnemyFixers.FixFlyin);
        AddEnemy("Verdanir", "verdanir", ("Clover_04b", "Battle Scene/Return Scene/Grasshopper Slasher"));
        AddEnemy("Escalion", "escalion", ("Clover_04b", "Grasshopper Fly")).DoFlipX();
        
        AddSolid("Verdania Platform 1", "verdania_plat_1", ("Clover_21", "Group/clover_gate_outer_0000_1 (53)"),
            preloadAction: o =>
            {
                o.transform.parent = null;
                o.transform.SetPositionZ(0.006f);
            });
        AddSolid("Verdania Platform 2", "verdania_plat_2", ("Clover_21", "Group/clover_gate_outer_0000_1 (54)"),
            preloadAction: o =>
            {
                o.transform.parent = null;
                o.transform.SetPositionZ(0.006f);
            });
        AddSolid("Verdania Platform 3", "verdania_plat_3", ("Clover_21", "clover___0019_roof2_plat (5)"));
        
        Categories.Interactable.Add(new PreloadObject("Verdania Button", "verdania_button",
                ("Clover_05c", "Hornet_pressure_plate_small_persistent"),
                postSpawnAction: InteractableFixers.FixButton,
                preloadAction: o => o.transform.GetChild(0).gameObject.SetActive(false))
            .WithBroadcasterGroup(BroadcasterGroup.Buttons)
            .WithConfigGroup(ConfigGroup.Buttons));
        
        Categories.Interactable.Add(new PreloadObject("Verdania Gate", "verdania_gate",
                ("Clover_05c", "Clover Gate (1)"),
                preloadAction: o => o.transform.GetChild(3).gameObject.SetActive(false))
            .WithReceiverGroup(ReceiverGroup.Gates));
    }

    private static void AddMoorObjects()
    {
        AddEnemy("Silk Snipper", "silk_snipper", ("Greymoor_06", "Farmer Scissors"));
        AddEnemy("Dreg Catcher", "dreg_catcher",
            ("Greymoor_05", "Scene Control/Farmer Enemies/Roosting Enemies/Farmer Catcher (2)"),
            preloadAction: EnemyFixers.KeepActive);
        AddEnemy("Thread Raker", "thread_raker",
            ("Greymoor_05", "Scene Control/Farmer Enemies/Farmer Centipede (1)"),
            preloadAction: EnemyFixers.KeepActiveRemoveConstrainPos);
        
        AddEnemy("Mite", "mite", ("Greymoor_06", "Mite"))
            .WithRotationGroup(RotationGroup.Three)
            .WithRotateAction((o, f) =>
            {
                o.LocateMyFSM("Control").FsmVariables.FindFsmBool("Eating").Value = f % 180 != 0;
                o.transform.SetRotation2D(f);
            }).DoFlipX();
        AddEnemy("Mitemother", "mitemother", ("Greymoor_16", "Gnat Giant")).DoFlipX();
        AddEnemy("Fluttermite", "mitefly", ("Greymoor_03", "Mitefly (1)"),
            preloadAction: EnemyFixers.FixPatroller)
            .WithConfigGroup(ConfigGroup.YPatroller);
        
        AddEnemy("Craw", "crow", ("Greymoor_15b",
                "Crow Court Objects (Children activated on start)/crowcourt - not in session/Crow (3)"),
            preloadAction: o => o.transform.SetPositionZ(0.006f));
        AddEnemy("Tallcraw", "crowman", ("Greymoor_15b", "Crowman"),
            preloadAction:EnemyFixers.KeepActive);
        AddEnemy("Squatcraw", "crowman_dagger", ("Greymoor_15b", "Crowman Dagger (1)"),
            preloadAction:EnemyFixers.KeepActive);
        
        AddEnemy("Craw Juror", "craw_juror",
            ("Room_CrowCourt_02", "Battle Scene/Wave 2/Crowman Juror Tiny"),
            preloadAction: EnemyFixers.FixCrawJurorPreload,
            postSpawnAction: EnemyFixers.FixTinyCrawJuror);
        AddEnemy("Tallcraw Juror", "tallcraw_juror",
            ("Room_CrowCourt_02", "Battle Scene/Wave 1/Crowman Juror"),
            preloadAction: EnemyFixers.FixCrawJurorPreload,
            postSpawnAction: EnemyFixers.FixTallcrawJuror);
        AddEnemy("Squatcraw Juror", "squatcraw_juror", 
            ("Room_CrowCourt_02", "Battle Scene/Wave 1/Crowman Dagger Juror"),
            preloadAction: EnemyFixers.FixCrawJurorPreload,
            postSpawnAction: EnemyFixers.FixCrawJuror);

        AddEnemy("Crawfather", "crawfather", ("Room_CrowCourt_02", "Battle Scene/Wave 6/Crawfather"),
                preloadAction: o => o.transform.SetPositionZ(0.006f),
                postSpawnAction: EnemyFixers.FixCrawfather)
            .WithConfigGroup(ConfigGroup.Bosses)
            .WithBroadcasterGroup(BroadcasterGroup.SummonerBosses);

        Categories.Hazards.Add(new PreloadObject("Craw Chain", "craw_chain",
                ("Room_CrowCourt_02", "Battle Scene/Wave 6/Crawfather/Chains/Crawfather Attack Chain"),
                description: "Starts hidden, the 'Activate' trigger will activate the chain.",
                preloadAction: o =>
                {
                    var anim = o.GetComponent<tk2dSpriteAnimator>();
                    anim.defaultClipId = anim.GetClipIdByName("Chain Spike");
                }, postSpawnAction: o => o.LocateMyFSM("Control").GetState("Emerge Pause").DisableAction(2))
            .WithReceiverGroup(ReceiverGroup.Trap)
            .WithRotationGroup(RotationGroup.All));
        
        AddEnemy("Moorwing", "moorwing", ("Greymoor_05_boss", "Vampire Gnat Boss Scene/Vampire Gnat"),
            postSpawnAction:EnemyFixers.FixMoorwing)
            .WithConfigGroup(ConfigGroup.Moorwing)
            .WithBroadcasterGroup(BroadcasterGroup.Bosses).DoFlipX();

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

        Categories.Effects.Add(new PreloadObject("Rain Effect", "rain_effect",
                ("Greymoor_07", "Greymoor_Rain_Tiled_Set"), description: "Affects the whole room.",
                preloadAction: MiscFixers.FixDecoration,
                sprite: ResourceUtils.LoadSpriteResource("rain", ppu: 377.5f)))
            .WithScaleAction((o, f) =>
            {
                o.transform.SetScale2D(new Vector2(f, f));
            })
            .WithConfigGroup(ConfigGroup.Decorations);
        
        AddSolid("Greymoor Platform 1", "moor_plat_1",
            ("Greymoor_03", "Black Thread States Thread Only Variant/Normal World/Strut Structure/Tilt Plat"),
            preloadAction: o =>
            {
                for (var i = 4; i <= 6; i++) o.transform.GetChild(1).GetChild(i).gameObject.SetActive(false);
            });
    }

    private static void AddWhitewardObjects()
    {
        AddEnemy("Surgeon", "surgeon",
                ("Ward_09", "Sherma Rescue Scene/Activation Folder/Battle Scene/Wave 1/Song Pilgrim 02"),
                preloadAction:EnemyFixers.FixSurgeon).WithConfigGroup(ConfigGroup.Surgeon);
        AddEnemy("Mortician", "mortician",
            ("Ward_03", "Song Creeper (2)"));
        AddEnemy("Dreg Husk", "slasher",
            ("Ward_02", "Boss Scene Parent/Respawn Scene/Husks/Slasher 1"), preloadAction:EnemyFixers.FixDregHusk)
            .DoFlipX();
        AddEnemy("Dregwheel", "slammer",
            ("Ward_02", "Boss Scene Parent/Respawn Scene/Husks/Slammer 1"), postSpawnAction:EnemyFixers.FixDregwheel)
            .DoFlipX();
        Categories.Misc.Add(new PreloadObject("Coal Bucket", "barrel_03_opencoal",
            ("Ward_03", "brk_barrel_03_opencoal")));
    }

    private static void AddSlabObjects()
    {
        AddEnemy("Penitent", "penitent", ("Slab_15", "Slab Prisoner Leaper New"));
        AddEnemy("Puny Penitent", "puny_penitent", ("Slab_15", "Slab Prisoner Fly New"));
        AddEnemy("Scabfly", "scabfly", ("Slab_05", "Slab Fly Small"));
        
        AddEnemy("Guardfly", "guardfly", ("Slab_04", "Slab Fly Mid (2)"));
        AddEnemy("Wardenfly", "wardenfly", ("Slab_22", "Slab Fly Large"));

        AddEnemy("Freshfly", "freshfly",
            ("Slab_16b",
                "Broodmother Scene Control/Broodmother Scene/Battle Scene Broodmother/Spawner Flies/Slab Fly Small Fresh"),
            postSpawnAction: EnemyFixers.FixFreshfly);
        
        AddEnemy("Broodmother", "broodmother",
            ("Slab_16b",
                "Broodmother Scene Control/Broodmother Scene/Battle Scene Broodmother/Wave 4/Slab Fly Broodmother"),
            postSpawnAction: EnemyFixers.FixBroodmother)
            .WithConfigGroup(ConfigGroup.Bosses)
            .WithBroadcasterGroup(BroadcasterGroup.Bosses);
        
        AddEnemy("Wardenfly Jailer", "slab_jailer",
                ("Bone_East_04c", "Scene Control/Slab Jailer Scene/Slab Fly Large Cage"),
                preloadAction: EnemyFixers.KeepActive, postSpawnAction: EnemyFixers.FixJailer)
            .WithConfigGroup(ConfigGroup.Jailer).DoFlipX();

        AddEnemy("First Sinner", "first_sinner", ("Slab_10b", "Boss Scene/First Weaver"),
            preloadAction: o =>
            {
                var anim = o.GetComponent<tk2dSpriteAnimator>();
                anim.defaultClipId = anim.GetClipIdByName("Idle");
            },
            postSpawnAction: EnemyFixers.FixFirstSinner)
            .WithConfigGroup(ConfigGroup.Bosses)
            .WithBroadcasterGroup(BroadcasterGroup.Bosses);

        Categories.Interactable.Add(new PreloadObject("Slab Pressure Plate", "slab_pressure_plate",
                ("Slab_05", "spike_trap_slab_jail/pressure_plate"), postSpawnAction: InteractableFixers.FixSlabPlate)
            .WithBroadcasterGroup(BroadcasterGroup.Activatable));

        Categories.Interactable.Add(new PreloadObject("Slab Lever", "jail_lever",
                ("Slab_22", "slab_jail_lever"), postSpawnAction: InteractableFixers.FixLever)
            .WithBroadcasterGroup(BroadcasterGroup.Levers)
            .WithConfigGroup(ConfigGroup.Levers)
            .WithRotationGroup(RotationGroup.Eight));

        Categories.Interactable.Add(new PreloadObject("Slab Gate", "jail_gate_door",
                ("Slab_05", "Jail Gate Door (2)"), preloadAction: EnemyFixers.KeepActive)
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

        Categories.Misc.Add(new PreloadObject("Coldshard", "snow_chunk",
                ("Bellway_Peak_02", "Snowflake Chunk (82)"), 
                preloadAction: o =>
                {
                    o.transform.GetChild(3).GetChild(0).gameObject.SetActive(false);
                    o.RemoveComponent<RandomlyFlipScale>();
                    o.LocateMyFSM("Control").enabled = true;
                })
            .WithRotationGroup(RotationGroup.All));
        Categories.Misc.Add(new PreloadObject("Floating Coldshard", "float_crystal",
                ("Peak_06", "Float Crystal (13)"),
                preloadAction: o =>
                {
                    o.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
                    o.RemoveComponent<RandomlyFlipScale>();
                })
            .WithRotationGroup(RotationGroup.All));

        Categories.Misc.Add(new PreloadObject("Weaver Heat Lamp S", "weaver_heat_lamp",
                ("Peak_05", "weaver_heat_lamp (2)/Lamp"), preloadAction: MiscFixers.FixLamp)
            .WithRotationGroup(RotationGroup.Four));
        Categories.Misc.Add(new PreloadObject("Weaver Heat Lamp L", "weaver_heat_lamp_l",
                ("Bellway_Peak_02", "weaver lamp_roof heat large"), preloadAction: MiscFixers.FixBigLamp)
            .WithRotationGroup(RotationGroup.Four));
        Categories.Misc.Add(new PreloadObject("Coal Lamp", "coal_lamp",
            ("Peak_05", "coal_lantern_jail_wall_mount/string_cap")));
        
        Categories.Platforming.Add(new PreloadObject("Slope Area", "slope_area",
            ("Peak_05", "Slide Surface (2)"), 
            sprite: ResourceUtils.LoadSpriteResource("slope", FilterMode.Point, ppu:25.6f),
            preloadAction: o =>
            {
                o.GetComponent<PolygonCollider2D>().points =
                [
                    new Vector2(-2.5f, -2.5f),
                    new Vector2(-2.5f, 2.5f),
                    new Vector2(2.5f, 2.5f),
                    new Vector2(2.5f, -2.5f)
                ];
            }).WithConfigGroup(ConfigGroup.Slope)
            .WithRotationGroup(RotationGroup.All));

        Categories.Effects.Add(new PreloadObject("Snow Effect", "snow_effect",
                ("Peak_05", "peak_storm_set_mid_strength"), 
                description: "Affects the whole room.\n" + 
                             "Rotate the object to rotate the direction of the storm.",
                preloadAction: MiscFixers.FixSnow,
                sprite: ResourceUtils.LoadSpriteResource("snow", ppu: 377.5f)))
            .WithScaleAction((o, f) =>
            {
                o.transform.SetScale2D(new Vector2(f, f));
            })
            .WithConfigGroup(ConfigGroup.Decorations).WithRotationGroup(RotationGroup.All);

        /*
        AddEnemy("Pinstress", "pinstress_boss", 
            ("Peak_07", "Pinstress Control/Pinstress Scene/Pinstress Boss"),
            postSpawnAction: EnemyFixers.FixPinstress);*/
    }

    private static void AddBileObjects()
    {
        AddEnemy("Bloatroach", "bloat_roach", ("Shadow_02", "Bloat Roach"),
            preloadAction: EnemyFixers.FixBloatroachPreload,
            postSpawnAction: EnemyFixers.FixBloatroach);
        AddEnemy("Miremite", "swamp_goomba",
            ("Shadow_02", "Black Thread States Thread Only Variant/Normal World/Swamp Goomba")).DoFlipX();
        AddEnemy("Swamp Squit", "swamp_mosquito",
            ("Shadow_04", "Swamp Mosquito (3)"));
        
        Categories.Platforming.Add(new PreloadObject("Water Area", "water_area",
                ("Shadow_04", "GameObject/Surface Water Region"), 
                sprite: ResourceUtils.LoadSpriteResource("water", FilterMode.Point, ppu:25.6f),
                preloadAction: MiscFixers.FixWater)
            .WithRotationGroup(RotationGroup.All)
            .WithConfigGroup(ConfigGroup.Water));

        AddEnemy("Stilkin", "stilkin",
            ("Shadow_12", "Swamp Muckman All Control/Swamp Muckman (4)"),
            postSpawnAction: EnemyFixers.FixStilkin);
        AddEnemy("Stilkin Trapper", "stilkin_trapper",
            ("Shadow_12", "Swamp Muckman All Control/Swamp Muckman Tall Control/Activation Folder/Swamp Muckman Tall"),
            postSpawnAction: EnemyFixers.FixStilkinTrapper).DoFlipX();

        AddEnemy("Mothleaf Lagnia", "mothleaf", ("Shadow_26", "Swamp Drifter"));

        AddEnemy("Groal the Great", "groal", ("Shadow_18", "Battle Scene/Wave 6 - Boss/Swamp Shaman"),
                preloadAction: EnemyFixers.RemoveConstrainPosition,
                postSpawnAction: EnemyFixers.FixGroal)
            .WithConfigGroup(ConfigGroup.Bosses).WithBroadcasterGroup(BroadcasterGroup.Groal);

        AddSolid("Bilewater Platform 1", "swamp_plat_1",
            ("Shadow_02", "plank_plat (4)"), preloadAction: MiscFixers.FixBilePlat);
        AddSolid("Bilewater Platform 2", "swamp_plat_2",
            ("Shadow_26", "gloom_lift_destroy/gloom_lift_set/gloom_plat_lift destroy"));

        /*
        Categories.Effects.Add(new PreloadObject("Maggots", "maggot_effect",
            ("Shadow_18", "maggot_pool (1)/swamp_maggot_animated0000 (10)"), preloadAction: MiscFixers.FixDecoration)
            .WithConfigGroup(ConfigGroup.Decorations));*/

        Categories.Platforming.Add(new PreloadObject("Muck Pod", "swap_bounce_pod",
            ("Shadow_02", "Swamp Bounce Pod")).DoFlipX());
        Categories.Platforming.Add(new PreloadObject("Crumbling Moss", "moss_crumble_plat",
                ("Shadow_02", "moss_crumble_plat"))
            .WithConfigGroup(ConfigGroup.CrumblePlat));
        
        Categories.Effects.Add(new PreloadObject("Maggot Effect", "maggot_effect",
                ("Shadow_02", "hero_maggoted_effect"), description:"Appears when the 'Burst' trigger is run.",
                hideAndDontSave: true, preloadAction: o =>
                {
                    o.RemoveComponent<PlayParticleEffects>();
                    o.RemoveComponent<ParticleSystemAutoDisable>();
                    o.transform.GetChild(1).gameObject.SetActive(false);
                    o.transform.GetChild(2).gameObject.SetActive(false);
                }, sprite: ResourceUtils.LoadSpriteResource("maggot_burst", ppu:62.5f)
                ).WithReceiverGroup(ReceiverGroup.Confetti));

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
                }).WithRotationGroup(RotationGroup.All).WithReceiverGroup(ReceiverGroup.Trap)
            .WithConfigGroup(ConfigGroup.Hazards));

        Categories.Hazards.Add(new PreloadObject("Groal's Spike Ball", "swing_trap_spike",
                ("Shadow_18", "Battle Scene/stake_trap_swing_repeater"),
                description: "Only has collision when swinging.",
                preloadAction: HazardFixers.FixSpikeBall)
            .WithConfigGroup(ConfigGroup.Hazards)
            .WithReceiverGroup(ReceiverGroup.SpikeBall));

        Categories.Hazards.Add(new PreloadObject("Groal's Vengeful Spirit", "groal_fireball",
            ("Shadow_18", "Swamp Shaman Fireball"), hideAndDontSave: true,
            postSpawnAction: o =>
            {
                o.transform.SetScaleX(-o.transform.GetScaleX());
                var rb2d = o.GetComponent<Rigidbody2D>();
                var fly = o.LocateMyFSM("Control").GetState("Fly");
                fly.DisableAction(2);
                fly.AddAction(() =>
                {
                    rb2d.linearVelocity = o.transform.rotation * (new Vector3(-28, 0, 0) * o.transform.GetScaleX());
                }, 0, true);
            }).WithRotationGroup(RotationGroup.All));
    }

    private static void AddMistObjects()
    {
        AddEnemy("Wraith", "wraith", ("Dust_Maze_01", "Wraith"),
            preloadAction: EnemyFixers.RemoveConstrainPosition);
        /*AddEnemy("Phantom", "phantom", ("Organ_01", "Boss Scene/Phantom"),
            postSpawnAction: EnemyFixers.FixPhantom);*/

        Categories.Hazards.Add(new PreloadObject("Pressure Plate Trap", "dust_trap_spike_plate",
            ("Dust_Maze_01", "Mist Maze Controller/Trap Sets/Trap Set/Dust Trap Spike Plate")));
        Categories.Hazards.Add(new PreloadObject("Falling Spike Trap", "dust_trap_spike_dropper",
            ("Dust_Maze_01", "Mist Maze Controller/Trap Sets/Trap Set/Dust Trap Spike Dropper")).DoFlipX());
        Categories.Hazards.Add(new PreloadObject("Mite Trap", "mite_trap",
            ("Dust_Maze_01", "Mist Maze Controller/Trap Sets/Trap Set/Mite Trap")).DoFlipX());
        
        Categories.Hazards.Add(new PreloadObject("Organ Spikes", "organ_spikes",
            ("Organ_01", "Spike (7)"))
            .WithRotationGroup(RotationGroup.Four));
        AddSolid("Organ Platform 1", "organ_plat_1", ("Organ_01", "Organ_outer__0012_balcony_side_plat"));
        AddSolid("Organ Platform 2", "organ_plat_2", ("Organ_01", "GameObject (58)/metal_bridge (2)"),
            preloadAction: o =>
            {
                for (var i = 1; i <= 4; i++) o.transform.GetChild(i).gameObject.SetActive(false);
            });
        AddSolid("Organ Platform 3", "organ_plat_3", ("Organ_01", "organ_lift_broken_drop/lift_bottom_broken"),
            preloadAction: o => o.transform.GetChild(3).gameObject.SetActive(false));
    }

    private static void AddStepsObjects()
    {
        Categories.Platforming.Add(new PreloadObject("Grey Ring", "harpoon_ring_pinstress",
                ("Coral_34", "Harpoon Ring Pinstress Rope (4)"), postSpawnAction: MiscFixers.FixRing).DoFlipX()
            .WithBroadcasterGroup(BroadcasterGroup.HarpoonRings));

        AddEnemy("Judge", "judge", ("Coral_32", "Black Thread States/Normal World/Coral Judge (3)"),
                preloadAction: EnemyFixers.FixJudge)
            .WithConfigGroup(ConfigGroup.Judge)
            .WithReceiverGroup(ReceiverGroup.Wakeable);

        Categories.Platforming.Add(new PreloadObject("Bell of Judgement", "hang_bell",
            ("Coral_32", "shell_plat_hang_bell (4)"), preloadAction: MiscFixers.FixBellSprite));

        AddEnemy("Last Judge", "last_judge", ("Coral_Judge_Arena", "Boss Scene/Last Judge"),
            postSpawnAction:EnemyFixers.FixLastJudge)
            .WithConfigGroup(ConfigGroup.LastJudge)
            .WithBroadcasterGroup(BroadcasterGroup.Bosses);
    }

    private static void AddMiscObjects()
    {
        Categories.Misc.Add(new PreloadObject("Breakable Wall A", "breakable_wall_2",
            ("Bone_19", "Breakable Wall"),
            preloadAction: o =>
            {
                o.transform.GetChild(0).gameObject.SetActive(false);
                o.transform.GetChild(1).gameObject.SetActive(false);
            },
            postSpawnAction: MiscFixers.FixBreakableWall)
            .WithConfigGroup(ConfigGroup.BreakableWall)
            .WithBroadcasterGroup(BroadcasterGroup.BreakableWall));

        Categories.Misc.Add(new PreloadObject("Breakable Wall B", "breakable_wall",
                ("Aqueduct_03", "Breakable Wall"),
                preloadAction: o =>
                {
                    o.transform.GetChild(0).gameObject.SetActive(false);
                    o.transform.GetChild(1).gameObject.SetActive(false);
                    var col2d = o.GetComponent<BoxCollider2D>();
                    col2d.offset = Vector2.zero;
                    col2d.size = new Vector2(2.25f, 4.25f);
                }, postSpawnAction: MiscFixers.FixBreakableWall)
            .WithConfigGroup(ConfigGroup.BreakableWall)
            .WithBroadcasterGroup(BroadcasterGroup.BreakableWall));
        
        Categories.Misc.Add(new PreloadObject("Rosary Shrine", "rosary_shrine_small",
            ("Bonetown", "rosary_shrine_small"),
            preloadAction: MiscFixers.FocusFirstChild));
        
        Categories.Misc.Add(new PreloadObject("Rosary Bell", "rosary_bell",
            ("Belltown_basement_03", "rosary_cache_bell_ground"),
            preloadAction: o => o.transform.GetChild(1).GetChild(3).SetAsFirstSibling()));

        Categories.Misc.AddStart(new PreloadObject("Toll Bench", "toll_bench",
                ("Under_08", "Understore Toll Bench (2)"),
                preloadAction: MiscFixers.FixTollBench, preview: true)
            .WithConfigGroup(ConfigGroup.Benches));
        
        Categories.Misc.AddStart(new PreloadObject("Bell Bench", "bell_bench",
                ("Bone_East_15", "bell_bench/RestBench"),
                preloadAction: MiscFixers.FixBench, preview: true)
            .WithConfigGroup(ConfigGroup.Benches));

        Categories.Interactable.Add(new PreloadObject("Toll Machine", "rosary_toll",
                ("Hang_06_bank", "rosary_cannon/Art/Rosary Cannon Scene/rosary_string_machine"),
                postSpawnAction: MiscFixers.FixToll)
            .WithBroadcasterGroup(BroadcasterGroup.Toll)
            .WithConfigGroup(ConfigGroup.Toll));

        Categories.Effects.Add(new PreloadObject("Reflection Effect", "mirror_effect",
                ("Hang_06b", "new_scene/Reflection_surface"),
                description: "Reflects objects above itself, can be configured\n" +
                             "in the same way as the Custom PNG for custom mirror shapes.",
                preloadAction: MiscFixers.FixMirror, sprite: ResourceUtils.LoadSpriteResource("reflection", ppu: 155))
            .WithConfigGroup(ConfigGroup.Mirror));

        Categories.Misc.Add(new PreloadObject("Gilly NPC", "gilly_npc",
                ("Ant_17", "Gilly"), 
                postSpawnAction: MiscFixers.FixGilly)
            .WithConfigGroup(ConfigGroup.Npcs).DoFlipX());

        Categories.Misc.Add(new PreloadObject("Wandering Seth NPC", "seth_npc",
                ("Coral_10", "Seth Stand NPC"), 
                postSpawnAction: MiscFixers.FixSeth)
            .WithConfigGroup(ConfigGroup.Npcs));

        Categories.Misc.Add(new PreloadObject("Garmond and Zaza NPC (Ally)", "garmond_zaza",
                ("Song_17", "Garmond Fight Scene/Garmond Fighter"),
                postSpawnAction: MiscFixers.FixGarmond)
            .WithConfigGroup(ConfigGroup.Npcs));

        AddEnemy("Garmond and Zaza (Boss)", "garmond_zaza_boss",
            ("Library_09", "Black Thread States/Normal World/Scene Control/Garmond Scene/Garmond Fighter"),
            preloadAction: o =>
            {
                var anim = o.GetComponent<tk2dSpriteAnimator>();
                anim.defaultClipId = anim.GetClipIdByName("Roar");
            },
            postSpawnAction: MiscFixers.FixGarmondBoss)
            .WithConfigGroup(ConfigGroup.GarmondBoss)
            .WithBroadcasterGroup(BroadcasterGroup.Bosses);

        AddEnemy("Lost Garmond", "lost_garmond",
            ("Coral_33", "Black Thread States/Black Thread World/Garmond Scenes/Garmond Black Threaded Scene/Garmond Black Threaded Fighter"),
            preloadAction: EnemyFixers.FixLostGarmondPreload,
            postSpawnAction: EnemyFixers.FixLostGarmond)
            .WithConfigGroup(ConfigGroup.Bosses)
            .WithBroadcasterGroup(BroadcasterGroup.Bosses);

        Categories.Misc.Add(new PreloadObject("Shakra Ring", "mapper_ring",
                ("Shadow_02",
                    "Mapper/Mapper_ambient_rings/rings/mapper extra rings/mapper rings/Mapper_Ring_world (3)"),
                preloadAction: MiscFixers.MarkRing)
            .WithConfigGroup(ConfigGroup.MapperRing)
            .WithBroadcasterGroup(BroadcasterGroup.MapperRing));
        
        Categories.Misc.Add(new PreloadObject("Silk Spool", "silk_spool_take",
            ("Hang_01", "Thread Spinner")).WithConfigGroup(ConfigGroup.SilkSpool));
        
        AddEnemy("Servitor Ignim", "servitor_small", ("Weave_04", "Weaver Servitor (2)"),
            preloadAction: EnemyFixers.FixServitorIgnim);
        
        AddEnemy("Servitor Boran", "servitor_large", ("Peak_04d", "Weaver Servitor Large"),
            preloadAction: EnemyFixers.FixServitorBoran)
            .WithConfigGroup(ConfigGroup.Boran);

        Categories.Interactable.Add(new PreloadObject("Silk Lever", "silk_lever",
            ("Weave_12", "weaver_lift_power_chamber/switches/Lever_Left"), 
            preloadAction: InteractableFixers.FixSilkLever)
            .WithConfigGroup(ConfigGroup.SilkLever)
            .WithBroadcasterGroup(BroadcasterGroup.Levers)
            .WithRotationGroup(RotationGroup.Eight));

        Categories.Misc.Add(new PreloadObject("Silkcatcher", "silkcatcher_plant",
            ("Ant_04", "Silkcatcher Plant")));
        
        Categories.Misc.Add(new PreloadObject("Silkdew", "silkcatcher_dew",
            ("Clover_06", "Group/Clover_Silk_Pod")));

        /*
        Categories.Interactable.Add(new PreloadObject("Shakra Summon Pole", "mapper_pole",
                ("Greymoor_08_mapper", "Mapper Call Pole")));

        AddEnemy("Shakra (Boss)", "shakra_boss",
                ("Greymoor_08_mapper", "Mapper Spar NPC"),
                postSpawnAction: MiscFixers.FixShakraBoss)
            .WithConfigGroup(ConfigGroup.Bosses)
            .WithBroadcasterGroup(BroadcasterGroup.Bosses);*/

        Categories.Misc.Add(new PreloadObject("Shakra NPC (Ally)", "shakra",
                ("Shellwood_01",
                    "Black Thread States/Black Thread World/Shakra Guard Scene/Scene Folder/Mapper StandGuard NPC"),
                postSpawnAction: MiscFixers.FixShakra)
            .WithConfigGroup(ConfigGroup.Shakra));

        Categories.Misc.Add(new PreloadObject("Second Sentinel NPC (Ally)", "second_sentinel_ally",
            ("Song_25", "Song Knight Control/Song Knight Present/Song Knight BattleEncounter"),
            postSpawnAction: MiscFixers.FixSecondSentinelAlly));

        AddEnemy("Second Sentinel (Boss)", "second_sentinel_boss",
            ("Hang_17b", "Boss Scene - To Additive Load/Song Knight"),
            postSpawnAction: EnemyFixers.FixSecondSentinelBoss)
            .WithConfigGroup(ConfigGroup.Bosses).DoFlipX();

        Categories.Misc.Add(new PreloadObject("Pilgrim Preacher", "pilgrim_preacher",
                ("Song_Enclave",
                    "Black Thread States/Normal World/Enclave States/States/Level 1/Enclave Simple NPC Tall"),
                postSpawnAction: MiscFixers.FixPreacher)
            .WithConfigGroup(ConfigGroup.Npcs).DoFlipX());

        Categories.Misc.Add(new PreloadObject("Caretaker NPC", "caretaker",
                ("Song_Enclave",
                    "Black Thread States/Normal World/Enclave States/States/Level 1/Enclave Caretaker"),
                postSpawnAction: MiscFixers.FixCaretaker)
            .WithConfigGroup(ConfigGroup.Caretaker).DoFlipX());

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

        AddEnemy("Snitchfly", "snitchfly",
                ("Bonetown", "Black Thread States/Black Thread World/Thief Scene/Rosary Thief Group/Rosary Thief"),
                postSpawnAction: o =>
                {
                    o.LocateMyFSM("Control").GetState("Left").AddAction(() => o.BroadcastEvent("OnFlee"), 0);
                }).WithBroadcasterGroup(BroadcasterGroup.Snitchfly)
            .DoFlipX();

        AddEnemy("Summoned Saviour", "summoned_saviour",
            ("Bone_Steel_Servant", "Steel Servant Scene/Battle Scene/Wave 1/Abyss Mass"),
            postSpawnAction: EnemyFixers.FixSummonedSaviour)
            .WithBroadcasterGroup(BroadcasterGroup.Bosses)
            .WithConfigGroup(ConfigGroup.Bosses).DoFlipX();
        
        Categories.Misc.Add(new PreloadObject("Greymoor Lamp", "greymoor_lamp",
                ("Greymoor_03", "break_grey_lamp_dual_twist (1)"), postSpawnAction: MiscFixers.FixBreakableLamp)
            .WithConfigGroup(ConfigGroup.BreakableDecor)
            .WithRotationGroup(RotationGroup.Four)
            .WithBroadcasterGroup(BroadcasterGroup.Breakable));
        Categories.Misc.Add(new PreloadObject("Vaults Lamp", "vault_lamp",
                ("Library_04", "library_lamp_stand (1)"), postSpawnAction: MiscFixers.FixBreakableLamp)
            .WithConfigGroup(ConfigGroup.BreakableDecor)
            .WithRotationGroup(RotationGroup.Four)
            .WithBroadcasterGroup(BroadcasterGroup.Breakable));
        Categories.Misc.Add(new PreloadObject("Vaults Wall Lamp", "vault_w_lamp",
                ("Library_04", "library_lamp_wall (2)"), postSpawnAction: MiscFixers.FixBreakableLamp)
            .WithConfigGroup(ConfigGroup.BreakableDecor)
            .WithRotationGroup(RotationGroup.Four)
            .WithBroadcasterGroup(BroadcasterGroup.Breakable));
    }

    private static void AddMemoriumObjects()
    {
        AddSolid("Memorium Platform 1", "memorium_plat_2", 
            ("Arborium_03", "hanging_gardens_plat_float_metal_small (3)"));
        AddSolid("Memorium Platform 2", "memorium_plat_1", ("Arborium_03", "Arborium Plat Mid"));
        
        AddEnemy("Memoria", "memoria", ("Arborium_03", "Arborium Keeper"),
            preloadAction: EnemyFixers.FixFlyin);

        AddEnemy("Huge Flea", "huge_flea", ("Arborium_08", "Giant Flea Scene/Giant Flea"),
            postSpawnAction: EnemyFixers.FixGiantFlea)
            .WithBroadcasterGroup(BroadcasterGroup.Bosses)
            .WithConfigGroup(ConfigGroup.HugeFlea);
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
        
        AddEnemy("Cogwork Defender", "cogwork_defender",
            ("Cog_06", "Song Automaton Shield")).DoFlipX();

        AddEnemy("Cogwork Spine", "cogwork_spine",
            ("Cog_05", "Battle Scene/Wave 2/Song Automaton Fly Spike"),
            preloadAction: EnemyFixers.FixCogworkSpine).WithConfigGroup(ConfigGroup.Patroller);
        
        AddEnemy("Cogwork Cleanser", "song_auto_2",
            ("Cog_04", "Black Thread States Thread Only Variant/Black Thread World/Group (1)/Song Automaton 02"));
        AddEnemy("Cogwork Crawler", "song_auto_crawl",
            ("Cog_04", "Song Automaton Goomba"), preloadAction:EnemyFixers.RemoveConstrainPosition)
            .WithRotateAction((obj, rot) =>
            {
                if (Mathf.RoundToInt(rot / 180) != 1) return;
                obj.transform.SetScaleY(-obj.transform.GetScaleY());
                obj.transform.SetRotation2D(obj.transform.GetRotation2D() + rot - 180);
            }).WithRotationGroup(RotationGroup.Vertical);
        
        AddEnemy("Cogwork Clapper", "cog_clapper",
            ("Cog_07", "Black Thread States/Normal World/Repairable Scene/Song Automaton Ball (1)"),
            preloadAction: EnemyFixers.FixCogworkClapperAnim,
            postSpawnAction: EnemyFixers.FixCogworkClapper);
        
        AddSolid("Cogworks Platform 1", "cog_plat_1", 
            ("Cog_04", "cog_plat_float_tiny (1)"),
            preloadAction: MiscFixers.FocusFirstChild);
        AddSolid("Cogworks Platform 2", "cog_plat_2", 
            ("Cog_04", "cog_plat_float (2)"),
            preloadAction: MiscFixers.FocusFirstChild);
    }

    private static void AddBellObjects()
    {
        AddEnemy("Furm", "furm", ("Belltown_basement_03", "Bell Goomba"),
            postSpawnAction: EnemyFixers.FixFurm)
            .WithConfigGroup(ConfigGroup.Furm)
            .WithRotationGroup(RotationGroup.Four).DoFlipX();
        AddEnemy("Winged Furm", "winged_furm", ("Belltown_basement_03", "Bell Fly"),
            preloadAction: EnemyFixers.RemoveConstrainPosition,
            postSpawnAction: EnemyFixers.FixWingedFurm)
            .WithConfigGroup(ConfigGroup.WingedFurm);
        
        Categories.Hazards.Add(new PreloadObject("Falling Bell", "falling_bell",
                ("Belltown_04", "Drop Bell (1)"))
            .WithConfigGroup(ConfigGroup.FallingBell)
            .WithReceiverGroup(ReceiverGroup.Dropper));
        
        AddSolid("Bell Platform 1", "bell_plat_1", 
            ("Belltown", "hanging_bell_house 2/plat_float_07"),
            preloadAction: o =>
            {
                o.transform.parent = null;
                o.transform.SetPositionZ(-0.1174f);
            });
        AddSolid("Bell Platform 2", "bell_plat_2", 
            ("Belltown", "Hornet House States/Full/plat_float_07 (1)"),
            preloadAction: o =>
            {
                o.transform.parent = null;
                o.transform.SetPositionZ(-0.1174f);
                o.transform.GetChild(0).SetLocalPositionZ(0.001f);
            });
        AddSolid("Bell Platform 3", "bell_plat_3",
            ("Belltown", "Black Thread States Thread Only Variant/Normal World/shop_sign"),
            preloadAction: o =>
            {
                o.transform.parent = null;
                o.transform.SetPositionZ(-0.1174f);
                for (var i = 2; i <= 5; i++) o.transform.GetChild(i).gameObject.SetActive(false);
                o.transform.GetChild(1).GetChild(3).gameObject
                    .AddComponent<PlaceableObject.SpriteSource>();
            });
    }

    private static void AddWispObjects()
    {
        Categories.Platforming.Add(new PreloadObject("Wisp Bounce Pod", "wisp_bounce_pod",
            ("Wisp_02", "Wisp Bounce Pod")));

        Categories.Hazards.Add(new PreloadObject("Wispfire Lantern", "wisp_flame_lantern",
                ("Wisp_02", "Wisp Flame Lantern"), preloadAction: HazardFixers.FixWispLantern)
            .WithConfigGroup(ConfigGroup.WispLanterns));

        Categories.Hazards.Add(new PreloadObject("Wisp", "wisp",
                ("Wisp_02", "Wisp Fireball"), postSpawnAction: HazardFixers.FixWisp, hideAndDontSave: true)
            .WithConfigGroup(ConfigGroup.Wisp));

        AddEnemy("Burning Bug", "farmer_wisp", 
            ("Wisp_02", "Wisp Farmers/Farmer Wisp"), preloadAction: o =>
            {
                EnemyFixers.KeepActive(o);
                var anim = o.GetComponent<tk2dSpriteAnimator>();
                anim.defaultClipId = anim.GetClipIdByName("Idle");

                var choice = o.LocateMyFSM("Control").GetState("Choice");
                choice.transitions = choice.transitions
                    .Where(trans => trans.EventName != "HORNET DEAD").ToArray();
            })
            .WithConfigGroup(ConfigGroup.BurningBug).DoFlipX();
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
        
        AddSolid("Shellwood Platform 1", "wood_plat_1", 
            ("Shellwood_01", "shellwood_plat_float_thin"));
        AddSolid("Shellwood Platform 2", "wood_plat_2", 
            ("Shellwood_01", "shellwood_plat_float_wide"));

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

        Categories.Enemies.Add(new PreloadObject("Wood Wasp Hive", "wasp_hive",
            ("Shellwood_02", "Shellwood Hive (1)"), preloadAction: o =>
            {
                o.transform.GetChild(2).GetChild(0).gameObject.AddComponent<PlaceableObject.SpriteSource>();
                o.transform.GetChild(2).GetChild(1).gameObject.SetActive(false);
            }));
        AddEnemy("Wood Wasp", "wood_wasp", ("Shellwood_02", "Shellwood Wasp"));
        AddEnemy("Splinter", "splinter", ("Shellwood_02", "Stick Insect"),
            preloadAction: EnemyFixers.FixSplinter);
        AddEnemy("Splinterhorn", "splinterhorn", ("Shellwood_02", "Stick Insect Charger"),
            preloadAction: EnemyFixers.FixSplinter);
        AddEnemy("Splinterbark", "splinterbark", 
            ("Shellwood_26", "Black Thread States/Normal World/Stick Insect Flyer (1)"));
        
        AddEnemy("Crawling Shellwood Gnat", "shellwood_gnat", 
            ("Shellwood_01", "Shellwood Goomba")).DoFlipX();
        
        AddEnemy("Flying Shellwood Gnat", "shellwood_gnat_fly", 
            ("Shellwood_01", "Shellwood Goomba Flyer (1)"));
        
        AddEnemy("Shellwood Gnat Core", "shellwood_gnat_core", 
            ("Shellwood_01", "Shellwood Gnat"), hideAndDontSave: true);

        Categories.Effects.Add(new PreloadObject("Pollen Effect", "pollen_effect",
                ("Shellwood_10", "pollen_particles (1)"), description: "Affects the whole room.",
                preloadAction: MiscFixers.FixDecoration,
                sprite: ResourceUtils.LoadSpriteResource("pollen", ppu: 377.5f)))
            .WithScaleAction((o, f) =>
            {
                o.transform.SetScale2D(new Vector2(f, f));
            })
            .WithConfigGroup(ConfigGroup.Decorations);
        
        /*
        AddEnemy("Sister Splinter", "sister_splinter",
            ("Shellwood_18", "Boss Scene Parent/Boss Scene/Splinter Queen"),
            postSpawnAction: EnemyFixers.FixSisterSplinter)
            .WithConfigGroup(ConfigGroup.Bosses).DoFlipX();*/

        /*AddEnemy("Shrine Guardian Seth", "seth_boss", ("Shellwood_22", "Boss Scene/Seth"),
            postSpawnAction: EnemyFixers.FixSeth);*/
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
                ("Ant_21", "Enemy Control/Ant Merchant Killed/Big Guard Dead/Bone Hunter Fly"),
                preloadAction: EnemyFixers.FixSpearSkarr)
            .WithConfigGroup(ConfigGroup.SpearSkarr)
            .WithReceiverGroup(ReceiverGroup.Wakeable);

        AddEnemy("Skarrgard", "bone_hunter_throw",
            ("Ant_21", "Enemy Control/Normal/Bone Hunter Throw"),
            preloadAction: EnemyFixers.RemoveConstrainPosition);

        AddEnemy("Last Claw", "last_claw",
            ("Memory_Ant_Queen", "Boss Scene/Battle Scene/Wave 4/Bone Hunter Fly Chief"),
            preloadAction: EnemyFixers.FixLastClawPreload,
            postSpawnAction: EnemyFixers.FixLastClaw)
            .WithScaleAction(EnemyFixers.ScaleLastClaw);

        /*
        AddEnemy("Skarrsinger Karmelita", "karmelita",
            ("Memory_Ant_Queen", "Boss Scene/Hunter Queen Boss"),
            preloadAction: EnemyFixers.FixKarmelitaPreload,
            postSpawnAction: EnemyFixers.FixKarmelita);
            */

        Categories.Platforming.Add(new PreloadObject("Hunterfruit", "march_pogo",
            ("Ant_04", "White Palace Fly")));

        Categories.Platforming.Add(new PreloadObject("Bending Pole Ring", "march_ring",
                ("Ant_09", "Fields Harpoon Ring Pole"), preloadAction: MiscFixers.FixPoleRing)
            .WithConfigGroup(ConfigGroup.PoleRing));

        Categories.Hazards.Add(new PreloadObject("Sickle Trap", "hunter_sickle_trap",
                ("Ant_04", "Hunter Sickle Trap"))
            .WithReceiverGroup(ReceiverGroup.Trap)).DoFlipX();

        Categories.Hazards.Add(new PreloadObject("Gurr Trap", "hunter_landmine",
                ("Bone_East_24", "Ant Trapper Quest Scene (3)/Tracking Scene/Trapper Barb Trap Landmine"))
            .WithReceiverGroup(ReceiverGroup.Trap)
            .WithRotationGroup(RotationGroup.All));

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

        AddEnemy("Skull Tyrant", "skull_tyrant", ("Bonetown_boss", "Boss Scene/Skull King"),
            postSpawnAction: EnemyFixers.FixSkullTyrant)
            .WithConfigGroup(ConfigGroup.Bosses)
            .WithBroadcasterGroup(BroadcasterGroup.SkullTyrant);

        Categories.Misc.Add(new PreloadObject("Large Skull", "bone_goomba_skull_large",
            ("Bone_East_03", "bone_goomba_skull_break_large)")));

        AddEnemy("Caranid", "bone_circler",
            ("Bone_East_03", "Black Thread States Thread Only Variant/Normal World/Hunting PreScene/Bone Circler"));

        AddEnemy("Vicious Caranid", "bone_circler_v",
            ("Bone_East_03", "Black Thread States Thread Only Variant/Black Thread World/Bone Circler Vicious (1)"));

        AddEnemy("Kilik", "bone_crawler", ("Ant_19", "Bone Crawler (2)"),
            preloadAction: EnemyFixers.RemoveConstrainPosition).WithRotationGroup(RotationGroup.Four);

        AddEnemy("Beastfly", "beastfly", ("Ant_19", "Enemy Break Cage (9)/Enemy/Bone Flyer"),
                preloadAction: EnemyFixers.RemoveConstrainPosition).DoFlipX();

        AddEnemy("Savage Beastfly", "savage_beastfly", ("Ant_19", "Boss Control/Boss Scene/Bone Flyer Giant"),
            preloadAction: EnemyFixers.RemoveConstrainPosition,
            postSpawnAction: EnemyFixers.FixSavageBeastfly)
            .WithConfigGroup(ConfigGroup.SavageBeastfly)
            .WithBroadcasterGroup(BroadcasterGroup.SavageBeastfly).DoFlipX();

        AddEnemy("Mawling", "bone_roller", ("Arborium_03", "Bone Roller"));

        AddEnemy("Marrowmaw", "bone_thumper", ("Arborium_04", "Enemy Respawner/Source Folder/Bone Thumper"),
            preloadAction: EnemyFixers.ApplyGravity);

        AddEnemy("Tarmite", "tarmite", ("Bone_East_LavaChallenge", "Bone Spitter"),
            preloadAction: EnemyFixers.KeepActive);

        AddEnemy("Flintbeetle", "flintbeetle", ("Bone_06", "Rock Roller Scene/Rock Roller"),
            preloadAction: EnemyFixers.FixFlintbeetlePreload,
            postSpawnAction: EnemyFixers.FixFlintbeetle)
            .WithRotationGroup(RotationGroup.Four)
            .WithConfigGroup(ConfigGroup.Wakeable)
            .WithReceiverGroup(ReceiverGroup.Wakeable);

        Categories.Hazards.Add(new PreloadObject("Flintbomb", "flint_bomb",
            ("Bone_06", "Rock Roller Bomb"),
            description:"Usually already landed by the time the room finishes loading.\n" +
                        "Best used with the Object Spawner.",
            hideAndDontSave: true)
            .WithConfigGroup(ConfigGroup.Velocity));

        AddEnemy("Shardillard", "shardillard", ("Bone_06", "Shell Fossil Mimic AppearVariant"),
            preloadAction: o => o.transform.SetRotation2D(0),
            postSpawnAction: EnemyFixers.FixShardillard).DoFlipX();

        Categories.Platforming.Add(new PreloadObject("Magnetite Platform 1", "bone_plat_crumble_1",
            ("Bone_East_LavaChallenge", "bone_plat_01_crumble_small (2)")));

        Categories.Platforming.Add(new PreloadObject("Magnetite Platform 2", "bone_plat_crumble_2",
            ("Bone_East_LavaChallenge", "bone_plat_01_crumble (2)")));

        Categories.Platforming.Add(new PreloadObject("Magnetite Platform 3", "bone_plat_crumble_3",
            ("Bone_East_LavaChallenge", "bone_plat_02_crumble (1)")));

        Categories.Platforming.Add(new PreloadObject("Magnetite Platform 4", "bone_plat_crumble_4",
            ("Bone_East_LavaChallenge", "bone_plat_crumble_tall (4)")));

        Categories.Hazards.Add(new PreloadObject("Bone Boulder", "bone_boulder",
            ("Bone_East_03", "Black Thread States Thread Only Variant/Normal World/Bone_Boulder"))
            .WithConfigGroup(ConfigGroup.Hazards)
            .WithReceiverGroup(ReceiverGroup.Dropper));

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

        AddEnemy("Lavalug", "tar_slug", ("Dock_02", "Tar Slug"))
            .WithRotationGroup(RotationGroup.Four).DoFlipX();
        AddEnemy("Lavalarga", "tar_slug_huge", ("Dock_11", "Tar Slug Huge (1)"))
            .WithRotationGroup(RotationGroup.Four);
        AddEnemy("Flintflame Flyer", "dock_bomber", ("Dock_02", "Dock Bomber"),
            postSpawnAction: EnemyFixers.FixFlintFlyer);
        AddEnemy("Smokerock Sifter", "shield_dockworker",
            ("Dock_02", "Shield Dockworker Spawn/Shield Dockworker (2)"));
        AddEnemy("Deep Diver", "dock_charger", ("Dock_02b", "Dock Charger")).DoFlipX();

        Categories.Interactable.Add(new PreloadObject("Deep Docks Gate", "song_gate_small",
                ("Bone_East_15", "Song_Gate_small (3)"),
                preloadAction: o => o.RemoveComponent<PersistentBoolItem>())
            .WithReceiverGroup(ReceiverGroup.Gates)
            .WithRotationGroup(RotationGroup.Eight));
        Categories.Interactable.Add(new PreloadObject("Deep Docks Lever", "song_lever_side",
                ("Bone_East_15", "Song_lever_side"), postSpawnAction: InteractableFixers.FixLever)
            .WithBroadcasterGroup(BroadcasterGroup.Levers)
            .WithConfigGroup(ConfigGroup.Levers)
            .WithRotationGroup(RotationGroup.Eight).DoFlipX());

        AddSolid("Docks Platform 1", "dock_plat_01", ("Bone_East_01", "dock_plat_float_01 (1)"));
        AddSolid("Docks Platform 2", "dock_plat_02", ("Bone_East_01", "dock_plat_float_01 (9)"));

        Categories.Platforming.Add(new PreloadObject("Crumbling Rocks 1", "lava_crumble_plat",
                ("Dock_02b", "lava_crumble_plat (5)"))
            .WithConfigGroup(ConfigGroup.CrumblePlat));

        Categories.Hazards.Add(new PreloadObject("Hot Coals", "hot_coal",
            ("Bone_East_03", "lava_rocks_top_glower"),
            preloadAction: o =>
            {
                o.transform.GetChild(0).Translate(-6.6806f, -0.9279f, 0);
                o.transform.GetChild(1).Translate(-6.6806f, -0.9279f, 0);
            },
            postSpawnAction: HazardFixers.FixCoal));

        Categories.Hazards.Add(new PreloadObject("Lava", "lava_area",
            ("Bone_East_09", "lava_set/LavaBase (1)"),
            preloadAction: o =>
            {
                o.transform.parent = null;
                o.transform.localScale = new Vector3(20, 5, 0);
                o.transform.SetPositionZ(-0.002f);
            },
            postSpawnAction: HazardFixers.FixLava,
            sprite: ResourceUtils.LoadSpriteResource("lava", FilterMode.Point, ppu: 128))
            .WithConfigGroup(ConfigGroup.StretchableHazards));

        Categories.Hazards.Add(new PreloadObject("Falling Lava", "falling_lava",
                ("Under_19", "Lava_Waterfall Set (4)"),
                preloadAction: o =>
                    o.transform.GetChild(0).GetChild(0).GetChild(0).gameObject
                        .AddComponent<PlaceableObject.SpriteSource>())
                .WithConfigGroup(ConfigGroup.Hazards))
            .Offset -= new Vector3(0, 8, 0);
    }

    private static void AddFieldsObjects()
    {
        AddSolid("Far Fields Platform 1", "fung_plat_float_01", ("Bone_East_15", "fung_plat_float_06"));
        AddSolid("Far Fields Platform 2", "fung_plat_float_02", ("Bone_East_14", "bone_plat_03"));

        AddEnemy("Brushflit", "brushflit", ("Bone_East_15", "Fields Flock Flyer"),
            preloadAction: EnemyFixers.FixBrushflit)
            .WithScaleAction(EnemyFixers.ScaleBrushflit);
        AddEnemy("Fertid", "fertid", ("Bone_East_15", "Fields Goomba")).DoFlipX();
        AddEnemy("Flapping Fertid", "flapping_fertid", ("Bone_East_15", "Fields Flyer"),
            preloadAction: EnemyFixers.FixPatroller)
            .WithConfigGroup(ConfigGroup.Patroller).DoFlipX();

        AddEnemy("Hardbone Hopper", "hardbone_hopper", ("Bone_East_24", "Bone Hopper Group/Bone Hopper Simple"));
        AddEnemy("Hardbone Elder", "hardbone_elder", ("Bone_East_24", "Bone Hopper Group/Bone Hopper Giant"));
        
        AddEnemy("Hoker", "spine_floater", ("Bone_East_14", "Spine Floater (9)"),
                postSpawnAction: MiscFixers.FixHoker)
            .WithConfigGroup(ConfigGroup.Hoker).DoFlipX();

        AddEnemy("Rhinogrund", "rhino", ("Bone_East_10_Church", "Rhino Scene/Rhino"),
            preloadAction:EnemyFixers.KeepActive);

        Categories.Misc.Add(new PreloadObject("Sleeping Flea", "flea_1",
            ("Bone_East_10_Church", "Black Thread States Thread Only Variant/Normal World/Flea Rescue Sleeping"),
            description:"Fluffy.\nDoes not add to collected fleas.",
            postSpawnAction:MiscFixers.FixFlea)
            .WithConfigGroup(ConfigGroup.Fleas)
            .WithBroadcasterGroup(BroadcasterGroup.Fleas));
    }

    private static void AddWormwaysObjects()
    {
        Categories.Misc.Add(new PreloadObject("Lifeblood Cocoon", "health_cocoon",
                ("Crawl_09", "Area_States/Infected/Health Cocoon"))
            .WithConfigGroup(ConfigGroup.LifebloodCocoons)
            .WithReceiverGroup(ReceiverGroup.LifebloodCocoons));

        AddEnemy("Cragglite", "cragglite", ("Crawl_04", "Little Crabs/Crabs/Small Crab"))
            .WithRotationGroup(RotationGroup.Four)
            .DoFlipX();
        AddEnemy("Craggler", "craggler", ("Crawl_04", "Roof Crab"),
            postSpawnAction: EnemyFixers.FixCraggler);
        
        AddEnemy("Plasmid", "plasmid",
            ("Crawl_03", "Area_States/Infected/Bone Worm BlueBlood (1)")).DoFlipX();
        AddEnemy("Plasmidas", "plasmidas",
            ("Crawl_03", "Area_States/Infected/Bone Worm BlueTurret"),
            preloadAction: o => o.transform.Find("blueblood_worm_growths").gameObject.SetActive(false));

        AddEnemy("Plasmified Zango", "zango_boss", ("Crawl_10", "Area_States/Infected/Blue Assistant"),
                postSpawnAction: EnemyFixers.FixZango)
            .WithBroadcasterGroup(BroadcasterGroup.Bosses)
            .WithConfigGroup(ConfigGroup.Bosses);
    }

    private static void AddFleaObjects()
    {
        Categories.Effects.Add(new PreloadObject("Confetti Burst", "confetti_burst",
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
        
        Categories.Misc.Add(new PreloadObject("Fleamaster NPC", "fleamaster_npc",
            ("Aqueduct_05_festival", "Caravan_States/Flea Festival/Flea Game - Juggling/Flea Games Host NPC"),
            preloadAction: MiscFixers.FixFleamaster)
            .WithConfigGroup(ConfigGroup.Npcs));

        /*Categories.Platforming.Add(new PreloadObject("Flea Dodge Platform", "dodge_plat",
            ("Aqueduct_05_festival",
                "Caravan_States/Flea Festival/Flea Game - Dodging/Active While Playing/Dodge Plat L")));*/
    }

    private static void AddMossObjects()
    {
        AddEnemy("Mossgrub", "mossbone_crawler", ("Arborium_09", "MossBone Crawler (1)"),
                preloadAction: EnemyFixers.FixMossgrub)
            .WithReceiverGroup(ReceiverGroup.Wakeable).DoFlipX()
            .WithRotationGroup(RotationGroup.Four)
            .WithConfigGroup(ConfigGroup.Wakeable);

        AddEnemy("Massive Mossgrub", "mossbone_crawler_fat",
            ("Arborium_09", "MossBone Crawler Fat"));

        AddEnemy("Mossmir", "mossbone_fly",
            ("Arborium_04", "MossBone Fly (1)"));

        AddEnemy("Moss Mother", "moss_mother",
            ("Tut_03", "Black Thread States/Normal World/Battle Scene/Wave 1/Mossbone Mother"), 
            postSpawnAction: EnemyFixers.FixMossMother)
            .WithConfigGroup(ConfigGroup.MossMother)
            .WithBroadcasterGroup(BroadcasterGroup.SlamBosses).DoFlipX();

        AddEnemy("Aknid Hatchling", "grove_pilgrim_hatchling",
            ("Dust_11", "Aspid Hatchling"), hideAndDontSave: true);
        AddEnemy("Aknid", "aspid_collector",
            ("Mosstown_01", "Black Thread States Thread Only Variant/Black Thread World/Aspid Collector"),
            postSpawnAction: EnemyFixers.FixAknid);
        AddEnemy("Aknid Mother", "grove_pilgrim",
            ("Dust_11", "Grove Pilgrim Fly"), hideAndDontSave: true,
            preloadAction: EnemyFixers.FixAknidMother).DoFlipX();

        AddEnemy("Overgrown Pilgrim", "pilgrim_moss_spitter",
            ("Mosstown_01", "Black Thread States Thread Only Variant/Normal World/Pilgrim Moss Spitter"));

        AddEnemy("Pilgrim Groveller", "pilgrim_03",
            ("Mosstown_01", "Pilgrim 03 (1)")).DoFlipX();

        AddEnemy("Pilgrim Pouncer", "pilgrim_01",
            ("Mosstown_01", "Pilgrim 01")).DoFlipX();

        AddEnemy("Pilgrim Hornfly", "pilgrim_hornfly",
            ("Bone_East_14b", "Pilgrim 04")).DoFlipX();

        AddEnemy("Pilgrim Hulk", "pilgrim_hulk",
            ("Bone_East_14b", "Pilgrim 02 (1)")).DoFlipX();

        AddEnemy("Winged Pilgrim", "pilgrim_fly",
            ("Coral_32", "Black Thread States/Black Thread World/Black_Thread_Core/Enemy Group/Pilgrim Fly")).DoFlipX();

        AddEnemy("Elder Pilgrim", "elder_pilgrim",
            ("Bonegrave", "Pilgrim Groups/Group 1/Act3 Pilgrim 05"), 
            preloadAction: EnemyFixers.FixElderPilgrim,
            postSpawnAction: EnemyFixers.FixBonegravePilgrim);
        AddEnemy("Pilgrim Guide", "pilgrim_guide",
            ("Bonegrave", "Pilgrim Groups/Group 2/Pilgrim StaffWielder"), 
            preloadAction: EnemyFixers.KeepActive,
            postSpawnAction: EnemyFixers.FixBonegravePilgrim);
        
        AddEnemy("Covetous Pilgrim", "covetous_pilgrim",
            ("Bonegrave", "Pilgrim Groups/Rosary Pilgrim Scene/Rosary Pilgrim"));
        
        Categories.Misc.Add(new PreloadObject("Rosary Bead", "rosary",
            ("Bonegrave", "Pilgrim Groups/Rosary Pilgrim Scene/Geo Small Persistent (4)"),
            preloadAction: o => o.AddComponent<PngObject>(),
            description:"Can be configured to be worth any number of rosaries\n" +
                        "and to use a Custom PNG texture.")
            .WithConfigGroup(ConfigGroup.RosaryBead));

        AddEnemy("Pilgrim Hiker", "pilgrim_hiker",
            ("Coral_32", "Black Thread States/Black Thread World/Black_Thread_Core/Enemy Group/Pilgrim Hiker"));

        AddEnemy("Pilgrim Bellbearer", "pilgrim_bell",
            ("Greymoor_13", "Black Thread States Thread Only Variant/Normal World/Pilgrim BellThrower"));
        AddEnemy("Winged Pilgrim Bellbearer", "pilgrim_wingbell",
            ("Greymoor_13", "Black Thread States Thread Only Variant/Black Thread World/Pilgrim Bellthrower Fly"))
            .DoFlipX();

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

        Categories.Misc.Add(new PreloadObject("Pilby NPC", "pilby_death",
            ("Bonetown", "Black Thread States/Normal World/Bonetown Resident"),
            preloadAction: EnemyFixers.KeepActive,
            postSpawnAction: MiscFixers.FixPilby))
            .WithConfigGroup(ConfigGroup.Npcs)
            .WithReceiverGroup(ReceiverGroup.Pilby);

        Categories.Misc.Add(new PreloadObject("Fixer Statue", "flick_statue",
            ("Bonetown", "Black Thread States/Normal World/fixer_constructs/fixer_statue/Shell Shard Fossil Big"),
            postSpawnAction: MiscFixers.FixStatue)
            .WithConfigGroup(ConfigGroup.PersistentBreakable)
            .WithBroadcasterGroup(BroadcasterGroup.Breakable));
    }

    private static PlaceableObject AddEnemy(
        string name,
        string id,
        (string, string) path, 
        bool hideAndDontSave = false,
        [CanBeNull] Action<GameObject> preloadAction = null,
        [CanBeNull] Action<GameObject> postSpawnAction = null)
    {
        return Categories.Enemies.Add(new PreloadObject(name, id,
                path,
                hideAndDontSave: hideAndDontSave,
                preloadAction: preloadAction,
                postSpawnAction: postSpawnAction)
            .WithReceiverGroup(ReceiverGroup.Enemies)
            .WithBroadcasterGroup(BroadcasterGroup.Enemies)
            .WithConfigGroup(ConfigGroup.Enemies)
            .WithOutputGroup(OutputGroup.Enemies));
    }

    private static void AddSolid(string name, string id, (string, string) path,
        [CanBeNull] Action<GameObject> preloadAction = null)
    {
        Categories.Solids.Add(new PreloadObject(name, id, path, preloadAction: preloadAction))
            .WithRotationGroup(RotationGroup.Four)
            .WithConfigGroup(ConfigGroup.Colliders);
    }
}