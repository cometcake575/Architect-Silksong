using System;
using Architect.Utils;
using UnityEngine;

namespace Architect.Workshop.Items;

public class CustomMateriumEntry : SpriteItem
{
    private MateriumItem _item;

    public LocalStr ItemName = string.Empty;
    public LocalStr Desc = string.Empty;

    public string InsertBefore = string.Empty;
    
    public override void Register()
    {
        _item = ScriptableObject.CreateInstance<MateriumItem>();
        _item.name = Id;

        _item.itemQuests = [];
        _item.playerDataCondition = new PlayerDataTest();
        
        _item.displayName = ItemName;
        _item.description = Desc;
        
        var l = MateriumItemManager.Instance.masterList.List;
        var i = l.FindIndex(o => o.name == InsertBefore);
        if (i != -1) l.Insert(i, _item);
        else l.Add(_item);
        
        base.Register();
    }

    public override void Unregister()
    {
        MateriumItemManager.Instance.masterList.Remove(_item);
    }

    protected override void OnReadySprite()
    {
        _item.icon = Sprite;
    }
}