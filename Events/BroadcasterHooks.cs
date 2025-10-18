using System;
using System.Reflection;
using Architect.Utils;
using MonoMod.RuntimeDetour;

namespace Architect.Events;

public static class BroadcasterHooks
{
    public static void Init()
    {
        typeof(HealthManager).Hook("TakeDamage", 
            (Action<HealthManager, HitInstance> orig, HealthManager self, HitInstance hitInstance) => 
            {
                orig(self, hitInstance);
                EventManager.BroadcastEvent(self.gameObject, "OnDamage");
            }
        );
        
        typeof(HealthManager).Hook(nameof(HealthManager.SendDeathEvent),
            (Action<HealthManager> orig, HealthManager self) =>
            {
                EventManager.BroadcastEvent(self.gameObject, "OnDeath");
                orig(self);
            }
        );

        typeof(Lever_tk2d).Hook(nameof(Lever_tk2d.Hit),
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