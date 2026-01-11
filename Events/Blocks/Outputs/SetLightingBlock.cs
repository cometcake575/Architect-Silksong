using System;
using System.Collections.Generic;
using Architect.Utils;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

namespace Architect.Events.Blocks.Outputs;

public class SetLightingBlock : ScriptBlock
{
    public static bool IsLocked;
    public static float SaturationLock;

    public static void Init()
    {
        typeof(HeroController).Hook(nameof(HeroController.SceneInit),
            (Action<HeroController> orig, HeroController self) =>
            {
                IsLocked = false;
                orig(self);
            });
        
        typeof(CustomSceneManager).Hook(nameof(CustomSceneManager.SetLighting),
            (Action<Color, float> orig, Color color, float intensity) =>
            {
                if (IsLocked) return;
                orig(color, intensity);
            });
        
        typeof(ColorCorrectionCurves).Hook(nameof(ColorCorrectionCurves.UpdateMaterial),
            (Action<ColorCorrectionCurves, Material> orig, ColorCorrectionCurves self, Material material) =>
            {
                if (IsLocked) self.saturation = SaturationLock;
                orig(self, material);
            }, typeof(Material));

        HookUtils.OnHeroUpdate += _ =>
        {
            if (IsLocked) GameCameras.instance.colorCorrectionCurves.saturation = SaturationLock;
        };
    }
    
    protected override IEnumerable<string> Inputs => ["Set"];

    private static readonly Color DefaultColor = new(0.2f, 0.4f, 0.8f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Set Lighting";

    protected override void Reset()
    {
        R = 1;
        G = 1;
        B = 1;
        Intensity = 1;
        Saturation = 1;
        Lock = true;
    }

    public float R = 1;
    public float G = 1;
    public float B = 1;
    public float Intensity = 1;
    public float Saturation = 1;
    public bool Lock;

    protected override void Trigger(string trigger)
    {
        IsLocked = false;
        var sm = GameManager.instance.sm;
        
        sm.saturation = Saturation;
        GameCameras.instance.colorCorrectionCurves.saturation = Saturation;
        sm.setSaturation = true;
        SaturationLock = Saturation;
        
        CustomSceneManager.SetLighting(new Color(R, G, B), Intensity);
        IsLocked = Lock;
    }
}