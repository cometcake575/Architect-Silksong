using System;
using System.Reflection;
using MonoMod.RuntimeDetour;

namespace Architect.Utils;

public static class HookUtils
{
    public static Action<HeroController> OnHeroUpdate;
    public static Action<HeroController> OnHeroAwake;
    public static Action<PlayMakerFSM> OnFsmAwake;

    public static void Init()
    {
        typeof(PlayMakerFSM).Hook("Awake", (Action<PlayMakerFSM> orig, PlayMakerFSM self) =>
        {
            orig(self);
            OnFsmAwake?.Invoke(self);
        });

        typeof(HeroController).Hook("Update", (Action<HeroController> orig, HeroController self) =>
        {
            orig(self);
            OnHeroUpdate?.Invoke(self);
        });
        
        typeof(HeroController).Hook("Awake", (Action<HeroController> orig, HeroController self) =>
        {
            orig(self);
            OnHeroAwake?.Invoke(self);
        });
    }

    public static void Hook(this Type type, string name, Delegate target, params Type[] types)
    {
        if (types.IsNullOrEmpty())
        {
            _ = new Hook(type.GetMethod(name,
                    BindingFlags.NonPublic | BindingFlags.Public | 
                    BindingFlags.Instance | BindingFlags.Static), target);
        }
        else _ = new Hook(type.GetMethod(name,
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static,
                null, types, null), 
            target);
    }
}