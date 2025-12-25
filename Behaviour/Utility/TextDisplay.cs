using System;
using System.Collections;
using Architect.Events.Blocks;
using Architect.Utils;
using TMProOld;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class TextDisplay : NPCControlBase, IDisplayable
{
    public ScriptBlock Block;
    
    public string text = "";
    
    public float offsetY;

    public int verticalAlignment;
    public int horizontalAlignment;

    public bool decorators = true;
    
    private DialogueBox.DisplayOptions _displayOptions;
    
    public override void Awake()
    {
        base.Awake();
        
        var alignment = (verticalAlignment, horizontalAlignment) switch
        {
            (0, 0) => TextAlignmentOptions.TopLeft,
            (0, 1) => TextAlignmentOptions.Top,
            (0, 2) => TextAlignmentOptions.TopRight,
            (1, 0) => TextAlignmentOptions.Left,
            (1, 1) => TextAlignmentOptions.Center,
            (1, 2) => TextAlignmentOptions.Right,
            (2, 0) => TextAlignmentOptions.BottomLeft,
            (2, 1) => TextAlignmentOptions.Bottom,
            (2, 2) => TextAlignmentOptions.BottomRight,
            _ => throw new ArgumentOutOfRangeException()
        };

        _displayOptions = new DialogueBox.DisplayOptions
        {
            ShowDecorators = decorators,
            Alignment = alignment,
            OffsetY = offsetY,
            TextColor = Color.white
        };
    }

    public void Display()
    {
        StartCoroutine(DoDisplay());
    }

    private IEnumerator DoDisplay()
    {
        yield return HeroController.instance.FreeControl(_ => InteractManager.CanInteract);
        
        HeroController.instance.RelinquishControl();

        DialogueBox.StartConversation(text, this, false, _displayOptions, () =>
        {
            if (Block != null) Block.Event("OnClose");
            else gameObject.BroadcastEvent("OnClose");
            StartCoroutine(RegainControlDelayed());
        });
    }

    private static IEnumerator RegainControlDelayed()
    {
        yield return new WaitForSeconds(0.1f);
        HeroController.instance.RegainControl();
    }
}