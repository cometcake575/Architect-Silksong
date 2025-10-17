﻿using Architect.Behaviour.Fixers;
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

[BepInPlugin("com.cometcake575.architect", "Architect", "2.0.3")]
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
        
        StorageManager.Init();
        Settings.Init(Config);
        
        MiscFixers.Init();
        EnemyFixers.Init();
        HazardFixers.Init();
        
        Categories.Init();
        
        PreloadManager.Init();
        
        VanillaObjects.Init();
        UtilityObjects.Init();
        AbilityObjects.Init();
        MiscObjects.Init();
        
        EditManager.Init();
        CursorManager.Init();
        EditorUI.Init();
        
        RespawnMarkerManager.Init();
        
        ActionManager.Init();
        CoopManager.Init();
        
        PlacementManager.Init();
        
        BroadcasterHooks.Init();
        
        LevelSharerUI.Init();
    }
    
    private void Update()
    {
        if (HeroController.instance)
        {
            EditManager.Update();
            HazardFixers.Update();
        }
        CursorManager.Update();
        LevelSharerUI.Update();
        AbilityObjects.Update();
    }
}
