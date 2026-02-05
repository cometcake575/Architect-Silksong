using Architect.Storage;
using BepInEx;
using TeamCherry.Localization;
using TeamCherry.SharedUtils;
using UnityEngine;

namespace Architect.Workshop.Items;

public class CustomQuest : SpriteItem
{
    public bool MainQuest;
    
    public string ItemName = string.Empty;
    public string ItemDesc = string.Empty;
    public string TypeName = string.Empty;

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
    
    public override (string, string)[] FilesToDownload => [
        (IconUrl, "png"),
        (LIconUrl, "png"),
        (GIconUrl, "png")
    ];
    
    public override void Register()
    {
        _type = QuestType.Create(
            new LocalisedString("ArchitectMod", TypeName),
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
        
        _quest.targets = [];
        
        _quest.overrideFontSize = new OverrideFloat();
        _quest.overrideParagraphSpacing = new OverrideFloat();
        _quest.overrideParagraphSpacingShort = new OverrideFloat();
        
        _quest.displayName = new LocalisedString("ArchitectMod", ItemName);
        _quest.inventoryDescription = new LocalisedString("ArchitectMod", ItemDesc);

        QuestManager.Instance.masterList.Add(_quest);
        
        base.Register();
        RefreshLSprite();
        RefreshGSprite();
        
        QuestManager.IncrementVersion();
    }

    public override void Unregister()
    {
        QuestManager.Instance.masterList.Remove(_quest);
        QuestManager.IncrementVersion();
    }

    protected override void OnReadySprite()
    {
        _type.icon = Sprite;
        if (_mq) _mq.typeIcon = Sprite;
    }

    public void RefreshLSprite()
    {
        if (GIconUrl.IsNullOrWhiteSpace()) return;
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