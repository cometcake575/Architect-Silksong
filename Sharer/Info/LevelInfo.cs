using System;
using System.Collections.Generic;

namespace Architect.Sharer.Info;

public class LevelInfo
{
    // Used to send a download/edit request
    public string LevelId;

    // Used for visuals
    public string CreatorId;
    public string CreatorName;
    public string LevelName;
    public string LevelDesc;
    public string IconURL;
    public bool HasSave;

    // Used for filters
    public int DownloadCount;
    public int LikeCount;
    public LevelDifficulty Difficulty = LevelDifficulty.None;
    public LevelDuration Duration = LevelDuration.None;
    public List<LevelTag> Tags = [];

    public bool Liked;

    // Chosen by creator
    public enum LevelTag
    {
        Platforming,
        Multiplayer,
        Gauntlets,
        Areas,
        Troll,
        Bosses,
        Minigames
    }

    // Chosen by creator
    public enum LevelDuration
    {
        /** Under 10 mins */
        Tiny,
        /** 10-30 mins */
        Short,
        /** 30-60 mins */
        Medium,
        /** 60+ mins */
        Long,
        /** Not applicable */
        None
    }

    // Chosen by creator
    public enum LevelDifficulty
    {
        None,
        Easy,
        Medium,
        Hard,
        ExtraHard
    }
}

public static class TagMethods
{
    public static string GetLabel(this LevelInfo.LevelDuration duration)
    {
        return duration switch
        {
            LevelInfo.LevelDuration.Tiny => "< 10 mins",
            LevelInfo.LevelDuration.Short => "10–30 mins",
            LevelInfo.LevelDuration.Medium => "30–60 mins",
            LevelInfo.LevelDuration.Long => "60+ mins",
            LevelInfo.LevelDuration.None => "N/A",
            _ => throw new ArgumentOutOfRangeException(nameof(duration), duration, null)
        };
    }
    
    public static string GetLabel(this LevelInfo.LevelDifficulty difficulty)
    {
        return difficulty switch
        {
            LevelInfo.LevelDifficulty.None => "N/A",
            LevelInfo.LevelDifficulty.Easy => "Easy",
            LevelInfo.LevelDifficulty.Medium => "Medium",
            LevelInfo.LevelDifficulty.Hard => "Hard",
            LevelInfo.LevelDifficulty.ExtraHard => "Extreme",
            _ => throw new ArgumentOutOfRangeException(nameof(difficulty), difficulty, null)
        };
    }
    
    public static string GetLabel(this LevelInfo.LevelTag tag)
    {
        return tag switch
        {
            LevelInfo.LevelTag.Platforming => "Platforming",
            LevelInfo.LevelTag.Minigames => "Minigames",
            LevelInfo.LevelTag.Multiplayer => "Multiplayer",
            LevelInfo.LevelTag.Gauntlets => "Gauntlets",
            LevelInfo.LevelTag.Areas => "Areas",
            LevelInfo.LevelTag.Troll => "Troll",
            LevelInfo.LevelTag.Bosses => "Bosses",
            _ => throw new ArgumentOutOfRangeException(nameof(tag), tag, null)
        };
    }
}