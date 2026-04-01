using System.Collections.Generic;
using System.Linq;
using Architect.Storage;
using Architect.Utils;
using BepInEx;
using TeamCherry.SharedUtils;
using UnityEngine;

namespace Architect.Workshop.Items;

public class CustomQuest : SpriteItem
{
    public bool MainQuest;
    
    public LocalStr ItemName = string.Empty;
    public LocalStr ItemDesc = string.Empty;
    public LocalStr WallDesc = string.Empty;
    public LocalStr TypeName = string.Empty;

    public Color Color = Color.white;

    private MainQuest _mq;
    private FullQuestBase _quest;
    private QuestType _type;
    
    public string LIconUrl = string.Empty;
    public bool LPoint;
    public float LPpu = 100;

    public string GIconUrl = string.Empty;
    public bool GPoint;
    public float GPpu = 100;

    public bool HasItem;
    public string ItemId = string.Empty;
    public int ItemCount = 1;

    public FullQuestBase.DescCounterTypes DescCounterType = FullQuestBase.DescCounterTypes.Icons;
    public FullQuestBase.ListCounterTypes ListCounterType = FullQuestBase.ListCounterTypes.Dots;
    public Color BarColour = Color.white;
    public LocalStr CollectedDesc = string.Empty;
    
    public override (string, string)[] FilesToDownload => [
        (IconUrl, "png"),
        (LIconUrl, "png"),
        (GIconUrl, "png")
    ];
    
    public override void Register()
    {
        _type = QuestType.Create(
            TypeName,
            null,
            Color,
            null,
            null
        );
        
        if (MainQuest)
        {
            _mq = ScriptableObject.CreateInstance<MainQuest>();
            _mq.questType = _type;
            _mq.subQuests = [];
            _mq.altTargets = [];
            _quest = _mq;
        }
        else
        {
            var q = ScriptableObject.CreateInstance<Quest>();
            q.questType = _type;
            _quest = q;
        }
        
        _quest.name = Id;
        
        _quest.hideDescCounterForLangs = [];
        _quest.persistentBoolTests = [];
        _quest.requiredCompleteQuests = [];
        _quest.requiredUnlockedTools = [];
        _quest.requiredCompleteTotalGroups = [];
        _quest.markCompleted = [];
        _quest.cancelIfIncomplete = [];
        _quest.hideIfComplete = [];
        _quest.playerDataTest = new PlayerDataTest();
        
        _quest.displayName = ItemName;
        _quest.inventoryDescription = ItemDesc;
        _quest.inventoryCompletableDescription = ItemDesc;
        _quest.wallDescription = WallDesc;

        _quest.descCounterType = DescCounterType;
        _quest.listCounterType = ListCounterType;
        _quest.progressBarTint = BarColour;
        List<FullQuestBase.QuestTarget> targets = [];
        if (HasItem)
        {
            _quest.inventoryCompletableDescription = CollectedDesc;
            var ic = ItemId.Split(",");
            var dictionary = ic.Distinct().ToDictionary(i => i, i => ic.Count(o => o == i));
            foreach (var (i, num) in dictionary)
            {
                var item = MiscUtils.GetSavedItem(i) as CollectableItem;
                if (!item) continue;
                targets.Add(new FullQuestBase.QuestTarget
                {
                    Counter = item,
                    Count = ItemCount / ic.Length * num,
                    AltTest = new PlayerDataTest(),
                    ItemName = (LocalStr)item.GetDisplayName(CollectableItem.ReadSource.Inventory)
                });
            }
        }

        _quest.targets = targets.ToArray();
        
        _quest.overrideFontSize = new OverrideFloat();
        _quest.overrideParagraphSpacing = new OverrideFloat();
        _quest.overrideParagraphSpacingShort = new OverrideFloat();
        
        QuestManager.Instance.masterList.Add(_quest);
        
        base.Register();
        RefreshLSprite();
        RefreshGSprite();
        
        QuestManager.IncrementVersion();
        
        WorkshopManager.CustomQuests.Add(Id, this);
    }

    public override void Unregister()
    {
        QuestManager.Instance.masterList.Remove(_quest);
        QuestManager.IncrementVersion();
        Object.Destroy(_quest);
        
        WorkshopManager.CustomQuests.Remove(Id);
    }

    protected override void OnReadySprite()
    {
        _type.icon = Sprite;
        if (_mq) _mq.typeIcon = Sprite;
    }

    public void RefreshLSprite()
    {
        if (LIconUrl.IsNullOrWhiteSpace()) return;
        CustomAssetManager.DoLoadSprite(LIconUrl, LPoint, LPpu, 1, 1, sprites =>
        {
            if (sprites.IsNullOrEmpty()) return;
            _type.largeIcon = sprites[0];
            if (_mq) _mq.typeLargeIcon = sprites[0];
        });
    }

    public void RefreshGSprite()
    {
        if (GIconUrl.IsNullOrWhiteSpace()) return;
        CustomAssetManager.DoLoadSprite(GIconUrl, GPoint, GPpu, 1, 1, sprites =>
        {
            if (sprites.IsNullOrEmpty()) return;
            _type.largeIconGlow = sprites[0];
            if (_mq) _mq.typeLargeIconGlow = sprites[0];
        });
    }
}