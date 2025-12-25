using System.Collections.Generic;
using UnityEngine;
using Camera = GlobalSettings.Camera;

namespace Architect.Events.Blocks.Outputs;

public class ShakeCameraBlock : ScriptBlock
{
    protected override IEnumerable<string> Inputs => ["Shake"];

    private static readonly Color DefaultColor = new(0.2f, 0.2f, 0.8f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Camera Shake";
    
    public int ShakeType;

    protected override void Trigger(string id)
    {
        Camera.MainCameraShakeManager.DoShake(ShakeType switch
        {
            0 => Camera.TinyShake,
            1 => Camera.SmallShake,
            2 => Camera.AverageShake,
            _ => Camera.BigShake
        }, HeroController.instance, false);
    }
}
