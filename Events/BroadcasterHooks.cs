using System;
using System.Reflection;
using MonoMod.RuntimeDetour;

namespace Architect.Events;

public static class BroadcasterHooks
{
    public static void Init()
    {
        _ = new Hook(typeof(HealthManager)
                .GetMethod("TakeDamage", BindingFlags.Instance | BindingFlags.NonPublic),
            (Action<HealthManager, HitInstance> orig, HealthManager self, HitInstance hitInstance) =>
            {
                orig(self, hitInstance);
                EventManager.BroadcastEvent(self.gameObject, "OnDamage");
            }
        );
        
        _ = new Hook(typeof(HealthManager).GetMethod(nameof(HealthManager.SendDeathEvent)),
            (Action<HealthManager> orig, HealthManager self) =>
            {
                EventManager.BroadcastEvent(self.gameObject, "OnDeath");
                orig(self);
            }
        );

        _ = new Hook(typeof(Lever_tk2d).GetMethod(nameof(Lever_tk2d.Hit)),
            (Func<Lever_tk2d, HitInstance, IHitResponder.HitResponse> orig, Lever_tk2d self, HitInstance hit) =>
            {
                if (!self.activated)
                {
                    EventManager.BroadcastEvent(self.gameObject, "OnPull");
                    EventManager.BroadcastEvent(self.gameObject, "FirstPull");
                }

                return orig(self, hit);
            }
        );
    }
}