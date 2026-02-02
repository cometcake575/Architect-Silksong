using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Architect.Content.Preloads;
using Architect.Utils;
using GlobalEnums;
using TeamCherry.Localization;
using TeamCherry.NestedFadeGroup;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Architect.Events.Blocks.Outputs;

public class TravelBlock : ScriptBlock
{
    private static FastTravelMap _ftm;
    private static SpriteRenderer _map;
    
    private static SpriteRenderer _top;
    private static SpriteRenderer _bot;
    
    public static void Init()
    {
        PreloadManager.RegisterPreload(new BasicPreload(
            "localpoolprefabs_assets_areabellway.bundle", 
            "Assets/Prefabs/UI/Fast Travel Map.prefab",
            o =>
            {
                o = Object.Instantiate(o);
                Object.DontDestroyOnLoad(o);
                o.transform.position = Vector3.zero;
                Object.Destroy(o.transform.GetChild(3).GetChild(0).gameObject);
                _ftm = o.GetComponent<FastTravelMap>();
                
                var constrain = o.transform.GetChild(1).gameObject.AddComponent<ConstrainPosition>();
                constrain.constrainX = true;
                constrain.constrainY = true;

                _map = Object.Instantiate(o.transform.Find("backing").gameObject)
                    .GetComponent<SpriteRenderer>();
                _map.transform.parent = o.transform;
                _map.name = "Map";
                _map.transform.localScale = Vector3.one;
                _map.transform.position = new Vector3(-6, 0, 2);

                _top = Object.Instantiate(o.transform.Find("backing").gameObject)
                    .GetComponent<SpriteRenderer>();
                _top.transform.parent = o.transform;
                _top.name = "Top";
                _top.transform.localScale = Vector3.one;
                _top.transform.position = new Vector3(0, 6.9173f, 2);

                _bot = Object.Instantiate(o.transform.Find("backing").gameObject)
                    .GetComponent<SpriteRenderer>();
                _bot.transform.parent = o.transform;
                _bot.name = "Bottom";
                _bot.transform.localScale = Vector3.one;
                _bot.transform.position = new Vector3(0, -6.7024f, 2);

                foreach (var btn in o.GetComponentsInChildren<FastTravelMapButton>())
                {
                    btn.playerDataBool = "";
                    var tb = btn.gameObject.AddComponent<TravelBtn>();
                    tb.loc = btn.targetLocation;

                    var piece = o.GetComponentsInChildren<FastTravelMapPiece>()
                        .First(piece => piece.pairedButton == btn);
                    tb.piece = piece;
                }
            }, notSceneBundle:true));
        
        typeof(FastTravelMapButtonBase<FastTravelLocations>)
            .Hook(nameof(FastTravelMapButtonBase<FastTravelLocations>.IsUnlocked),
                (Func<FastTravelMapButtonBase<FastTravelLocations>, bool> orig,
                    FastTravelMapButtonBase<FastTravelLocations> self) =>
                {
                    var btn = self.GetComponent<TravelBtn>();
                    if (btn) return btn.Block is { Unlocked: true };
                    return orig(self);
                });
        
        typeof(FastTravelMapButtonBase<FastTravelLocations>)
            .Hook(nameof(FastTravelMapButtonBase<FastTravelLocations>.Submit),
                (Action<FastTravelMapButtonBase<FastTravelLocations>> orig,
                    FastTravelMapButtonBase<FastTravelLocations> self) =>
                {
                    var btn = self.GetComponent<TravelBtn>();
                    if (btn) btn.Block.Event("OnChoose");
                    orig(self);
                });
    }

    protected override void Reset()
    {
        Title = "Sample Text";
    }
    
    protected override IEnumerable<string> Inputs => ["Display"];
    protected override IEnumerable<string> Outputs => ["Dismiss"];
    protected override IEnumerable<(string, string)> InputVars => [
        Space,
        Space,
        ("Map", "Sprite"),
        ("Top", "Sprite"),
        ("Bottom", "Sprite"),
        ("Dest 1", "Travel"),
        ("Dest 2", "Travel"),
        ("Dest 3", "Travel"),
        ("Dest 4", "Travel"),
        ("Dest 5", "Travel"),
        ("Dest 6", "Travel"),
        ("Dest 7", "Travel"),
        ("Dest 8", "Travel"),
        ("Dest 9", "Travel"),
        ("Dest 10", "Travel"),
        ("Dest 11", "Travel"),
        ("Dest 12", "Travel")
    ];

    private static readonly Color DefaultColor = new(0.2f, 0.4f, 0.8f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Travel List";

    public string Title = "Sample Text";

    protected override void Trigger(string trigger)
    {
        ArchitectPlugin.Instance.StartCoroutine(Coroutine());
    }

    private IEnumerator Coroutine()
    {
        foreach (var icon in _ftm.GetComponentsInChildren<MenuButtonIcon>())
            icon.uibs = UIManager.instance.uiButtonSkins;
        
        var setTitle = _ftm.transform.Find("Station List").Find("List").Find("Title Box").Find("Title")
            .GetComponent<SetTextMeshProGameText>();
        setTitle.text = new LocalisedString("ArchitectMod", Title);
        setTitle.setTextOn.text = Title;

        _map.sprite = GetVariable<Sprite>("Map");
        _top.sprite = GetVariable<Sprite>("Top");
        _ftm.transform.GetChild(0).GetChild(1).gameObject.SetActive(!_top.sprite);
        _bot.sprite = GetVariable<Sprite>("Bottom");
        _ftm.transform.GetChild(0).GetChild(0).gameObject.SetActive(!_bot.sprite);
        
        var hasSet = false;
        for (var i = 0; i < 12; i++)
        {
            var travelLoc = GetVariable<TravelLoc>($"Dest {i+1}");
            
            var obj = _ftm.list.listItems[i];
            var btn = obj.GetComponent<TravelBtn>();
            btn.Block = travelLoc;
            
            if (travelLoc is not { Unlocked: true }) continue;

            btn.piece.transform.localPosition = Vector3.zero;
            btn.piece.indicatorOffset = new Vector2(travelLoc.XPos, travelLoc.YPos);
            
            var set = obj.GetComponent<SetTextMeshProGameText>();
            set.text = new LocalisedString("ArchitectMod", travelLoc.ListName);
            set.setTextOn.text = travelLoc.ListName;

            if (!hasSet)
            {
                hasSet = true;
                _ftm.AutoSelectLocation = btn.loc;
            }
        }

        if (!hasSet) yield break;

        yield return HeroController.instance.FreeControl(_ => !GameManager.instance.isPaused);
        HeroController.instance.RelinquishControl();
        PlayerData.instance.disablePause = true;
        _ftm.Open();
        _ftm.transform.GetChild(1).GetChild(0).gameObject.SetActive(false);
        _ftm.transform.GetChild(1).GetChild(2).gameObject.SetActive(false);
        
        _ftm.LocationConfirmed += Dismiss;
        yield break;

        void Dismiss(FastTravelLocations fastTravelLocations)
        {
            _ftm.LocationConfirmed -= Dismiss;
            if (fastTravelLocations == default) Event("Dismiss");
            GameCameras.instance.HUDIn();
            ArchitectPlugin.Instance.StartCoroutine(RegainControlDelayed());
        }
    }

    private static IEnumerator RegainControlDelayed()
    {
        yield return new WaitForSeconds(0.1f);
        HeroController.instance.RegainControl();
        PlayerData.instance.disablePause = false;
    }

    public class TravelBtn : MonoBehaviour
    {
        public TravelLoc Block;
        public FastTravelLocations loc;
        public FastTravelMapPiece piece;
    }
}

public class TravelLoc : ScriptBlock
{
    protected override IEnumerable<(string, string)> InputVars => [("Unlocked", "Boolean")];
    protected override IEnumerable<(string, string)> OutputVars => [("This", "Travel")];
    protected override IEnumerable<string> Outputs => ["OnChoose"];

    private static readonly Color DefaultColor = new(0.2f, 0.4f, 0.8f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Travel Point";

    public string ListName = "Sample Text";
    public float XPos;
    public float YPos;
    public bool Unlocked => GetVariable<bool>("Unlocked", true);

    protected override void Reset()
    {
        ListName = "Sample Text";
        XPos = -99999;
        YPos = -99999;
    }

    protected override object GetValue(string id) => this;
}