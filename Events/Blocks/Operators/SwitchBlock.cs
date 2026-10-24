using System.Collections.Generic;
using System.Linq;

namespace Architect.Events.Blocks.Operators;

public class SwitchBlock : CollectionBlock<SwitchBlock.TriggerBlock>
{
    protected override string Name => "Switch";
    
    protected override IEnumerable<string> Inputs => ["Trigger", "DisableAll", "EnableAll"];
    protected override IEnumerable<(string, string)> InputVars => [("Value", "Any")];

    protected override string ChildName => "Switch Trigger";
    protected override bool NeedsGap => false;

    protected override void Trigger(string trigger)
    {
        switch (trigger)
        {
            case "Trigger":
            {
                var inp = GetVariable<object>("Value")?.ToString() ?? "";
                var ec = Children.Children.Where(b => b.Enabled && b.Value == inp).ToArray();
                foreach (var child in ec) child.Event("OnTrigger");
                break;
            }
            case "DisableAll":
                foreach (var c in Children.Children) c.Enabled = false;
                break;
            case "EnableAll":
                foreach (var c in Children.Children) c.Enabled = true;
                break;
        }
    }

    public class TriggerBlock : ChildBlock
    {
        public string Value = string.Empty;
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
