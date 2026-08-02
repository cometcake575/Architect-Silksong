using System.Collections;
using System.Collections.Generic;
using Architect.Behaviour.Fixers;
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

    public List<SavedItem> savedItems = [];
    public List<int> costs = [];

    public ScriptBlock Block;

    public void Display()
    {
        StartCoroutine(DoDisplay());
    }

    private IEnumerator DoDisplay()
    {
        yield return HeroController.instance.FreeControl(_ => InteractManager.CanInteract);
        
        HeroController.instance.RelinquishControl();

        var txt = MiscFixers.SubstituteVars(text);

        if (useItem)
        {
            if (!savedItems.IsNullOrEmpty())
            {
                DialogueYesNoBox.Open(Yes, No, true, txt, savedItems, costs, 
                    true, takeItem, null); 
                yield break;
            }
            
            var i = MiscUtils.GetSavedItem(item);
            if (i)
            {
                DialogueYesNoBox.Open(Yes, No, true, txt, i, cost, 
                    true, takeItem); 
                yield break;
            }
        }
        DialogueYesNoBox.Open(Yes, No, true, txt, currencyType, cost);
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