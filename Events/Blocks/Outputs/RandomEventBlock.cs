using System.Collections.Generic;
using UnityEngine;

namespace Architect.Events.Blocks.Outputs;

public class RandomEventBlock : CollectionBlock<RandomEventBlock.TriggerBlock>
{
    private static readonly Color DefaultColor = new(0.9f, 0.5f, 0.2f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Random Event";
    
    protected override IEnumerable<string> Inputs => ["Trigger"];

    public class TriggerBlock : ChildBlock
    {
        protected override Color Color => DefaultColor; // Maybe change a bit

        protected override IEnumerable<string> Outputs => ["OnTrigger"];
    }
}
