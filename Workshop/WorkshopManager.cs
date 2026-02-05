using System;
using System.Collections.Generic;
using System.Linq;
using Architect.Editor;
using Architect.Storage;
using Architect.Utils;
using Architect.Workshop.Config;
using Architect.Workshop.Items;
using Architect.Workshop.Types;
using UnityEngine;

namespace Architect.Workshop;

public static class WorkshopManager
{
    public static WorkshopData WorkshopData;

    public static readonly Dictionary<string, (Vector2, Func<string, WorkshopItem>)> WorkshopItems = [];

    public static void Init()
    {
        Register<CustomItem>("Item",
            new Vector2(-300, -150),
            ConfigGroup.CustomItem, 
            ConfigGroup.SpriteItem,
            ConfigGroup.ItemUsing,
            ConfigGroup.ItemUse);
        
        Register<CustomQuest>("Quest",
            new Vector2(-100, -150),
            ConfigGroup.Quest,
            ConfigGroup.QuestSprites);
        
        Register<SceneGroup>("Scene Group",
            new Vector2(-300, -300),
            ConfigGroup.SceneGroup,
            ConfigGroup.SpriteItem);
        
        Register<CustomScene>("Scene",
            new Vector2(-100, -300),
            ConfigGroup.Scene);
    }
    
    public static void Setup()
    {
        StorageManager.LoadWorkshopData();
        
        typeof(CollectableItemManager).Hook(nameof(CollectableItemManager.InternalGetCollectedItems),
            (Func<CollectableItemManager, Func<CollectableItem, bool>, List<CollectableItem>> orig,
                CollectableItemManager self, Func<CollectableItem, bool> predicate) =>
            {
                if (Application.isPlaying)
                {
                    var collectables = PlayerData.instance.Collectables;
                    foreach (var name in collectables.GetValidNames()
                                 .Where(item => !self.IsItemInMasterList(item)))
                    {
                        collectables.RuntimeData.Remove(name);
                        CollectableItemManager.IncrementVersion();
                    }
                }
                return orig(self, predicate);
            });
    }

    public static void LoadWorkshop(WorkshopData data)
    {
        if (WorkshopData != null) foreach (var item in WorkshopData.Items) item.Unregister();

        WorkshopData = data;
        foreach (var item in WorkshopData.Items)
        {
            foreach (var cfg in item.CurrentConfig.Values) cfg.Setup(item);
            item.Register();
        }
        WorkshopUI.Refresh();
    }
    
    private static void Register<T>(string type, Vector2 pos, params List<ConfigType>[] config) where T : WorkshopItem, new()
    {
        WorkshopItems[type] = (pos, s => new T
        {
            Id = s,
            Type = type,
            Config = config,
            CurrentConfig = []
        });
    }
}