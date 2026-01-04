using System.Collections.Generic;
using Architect.Content.Preloads;
using BepInEx;
using TMProOld;
using UnityEngine;

namespace Architect.Events.Blocks.Outputs;

public class PowerupGetBlock : ScriptBlock
{
    private static PowerUpGetMsg _pugm;
    public static void Init()
    {
        PreloadManager.RegisterPreload(new BasicPreload("Shellwood_10", "Ancestral_Art_Get_Prompt",
            o => _pugm = o.GetComponent<PowerUpGetMsg>(), hads:true));
    }
    
    protected override IEnumerable<string> Inputs => ["Display"];
    protected override IEnumerable<string> Outputs => ["Dismiss"];
    protected override IEnumerable<(string, string)> InputVars => [
        ("", ""),
        ("", ""),
        ("Outline", "Sprite"),
        ("Solid", "Sprite"),
        ("Glow", "Sprite"),
        ("Prompt", "Sprite")
    ];

    protected override void Reset()
    {
        PrefixText = "";
        NameText = "";
        SuffixText = "";
        ButtonText = "";
        DescTopText = "";
        DescBotText = "";
    }

    private static readonly Color DefaultColor = new(0.9f, 0.2f, 0.2f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Powerup Display";

    public string PrefixText = "";
    public string NameText = "";
    public string SuffixText = "";
    public string ButtonText = "";
    public string DescTopText = "";
    public string DescBotText = "";
    
    protected override void Trigger(string trigger)
    {
        PowerUpGetMsg msg = null;
        msg = PowerUpGetMsg.Spawn(PowerUpGetMsg.PowerUps.Sprint, _pugm, End) as PowerUpGetMsg;
        if (!msg) return;

        msg.lineSprite.sprite = GetVariable<Sprite>("Outline");
        msg.solidSprite.sprite = GetVariable<Sprite>("Solid");
        msg.glowSprite.sprite = GetVariable<Sprite>("Glow");
        msg.promptSprite.sprite = GetVariable<Sprite>("Prompt");

        msg.nameText.text = NameText;
        msg.prefixText.text = PrefixText;
        msg.promptButtonSingleText.text = SuffixText;
        msg.descTextTop.text = DescTopText;
        msg.descTextBot.text = DescBotText;

        if (ButtonText.IsNullOrWhiteSpace())
        {
            msg.promptButtonSingle.gameObject.SetActive(false);
            msg.promptButtonSingleText.transform.SetLocalPositionX(0);
            msg.promptButtonSingleText.alignment = TextAlignmentOptions.Center;
        } else msg.promptButtonSingle.textContainer.textMeshPro.text = ButtonText;
        
        msg.transform.SetLocalPosition2D(Vector2.zero);
        GameCameras.instance.HUDOut();
        HeroController.instance.AddInputBlocker(msg);

        return;

        void End()
        {
            // ReSharper disable once AccessToModifiedClosure
            HeroController.instance.RemoveInputBlocker(msg);
            GameCameras.instance.HUDIn();
            Event("Dismiss");
        }
    }
}