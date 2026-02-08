using System.Collections.Generic;
using UnityEngine;

namespace Architect.Events.Blocks.Outputs;

public class UIBlock : ScriptBlock
{
    protected override IEnumerable<string> Inputs => ["HudIn", "HudOut", "FadeIn", "FadeOut", "CloseInventory"];

    private static readonly Color DefaultColor = new(0.2f, 0.2f, 0.8f);
    protected override Color Color => DefaultColor;
    protected override string Name => "UI Control";

    protected override void Reset()
    {
        Duration = 1;
        R = 1;
        G = 1;
        B = 1;
        A = 1;
    }

    public float Duration = 1;
    public float R = 1;
    public float G = 1;
    public float B = 1;
    public float A = 1;

    protected override void Trigger(string id)
    {
        switch (id)
        {
            case "HudIn":
                GameCameras.instance.HUDIn();
                break;
            case "HudOut":
                GameCameras.instance.HUDOut();
                break;
            case "FadeIn":
                ScreenFaderUtils.Fade(new Color(R, G, B, A), Color.clear, Duration);
                break;
            case "FadeOut":
                ScreenFaderUtils.Fade(Color.clear, new Color(R, G, B, A), Duration);
                break;
            case "CloseInventory":
                EventRegister.SendEvent(EventRegisterEvents.InventoryCancel);
                break;
        }
    }
}
