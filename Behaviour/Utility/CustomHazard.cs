using System;
using Architect.Utils;
using GlobalEnums;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class CustomHazard : MonoBehaviour
{
    public bool overrideMultiHit;
    public MultihitTypes multihitType = MultihitTypes.Regular;
    
    public static readonly Sprite SquareZone =
        ResourceUtils.LoadSpriteResource("player_damager", FilterMode.Point, ppu: 64);
    public static readonly Sprite CircleZone =
        ResourceUtils.LoadSpriteResource("player_damager_circle", FilterMode.Point, ppu: 64);

    public static void Init()
    {
        typeof(DamageHero).Hook(nameof(DamageHero.Awake),
            (Action<DamageHero> orig, DamageHero self) =>
            {
                orig(self);
                if (self.hazardType != HazardType.STEAM) return;
                var customHazard = self.GetComponent<CustomHazard>();
                if (!customHazard || !customHazard.overrideMultiHit) return;

                var fsm = self.GetComponent<PlayMakerFSM>();
                fsm.FsmVariables.FindFsmBool("z2 Steam Hazard").Value = false;
                fsm.FsmVariables.FindFsmEnum("Multihit Type").Value = customHazard.multihitType;
            });
    }
}