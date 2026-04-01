using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Architect.Events.Blocks.Operators;

public class OneByOneBlock : CollectionBlock<OneByOneBlock.TriggerBlock>
{
    private static readonly Color DefaultColor = new(0.9f, 0.7f, 0.3f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Step Counter";
    
    protected override IEnumerable<string> Inputs => ["Trigger", "DisableAll", "EnableAll"];

    protected override string ChildName => "Step Trigger";
    protected override bool NeedsGap => false;
    
    private int _index;

    public override void Reset()
    {
        _index = 0;
    }

    protected override void Trigger(string trigger)
    {
        if (Children.Children.Count(c => c.Enabled) == 0) return;
        
        var count = Children.Blocks.Count;
        
        _index %= count;
        Children.Blocks[_index].Event("OnTrigger");
        do
        {
            _index += 1;
            _index %= count;
        } while (!(Children.Blocks[_index] as TriggerBlock)!.Enabled);
    }

    public class TriggerBlock : ChildBlock
    {
        protected override Color Color => DefaultColor;

        public bool Enabled = true;

        public override void Reset()
        {
            Enabled = true;
        }

        protected override void Trigger(string trigger)
        {
            Enabled = trigger == "Enable";
        }

        protected override IEnumerable<string> Inputs => ["Disable", "Enable"];
        protected override IEnumerable<string> Outputs => ["OnTrigger"];
    }
}
