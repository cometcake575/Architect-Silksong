using System.Collections.Generic;
using GlobalEnums;
using Unity.Mathematics.Geometry;
using UnityEngine;
using Math = System.Math;

namespace Architect.Events.Blocks.Outputs;

public class HpBlock : ScriptBlock
{
    protected override IEnumerable<string> Inputs => ["Give", "Take", "TakeInstant", "TakeHazard"];
    protected override IEnumerable<(string, string)> OutputVars => [
        ("Amount", "Number"),
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
        return id == "Amount" ? PlayerData.instance.health : PlayerData.instance.healthBlue;
    }
}

public class SilkBlock : ScriptBlock
{
    protected override IEnumerable<string> Inputs => ["Give", "Take"];
    protected override IEnumerable<(string, string)> OutputVars => [("Amount", "Number")];

    private static readonly Color DefaultColor = new(0.2f, 0.6f, 0.8f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Silk Control";

    protected override void Reset()
    {
        Amount = 0;
    }

    public int Amount;
    
    protected override void Trigger(string trigger)
    {
        if (trigger == "Give") HeroController.instance.AddSilk(Amount, true);
        else HeroController.instance.TakeSilk(Amount);
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