namespace Architect.Utils;

public class QuickWarpHookLoader
{
    public static void Init()
    {
        QuickWarpHook.Init();
    }

    public static void RegisterScene(string groupName, string sceneName)
    {
        QuickWarpHook.RegisterScene(groupName, sceneName);
    }

    public static void UnregisterScene(string groupName, string sceneName)
    {
        QuickWarpHook.UnregisterScene(groupName, sceneName);
    }
}