using System;
using System.IO;
using Architect.Api;
using Architect.Behaviour.Fixers;
using Architect.Content;
using Architect.Content.Custom;
using Architect.Content.Preloads;
using Architect.Editor;
using Architect.Events;
using Architect.Events.Blocks;
using Architect.Events.Blocks.Operators;
using Architect.Multiplayer;
using Architect.Objects.Categories;
using Architect.Objects.Groups;
using Architect.Placements;
using Architect.Prefabs;
using Architect.Sharer;
using Architect.Storage;
using Architect.Utils;
using Architect.Workshop;
using BepInEx;
using BepInEx.Logging;
using Newtonsoft.Json;
using Silksong.DataManager;
using UnityEngine;

namespace Architect;

[BepInPlugin("com.cometcake575.architect", "Architect", "3.27.0")]
[BepInDependency("org.silksong-modding.prepatcher")]
[BepInDependency("org.silksong-modding.assethelper")]
[BepInDependency("org.silksong-modding.modmenu")]
[BepInDependency("io.github.hk-speedrunning.quickwarp", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("ssmp", BepInDependency.DependencyFlags.SoftDependency)]
public class ArchitectPlugin : BaseUnityPlugin, ISaveDataMod<ArchitectData>, IGlobalDataMod<GlobalArchitectData>
{
    internal static ArchitectPlugin Instance;

    internal new static ManualLogSource Logger;
    
    public static readonly Sprite BlankSprite = ResourceUtils.LoadSpriteResource("blank", ppu:300);
    
    private void Awake()
    {
        Instance = this;
        
        Logger = base.Logger;
        Logger.LogInfo("Architect has loaded!");

        SceneUtils.Init();
        PrefabManager.Init();
        
        HookUtils.Init();
        TitleUtils.Init();
        
        StorageManager.Init();
        Settings.Init(Config);
        
        MiscFixers.Init();
        EnemyFixers.Init();
        HazardFixers.Init();
        InteractableFixers.Init();
        
        Categories.Init();
        
        ActionManager.Init();
        CoopManager.Init();
        
        WorkshopManager.Init();
        ScriptManager.Init();
        EditManager.Init();
        CursorManager.Init();
        
        VanillaObjects.Init();
        SplineObjects.Init();
        LegacyObjects.Init();
        UtilityObjects.Init();
        AbilityObjects.Init();
        MiscObjects.Init();
        CameraObjects.Init();
        ParticleObjects.Init();
        // CollectableObjects.Init();
        
        RespawnMarkerManager.Init();
        
        PlacementManager.Init();
        
        BroadcasterHooks.Init();

        SharerManager.Init();
        
        PreloadManager.Init();
        EditorUI.Setup();
        
        ConfigGroup.Init();
        
        StorageManager.MakeBackup(DateTime.Now.ToString("yy-MM-dd-HH-mm-ss"));
        ProjectManager.Init();

        typeof(GameManager).Hook(nameof(GameManager.ResetSemiPersistentItems),
            (Action<GameManager> orig, GameManager self) =>
            {
                BoolVarBlock.SemiVars.Clear();
                NumVarBlock.SemiVars.Clear();
                StringVarBlock.SemiVars.Clear();
                orig(self);
            });
        
        FindMaps();
    }
    
    private static void FindMaps()
    {
        foreach (var dir in Directory.GetDirectories(
                     Paths.PluginPath, 
                     "Architect",
                     SearchOption.AllDirectories))
        {
            if (dir == Path.Combine(Paths.PluginPath, "Architect")) continue;

            var scenes = Path.Combine(dir, "Scenes");
            if (Directory.Exists(scenes)) foreach (var path in Directory.GetFiles(scenes))
            {
                if (!path.EndsWith(".architect.json")) continue;
                var sceneName = Path.GetFileNameWithoutExtension(path).Replace(".architect", "");
                MapLoader.LoadStandaloneMap(sceneName, path);
            }

            var prefabs = Path.Combine(dir, "Prefabs");
            if (Directory.Exists(prefabs)) foreach (var path in Directory.GetFiles(prefabs))
            {
                if (!path.EndsWith(".architect.json")) continue;
                var sceneName = Path.GetFileNameWithoutExtension(path).Replace(".architect", "");
                MapLoader.LoadStandalonePrefab(sceneName, path);
            }
            
            var assets = Path.Combine(dir, "Assets");
            if (Directory.Exists(assets)) CustomAssetManager.AssetPaths.Add(assets);
            StorageManager.LoadPrefabs(dir);
            
            var workshop = Path.Combine(dir, "workshop.json");
            if (File.Exists(workshop))
            {
                var data = File.ReadAllText(workshop);
                WorkshopManager.LoadExtWorkshop(JsonConvert.DeserializeObject<WorkshopData>(data));
            }

            StorageManager.Directories.Add(dir);
        }
    }

    private void Start()
    {
        EditorUI.SetupCategories();
    }
    
    private void Update()
    {
        if (HeroController.instance)
        {
            EditManager.Update();
            HazardFixers.UpdateLanterns();
        }
        CursorManager.Update();
        SharerManager.Update();
        AbilityObjects.Update();
    }

    public ArchitectData SaveData { get; set; }
    public GlobalArchitectData GlobalData { get; set; }
}
