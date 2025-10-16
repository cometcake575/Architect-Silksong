using System;
using System.Collections;
using Architect.Utils;
using TMProOld;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class TextDisplay : NPCControlBase
{
    public string text;
    
    public float offsetY;

    public int verticalAlignment;
    public int horizontalAlignment;
    
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
            ShowDecorators = true,
            Alignment = alignment,
            OffsetY = offsetY
        };
    }

    public void Display()
    {
        StartCoroutine(DoDisplay());
    }

    private IEnumerator DoDisplay()
    {
        var fsm = HeroController.instance.sprintFSM;
        if (fsm.ActiveStateName.Contains("Sprint")) fsm.SendEvent("SKID END");
        
        yield return new WaitUntil(() => InteractManager.CanInteract && !HeroController.instance.controlReqlinquished);
        
        HeroController.instance.RelinquishControl();

        DialogueBox.StartConversation(text, this, false, _displayOptions, () =>
        {
            gameObject.BroadcastEvent("OnClose");
            StartCoroutine(RegainControlDelayed());
        });
    }

    private static IEnumerator RegainControlDelayed()
    {
        yield return new WaitForSeconds(0.1f);
        HeroController.instance.RegainControl();
    }
}