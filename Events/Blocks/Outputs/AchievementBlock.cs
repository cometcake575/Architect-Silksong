using System.Collections.Generic;
using UnityEngine;

namespace Architect.Events.Blocks.Outputs;

public class AchievementBlock : ScriptBlock
{
    protected override IEnumerable<string> Inputs => ["Complete"];
    protected override IEnumerable<(string, string)> OutputVars => [
        ("Completed", "Boolean")
    ];

    private static readonly Color DefaultColor = new(0.6f, 0.2f, 0.9f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Achievement Control";

    public override void Reset()
    {
        AchievementName = "";
    }

    public string AchievementName;
    
    protected override void Trigger(string trigger)
    {
        var gs = GameManager.instance.gameSettings;
        var was = gs.showNativeAchievementPopups;
        gs.showNativeAchievementPopups = 1;
        GameManager.instance.achievementHandler.AwardAchievementToPlayer(AchievementName);
        gs.showNativeAchievementPopups = was;
    }

    protected override object GetValue(string id)
    {
        return GameManager.instance.achievementHandler.AchievementWasAwarded(AchievementName);
    }
}
