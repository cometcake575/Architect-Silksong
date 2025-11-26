using System;
using Architect.Utils;
using HutongGames.PlayMaker.Actions;
using UnityEngine;

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
        
        typeof(HealthManager).Hook(nameof(HealthManager.Die),
            (Action<HealthManager, float?, AttackTypes, NailElements, GameObject, bool, float, bool, bool> orig, 
                HealthManager self,
                float? attackDirection,
                AttackTypes attackType,
                NailElements nailElement,
                GameObject damageSource,
                bool ignoreEvasion,
                float corpseFlingMultiplier,
                bool overrideSpecialDeath,
                bool disallowDropFling) =>
            {
                self.gameObject.BroadcastEvent("OnDeath");
                self.gameObject.BroadcastEvent("FirstDeath");
                
                var pbi = self.GetComponent<PersistentBoolItem>();
                if (pbi) pbi.SetValueOverride(true);
                
                orig(self, attackDirection, attackType, nailElement, damageSource, ignoreEvasion, corpseFlingMultiplier, overrideSpecialDeath, disallowDropFling);
            }, 
            typeof(float?), 
            typeof(AttackTypes), 
            typeof(NailElements), 
            typeof(GameObject),
            typeof(bool),
            typeof(float),
            typeof(bool),
            typeof(bool)
        );
        
        typeof(HealthManager).Hook(nameof(HealthManager.Awake),
            (Action<HealthManager> orig, HealthManager self) =>
            {
                var component = self.GetComponent<PersistentBoolItem>();
                if (!component) return;
                component.OnSetSaveState += value =>
                {
                    if (value) self.gameObject.BroadcastEvent("OnDeath");
                    if (value) self.gameObject.BroadcastEvent("LoadedDead");
                };
                orig(self);
            }
        );

        typeof(Lever_tk2d).Hook(nameof(Lever_tk2d.Hit),
            (Func<Lever_tk2d, HitInstance, IHitResponder.HitResponse> orig, Lever_tk2d self, HitInstance hit) =>
            {
                if (!self.activated)
                {
                    self.gameObject.BroadcastEvent("OnPull");
                    self.gameObject.BroadcastEvent("FirstPull");
                }

                return orig(self, hit);
            }
        );
        
        typeof(StartRoarEmitter).Hook(nameof(StartRoarEmitter.StartRoar),
            (Action<StartRoarEmitter> orig, StartRoarEmitter self) =>
            {
                var o = self.fsmComponent.gameObject;
                if (!o.GetComponent<DoneRoar>())
                {
                    o.AddComponent<DoneRoar>();
                    o.BroadcastEvent("OnRoar");
                }
                orig(self);
            });
        
        typeof(StartRoarEmitterV2).Hook(nameof(StartRoarEmitterV2.StartRoar),
            (Action<StartRoarEmitterV2> orig, StartRoarEmitterV2 self) =>
            {
                var o = self.fsmComponent.gameObject;
                if (!o.GetComponent<DoneRoar>())
                {
                    o.AddComponent<DoneRoar>();
                    o.BroadcastEvent("OnRoar");
                }
                orig(self);
            });
    }

    private class DoneRoar : MonoBehaviour;
}