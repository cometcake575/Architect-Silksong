using Architect.Editor;
using Architect.Storage;
using BepInEx;
using UnityEngine;

namespace Architect.Workshop.Items;

public abstract class SpriteItem : WorkshopItem
{
    public string IconUrl = string.Empty;
    public bool Point;
    public float Ppu = 100;
    protected Sprite Sprite;
    
    public override (string, string)[] FilesToDownload => [(IconUrl, "png")];

    public override void Register()
    {
        RefreshSprite();
    }

    public void RefreshSprite()
    {
        if (IconUrl.IsNullOrWhiteSpace()) return;
        CustomAssetManager.DoLoadSprite(IconUrl, Point, Ppu, 1, 1, sprites =>
        {
            if (sprites.IsNullOrEmpty()) return;
            Sprite = sprites[0];
            OnReadySprite();
            WorkshopUI.RefreshIcon(this);
        });
    }

    public override Sprite GetIcon()
    {
        return Sprite;
    }

    protected virtual void OnReadySprite() { }
}