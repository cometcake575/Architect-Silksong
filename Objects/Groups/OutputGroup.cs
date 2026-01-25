using System.Collections.Generic;
using Architect.Behaviour.Utility;
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
            new OutputType("hp", "Health", "Number", o =>
            {
                var hm = o.GetComponent<HealthManager>();
                return hm ? hm.hp : 0;
            })
        ),
        EventManager.RegisterOutputType(
            new OutputType("enemy_self", "Self", "Enemy", o =>
            {
                var hm = o.GetComponent<HealthManager>();
                return hm ? hm.hp : 0;
            })
        )
    ];

    public static readonly List<OutputType> EnemyHook =
    [
        EventManager.RegisterOutputType(
            new OutputType("hp_hook", "Health", "Number", o =>
            {
                var hm = o.GetComponent<EnemyHook>().hm;
                return hm ? hm.hp : 0;
            })
        ),
        EventManager.RegisterOutputType(
            new OutputType("hp_hook_target", "Target", "Enemy", 
                o => o.GetComponent<EnemyHook>().hm)
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