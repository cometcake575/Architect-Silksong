using System.Collections.Generic;
using UnityEngine;

namespace Architect.Events.Blocks.Operators;

public class IfBlock : ScriptBlock
{
    public override IEnumerable<string> Inputs => ["In"];
    public override IEnumerable<string> Outputs => ["Out"];

    protected override int InputCount => 1;
    protected override int OutputCount => 1;
    
    protected override string Type => "if"; 
    
    protected override Color Color => new(0.9f, 0.7f, 0.3f);
    protected override string Name => "If";
    
    protected override bool SetupReference()
    {
        return true;
    }

    protected override void Trigger(string trigger)
    {
        Event("Out");
    }
}