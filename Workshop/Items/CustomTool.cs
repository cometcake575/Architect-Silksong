using System.Collections.Generic;
using Architect.Storage;
using Architect.Utils;
using BepInEx;
using UnityEngine;

namespace Architect.Workshop.Items;

public class CustomTool : SpriteItem
{
    private ToolItemBasic _tool;

    public static readonly List<string> List = [];

    public LocalStr ItemName = string.Empty;
    public LocalStr ItemDesc = string.Empty;
    public ToolItemType ItemType = ToolItemType.Red;
    
    public string HIconUrl = string.Empty;
    public bool HPoint;
    public float HPpu = 100;
    
    public string GIconUrl = string.Empty;
    public bool GPoint;
    public float GPpu = 100;

    public int RepairCost = 5;
    public bool PreventIncrease;
    public int MaxAmount = 10;
    
    public override (string, string)[] FilesToDownload => [
        (IconUrl, "png"),
        (HIconUrl, "png"),
        (GIconUrl, "png")
    ];
    
    public override void Register()
    {
        List.Add(Id);

        _tool = ItemType == ToolItemType.Skill ? 
            ScriptableObject.CreateInstance<ToolItemSkill>() : 
            ScriptableObject.CreateInstance<CustomToolItem>();
        _tool.usageOptions.FsmEventName = "";
        _tool.name = Id;
        _tool.type = ItemType;
        _tool.displayName = ItemName;
        _tool.description = ItemDesc;
        
        _tool.alternateUnlockedTest = new PlayerDataTest();

        _tool.preventStorageIncrease = PreventIncrease;
        if (ItemType == ToolItemType.Red) _tool.baseStorageAmount = MaxAmount;
        if (_tool is CustomToolItem cti) cti.cost = RepairCost;
        
        ToolItemManager.Instance.toolItems.Add(_tool);
        WorkshopManager.CustomItems.Add(this);
        ToolItemManager.IncrementVersion();
        
        base.Register();
        RefreshHSprite();
        RefreshGSprite();
    }

    protected override void OnReadySprite()
    {
        _tool.inventorySprite = Sprite;
    }

    public override void Unregister()
    {
        List.Remove(Id);
        
        ToolItemManager.Instance.toolItems.Remove(_tool);
        WorkshopManager.CustomItems.Remove(this);
        ToolItemManager.IncrementVersion();
    }

    public void RefreshHSprite()
    {
        if (HIconUrl.IsNullOrWhiteSpace()) return;
        CustomAssetManager.DoLoadSprite(HIconUrl, HPoint, HPpu, 1, 1, sprites =>
        {
            if (sprites.IsNullOrEmpty()) return;
            _tool.hudSprite = sprites[0];
        });
    }

    public void RefreshGSprite()
    {
        if (GIconUrl.IsNullOrWhiteSpace()) return;
        CustomAssetManager.DoLoadSprite(GIconUrl, GPoint, GPpu, 1, 1, sprites =>
        {
            if (sprites.IsNullOrEmpty()) return;
            switch (_tool)
            {
                case ToolItemSkill tis:
                    tis.hudGlowSprite = sprites[0];
                    break;
                case CustomToolItem cti:
                    cti.fullSprite = sprites[0];
                    break;
            }
        });
    }

    public class CustomToolItem : ToolItemBasic
    {
        public Sprite fullSprite;

        public int cost = 5;
        
        public override Sprite GetHudSprite(IconVariants iconVariant)
        {
            var orig = base.GetHudSprite(iconVariant);
            if (!IsEmpty) return fullSprite ?? orig;
            return orig;
        }

        public override bool TryReplenishSingle(
            bool doReplenish,
            float inCost,
            out float outCost,
            out int reserveCost)
        {
            base.TryReplenishSingle(doReplenish, inCost, out outCost, out reserveCost);
            outCost = cost;
            return true;
        }
    }
}