using System;

namespace Architect.Utils;

public static class SceneUtils
{
    public static void Init()
    {
        typeof(GameManager).Hook(nameof(GameManager.LoadScene),
            (Action<GameManager, string> orig, GameManager self, string destScene) =>
            {
                if (destScene.StartsWith("Architect_"))
                {
                } else orig(self, destScene);
            });
        
    }
}