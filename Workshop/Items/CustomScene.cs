using Architect.Storage;
using Architect.Utils;
using BepInEx;
using UnityEngine;

namespace Architect.Workshop.Items;

public class CustomScene : SpriteItem
{
    private static readonly Sprite Icon = ResourceUtils.LoadSpriteResource("door");

    public string Group = "None";
    
    public int TilemapWidth = 500;
    public int TilemapHeight = 500;

    public string EIconUrl = string.Empty;
    public bool EPoint;
    public float EPpu = 100;
    
    public override (string, string)[] FilesToDownload => [
        (IconUrl, "png"),
        (EIconUrl, "png")
    ];
    
    public override void Register()
    {
        SceneUtils.CustomScenes.Add(Id, this);

        if (SceneUtils.SceneGroups.TryGetValue(Group, out var group) && group.HasMapZone)
        {
            
        } 
        
        if (SceneUtils.QWHookEnabled) QuickWarpHookLoader.RegisterScene(Group, Id);
        
        base.Register();
        RefreshESprite();
    }

    public void RefreshESprite()
    {
        if (EIconUrl.IsNullOrWhiteSpace()) return;
        CustomAssetManager.DoLoadSprite(EIconUrl, EPoint, EPpu, 1, 1, sprites =>
        {
            if (sprites.IsNullOrEmpty()) return;
            
        });
    }

    protected override void OnReadySprite()
    {
        
    }

    public override void Unregister()
    {
        SceneUtils.CustomScenes.Remove(Id);
        
        if (SceneUtils.QWHookEnabled) QuickWarpHookLoader.UnregisterScene(Group, Id);
    }

    public override Sprite GetIcon() => Icon;
}