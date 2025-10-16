using System;
using System.Collections.Generic;
using System.Reflection;
using Architect.Utils;
using GlobalEnums;
using MonoMod.RuntimeDetour;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class PlayerHook : MonoBehaviour
{
    public static readonly List<GameObject> PlayerListeners = [];
    
    public static void Init()
    {
        _ = new Hook(typeof(HeroController).GetMethod(nameof(HeroController.FlipSprite)),
            (Action<HeroController> orig, HeroController self) =>
            {
                orig(self);
                PlayerEvent(self.cState.facingRight ? "FaceRight" : "FaceLeft");
            });

        _ = new Hook(typeof(HeroController).GetMethod("HeroJump", 
                BindingFlags.Instance | BindingFlags.NonPublic, 
                null, [typeof(bool)], null),
            (Action<HeroController, bool> orig, HeroController self, bool checkSprint) =>
            {
                orig(self, checkSprint);
                PlayerEvent("Jump");
            });

        _ = new Hook(typeof(HeroController).GetMethod("DoWallJump", 
                BindingFlags.Instance | BindingFlags.NonPublic),
            (Action<HeroController> orig, HeroController self) =>
            {
                orig(self);
                PlayerEvent("WallJump");
            });

        _ = new Hook(typeof(HeroController).GetMethod("DoDoubleJump", 
                BindingFlags.Instance | BindingFlags.NonPublic),
            (Action<HeroController> orig, HeroController self) =>
            {
                orig(self);
                PlayerEvent("DoubleJump");
            });

        _ = new Hook(typeof(HeroController).GetMethod("HeroDash", 
                BindingFlags.NonPublic | BindingFlags.Instance),
            (Action<HeroController, bool> orig, HeroController self, bool start) =>
            {
                orig(self, start);
                PlayerEvent("Dash");
            });

        _ = new Hook(typeof(HeroController).GetMethod("BackOnGround", 
                BindingFlags.Instance | BindingFlags.NonPublic),
            (Action<HeroController, bool> orig, HeroController self, bool force) =>
            {
                orig(self, force);
                PlayerEvent("Land");
            });

        _ = new Hook(typeof(HeroController).GetMethod(nameof(HeroController.DoHardLanding)),
            (Action<HeroController> orig, HeroController self) =>
            {
                orig(self);
                PlayerEvent("HardLand");
            });

        _ = new Hook(typeof(HeroController).GetMethod("DoAttack", 
                BindingFlags.NonPublic | BindingFlags.Instance),
            (Action<HeroController> orig, HeroController self) =>
            {
                orig(self);
                PlayerEvent("Attack");
            });

        _ = new Hook(typeof(HeroController).GetMethod(nameof(HeroController.AddHealth)),
            (Action<HeroController, int> orig, HeroController self, int amount) =>
            {
                orig(self, amount);
                PlayerEvent("OnHeal");
            });

        _ = new Hook(typeof(PlayerData).GetMethod(nameof(PlayerData.TakeHealth)),
            (Action<PlayerData, int, bool, bool> orig, PlayerData self, int amount,
                bool hasBlue, bool breakMask) =>
            {
                orig(self, amount, hasBlue, breakMask);
                PlayerEvent("OnDamage");
            });

        _ = new Hook(typeof(HeroController).GetMethod("Awake", 
                BindingFlags.NonPublic | BindingFlags.Instance),
            (Action<HeroController> orig, HeroController self) =>
            {
                orig(self);
                self.OnDeath += () => PlayerEvent("OnDeath");
                self.OnHazardRespawn += () => PlayerEvent("OnHazardRespawn");
            });
    }

    private static void PlayerEvent(string triggerName)
    {
        foreach (var obj in PlayerListeners) obj?.BroadcastEvent(triggerName);
    }
    
    private void OnEnable()
    {
        PlayerListeners.Add(gameObject);
    }

    private void OnDisable()
    {
        PlayerListeners.Remove(gameObject);
    }

    public static void KillPlayer(GameObject o)
    {
        HeroController.instance.TakeDamage(o, CollisionSide.other, 999, HazardType.ENEMY);
    }

    public static void DamagePlayer(GameObject o)
    {
        HeroController.instance.TakeDamage(o, CollisionSide.other, 1, HazardType.ENEMY);
    }

    public static void HazardDamagePlayer(GameObject o)
    {
        HeroController.instance.TakeDamage(o, CollisionSide.other, 1, HazardType.SPIKES);
    }

    public static void HealPlayer()
    {
        HeroController.instance.AddHealth(1);
    }

    public static void FullHealPlayer()
    {
        HeroController.instance.RefillHealthToMax();
    }

    public static void GiveSilk()
    {
        HeroController.instance.AddSilk(1, true);
    }

    public static void MaxSilk()
    {
        HeroController.instance.AddSilk(999, true);
    }

    public static void TakeSilk()
    {
        HeroController.instance.TakeSilk(1);
    }

    public static void ClearSilk()
    {
        HeroController.instance.TakeSilk(999);
    }

    public static void DisableVignette()
    {
        HeroController.instance.vignette.gameObject.SetActive(false);
    }

    public static void EnableVignette()
    {
        HeroController.instance.vignette.gameObject.SetActive(true);
    }
}