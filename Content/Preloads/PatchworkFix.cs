using System;
using Architect.Utils;
using Patchwork.Handlers;

namespace Architect.Content.Preloads;

public static class PatchworkFix
{
    public static void Init()
    {
        typeof(SpriteLoader).Hook("LoadCollection",
            (Action<tk2dSpriteCollectionData> orig, tk2dSpriteCollectionData self) =>
            {
                if (!PreloadManager.HasPreloaded) return;
                orig(self);
            });
    }
}