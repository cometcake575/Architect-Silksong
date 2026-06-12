using System.Collections.Generic;
using System.Linq;
using Architect.Placements;
using UnityEngine;

namespace Architect.Events.Blocks.Objects;

public class ToggleLayerBlock : ScriptBlock
{
    protected override IEnumerable<string> Inputs => ["Disable", "Enable"];

    public override Color Color => ObjectBlock.ValidColor;
    protected override string Name => "Toggle Layer";

    public override void Reset()
    {
        LayerNumber = 0;
    }

    public int LayerNumber;

    protected override void Trigger(string trigger)
    {
        var active = trigger == "Enable";

        if (!PlacementManager.Layers.TryGetValue(LayerNumber, out var list)) return;
        foreach (var obj in list.Where(obj => obj && obj.activeSelf != active))
        {
            obj.SetActive(active);
        }
    }
}