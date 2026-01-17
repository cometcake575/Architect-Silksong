using System.Collections.Generic;

namespace Architect.Sharer.Info;

public class LevelInfo
{
    // Used to send a download request
    public string LevelId;

    // Used for visuals
    public string CreatorName;
    public string LevelName;
    public string LevelDesc;
    public string IconURL;

    // Used for filters
    public int DownloadCount;
    public int LikeCount;
    public int RecentLikeCount;
    public LevelDifficulty Difficulty;
    public List<LevelTags> Tags;

    // Chosen by creator
    public enum LevelTags
    {
        Platforming,
        Multiplayer,
        Gauntlets,
        Areas,
        Troll,
        Boss
    }

    // Determined by votes
    public enum LevelDifficulty
    {
        None,
        Easy,
        Medium,
        Hard,
        ExtraHard
    }
}