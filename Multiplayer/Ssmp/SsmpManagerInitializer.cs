namespace Architect.Multiplayer.Ssmp;

public static class SsmpManagerInitializer
{
    public static void Init()
    {
        CoopManager.Instance = new SsmpManager();
    }
}