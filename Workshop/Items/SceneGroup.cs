using Architect.Utils;

namespace Architect.Workshop.Items;

public class SceneGroup : SpriteItem
{
    public string GroupName = string.Empty;
    public bool DisableAct3Bg;

    public SaveSlotBackgrounds.AreaBackground Background;
    
    public override void Register()
    {
        Background = new SaveSlotBackgrounds.AreaBackground
        {
            BackgroundImage = ArchitectPlugin.BlankSprite,
            NameOverride = (LocalStr)GroupName,
            Act3OverlayOptOut = DisableAct3Bg
        };
        
        SceneUtils.SceneGroups.Add(Id, this);
        
        base.Register();
    }

    protected override void OnReadySprite()
    {
        Background.BackgroundImage = Sprite;
    }

    public override void Unregister()
    {
        SceneUtils.SceneGroups.Remove(Id);
    }
}