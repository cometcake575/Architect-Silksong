namespace Architect.Skins;

public abstract class SkinManager
{
    public static SkinManager Instance;
    public static bool Active;
    
    public static void Init()
    {
        if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("Stitchwork"))
        {
            Active = true;
            Instance = new StitchworkManager();
        }
        
        else Instance = new DummyManager();
    }
}