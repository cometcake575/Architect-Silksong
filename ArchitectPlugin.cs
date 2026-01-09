using Architect.Behaviour.Fixers;
using Architect.Content;
using Architect.Content.Custom;
using Architect.Content.Preloads;
using Architect.Editor;
using Architect.Events;
using Architect.Multiplayer;
using Architect.Objects.Categories;
using Architect.Placements;
using Architect.Storage;
using Architect.Storage.Sharer;
using Architect.Utils;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;

namespace Architect;

[BepInPlugin("com.cometcake575.architect", "Architect", "3.7.2")]
[BepInDependency("org.silksong-modding.prepatcher")]
[BepInDependency("ssmp", BepInDependency.DependencyFlags.SoftDependency)]
public class ArchitectPlugin : BaseUnityPlugin
{
    internal static ArchitectPlugin Instance;

    internal new static ManualLogSource Logger;
    
    public static readonly Sprite BlankSprite = ResourceUtils.LoadSpriteResource("blank", ppu:300);
    
    private void Awake()
    {
        Instance = this;
        
        Logger = base.Logger;
        Logger.LogInfo("Architect has loaded!");
        
        HookUtils.Init();
        TitleUtils.Init();
        
        StorageManager.Init();
        Settings.Init(Config);
        
        MiscFixers.Init();
        EnemyFixers.Init();
        HazardFixers.Init();
        
        Categories.Init();
        PreloadManager.Init();
        
        ActionManager.Init();
        CoopManager.Init();
        
        EditManager.Init();
        CursorManager.Init();
        EditorUI.Init();
        
        VanillaObjects.Init();
        SplineObjects.Init();
        LegacyObjects.Init();
        UtilityObjects.Init();
        AbilityObjects.Init();
        MiscObjects.Init();
        
        RespawnMarkerManager.Init();
        
        PlacementManager.Init();
        
        BroadcasterHooks.Init();
        
        LevelSharerUI.Init();
    }
    
    private void Update()
    {
        if (HeroController.instance)
        {
            EditManager.Update();
            HazardFixers.UpdateLanterns();
        }
        CursorManager.Update();
        LevelSharerUI.Update();
        AbilityObjects.Update();
    }
}
