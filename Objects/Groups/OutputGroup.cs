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
                var hm = o.GetComponentInChildren<HealthManager>();
                return hm ? hm.hp : 0;
            })
        ),
        EventManager.RegisterOutputType(
            new OutputType("enemy_self", "Self", "Enemy", o =>
            {
                var hm = o.GetComponentInChildren<HealthManager>();
                return hm;
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

    public static readonly List<OutputType> FsmHook =
    [
        EventManager.RegisterOutputType(
            new OutputType("fsm_hoo_state", "State", "Text",
                o => o.GetComponent<FsmHook>().GetState())
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

    public static readonly List<OutputType> LastJudge =
    [
        EventManager.RegisterOutputType
        (
            new OutputType("hp", "Health", "Number", o =>
            {
                var hm = o.GetComponentInChildren<HealthManager>();
                return hm ? hm.hp : 0;
            })
        ),
        EventManager.RegisterOutputType
        (
            new OutputType("enemy_self", "Self", "Enemy", o =>
            {
                var hm = o.GetComponentInChildren<HealthManager>();
                return hm;
            })
        ),
        EventManager.RegisterOutputType
        (
            new OutputType
            ("censer_x", "Censer X", "Number",
                o =>
                {
                    var thurible = o.transform.Find("Censer Slam");
                    var realThurible = thurible.transform.Find("censer sphere");
                    var tx = realThurible != null ? realThurible.GetComponent<Transform>() : null;
                    return tx.position.x;
                }
            )
        ),
        EventManager.RegisterOutputType
        (
            new OutputType
            ("censer_y", "Censer Y", "Number",
                o =>
                {
                    var thurible = o.transform.Find("Censer Slam");
                    var realThurible = thurible.transform.Find("censer sphere");
                    var ty = realThurible != null ? realThurible.GetComponent<Transform>() : null;
                    return ty.position.y;
                }
            )
        )
    ];
}
