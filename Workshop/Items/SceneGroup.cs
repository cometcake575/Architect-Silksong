using Architect.Utils;

namespace Architect.Workshop.Items;

public class SceneGroup : SpriteItem
{
    public string GroupName = string.Empty;

    public SaveSlotBackgrounds.AreaBackground Background;
    
    public override void Register()
    {
        Background = new SaveSlotBackgrounds.AreaBackground
        {
            BackgroundImage = ArchitectPlugin.BlankSprite,
            NameOverride = (LocalStr)GroupName
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