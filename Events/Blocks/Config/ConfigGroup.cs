using System;
using System.Collections.Generic;
using Architect.Events.Blocks.Config.Types;
using Architect.Events.Blocks.Events;
using Architect.Events.Blocks.Operators;
using Architect.Events.Blocks.Outputs;
using UnityEngine;

namespace Architect.Events.Blocks.Config;

public static class ConfigGroup
{
    public static readonly List<ConfigType> ConstantNum =
    [
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<ConstantNumBlock>("Number", "constant_num", 
                (b, f) => b.Value = f.GetValue())
        )
    ];
    
    public static readonly List<ConfigType> Counter =
    [
        ConfigurationManager.RegisterConfigType(
            new IntConfigType<CounterBlock>("Target", "counter_target", 
                (b, f) => b.Count = f.GetValue())
        )
    ];
    
    public static readonly List<ConfigType> BoolVar =
    [
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<BoolVarBlock>("Variable ID", "var_id_bool", 
                (b, f) => b.Id = f.GetValue())
        ),
        ConfigurationManager.RegisterConfigType(
            new ChoiceConfigType<BoolVarBlock>("Persistence", "var_persistence_bool", 
                (b, f) => b.PType = f.GetValue())
                .WithOptions("None", "Bench", "Global").WithDefaultValue(2)
        )
    ];
    
    public static readonly List<ConfigType> NumVar =
    [
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<NumVarBlock>("Variable ID", "var_id_num", 
                (b, f) => b.Id = f.GetValue())
        ),
        ConfigurationManager.RegisterConfigType(
            new ChoiceConfigType<NumVarBlock>("Persistence", "var_persistence_num", 
                (b, f) => b.PType = f.GetValue())
                .WithOptions("None", "Bench", "Global").WithDefaultValue(2)
        )
    ];

    public static readonly List<ConfigType> TimeSlower = [
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<TimeSlowerBlock>("Time Scale", "time_scale", (o, value) =>
            {
                o.TargetSpeed = value.GetValue();
            }).WithDefaultValue(0.25f)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<TimeSlowerBlock>("Change Time", "time_change", (o, value) =>
            {
                o.ChangeTime = value.GetValue();
            }).WithDefaultValue(0.1f)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<TimeSlowerBlock>("Wait Time", "time_wait", (o, value) =>
            {
                o.WaitTime = value.GetValue();
            }).WithDefaultValue(1)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<TimeSlowerBlock>("Return Time", "time_return", (o, value) =>
            {
                o.ReturnTime = value.GetValue();
            }).WithDefaultValue(0.75f)),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType<TimeSlowerBlock>("Disable Pause", "time_prevent_pausing", (o, value) =>
            {
                o.NoPause = value.GetValue();
            }).WithDefaultValue(true))
    ];
    
    public static readonly List<ConfigType> AnimPlayer =
    [
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<AnimatorBlock>("Clip Name", "anim_clip", (o, value) =>
            {
                o.ClipName = value.GetValue();
            })),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<AnimatorBlock>("Time Override", "anim_duration", (o, value) =>
            {
                o.OverrideAnimTime = true;
                o.AnimTime = value.GetValue();
            })),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType<AnimatorBlock>("Take Control", "anim_take_ctrl", (o, value) =>
            {
                o.TakeCtrl = value.GetValue();
            }).WithDefaultValue(true))
    ];
    
    public static readonly List<ConfigType> MultiplayerOut =
    [
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<MultiplayerOutBlock>("Event ID", "multi_out_name", (o, value) =>
            {
                o.EventName = value.GetValue();
            }))
    ];
    
    public static readonly List<ConfigType> MultiplayerIn =
    [
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<MultiplayerInBlock>("Event ID", "multi_in_name", (o, value) =>
            {
                o.EventName = value.GetValue();
            }))
    ];
    
    public static readonly List<ConfigType> RandomNumber =
    [
        ConfigurationManager.RegisterConfigType(
            new IntConfigType<RandomNumBlock>("Lower Bound", "random_lower", 
                (b, f) => b.LowerBound = f.GetValue())
        ),
        ConfigurationManager.RegisterConfigType(
            new IntConfigType<RandomNumBlock>("Upper Bound", "random_upper", 
                (b, f) => b.UpperBound = f.GetValue())
        ),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType<RandomNumBlock>("Whole Num", "random_whole", 
                (b, f) => b.WholeNumber = f.GetValue())
                .WithDefaultValue(false)
        )
    ];
    
    public static readonly List<ConfigType> CameraShaker =
    [
        ConfigurationManager.RegisterConfigType(
            new ChoiceConfigType<ShakeCameraBlock>("Shake Type", "camera_shake_type", 
                (b, f) =>
                {
                    b.ShakeType = f.GetValue();
                })
                .WithOptions("Tiny", "Small", "Medium", "Large").WithDefaultValue(2)
        )
    ];
    
    public static readonly List<ConfigType> HealthHook =
    [
        ConfigurationManager.RegisterConfigType(
            new IntConfigType<HpBlock>("Amount", "health_amount", 
                (b, f) =>
                {
                    b.Amount = f.GetValue();
                }).WithDefaultValue(1)
        )
    ];
    
    public static readonly List<ConfigType> SilkHook =
    [
        ConfigurationManager.RegisterConfigType(
            new IntConfigType<SilkBlock>("Amount", "silk_amount", 
                (b, f) =>
                {
                    b.Amount = f.GetValue();
                }).WithDefaultValue(1)
        )
    ];

    public static readonly List<ConfigType> TextDisplay =
    [
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<TextBlock>("Text", "display_text", (o, value) =>
            {
                o.Text = value.GetValue();
            }).WithDefaultValue("Sample Text")),
        ConfigurationManager.RegisterConfigType(
            new ChoiceConfigType<TextBlock>("Vertical", "display_align_v", (o, value) =>
            {
                o.VerticalAlignment = value.GetValue();
            }).WithOptions("Top", "Middle", "Bottom").WithDefaultValue(0)),
        ConfigurationManager.RegisterConfigType(
            new ChoiceConfigType<TextBlock>("Horizontal", "display_align_h", (o, value) =>
            {
                o.HorizontalAlignment = value.GetValue();
            }).WithOptions("Left", "Middle", "Right").WithDefaultValue(0)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<TextBlock>("Y Offset", "display_y_offset", (o, value) =>
            {
                o.OffsetY = value.GetValue();
            }).WithDefaultValue(0)),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType<TextBlock>("Decorators", "display_decor", (o, value) =>
            {
                o.Decorators = value.GetValue();
            }).WithDefaultValue(true))
    ];

    public static readonly List<ConfigType> TitleDisplay =
    [
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<TitleBlock>("Header", "title_header", (o, value) =>
            {
                o.Header = value.GetValue();
            })),
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<TitleBlock>("Body", "title_body", (o, value) =>
            {
                o.Body = value.GetValue();
            }).WithDefaultValue("Sample Text")),
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<TitleBlock>("Footer", "title_footer", (o, value) =>
            {
                o.Footer = value.GetValue();
            })),
        ConfigurationManager.RegisterConfigType(
            new ChoiceConfigType<TitleBlock>("Mode", "title_type", (o, value) =>
            {
                o.TitleType = value.GetValue();
            }).WithOptions("Large", "Left", "Right").WithDefaultValue(0))
    ];

    public static readonly List<ConfigType> ChoiceDisplay = [
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<ChoiceBlock>("Text", "choice_text", (o, value) =>
            {
                o.Text = value.GetValue();
            }).WithDefaultValue("Sample Text")),
        ConfigurationManager.RegisterConfigType(
            new ChoiceConfigType<ChoiceBlock>("Requirement", "choice_requirement", (o, value) =>
            {
                var val = value.GetValue();
                switch (val)
                {
                    case 0:
                        o.Cost = 0;
                        break;
                    case 3:
                        o.UseItem = true;
                        break;
                }
                o.CurrencyType = val == 1 ? CurrencyType.Money : CurrencyType.Shard;
            }).WithOptions("None", "Rosaries", "Shell Shards", "Item").WithDefaultValue(0)),
        ConfigurationManager.RegisterConfigType(
            new IntConfigType<ChoiceBlock>("Cost Amount", "choice_cost", (o, value) =>
            {
                o.Cost = value.GetValue();
            }).WithDefaultValue(0)),
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<ChoiceBlock>("Item ID", "choice_item_id", (o, value) =>
            {
                o.Item = value.GetValue();
            }).WithDefaultValue("Simple Key")),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType<ChoiceBlock>("Consume Item", "choice_take_item", (o, value) =>
            {
                o.TakeItem = value.GetValue();
            }).WithDefaultValue(true))
    ];
    
    public static readonly List<ConfigType> ConstantBool =
    [
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType<ConstantBoolBlock>("Bool", "constant_bool", 
                (b, f) => b.Value = f.GetValue())
        )
    ];
    
    public static readonly List<ConfigType> PdBool =
    [
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<PlayerDataBoolBlock>("Data ID", "pd_bool_type", 
                (b, f) => b.Data = f.GetValue())
        ),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType<PlayerDataBoolBlock>("Value", "pd_bool", 
                (b, f) => b.Value = f.GetValue())
        )
    ];
    
    public static readonly List<ConfigType> PdInt =
    [
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<PlayerDataIntBlock>("Data ID", "pd_int_type", 
                (b, f) => b.Data = f.GetValue())
        ),
        ConfigurationManager.RegisterConfigType(
            new IntConfigType<PlayerDataIntBlock>("Value", "pd_int", 
                (b, f) => b.Value = f.GetValue())
        )
    ];
    
    public static readonly List<ConfigType> PdFloat =
    [
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<PlayerDataFloatBlock>("Data ID", "pd_float_type", 
                (b, f) => b.Data = f.GetValue())
        ),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<PlayerDataFloatBlock>("Value", "pd_float", 
                (b, f) => b.Value = f.GetValue())
        )
    ];
    
    public static readonly List<ConfigType> Delay =
    [
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<DelayBlock>("Delay", "delay_num", 
                (b, f) => b.Delay = f.GetValue())
        )
    ];
    
    public static readonly List<ConfigType> Compare =
    [
        ConfigurationManager.RegisterConfigType(
            new ChoiceConfigType<CompareBlock>("Mode", "compare_mode", 
                (b, f) => b.Mode = f.GetValue())
                .WithOptions("=", ">", "<", ">=", "<=").WithDefaultValue(0)
        )
    ];
    
    public static readonly List<ConfigType> Maths =
    [
        ConfigurationManager.RegisterConfigType(
            new ChoiceConfigType<MathsBlock>("Mode", "maths_mode", 
                (b, f) => b.Mode = f.GetValue())
                .WithOptions("+", "–", "*", "/", "//", "%").WithDefaultValue(0)
        )
    ];
    
    public static readonly List<ConfigType> KeyListener =
    [
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<KeyBlock>("Key", "key_listener_key", 
                (o, value) =>
                {
                    if (!Enum.TryParse<KeyCode>(value.GetValue(), true, out var key)) return;
                    o.Key = key;
                })
        )
    ];
    
    public static readonly List<ConfigType> Timer =  [
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<TimerBlock>("Start Delay", "timer_start_delay", (o, value) =>
            {
                o.StartDelay = value.GetValue();
            }).WithDefaultValue(0)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<TimerBlock>("Repeat Delay", "timer_repeat_delay", (o, value) =>
            {
                o.RepeatDelay = value.GetValue();
            }).WithDefaultValue(1)),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<TimerBlock>("Random Delay", "timer_rand_delay", (o, value) =>
            {
                o.RandDelay = value.GetValue();
            }).WithDefaultValue(0)),
        ConfigurationManager.RegisterConfigType(
            new IntConfigType<TimerBlock>("Max Calls", "timer_limit", (o, value) =>
            {
                o.MaxCalls = value.GetValue();
            }))
    ];
}