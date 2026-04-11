using System.Collections.Generic;
using UnityEngine;

namespace Architect.Events.Blocks.Operators;

public class NotBlock : ScriptBlock
{
    protected override IEnumerable<(string, string)> OutputVars => [("Value", "Boolean")];
    protected override IEnumerable<(string, string)> InputVars => [("Value", "Boolean")];
    
    protected override string Name => "Not";

    protected override object GetValue(string id)
    {
        return !GetVariable<bool>("Value");
    }
}

public class AndBlock : ScriptBlock
{
    protected override IEnumerable<(string, string)> OutputVars => [("Value", "Boolean")];
    protected override IEnumerable<(string, string)> InputVars => [("1", "Boolean"), ("2", "Boolean")];
    
    protected override string Name => "And";

    protected override object GetValue(string id)
    {
        return GetVariable<bool>("1") && GetVariable<bool>("2");
    }
}

public class OrBlock : ScriptBlock
{
    protected override IEnumerable<(string, string)> OutputVars => [("Value", "Boolean")];
    protected override IEnumerable<(string, string)> InputVars => [("1", "Boolean"), ("2", "Boolean")];
    
    protected override string Name => "Or";

    protected override object GetValue(string id)
    {
        return GetVariable<bool>("1") || GetVariable<bool>("2");
    }
}