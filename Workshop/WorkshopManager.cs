using System;
using System.Collections.Generic;
using System.Linq;
using Architect.Editor;
using Architect.Events.Blocks.Outputs;
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
    
    public static readonly Dictionary<string, WorkshopItem> CustomItems = [];
    public static readonly Dictionary<string, WorkshopItem> CustomEntries = [];
    public static readonly Dictionary<string, WorkshopItem> CustomCrests = [];
    public static readonly Dictionary<string, WorkshopItem> CustomQuests = [];
    public static readonly Dictionary<string, WorkshopItem> CustomTools = [];

    public static readonly Dictionary<string, (Vector2, Func<string, WorkshopItem>)> WorkshopItems = [];

    public static void Init()
    {
        Register<CustomItem>("Item",
            new Vector2(-300, -150),
            ConfigGroup.CustomItem, 
            ConfigGroup.SpriteItem,
            ConfigGroup.UsableItem,
            ConfigGroup.CourierItem);

        Register<CustomTool>("Tool",
            new Vector2(-300, -187.5f),
            ConfigGroup.CustomTool,
            ConfigGroup.SpriteItem,
            ConfigGroup.UseToolSprites,
            ConfigGroup.RedTools);

        Register<CustomJournalEntry>("Journal Entry",
            new Vector2(-100, -187.5f),
            ConfigGroup.JournalEntry,
            ConfigGroup.SpriteItem,
            ConfigGroup.JournalEntrySprites);
        
        Register<CustomQuest>("Quest",
            new Vector2(-100, -150),
            ConfigGroup.Quest,
            ConfigGroup.QuestSprites,
            [],
            ConfigGroup.QuestItem);
        
        Register<SceneGroup>("Scene Group",
            new Vector2(-300, -225),
            ConfigGroup.SceneGroup,
            ConfigGroup.SceneGroupIcon,
            ConfigGroup.SceneGroupMap,
            ConfigGroup.SceneGroupMapPos,
            ConfigGroup.SceneGroupMapDirIn,
            ConfigGroup.SceneGroupMapDirOut);
        
        Register<CustomScene>("Scene",
            new Vector2(-100, -225),
            ConfigGroup.Scene,
            ConfigGroup.SceneMap,
            ConfigGroup.SceneMapColour);
        
        Register<CustomMateriumEntry>("Material",
            new Vector2(-300, -262.5f),
            ConfigGroup.MateriumEntry,
            ConfigGroup.SpriteItem);
        
        Register<CustomMapIcon>("Map Icon",
            new Vector2(-100, -262.5f),
            ConfigGroup.MapIcon,
            ConfigGroup.SpriteItem,
            ConfigGroup.MapIconLabel);
        
        CustomKeybind.Init();
        Register<CustomKeybind>("Keybind",
            new Vector2(-300, -300),
            ConfigGroup.Keybind);
        
        Register<CustomCue>("Audio Cue",
            new Vector2(-100, -300),
            ConfigGroup.Cue);
        
        CustomNeedle.Init();
        Register<CustomNeedle>("Needle",
            new Vector2(-300, -337.5f),
            ConfigGroup.Needle,
            ConfigGroup.SpriteItem);
        
        /*
        Register<StatusEffect>("Status Effect",
            new Vector2(-100, -337.5f));*/
        
        Register<CustomAchievement>("Achievement",
            new Vector2(-100, -337.5f),
            ConfigGroup.Achievement);
        
        CustomCrest.Init();
        Register<CustomCrest>("Crest",
            new Vector2(-300, -375),
            ConfigGroup.Crest,
            ConfigGroup.SpriteItem,
            ConfigGroup.CrestSprites);
        
        Register<CustomCrest.CrestSlot>("Crest Slot",
            new Vector2(-100, -375),
            ConfigGroup.CrestSlot);
        
        SceneGroup.Init();
        
        /*CustomMenuStyle.Init();
        Register<CustomMenuStyle>("Menu Style",
            new Vector2(-300, -412.5f));*/
        
        typeof(CollectableItemManager).Hook(nameof(CollectableItemManager.InternalGetCollectedItems),
            (Func<CollectableItemManager, Func<CollectableItem, bool>, List<CollectableItem>> orig,
                CollectableItemManager self, Func<CollectableItem, bool> predicate) =>
            {
                if (Application.isPlaying)
                {
                    RefreshItems();

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
        
        typeof(HeroController).Hook(nameof(HeroController.ThrowTool),
            (Action<HeroController, bool> orig, HeroController self, bool isAutoThrow) =>
            {
                if (self.willThrowTool)
                {
                    if (CustomTool.List.Contains(self.willThrowTool.name))
                    {
                        switch (self.willThrowTool.type)
                        {
                            case ToolItemType.Skill:
                                HeroController.instance.TakeSilk(PlayerData.instance.SilkSkillCost);
                                break;
                            case ToolItemType.Red:
                            {
                                var sd = self.willThrowTool.SavedData;
                                sd.AmountLeft--;
                                self.willThrowTool.SavedData = sd;
                                ToolItemManager.ReportAllBoundAttackToolsUpdated();
                                break;
                            }
                            case ToolItemType.Blue:
                            case ToolItemType.Yellow:
                            default:
                                break;
                        }

                        ToolBlock.DoBroadcast(self.willThrowTool.name);
                    }
                }
                orig(self, isAutoThrow);
            });

        return;

        void RefreshItems()
        {
            foreach (var (id, item) in CustomItems.ToArray())
            {
                if (!CollectableItemManager.Instance.masterList.dictionary.ContainsKey(id))
                {
                    item.Unregister();
                    item.Register();
                }
            }
            foreach (var (id, item) in CustomEntries.ToArray())
            {
                if (!EnemyJournalManager.Instance.recordList.dictionary.ContainsKey(id))
                {
                    item.Unregister();
                    item.Register();
                }
            }
            foreach (var (id, item) in CustomCrests.ToArray())
            {
                if (!ToolItemManager.Instance.crestList.dictionary.ContainsKey(id))
                {
                    item.Unregister();
                    item.Register();
                }
            }
            foreach (var (id, item) in CustomQuests.ToArray())
            {
                if (!QuestManager.Instance.masterList.dictionary.ContainsKey(id))
                {
                    item.Unregister();
                    item.Register();
                }
            }
            foreach (var (id, item) in CustomTools.ToArray())
            {
                if (!ToolItemManager.Instance.toolItems.dictionary.ContainsKey(id))
                {
                    item.Unregister();
                    item.Register();
                }
            }
        }
    }
    
    public static void Setup()
    {
        CustomAchievement.Init();
        SceneUtils.InitQWHook();
        StorageManager.LoadWorkshopData();
    }

    public static void LoadWorkshop(WorkshopData data)
    {
        data ??= new WorkshopData();
        if (WorkshopData != null) foreach (var item in WorkshopData.Items) item.Unregister();

        WorkshopData = data;
        foreach (var item in WorkshopData.Items.OrderBy(i => i is CustomQuest ? 1 : 0))
        {
            foreach (var cfg in item.CurrentConfig.Values) cfg.Setup(item);
            item.Register();
        }
        WorkshopUI.Refresh();
    }
    
    private static void Register<T>(string type, Vector2 pos, params List<ConfigType>[] config) where T : WorkshopItem, new()
    {
        pos.y += 5;
        WorkshopItems[type] = (pos, s => new T
        {
            Id = s,
            Type = type,
            Config = config,
            CurrentConfig = []
        });
    }
}