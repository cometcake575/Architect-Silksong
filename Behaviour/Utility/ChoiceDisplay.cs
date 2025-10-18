using System.Collections;
using Architect.Utils;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class ChoiceDisplay : MonoBehaviour, IDisplayable
{
    public string text;
    public CurrencyType currencyType = CurrencyType.Money;
    public int cost;

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

        DialogueYesNoBox.Open(Yes, No, true, text, currencyType, cost);
    }

    private void Yes()
    {
        if (!this) return;
        StartCoroutine(RegainControlDelayed());
        gameObject.BroadcastEvent("Yes");
    }

    private void No()
    {
        if (!this) return;
        StartCoroutine(RegainControlDelayed());
        gameObject.BroadcastEvent("No");
    }

    private static IEnumerator RegainControlDelayed()
    {
        yield return new WaitForSeconds(0.1f);
        HeroController.instance.RegainControl();
    }
}