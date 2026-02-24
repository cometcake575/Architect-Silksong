using System.Collections;
using System.Collections.Generic;
using Architect.Behaviour.Utility;
using Architect.Utils;
using TeamCherry.Localization;
using UnityEngine;

namespace Architect.Events.Blocks.Outputs;

public class TitleBlock : ScriptBlock
{
    protected override IEnumerable<string> Inputs => ["Display"];

    private static readonly Color DefaultColor = new(0.9f, 0.2f, 0.2f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Title Display";
    
    public string Header = "";
    public string Body = "";
    public string Footer = "";
    public int TitleType;

    protected override void Trigger(string trigger)
    {
        TitleUtils.DisplayTitle(Header, Body, Footer, TitleType);
    }
}

public class TextBlock : ScriptBlock
{
    protected override IEnumerable<string> Inputs => ["Display", "Stop"];
    protected override IEnumerable<string> Outputs => ["OnClose"];

    private static readonly Color DefaultColor = new(0.9f, 0.2f, 0.2f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Text Display";

    private TextDisplay _display;

    protected override void Reset()
    {
        Text = "";
        OffsetY = 0;
    } 
    
    public string Text = "";
    
    public float OffsetY;

    public int VerticalAlignment;
    public int HorizontalAlignment;
    
    public bool Decorators;

    public override void SetupReference()
    {
        _display = new GameObject("[Architect] Text Display").AddComponent<TextDisplay>();
        _display.Block = this;

        _display.text = Text;
        _display.offsetY = OffsetY;
        _display.verticalAlignment = VerticalAlignment;
        _display.horizontalAlignment = HorizontalAlignment;
        _display.decorators = Decorators;
        
        _display.Setup();
    }

    protected override void Trigger(string trigger)
    {
        if (trigger == "Stop") DialogueBox.EndConversation();
        else _display.Display();
    }
}

public class ChoiceBlock : ScriptBlock
{
    protected override IEnumerable<string> Inputs => ["Display"];
    protected override IEnumerable<string> Outputs => ["Yes", "No"];

    private static readonly Color DefaultColor = new(0.9f, 0.2f, 0.2f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Choice Display";
    
    public string Text;
    public string Item;
    public bool TakeItem;
    public bool UseItem;
    public CurrencyType CurrencyType = CurrencyType.Money;
    public int Cost;

    private ChoiceDisplay _display;

    public override void SetupReference()
    {
        _display = new GameObject("[Architect] Text Display").AddComponent<ChoiceDisplay>();
        _display.Block = this;

        _display.text = Text;
        _display.item = Item;
        _display.takeItem = TakeItem;
        _display.useItem = UseItem;
        _display.currencyType = CurrencyType;
        _display.cost = Cost;
    }

    protected override void Trigger(string trigger)
    {
        _display.Display();
    }
}

public class InputBlock : ScriptBlock
{
    protected override IEnumerable<string> Inputs => ["Display"];
    protected override IEnumerable<string> Outputs => ["OnSubmit"];
    protected override IEnumerable<(string, string)> OutputVars => [("Input", "Text")];

    private static readonly Color DefaultColor = new(0.9f, 0.2f, 0.2f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Input Display";
    
    public string Text;

    private InputDisplay _display;

    public static bool IsActive;
    
    public static void Init()
    {
        
    }

    public override void SetupReference()
    {
        _display = new GameObject("[Architect] Text Display").AddComponent<InputDisplay>();
        _display.Block = this;
    }
    
    protected override void Trigger(string trigger)
    {
        _display.Display();
    }

    public class InputDisplay : MonoBehaviour
    {
        public InputBlock Block;
        
        public void Display()
        {
            StartCoroutine(DoDisplay());
        }

        private IEnumerator DoDisplay()
        {
            yield return HeroController.instance.FreeControl(_ => InteractManager.CanInteract);
            
            HeroController.instance.RelinquishControl();
            
            IsActive = true;
            DialogueYesNoBox.Open(Submit, Submit, true, Block.Text, CurrencyType.Money, 0);
        }

        private void Submit()
        {
            IsActive = false;
            if (!this) return;
            StartCoroutine(RegainControlDelayed());
            Block.Event("OnSubmit");
        }
        
        private static IEnumerator RegainControlDelayed()
        {
            yield return new WaitForSeconds(0.1f);
            HeroController.instance.RegainControl();
        }
    }
}

public class NeedolinBlock : ScriptBlock
{
    protected override IEnumerable<string> Inputs => ["Display"];

    private static readonly Color DefaultColor = new(0.9f, 0.2f, 0.2f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Song Display";

    public string Text = "";
    public float Delay = 1;
    private LocalisedTextCollection _collection;
    
    protected override void Reset()
    {
        Text = "";
        Delay = 1;
    }

    public override void SetupReference()
    {
        _collection = ScriptableObject.CreateInstance<LocalisedTextCollection>();
        _collection.data = new LocalisedTextCollectionData(new LocalisedString("ArchitectMod", Text));
    }

    protected override void Trigger(string trigger)
    {
        NeedolinMsgBox.AddText(_collection, true, true);
        ArchitectPlugin.Instance.StartCoroutine(RemoveText());
    }

    private IEnumerator RemoveText()
    {
        yield return new WaitForSeconds(Delay);
        NeedolinMsgBox.RemoveText(_collection);
    }
}
