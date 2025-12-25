using System.Collections.Generic;
using Architect.Events;
using Architect.Events.Vars;

namespace Architect.Objects.Groups;

public static class OutputGroup
{
    public static readonly List<OutputType> Generic = [];

    public static readonly List<OutputType> Enemies =
    [
        EventManager.RegisterOutputType(
            new NumOutputType("hp", "Health", o => o.GetComponent<HealthManager>().hp)
        )
    ];
}