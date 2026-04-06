using System.Collections.Generic;
using Architect.Behaviour.Custom;
using Architect.Behaviour.Fixers;
using Architect.Behaviour.Utility;
using Architect.Content.Custom;
using Architect.Events;
using Architect.Events.Vars;
using UnityEngine;

namespace Architect.Objects.Groups;

public static class OutputGroup
{
    private static readonly OutputType Space = new("space", "", "", null);
    
    public static readonly List<OutputType> Generic = [];
    
    public static readonly List<OutputType> Objects = [
        EventManager.RegisterOutputType(
            new OutputType("object_self", "Self", "Object", o => o)
        )
    ];

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

    public static readonly List<OutputType> EnemyDamager = GroupUtils.Merge(Objects,
    [
        EventManager.RegisterOutputType(
            new OutputType("enemy_lastdmg", "LastDamaged", "Enemy", o =>
            {
                var hm = o.GetComponent<UtilityObjects.EnemyDamager>();
                return hm.last;
            })
        )
    ]);

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

    public static readonly List<OutputType> ObjectHook =
    [
        EventManager.RegisterOutputType(
            new OutputType("obj_hook_target", "Target", "Object", 
                o => o.GetComponent<ObjectHook>().o)
        )
    ];

    public static readonly List<OutputType> TriggerZone = GroupUtils.Merge(Objects,
    [
        EventManager.RegisterOutputType(
            new OutputType("trigger_inside", "Inside", "Boolean", 
                o => o.GetComponent<TriggerZone>().inside)
        )
    ]);

    public static readonly List<OutputType> FleaCounter = GroupUtils.Merge(Objects,
    [
        EventManager.RegisterOutputType(
            new OutputType("flea_score", "Score", "Number", 
                o => o.GetComponent<MiscFixers.CustomFleaCounter>().currentCount)
        )
    ]);

    public static readonly List<OutputType> Png = GroupUtils.Merge(Objects,
    [
        EventManager.RegisterOutputType(
            new OutputType("png_sprite", "Current Sprite", "Sprite", 
                o => o.GetComponent<SpriteRenderer>().sprite)
        ),
        Space,
        EventManager.RegisterOutputType(
            new OutputType("png_frame", "Frame", "Number", 
                o => o.GetComponent<PngObject>().frame)
        )
    ]);

    public static readonly List<OutputType> FsmHook = GroupUtils.Merge(Objects,
    [
        EventManager.RegisterOutputType(
            new OutputType("fsm_hoo_state", "State", "Text",
                o => o.GetComponent<FsmHook>().GetState())
        ),
        EventManager.RegisterOutputType(
            new OutputType("fsm_hook_time", "Time", "Number",
                o => o.GetComponent<FsmHook>().GetTime())
        )
    ]);

    public static readonly List<OutputType> ObjectAnchor = GroupUtils.Merge(Objects,
    [
        EventManager.RegisterOutputType(
            new OutputType("anchor_x", "X", "Number", 
                o => o.transform.GetPositionX()
        )),
        EventManager.RegisterOutputType(
            new OutputType("anchor_y", "Y", "Number", 
                o => o.transform.GetPositionY()
        ))
    ]);

    public static readonly List<OutputType> LastJudge = GroupUtils.Merge(Enemies, [
        EventManager.RegisterOutputType
        (
            new OutputType
            ("censer_x", "Censer X", "Number",
                o =>
                {
                    var fsm = o.LocateMyFSM("Control");
                    var censer = fsm.FsmVariables.FindFsmGameObject("Censer Throw");
                    return censer.value ? censer.value.transform.GetPositionX() : 0f;
                }
            )
        ),
        EventManager.RegisterOutputType
        (
            new OutputType
            ("censer_y", "Censer Y", "Number",
                o =>
                {
                    var fsm = o.LocateMyFSM("Control");
                    var censer = fsm.FsmVariables.FindFsmGameObject("Censer Throw");
                    return censer.value ? censer.value.transform.GetPositionY() : 0f;
                }
            )
        )
    ]);
}
