using System;
using System.Collections.Generic;
using System.Linq;
using Architect.Utils;
using GlobalEnums;
using PolyAndCode.UI;
using TeamCherry.Localization;

namespace Architect.Workshop.Items;

public class CustomAchievement : SpriteItem
{
    private static readonly Dictionary<string, CustomAchievement> Achievements = [];

    public string Name = string.Empty;
    public string Desc = string.Empty;
    public AchievementType AchievementType = AchievementType.Normal;
    public string InsertBefore = string.Empty;
    
    private Achievement _achievement;
    
    public static void Init()
    {
        typeof(AchievementHandler).Hook(nameof(AchievementHandler.Start),
            (Action<AchievementHandler> orig, AchievementHandler self) =>
            {
                orig(self);
                foreach (var achievement in Achievements.Values.ToArray())
                {
                    achievement.Unregister();
                    achievement.Register();
                }
            });
        
        typeof(UIManager).Hook(nameof(UIManager.UIGoToAchievementsMenu),
            (Action<UIManager> orig, UIManager self) =>
            {
                orig(self);
                
                var i = 0;
                foreach (var cell in UIManager.instance.menuAchievementsList.GetComponentsInChildren<ICell>())
                {
                    UIManager.instance.menuAchievementsList.SetCell(cell, i);
                    i++;
                }
            });
        
        typeof(Language).Hook(nameof(Language.Get),
            (Func<string, string, string> orig, string key, string sheetTitle) =>
            {
                if (sheetTitle == "Achievements")
                {
                    foreach (var achievement in Achievements.Values)
                    {
                        if (key == achievement._achievement.TitleCell) return achievement.Name;
                        if (key == achievement._achievement.DescriptionCell) return achievement.Desc;
                    }
                }
                return orig(key, sheetTitle);
            }, typeof(string), typeof(string));
    }
    
    public override void Register()
    {
        Achievements[Id] = this;
        if (!GameManager.instance) return;

        var ach = GameManager.instance.achievementHandler;
        _achievement = new Achievement
        {
            PlatformKey = Id,
            Type = AchievementType
        };
        var l = ach.achievementsList.achievements;
        
        var i = l.FindIndex(o => o.PlatformKey == InsertBefore);
        if (i != -1) l.Insert(i, _achievement);
        else l.Add(_achievement);
        
        base.Register();
    }

    protected override void OnReadySprite()
    {
        _achievement.Icon = Sprite;
    }

    public override void Unregister()
    {
        Achievements.Remove(Id);

        if (!GameManager.instance) return;
        GameManager.instance.achievementHandler.achievementsList.achievements.Remove(_achievement);
    }
}