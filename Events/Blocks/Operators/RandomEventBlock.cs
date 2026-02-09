using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Architect.Events.Blocks.Operators;

public class RandomEventBlock : CollectionBlock<RandomEventBlock.TriggerBlock>
{
    private static readonly Color DefaultColor = new(0.9f, 0.7f, 0.3f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Random Event";
    
    protected override IEnumerable<string> Inputs => ["Trigger"];

    protected override string ChildName => "Random Trigger";
    protected override bool NeedsGap => false;

    protected override void Trigger(string trigger)
    {
        var sum = Children.Children.Sum(b => b.Chance);
        if (sum <= 0) return;
        var value = Random.Range(0, sum);
        foreach (var child in Children.Children)
        {
            value -= child.Chance;
            if (value <= 0)
            {
                child.Event("OnTrigger");
                break;
            }
        }
    }

    public class TriggerBlock : ChildBlock
    {
        protected override Color Color => DefaultColor;

        public float Chance;

        protected override IEnumerable<string> Outputs => ["OnTrigger"];
    }
}
