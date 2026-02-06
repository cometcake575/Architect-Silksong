using System.Collections.Generic;
using Architect.Workshop.Items;
using Architect.Workshop.Types;

namespace Architect.Workshop.Config;

public static class ConfigGroup
{
    public static readonly List<ConfigType> SpriteItem =
    [
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<SpriteItem>("Texture URL", "png_url", (item, value) =>
            {
                item.IconUrl = value.GetValue();
            })
        ),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType<SpriteItem>("Anti Aliasing", "png_antialias", (item, value) =>
            {
                item.Point = !value.GetValue();
            }).WithDefaultValue(true)
        ),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<SpriteItem>("Pixels Per Unit", "png_ppu", (item, value) =>
            {
                item.Ppu = value.GetValue();
            }).WithDefaultValue(100)
        )
    ];
    
    public static readonly List<ConfigType> ItemUsing =
    [
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<CustomItem>("Using Audio URL", "wav_url1", (item, value) =>
            {
                item.WavURL1 = value.GetValue();
            })
        ),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<CustomItem>("Volume", "wav_volume1", (item, value) =>
            {
                item.Volume1 = value.GetValue();
            }).WithDefaultValue(1)
        ),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<CustomItem>("Min Pitch", "wav_min_pitch1", (item, value) =>
            {
                item.MinPitch1 = value.GetValue();
            }).WithDefaultValue(0.8f)
        ),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<CustomItem>("Max Pitch", "wav_max_pitch1", (item, value) =>
            {
                item.MaxPitch1 = value.GetValue();
            }).WithDefaultValue(1.2f)
        )
    ];
    
    public static readonly List<ConfigType> ItemUse =
    [
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<CustomItem>("Used Audio URL", "wav_url2", (item, value) =>
            {
                item.WavURL2 = value.GetValue();
            })
        ),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<CustomItem>("Volume", "wav_volume2", (item, value) =>
            {
                item.Volume2 = value.GetValue();
            }).WithDefaultValue(1)
        ),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<CustomItem>("Min Pitch", "wav_min_pitch2", (item, value) =>
            {
                item.MinPitch2 = value.GetValue();
            }).WithDefaultValue(0.8f)
        ),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<CustomItem>("Max Pitch", "wav_max_pitch2", (item, value) =>
            {
                item.MaxPitch2 = value.GetValue();
            }).WithDefaultValue(1.2f)
        )
    ];
    
    public static readonly List<ConfigType> CustomItem = [
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<CustomItem>("Item Name", "item_display_name", (item, value) =>
            {
                item.ItemName = value.GetValue();
            }).WithDefaultValue("Sample Name")
        ),
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<CustomItem>("Description", "item_desc", (item, value) =>
            {
                item.ItemDesc = value.GetValue();
            }).WithDefaultValue("Sample Description")
        ),
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<CustomItem>("Use Description", "item_udesc", (item, value) =>
            {
                item.UseDesc = value.GetValue();
            }).WithDefaultValue("Sample Use Description")
        ),
        ConfigurationManager.RegisterConfigType(
            new IntConfigType<CustomItem>("Max Amount", "item_max", (item, value) =>
            {
                item.MaxAmount = value.GetValue();
            })
        ),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType<CustomItem>("Usable", "item_consume", (item, value) =>
            {
                item.CanUse = value.GetValue();
            }).WithDefaultValue(false)
        ),
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<CustomItem>("Use Action", "item_consume_desc", (item, value) =>
            {
                item.UseType = value.GetValue();
            }).WithDefaultValue("Break")
        ),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType<CustomItem>("Consume on Use", "item_consume_use", (item, value) =>
            {
                item.Consume = value.GetValue();
            }).WithDefaultValue(true)
        ),
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<CustomItem>("Use Event", "item_onconsume", (item, value) =>
            {
                item.UseEvent = value.GetValue();
            })
        )
    ];
    
    public static readonly List<ConfigType> CustomTool = [
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<CustomTool>("Tool Name", "tool_name", (item, value) =>
            {
                item.ItemName = value.GetValue();
            }).WithDefaultValue("Sample Name")
        ),
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<CustomTool>("Description", "tool_desc", (item, value) =>
            {
                item.ItemDesc = value.GetValue();
            }).WithDefaultValue("Sample Description")
        ),
        ConfigurationManager.RegisterConfigType(
            new ChoiceConfigType<CustomTool>("Type", "tool_type", (item, value) =>
            {
                item.ItemType = (ToolItemType)value.GetValue();
            }).WithDefaultValue(0).WithOptions("Red", "Blue", "Yellow", "Skill")
        )
    ];
    
    public static readonly List<ConfigType> Scene = [
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<CustomScene>("Group ID", "scene_group", (item, value) =>
            {
                item.Group = value.GetValue();
            }).WithDefaultValue("None")
        ),
        ConfigurationManager.RegisterConfigType(
            new IntConfigType<CustomScene>("Tilemap Width", "scene_tilemap_width", (item, value) =>
            {
                item.TilemapWidth = value.GetValue();
            }).WithDefaultValue(500)
        ),
        ConfigurationManager.RegisterConfigType(
            new IntConfigType<CustomScene>("Tilemap Height", "scene_tilemap_height", (item, value) =>
            {
                item.TilemapHeight = value.GetValue();
            }).WithDefaultValue(500)
        )
    ];
    
    public static readonly List<ConfigType> SceneGroup = [
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<SceneGroup>("Area Name", "scene_group_name", (item, value) =>
            {
                item.GroupName = value.GetValue();
            })
        ),
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<SpriteItem>("Save Icon URL", "scene_group_url", (item, value) =>
            {
                item.IconUrl = value.GetValue();
            })
        ),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType<SpriteItem>("Anti Aliasing", "scene_group_antialias", (item, value) =>
            {
                item.Point = !value.GetValue();
            }).WithDefaultValue(true)
        )
    ];
    
    public static readonly List<ConfigType> Quest = [
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<CustomQuest>("Quest Name", "quest_display_name", (item, value) =>
            {
                item.ItemName = value.GetValue();
            }).WithDefaultValue("Sample Name")
        ),
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<CustomQuest>("Description", "quest_desc", (item, value) =>
            {
                item.ItemDesc = value.GetValue();
            }).WithDefaultValue("Sample Description")
        ),
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<CustomQuest>("Quest Category", "quest_display_type", (item, value) =>
            {
                item.TypeName = value.GetValue();
            }).WithDefaultValue("Seek")
        ),
        ConfigurationManager.RegisterConfigType(
            new ChoiceConfigType<CustomQuest>("Quest Type", "quest_type", (item, value) =>
            {
                item.MainQuest = value.GetValue() == 1;
            }).WithDefaultValue(0).WithOptions("Regular", "Major")
        ),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<CustomQuest>("Colour R", "quest_r", (item, value) =>
            {
                item.Color.r = value.GetValue();
            }).WithDefaultValue(1)
        ),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<CustomQuest>("Colour G", "quest_g", (item, value) =>
            {
                item.Color.g = value.GetValue();
            }).WithDefaultValue(1)
        ),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<CustomQuest>("Colour B", "quest_b", (item, value) =>
            {
                item.Color.b = value.GetValue();
            }).WithDefaultValue(1)
        )
    ];
    
    public static readonly List<ConfigType> UseToolSprites =
    [
        ConfigurationManager.RegisterConfigType(
            new NoteConfigType<CustomTool>("Only apply to Red and Skill tools", "tool_sprite_type_info")
        ),
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<CustomTool>("HUD Icon URL", "png_tool_ui_url", (item, value) =>
            {
                item.HIconUrl = value.GetValue();
            })
        ),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType<CustomTool>("Anti Aliasing", "png_tool_ui_antialias", (item, value) =>
            {
                item.HPoint = !value.GetValue();
            }).WithDefaultValue(true)
        ),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<CustomTool>("Pixels Per Unit", "png_tool_ui_ppu", (item, value) =>
            {
                item.HPpu = value.GetValue();
            }).WithDefaultValue(100)
        ),
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<CustomTool>("HUD Usable URL", "png_tool_glow_url", (item, value) =>
            {
                item.GIconUrl = value.GetValue();
            })
        ),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType<CustomTool>("Anti Aliasing", "png_tool_glow_antialias", (item, value) =>
            {
                item.GPoint = !value.GetValue();
            }).WithDefaultValue(true)
        ),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<CustomTool>("Pixels Per Unit", "png_tool_glow_ppu", (item, value) =>
            {
                item.GPpu = value.GetValue();
            }).WithDefaultValue(100)
        )
    ];
    
    public static readonly List<ConfigType> RedTools =
    [
        ConfigurationManager.RegisterConfigType(
            new NoteConfigType<CustomTool>("Only apply to Red tools", "tool_red_info")
        ),
        ConfigurationManager.RegisterConfigType(
            new IntConfigType<CustomTool>("Max Amount", "tool_red_max", (item, value) =>
            {
                item.MaxAmount = value.GetValue();
            }).WithDefaultValue(10)
        ),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType<CustomTool>("Affected by Pouches", "tool_red_pouch", (item, value) =>
            {
                item.PreventIncrease = !value.GetValue();
            }).WithDefaultValue(true)
        ),
        ConfigurationManager.RegisterConfigType(
            new IntConfigType<CustomTool>("Repair Cost", "tool_red_repair_cost", (item, value) =>
            {
                item.RepairCost = value.GetValue();
            }).WithDefaultValue(5)
        )
    ];
    
    public static readonly List<ConfigType> QuestSprites =
    [
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<CustomQuest>("Icon URL", "png_quest_url", (item, value) =>
            {
                item.IconUrl = value.GetValue();
            })
        ),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType<CustomQuest>("Anti Aliasing", "png_quest_antialias", (item, value) =>
            {
                item.Point = !value.GetValue();
            }).WithDefaultValue(true)
        ),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<CustomQuest>("Pixels Per Unit", "png_quest_ppu", (item, value) =>
            {
                item.Ppu = value.GetValue();
            }).WithDefaultValue(100)
        ),
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<CustomQuest>("Prompt Icon URL", "png_lquest_url", (item, value) =>
            {
                item.LIconUrl = value.GetValue();
            })
        ),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType<CustomQuest>("Anti Aliasing", "png_lquest_antialias", (item, value) =>
            {
                item.LPoint = !value.GetValue();
            }).WithDefaultValue(true)
        ),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<CustomQuest>("Pixels Per Unit", "png_lquest_ppu", (item, value) =>
            {
                item.LPpu = value.GetValue();
            }).WithDefaultValue(100)
        ),
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<CustomQuest>("Glow Icon URL", "png_gquest_url", (item, value) =>
            {
                item.GIconUrl = value.GetValue();
            })
        ),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType<CustomQuest>("Anti Aliasing", "png_gquest_antialias", (item, value) =>
            {
                item.GPoint = !value.GetValue();
            }).WithDefaultValue(true)
        ),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<CustomQuest>("Pixels Per Unit", "png_gquest_ppu", (item, value) =>
            {
                item.GPpu = value.GetValue();
            }).WithDefaultValue(100)
        )
    ];
}