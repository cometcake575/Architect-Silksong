using System.Collections.Generic;
using Architect.Behaviour.Utility;
using Architect.Utils;
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
    protected override IEnumerable<string> Inputs => ["Display"];
    protected override IEnumerable<string> Outputs => ["OnClose"];

    private static readonly Color DefaultColor = new(0.9f, 0.2f, 0.2f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Text Display";

    private TextDisplay _display;
    
    public string Text = "";
    
    public float OffsetY;

    public int VerticalAlignment;
    public int HorizontalAlignment;
    
    public bool Decorators;

    protected override void SetupReference()
    {
        _display = new GameObject("[Architect] Text Display").AddComponent<TextDisplay>();
        _display.Block = this;

        _display.text = Text;
        _display.offsetY = OffsetY;
        _display.verticalAlignment = VerticalAlignment;
        _display.horizontalAlignment = HorizontalAlignment;
        _display.decorators = Decorators;
    }

    protected override void Trigger(string trigger)
    {
        _display.Display();
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

    protected override void SetupReference()
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
