using System.Linq;
using Architect.Storage;
using Architect.Utils;
using BepInEx;
using Needleforge;
using Needleforge.Data;
using Needleforge.Makers;
using UnityEngine;

namespace Architect.Workshop.Items;

public class CustomCrest : SpriteItem
{
    public string Name = "Sample Text";
    public string Desc = "Sample Desc";
    
    public string SIconUrl = string.Empty;
    public bool SPoint;
    public float SPpu = 100;
    
    public string GIconUrl = string.Empty;
    public bool GPoint;
    public float GPpu = 100;

    public string AttackSource = string.Empty;
    public string AltAttackSource = string.Empty;
    public string UpAttackSource = string.Empty;
    public string UpAltAttackSource = string.Empty;

    private ToolCrest _crest;
    private CrestData _data;
    
    public override (string, string)[] FilesToDownload => [
        (IconUrl, "png"),
        (SIconUrl, "png"),
        (GIconUrl, "png")
    ];
    
    public override void Register()
    {
        _data = NeedleforgePlugin.AddCrest(Id, (LocalStr)Name, (LocalStr)Desc);
        _data.Moveset.Slash = VanillaAttacks.Architect.Slash();
        _data.Moveset.AltSlash = VanillaAttacks.Beast.Slash();
        
        _data.Moveset.UseVanillaDashSlash = VanillaCrest.WITCH;

        _crest = CrestMaker.CreateCrest(_data);
        
        base.Register();
        RefreshSilhouette();
        RefreshGlow();
    }

    private static InventoryToolCrestList List
    {
        get
        {
            if (field) return field;
            return field = Resources.FindObjectsOfTypeAll<InventoryToolCrestList>()
                .FirstOrDefault(c => c.crests.Count > 0);
        }
    }

    public override void Unregister()
    {
        NeedleforgePlugin.newCrestData.Remove(_data);
        
        var crest = ToolItemManager.GetCrestByName(Id);
        if (!crest) return;
        
        NeedleforgePlugin.newCrests.Remove(crest);
        
        ToolItemManager.Instance.crestList.Remove(crest);
        if (List)
        {
            var c = List.crests.FirstOrDefault(c => c.CrestData == crest);
            if (!c) return;
            List.crests.Remove(c);
            foreach (var slot in c.activeSlots) slot.gameObject.SetActive(false);
            c.crestSprite.gameObject.SetActive(false);
            c.crestGlowSprite.gameObject.SetActive(false);
            c.crestSilhouette.gameObject.SetActive(false);
            Object.Destroy(c);
        }
    }

    protected override void OnReadySprite()
    {
        _crest.crestSprite = Sprite;
        _data.RealSprite = Sprite;
    }
    
    private void RefreshSilhouette()
    {
        if (SIconUrl.IsNullOrWhiteSpace()) return;
        CustomAssetManager.DoLoadSprite(SIconUrl, SPoint, SPpu, 1, 1, sprites =>
        {
            if (sprites.IsNullOrEmpty()) return;
            _crest.crestSilhouette = sprites[0];
            _data.Silhouette = sprites[0];
        });
    }

    private void RefreshGlow()
    {
        if (GIconUrl.IsNullOrWhiteSpace()) return;
        CustomAssetManager.DoLoadSprite(GIconUrl, GPoint, GPpu, 1, 1, sprites =>
        {
            if (sprites.IsNullOrEmpty()) return;
            _crest.crestGlow = sprites[0];
            _data.CrestGlow = sprites[0];
        });
    }
}