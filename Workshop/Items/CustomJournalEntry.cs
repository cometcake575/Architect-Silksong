using Architect.Storage;
using BepInEx;
using TeamCherry.Localization;
using UnityEngine;

namespace Architect.Workshop.Items;

public class CustomJournalEntry : SpriteItem
{
    private EnemyJournalRecord _record;
    
    public string LIconUrl = string.Empty;
    public bool LPoint;
    public float LPpu = 100;
    
    public string ItemName = string.Empty;
    public string ItemDesc = string.Empty;
    public string ItemHDesc = string.Empty;

    public string InsertBefore = string.Empty;

    public int KillsRequired = 1;

    public override void Register()
    {
        _record = ScriptableObject.CreateInstance<EnemyJournalRecord>();
        _record.name = Id;
        
        _record.altNotesTest = new PlayerDataTest();
        _record.completeOthers = [];
        _record.recordType = EnemyJournalRecord.RecordTypes.Enemy;

        _record.killsRequired = KillsRequired;

        _record.displayName = new LocalisedString("ArchitectMod", ItemName);
        _record.description = new LocalisedString("ArchitectMod", ItemDesc);
        _record.notes = new LocalisedString("ArchitectMod", ItemHDesc);

        var l = EnemyJournalManager.Instance.recordList.List;
        var i = l.FindIndex(o => o.name == InsertBefore);
        if (i != -1) l.Insert(i, _record);
        else l.Add(_record);
        
        base.Register();
        RefreshLSprite();
    }

    public void RefreshLSprite()
    {
        if (LIconUrl.IsNullOrWhiteSpace()) return;
        CustomAssetManager.DoLoadSprite(LIconUrl, LPoint, LPpu, 1, 1, sprites =>
        {
            if (sprites.IsNullOrEmpty()) return;
            _record.enemySprite = sprites[0];
        });
    }

    protected override void OnReadySprite()
    {
        _record.iconSprite = Sprite;
    }

    public override void Unregister()
    {
        EnemyJournalManager.Instance.recordList.Remove(_record);
    }
}