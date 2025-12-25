using System.Collections;
using Architect.Events.Blocks;
using Architect.Utils;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class ChoiceDisplay : MonoBehaviour, IDisplayable
{
    public string text;
    public string item;
    public bool takeItem;
    public bool useItem;
    public CurrencyType currencyType = CurrencyType.Money;
    public int cost;

    public ScriptBlock Block;

    public void Display()
    {
        StartCoroutine(DoDisplay());
    }

    private IEnumerator DoDisplay()
    {
        yield return HeroController.instance.FreeControl(_ => InteractManager.CanInteract);
        
        HeroController.instance.RelinquishControl();

        if (useItem)
        {
            var i = MiscUtils.GetSavedItem(item);
            if (i) {
                DialogueYesNoBox.Open(Yes, No, true, text, MiscUtils.GetSavedItem(item), cost, 
                true, takeItem); 
                yield break;
            }
        }
        DialogueYesNoBox.Open(Yes, No, true, text, currencyType, cost);
    }

    private void Yes()
    {
        if (!this) return;
        StartCoroutine(RegainControlDelayed());
        if (Block != null) Block.Event("Yes");
        else gameObject.BroadcastEvent("Yes");
    }

    private void No()
    {
        if (!this) return;
        StartCoroutine(RegainControlDelayed());
        if (Block != null) Block.Event("No");
        else gameObject.BroadcastEvent("No");
    }

    private static IEnumerator RegainControlDelayed()
    {
        yield return new WaitForSeconds(0.1f);
        HeroController.instance.RegainControl();
    }
}