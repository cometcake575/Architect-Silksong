using System;
using System.Collections.Generic;
using Architect.Utils;
using BepInEx;
using GlobalEnums;
using UnityEngine;
using Math = System.Math;

namespace Architect.Events.Blocks.Outputs;

public class HpBlock : ScriptBlock
{
    protected override IEnumerable<string> Inputs => [
        "Give", 
        "GiveBlue",
        "Take",
        "TakeInstant",
        "TakeHazard"];
    protected override IEnumerable<(string, string)> OutputVars => [
        ("Amount", "Number"),
        ("MaxAmount", "Number"),
        ("Lifeblood", "Number")
    ];

    private static readonly Color DefaultColor = new(0.2f, 0.6f, 0.8f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Health Control";

    protected override void Reset()
    {
        Amount = 0;
    }

    public int Amount;

    protected override void Trigger(string trigger)
    {
        switch (trigger)
        {
            case "Give":
                HeroController.instance.AddHealth(Amount);
                break;
            case "GiveBlue":
                for (var i = 0; i < Amount; i++) GameManager.instance.AddBlueHealthQueued();
                break;
            case "Take":
                HeroController.instance.TakeDamage(HeroController.instance.gameObject, CollisionSide.other, Amount,
                    HazardType.ENEMY);
                break;
            case "TakeInstant":
                HeroController.instance.DoSpecialDamage(Amount, false, "Instant", true, true, true, false);
                break;
            case "TakeHazard":
                HeroController.instance.TakeHealth(Amount - 1);
                HeroController.instance.TakeDamage(HeroController.instance.gameObject, CollisionSide.other, 1,
                    HazardType.SPIKES);
                break;
        }
    }

    protected override object GetValue(string id)
    {
        return id switch
        {
            "Amount" => PlayerData.instance.health,
            "MaxAmount" => PlayerData.instance.maxHealth,
            "Lifeblood" => PlayerData.instance.healthBlue,
            _ => 0
        };
    }
}

public class SilkBlock : ScriptBlock
{
    public static void Init()
    {
        typeof(PlayerData).Hook(nameof(PlayerData.AddSilk),
            (Func<PlayerData, int, bool> orig, PlayerData self, int amount) =>
            {
                _onSilkGain?.Invoke();
                return orig(self, amount);
            });
    }
    
    protected override IEnumerable<string> Inputs => ["Give", "Take", "BreakCocoon"];
    protected override IEnumerable<string> Outputs => ["OnGain"];
    protected override IEnumerable<(string, string)> OutputVars => [("Amount", "Number")];

    private static readonly Color DefaultColor = new(0.2f, 0.6f, 0.8f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Silk Control";

    protected override void Reset()
    {
        Amount = 0;
    }

    public override void SetupReference()
    {
        var scr = new GameObject("[Architect] Silk Control Ref");
        scr.AddComponent<SilkControl>().Block = this;
    }

    private static Action _onSilkGain;

    public class SilkControl : MonoBehaviour
    {
        public SilkBlock Block;
        
        private void Start()
        {
            _onSilkGain += OnGain;
        }

        private void OnDisable()
        {
            _onSilkGain -= OnGain;
        }

        public void OnGain() => Block.Event("OnGain");
    }

    public int Amount;

    protected override void Trigger(string trigger)
    {
        switch (trigger)
        {
            case "Give":
                HeroController.instance.AddSilk(Amount, true);
                break;
            case "Take":
                HeroController.instance.TakeSilk(Amount);
                break;
            case "BreakCocoon":
                if (PlayerData.instance.HeroCorpseScene.IsNullOrWhiteSpace()) return;
                HeroController.instance.CocoonBroken();
                EventRegister.SendEvent(EventRegisterEvents.BreakHeroCorpse);
                break;
        }
    }

    protected override object GetValue(string id)
    {
        return PlayerData.instance.silk;
    }
}

public class CurrencyBlock : ScriptBlock
{
    protected override IEnumerable<string> Inputs => ["Give", "Take"];
    protected override IEnumerable<(string, string)> OutputVars => [("Amount", "Number")];

    private static readonly Color DefaultColor = new(0.2f, 0.6f, 0.8f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Currency Control";

    protected override void Reset()
    {
        Amount = 0;
    }

    public int Amount;
    public CurrencyType CurrencyType = CurrencyType.Money;
    public bool ShowCounter = true;

    protected override void Trigger(string trigger)
    {
        if (trigger == "Give") HeroController.instance.AddCurrency(Amount, CurrencyType, ShowCounter);
        else
        {
            var a = Math.Min(HeroController.instance.GetCurrencyAmount(CurrencyType), Amount);
            HeroController.instance.TakeCurrency(a, CurrencyType, ShowCounter);
        }
    }

    protected override object GetValue(string id)
    {
        return HeroController.instance.GetCurrencyAmount(CurrencyType);
    }
}

public class StatusBlock : ScriptBlock
{
    protected override IEnumerable<string> Inputs => ["Maggot", "Unmaggot", "Void", "Plasmify", "Deplasmify"];
    protected override IEnumerable<(string, string)> OutputVars => [
        ("Maggoted", "Boolean"),
        ("Frosted", "Boolean"), 
        ("Plasmified", "Boolean")];

    private static readonly Color DefaultColor = new(0.2f, 0.6f, 0.8f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Status Control";

    protected override void Trigger(string trigger)
    {
        switch (trigger)
        {
            case "Void":
                HeroController.instance.ActivateVoidAcid();
                break;
            case "Maggot":
                HeroController.instance.SetIsMaggoted(true);
                break;
            case "Unmaggot":
                HeroController.instance.SetIsMaggoted(false);
                break;
            case "Plasmify":
                HeroController.instance.HitMaxBlueHealth();
                HeroController.instance.HitMaxBlueHealthBurst();
                break;
            case "Deplasmify":
                HeroController.instance.ResetLifebloodState();
                break;
        }
    }

    protected override object GetValue(string id)
    {
        return id switch
        {
            "Maggoted" => HeroController.instance.cState.isMaggoted,
            "Plasmified" => HeroController.instance.IsInLifebloodState,
            "Frosted" => HeroController.instance.cState.isFrosted,
            _ => false
        };
    }
}