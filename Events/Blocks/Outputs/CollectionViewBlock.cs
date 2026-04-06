namespace Architect.Events.Blocks.Outputs;

using System.Collections.Generic;
using Content.Preloads;
using UnityEngine;

public class CollectionViewBlock : CollectionBlock<ShopBlock.ShopItemBlock>
{
    private static readonly Color DefaultColor = new(0.2f, 0.4f, 0.8f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Collection View";

    private static GameObject _deskPrefab;
    
    public static void Init()
    {
        PreloadManager.RegisterPreload(new BasicPreload("Belltown_Room_Spare", "furnishings/Desk Group/desk",
            o =>
            {
                o.SetActive(false);
                _deskPrefab = Object.Instantiate(o);
                Object.DontDestroyOnLoad(_deskPrefab);
            }));
    }
    
    protected override IEnumerable<string> Inputs => ["Open"];

    protected override string ChildName => "Collection Item";
    protected override bool NeedsGap => true;

    public override void SetupReference()
    {
        var desk = Object.Instantiate(_deskPrefab);
        desk.name = "[Architect] Collection View";
        
    }
    
    protected override void Trigger(string trigger)
    {
    }
    
    public class CollectionItemBlock : ChildBlock
    {
        protected override Color Color => DefaultColor;
        
        public string ItemId = "Rosary_Set_Small";

        public override void SetupReference()
        {
        }

        protected override IEnumerable<(string, string)> InputVars => [("Available", "Boolean")];
        protected override IEnumerable<string> Outputs => ["OnPurchase"];
    }
}