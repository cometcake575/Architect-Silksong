using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Architect.Content.Preloads;
using Architect.Utils;
using TeamCherry.Localization;
using UnityEngine;
using UnityEngine.Events;

namespace Architect.Events.Blocks.Outputs;

public class ShopBlock : CollectionBlock<ShopBlock.ShopItemBlock>
{
    private static readonly Color DefaultColor = new(0.2f, 0.4f, 0.8f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Shop";

    private static ShopMenuStock _shopPrefab;
    
    public static void Init()
    {
        PreloadManager.RegisterPreload(new BasicPreload("shopui_assets_all.bundle", "Assets/Prefabs/UI/Shop/Shop Menu.prefab",
            o =>
            {
                _shopPrefab = o.GetComponent<ShopMenuStock>();
            }, notSceneBundle: true));
    }
    
    protected override IEnumerable<string> Inputs => ["Open"];

    protected override string ChildName => "Shop Item";
    protected override bool NeedsGap => true;

    private ShopOwner _shopOwner;

    public override void SetupReference()
    {
        var obj = new GameObject("[Architect] Shop Object");
        obj.SetActive(false);
        _shopOwner = obj.AddComponent<ShopOwner>();
        _shopOwner.shopPrefab = _shopPrefab;
    }
    
    protected override void Trigger(string trigger)
    {
        foreach (var child in Children.Children) child.Shop = this;
            
        ArchitectPlugin.Instance.StartCoroutine(Coroutine());
    }

    public void Refresh()
    {
        _shopOwner.stock = Children.Children
            .Where(i => i.GetVariable<bool>("Available", true) && i.Item)
            .Select(i => i.Item)
            .ToArray();
        _shopOwner.SpawnUpdateShop();
    }

    private IEnumerator Coroutine()
    {
        yield return HeroController.instance.FreeControl(_ => !GameManager.instance.isPaused);
        HeroController.instance.RelinquishControl();
        
        Refresh();
        _shopOwner.gameObject.SetActive(true);
        var sc = _shopOwner.ShopObject.LocateMyFSM("shop_control");
        sc.SendEvent("SHOP UP");
        yield return new WaitUntil(() => sc.ActiveStateName == "Idle");
        HeroController.instance.RegainControl();
    }
    
    public class ShopItemBlock : ChildBlock
    {
        protected override Color Color => DefaultColor;
        
        public string ItemId = "Rosary_Set_Small";
        public string ItemName = string.Empty;
        public string ItemDesc = string.Empty;
        public CurrencyType Currency = CurrencyType.Money;
        public int Cost = 80;

        public ShopBlock Shop;

        protected override void Reset()
        {
            ItemId = "Rosary_Set_Small";
            ItemName = string.Empty;
            ItemDesc = string.Empty;
            Cost = 80;
        }
        
        public ShopItem Item;

        public override void SetupReference()
        {
            Item = ScriptableObject.CreateInstance<ShopItem>();
            var i = MiscUtils.GetSavedItem(ItemId);
            Item.savedItem = i;
            Item.questsAppearConditions = [];
            Item.extraAppearConditions = new PlayerDataTest();
            Item.spawnOnPurchaseConditionals = [];
            Item.setExtraPlayerDataBools = [];
            Item.setExtraPlayerDataInts = [];
            Item.displayName = new LocalisedString("ArchitectMod", ItemName);
            Item.description = new LocalisedString("ArchitectMod", ItemDesc);
            Item.currencyType = Currency;
            Item.cost = Cost;
            Item.onPurchase = new UnityEvent();
            Item.onPurchase.AddListener(() =>
            {
                Event("OnPurchase");
                Shop?.Refresh();
            });
        }

        protected override IEnumerable<(string, string)> InputVars => [("Available", "Boolean")];
        protected override IEnumerable<string> Outputs => ["OnPurchase"];
    }
}