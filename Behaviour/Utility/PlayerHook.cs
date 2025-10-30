using System;
using System.Collections.Generic;
using Architect.Utils;
using GlobalEnums;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class PlayerHook : MonoBehaviour
{
    public static readonly List<GameObject> PlayerListeners = [];
    
    public static void Init()
    {
        typeof(HeroController).Hook(nameof(HeroController.FlipSprite),
            (Action<HeroController> orig, HeroController self) =>
            {
                orig(self);
                PlayerEvent(self.cState.facingRight ? "FaceRight" : "FaceLeft");
            });

        typeof(HeroController).Hook("HeroJump",
            (Action<HeroController, bool> orig, HeroController self, bool checkSprint) =>
            {
                orig(self, checkSprint);
                PlayerEvent("Jump");
            }, typeof(bool));

        typeof(HeroController).Hook("DoWallJump",
            (Action<HeroController> orig, HeroController self) =>
            {
                orig(self);
                PlayerEvent("WallJump");
            });

        typeof(HeroController).Hook("DoDoubleJump",
            (Action<HeroController> orig, HeroController self) =>
            {
                orig(self);
                PlayerEvent("DoubleJump");
            });

        typeof(HeroController).Hook("HeroDash",
            (Action<HeroController, bool> orig, HeroController self, bool start) =>
            {
                orig(self, start);
                PlayerEvent("Dash");
            });

        typeof(HeroController).Hook("BackOnGround",
            (Action<HeroController, bool> orig, HeroController self, bool force) =>
            {
                orig(self, force);
                PlayerEvent("Land");
            });

        typeof(HeroController).Hook(nameof(HeroController.DoHardLanding),
            (Action<HeroController> orig, HeroController self) =>
            {
                orig(self);
                PlayerEvent("HardLand");
            });

        typeof(HeroController).Hook("DoAttack",
            (Action<HeroController> orig, HeroController self) =>
            {
                orig(self);
                PlayerEvent("Attack");
            });

        typeof(HeroController).Hook(nameof(HeroController.AddHealth),
            (Action<HeroController, int> orig, HeroController self, int amount) =>
            {
                orig(self, amount);
                PlayerEvent("OnHeal");
            });

        typeof(PlayerData).Hook(nameof(PlayerData.TakeHealth),
            (Action<PlayerData, int, bool, bool> orig, PlayerData self, int amount,
                bool hasBlue, bool breakMask) =>
            {
                orig(self, amount, hasBlue, breakMask);
                PlayerEvent("OnDamage");
            });

        HookUtils.OnHeroAwake += self =>
        {
            self.OnDeath += () => PlayerEvent("OnDeath");
            self.OnHazardRespawn += () => PlayerEvent("OnHazardRespawn");

            HeroPerformanceRegion.StartedPerforming += () => PlayerEvent("NeedolinStart");
            HeroPerformanceRegion.StoppedPerforming += () => PlayerEvent("NeedolinStop");
        };
    }

    private static void PlayerEvent(string triggerName)
    {
        foreach (var obj in PlayerListeners.ToArray()) obj?.BroadcastEvent(triggerName);
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
        HeroController.instance.TakeDamage(o, CollisionSide.other, 999, HazardType.SPIKES);
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