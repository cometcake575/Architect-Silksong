using System.Collections.Generic;

namespace Architect.Events.Blocks.Functions;

public class CallFunctionBlock(string name) : ScriptBlock
{
    protected override string Name => name;
    
    protected override IEnumerable<string> Outputs => ["Call"];
}