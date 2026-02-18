using System.Linq;
using Architect.Utils;
using UnityEngine;

namespace Architect.Workshop.Items;

public class SceneGroup : SpriteItem
{
    public string GroupName = string.Empty;
    public bool DisableAct3Bg;

    public Vector2 MapPos;

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

        /*
        var mz = GameManager.instance.gameMap.mapZoneInfo.ToList();
        mz.Add(new GameMap.ZoneInfo());
        GameManager.instance.gameMap.mapZoneInfo = mz.ToArray();

        var gm = GameManager.instance.gameMap.transform;
        var obj = new GameObject(Id)
        {
            transform =
            {
                parent = gm
            }
        };
        */
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