using System.Collections.Generic;
using Architect.Utils;
using UnityEngine;

namespace Architect.Events.Blocks.Outputs;

public class EnemyBlock : ScriptBlock
{
    protected override IEnumerable<string> Inputs => ["Damage", "Heal", "CappedHeal", "Set"];
    protected override IEnumerable<(string, string)> InputVars => [("Target", "Enemy"), ("Multiplier", "Number")];
    protected override IEnumerable<(string, string)> OutputVars => [("Path", "Text")];

    private static readonly Color DefaultColor = new(0.2f, 0.6f, 0.8f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Enemy Control";

    public int Health;
    public AttackTypes AttackType;

    protected override void Reset()
    {
        Health = 1;
        AttackType = AttackTypes.Generic;
    }

    protected override object GetValue(string id)
    {
        var target = GetVariable<HealthManager>("Target");
        return target ? target.transform.GetPath() : "";
    }

    protected override void Trigger(string trigger)
    {
        var target = GetVariable<HealthManager>("Target");
        if (!target) return;
        switch (trigger)
        {
            case "Damage":
                target.TakeDamage(
                    new HitInstance
                    {
                        Source = target.gameObject,
                        AttackType = AttackType,
                        NailElement = NailElements.None,
                        DamageDealt = (int)(Health * GetVariable<float>("Multiplier", 1)),
                        ToolDamageFlags = ToolDamageFlags.None,
                        SpecialType = SpecialTypes.None,
                        SlashEffectOverrides = [],
                        HitEffectsType = EnemyHitEffectsProfile.EffectsTypes.Minimal,
                        SilkGeneration = HitSilkGeneration.None,
                        Multiplier = 1
                    });
                break;
            case "Heal":
                target.hp += (int)(Health * GetVariable<float>("Multiplier", 1));
                break;
            case "Set":
                target.hp = (int)(Health * GetVariable<float>("Multiplier", 1));
                break;
            case "CappedHeal":
                target.AddHP((int)(Health * GetVariable<float>("Multiplier", 1)), target.initHp);
                break;
        }
    }
}