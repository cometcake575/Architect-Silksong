using System.Collections.Generic;

namespace Architect.Events.Blocks.Functions;

public class FunctionDefinitionBlock : ScriptBlock
{
    protected override string Name => "Function";

    protected override IEnumerable<string> Outputs => ["OnCall"];
}
