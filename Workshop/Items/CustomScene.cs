using Architect.Utils;
using UnityEngine;

namespace Architect.Workshop.Items;

public class CustomScene : WorkshopItem
{
    private static readonly Sprite Icon = ResourceUtils.LoadSpriteResource("door");

    public string Group = "None";
    
    public int TilemapWidth = 500;
    public int TilemapHeight = 500;
    
    public override void Register()
    {
        SceneUtils.CustomScenes.Add(Id, this);
        
        if (SceneUtils.QWHookEnabled) QuickWarpHook.RegisterScene(Group, Id);
    }

    public override void Unregister()
    {
        SceneUtils.CustomScenes.Remove(Id);
        
        if (SceneUtils.QWHookEnabled) QuickWarpHook.UnregisterScene(Group, Id);
    }

    public override Sprite GetIcon() => Icon;
}