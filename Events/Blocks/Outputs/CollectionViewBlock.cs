using Architect.Utils;

namespace Architect.Events.Blocks.Outputs;

using System.Collections.Generic;
using Content.Preloads;
using UnityEngine;

public class CollectionViewBlock : CollectionBlock<CollectionViewBlock.CollectionItemBlock>
{
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

    private CollectionViewerDesk _cvd;
    private CollectionViewBoard _cbd;

    public override void SetupReference()
    {
        var desk = Object.Instantiate(_deskPrefab);
        desk.name = "[Architect] Collection View";

        _cvd = desk.GetComponent<CollectionViewerDesk>();
        _cbd = Object.Instantiate(_cvd.board.gameObject).GetComponent<CollectionViewBoard>();
        _cvd.board = _cbd;

        _cvd.sections = [];
        foreach (var child in Children.Children)
        {
            _cvd.sections.Add(new CollectionViewerDesk.Section
            {
                Heading = (LocalStr)child.Section,
                DisplayObjects = [],
                ConstructEventRegister = string.Empty
            });
        }
    }
    
    protected override void Trigger(string trigger)
    {
        _cbd.OpenBoard(_cvd);
    }
    
    public class CollectionItemBlock : ChildBlock
    {
        public string ItemId = "Rosary_Set_Small";
        public string Section = "Sample Text";

        public override void SetupReference()
        {
            
        }

        protected override IEnumerable<(string, string)> InputVars => [("Available", "Boolean")];
        protected override IEnumerable<string> Outputs => ["OnPurchase"];
    }
}