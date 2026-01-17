using System.Collections.Generic;
using Architect.Events;
using Architect.Events.Vars;
using UnityEngine;

namespace Architect.Objects.Groups;

public static class OutputGroup
{
    public static readonly List<OutputType> Generic = [];

    public static readonly List<OutputType> Enemies =
    [
        EventManager.RegisterOutputType(
            new OutputType("hp", "Health", "Number", o => o.GetComponent<HealthManager>().hp)
        )
    ];

    public static readonly List<OutputType> Png =
    [
        EventManager.RegisterOutputType(
            new OutputType("png_sprite", "Current Sprite", "Sprite", 
                o => o.GetComponent<SpriteRenderer>().sprite)
        )
    ];

    public static readonly List<OutputType> ObjectAnchor =
    [
        EventManager.RegisterOutputType(
            new OutputType("anchor_x", "X", "Number", 
                o => o.transform.GetPositionX()
        )),
        EventManager.RegisterOutputType(
            new OutputType("anchor_y", "Y", "Number", 
                o => o.transform.GetPositionY()
        ))
    ];
}