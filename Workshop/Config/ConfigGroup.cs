using System;
using System.Collections.Generic;
using Architect.Workshop.Items;
using Architect.Workshop.Types;

namespace Architect.Workshop.Config;

public static class ConfigGroup
{
    public static readonly List<ConfigType> SpriteItem =
    [
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<SpriteItem>("Icon URL", "png_url", (item, value) =>
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
    
    public static readonly List<ConfigType> JournalEntry =
    [
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<CustomJournalEntry>("Enemy Name", "journal_display_name", (item, value) =>
            {
                item.ItemName = value.GetValue();
            }).WithDefaultValue("Sample Name")
        ),
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<CustomJournalEntry>("Description", "journal_desc", (item, value) =>
            {
                item.ItemDesc = value.GetValue();
            }).WithDefaultValue("Sample Description")
        ),
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<CustomJournalEntry>("Hunter's Notes", "journal_hdesc", (item, value) =>
            {
                item.ItemHDesc = value.GetValue();
            }).WithDefaultValue("Sample Hunter's Notes")
        ),
        ConfigurationManager.RegisterConfigType(
            new IntConfigType<CustomJournalEntry>("Required Kills", "journal_kills", (item, value) =>
            {
                item.KillsRequired = value.GetValue();
            }).WithDefaultValue(1)
        ),
        (NoteConfigType) "A vanilla entry (list can be found in the guide)",
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<CustomJournalEntry>("Add Before", "journal_before", (item, value) =>
            {
                item.InsertBefore = value.GetValue();
            })
        )
    ];
    
    public static readonly List<ConfigType> MateriumEntry =
    [
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<CustomMateriumEntry>("Item Name", "materium_display_name", (item, value) =>
            {
                item.ItemName = value.GetValue();
            }).WithDefaultValue("Sample Name")
        ),
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<CustomMateriumEntry>("Description", "materium_desc", (item, value) =>
            {
                item.Desc = value.GetValue();
            }).WithDefaultValue("Sample Description")
        ),
        (NoteConfigType) "A vanilla entry (list can be found in the guide)",
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<CustomMateriumEntry>("Add Before", "materium_before", (item, value) =>
            {
                item.InsertBefore = value.GetValue();
            })
        )
    ];
    
    public static readonly List<ConfigType> JournalEntrySprites =
    [
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<CustomJournalEntry>("Enemy Image URL", "png_journal_url", (item, value) =>
            {
                item.LIconUrl = value.GetValue();
            })
        ),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType<CustomJournalEntry>("Anti Aliasing", "png_journal_antialias", (item, value) =>
            {
                item.LPoint = !value.GetValue();
            }).WithDefaultValue(true)
        ),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<CustomJournalEntry>("Pixels Per Unit", "png_journal_ppu", (item, value) =>
            {
                item.LPpu = value.GetValue();
            }).WithDefaultValue(100)
        )
    ];
    
    public static readonly List<ConfigType> MapIcon =
    [
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<CustomMapIcon>("Custom Scene ID", "map_icon_scene", (item, value) =>
            {
                item.Scene = value.GetValue();
            })
        ),
        (NoteConfigType) "The icon is unlocked when this global variable is true (if set)",
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<CustomMapIcon>("Required Variable", "map_icon_reqvar", (item, value) =>
            {
                item.ReqVar = value.GetValue();
            })
        ),
        ConfigurationManager.RegisterConfigType(
            new ChoiceConfigType<CustomMapIcon>("Visibility Mode", "map_icon_visual_mode", (item, value) =>
            {
                item.Mode = value.GetValue();
            }).WithOptions("Both", "Quick Map", "Inventory").WithDefaultValue(0)
        ),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<CustomMapIcon>("Offset X", "map_icon_x", (item, value) =>
            {
                item.Pos.x = value.GetValue();
            }).WithDefaultValue(0)
        ),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<CustomMapIcon>("Offset Y", "map_icon_y", (item, value) =>
            {
                item.Pos.y = value.GetValue();
            }).WithDefaultValue(0)
        )
    ];
    
    public static readonly List<ConfigType> MapIconLabel =
    [
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<CustomMapIcon>("Text", "map_icon_text", (item, value) =>
            {
                item.Text = value.GetValue();
            })
        ),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<CustomMapIcon>("Font Size", "map_icon_font_size", (item, value) =>
            {
                item.FontSize = value.GetValue();
            }).WithDefaultValue(6.2f)
        ),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<CustomMapIcon>("Text Offset X", "map_icon_text_x", (item, value) =>
            {
                item.Offset.x = value.GetValue();
            }).WithDefaultValue(0)
        ),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<CustomMapIcon>("Text Offset Y", "map_icon_text_y", (item, value) =>
            {
                item.Offset.y = value.GetValue();
            }).WithDefaultValue(0)
        ),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<CustomMapIcon>("Text Colour R", "map_icon_text_r", (item, value) =>
            {
                item.Colour.r = value.GetValue();
            }).WithDefaultValue(1)
        ),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<CustomMapIcon>("Text Colour G", "map_icon_text_g", (item, value) =>
            {
                item.Colour.g = value.GetValue();
            }).WithDefaultValue(1)
        ),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<CustomMapIcon>("Text Colour B", "map_icon_text_b", (item, value) =>
            {
                item.Colour.b = value.GetValue();
            }).WithDefaultValue(1)
        )
    ];
    
    public static readonly List<ConfigType> UsableItem =
    [
        (NoteConfigType) "Only apply to Usable items",
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
        ),
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
        ),
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
                item.ItemDesc = value.GetValue().Replace("<br>", "\n");
            }).WithDefaultValue("Sample Description")
        ),
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<CustomItem>("Use Description", "item_udesc", (item, value) =>
            {
                item.UseDesc = value.GetValue().Replace("<br>", "\n");
            }).WithDefaultValue("Sample Use Description")
        ),
        ConfigurationManager.RegisterConfigType(
            new ChoiceConfigType<CustomItem>("Item Type", "item_consume", (item, value) =>
            {
                item.ItemType = Enum.Parse<CustomItem.CustomItemType>(value.GetStringValue());
            }).WithOptions("Normal", "Usable", "Memento").WithDefaultValue(0)
        ),
        ConfigurationManager.RegisterConfigType(
            new IntConfigType<CustomItem>("Max Amount", "item_max", (item, value) =>
            {
                item.MaxAmount = value.GetValue();
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
                item.ItemDesc = value.GetValue().Replace("<br>", "\n");
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
            new IntConfigType<CustomScene>("Scene Width", "scene_tilemap_width", (item, value) =>
            {
                item.TilemapWidth = value.GetValue();
            }).WithDefaultValue(500)
        ),
        ConfigurationManager.RegisterConfigType(
            new IntConfigType<CustomScene>("Scene Height", "scene_tilemap_height", (item, value) =>
            {
                item.TilemapHeight = value.GetValue();
            }).WithDefaultValue(500)
        )
    ];
    
    public static readonly List<ConfigType> SceneMap = [
        (NoteConfigType) "Only applies if the scene's group has a map",
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<CustomScene>("Map X Offset", "scene_map_x", (item, value) =>
            {
                item.MapPos.x = value.GetValue();
            }).WithDefaultValue(0)
        ),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<CustomScene>("Map Y Offset", "scene_map_y", (item, value) =>
            {
                item.MapPos.y = value.GetValue();
            }).WithDefaultValue(0)
        ),
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<SpriteItem>("Full Map URL", "scene_full_map_url", (item, value) =>
            {
                item.IconUrl = value.GetValue();
            })
        ),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType<SpriteItem>("Anti Aliasing", "scene_full_map_antialias", (item, value) =>
            {
                item.Point = !value.GetValue();
            }).WithDefaultValue(true)
        ),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<SpriteItem>("Pixels Per Unit", "scene_full_map_ppu", (item, value) =>
            {
                item.Ppu = value.GetValue();
            }).WithDefaultValue(100)
        ),
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<CustomScene>("Empty Room URL", "scene_empty_map_url", (item, value) =>
            {
                item.EIconUrl = value.GetValue();
            })
        ),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType<CustomScene>("Anti Aliasing", "scene_empty_map_antialias", (item, value) =>
            {
                item.EPoint = !value.GetValue();
            }).WithDefaultValue(true)
        ),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<CustomScene>("Pixels Per Unit", "scene_empty_map_ppu", (item, value) =>
            {
                item.EPpu = value.GetValue();
            }).WithDefaultValue(100)
        )
    ];
    
    public static readonly List<ConfigType> SceneMapColour = [
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType<CustomScene>("Override Colour", "scene_override_col", (item, value) =>
            {
                item.OverrideColour = value.GetValue();
            }).WithDefaultValue(false)
        ),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<CustomScene>("Colour R", "scene_override_col_r", (item, value) =>
            {
                item.MapColour.r = value.GetValue();
            }).WithDefaultValue(1)
        ),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<CustomScene>("Colour G", "scene_override_col_g", (item, value) =>
            {
                item.MapColour.g = value.GetValue();
            }).WithDefaultValue(1)
        ),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<CustomScene>("Colour B", "scene_override_col_b", (item, value) =>
            {
                item.MapColour.b = value.GetValue();
            }).WithDefaultValue(1)
        )
    ];
    
    public static readonly List<ConfigType> SceneGroup = [
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<SceneGroup>("Area Name", "scene_group_name", (item, value) =>
            {
                item.GroupName = value.GetValue();
            })
        )
    ];
    
    public static readonly List<ConfigType> SceneGroupIcon = [
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
        ),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType<SceneGroup>("Hide Void Overlay", "scene_group_no_void", (item, value) =>
            {
                item.DisableAct3Bg = value.GetValue();
            }).WithDefaultValue(false)
        )
    ];
    
    public static readonly List<ConfigType> SceneGroupMap = [
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType<SceneGroup>("Has Map", "scene_group_map", (item, value) =>
            {
                item.HasMapZone = value.GetValue();
            }).WithDefaultValue(false)
        ),
        (NoteConfigType) "The map is unlocked when this global variable is true (if set)",
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<SceneGroup>("Required Variable", "scene_group_map_var", (item, value) =>
            {
                item.Variable = value.GetValue();
            })
        ),
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<SceneGroup>("Map Icon URL", "scene_group_map_url", (item, value) =>
            {
                item.MapUrl = value.GetValue();
            })
        ),
        ConfigurationManager.RegisterConfigType(
            new BoolConfigType<SceneGroup>("Anti Aliasing", "scene_group_map_antialias", (item, value) =>
            {
                item.MPoint = !value.GetValue();
            }).WithDefaultValue(true)
        ),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<SceneGroup>("Pixels Per Unit", "scene_group_map_ppu", (item, value) =>
            {
                item.MPpu = value.GetValue();
            }).WithDefaultValue(100)
        ),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<SceneGroup>("Map Colour R", "scene_group_map_cr", (item, value) =>
            {
                item.MapColour.r = value.GetValue();
            }).WithDefaultValue(1)
        ),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<SceneGroup>("Map Colour G", "scene_group_map_cg", (item, value) =>
            {
                item.MapColour.g = value.GetValue();
            }).WithDefaultValue(1)
        ),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<SceneGroup>("Map Colour B", "scene_group_map_cb", (item, value) =>
            {
                item.MapColour.b = value.GetValue();
            }).WithDefaultValue(1)
        )
    ];
    
    public static readonly List<ConfigType> SceneGroupMapPos = [
        (NoteConfigType) "The position of the map, label and zoomed in map",
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<SceneGroup>("Map X", "scene_group_map_px", (item, value) =>
            {
                item.MapPos.x = value.GetValue();
            }).WithDefaultValue(0)
        ),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<SceneGroup>("Map Y", "scene_group_map_py", (item, value) =>
            {
                item.MapPos.y = value.GetValue();
            }).WithDefaultValue(0)
        ),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<SceneGroup>("Title Offset X", "scene_group_map_lx", (item, value) =>
            {
                item.LabelPos.x = value.GetValue();
            }).WithDefaultValue(0)
        ),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<SceneGroup>("Title Offset Y", "scene_group_map_ly", (item, value) =>
            {
                item.LabelPos.y = value.GetValue();
            }).WithDefaultValue(0)
        ),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<SceneGroup>("Zoom X", "scene_group_map_zx", (item, value) =>
            {
                item.ZoomPos.x = value.GetValue();
            }).WithDefaultValue(0)
        ),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<SceneGroup>("Zoom Y", "scene_group_map_zy", (item, value) =>
            {
                item.ZoomPos.y = value.GetValue();
            }).WithDefaultValue(0)
        ),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<SceneGroup>("Area Label X", "scene_group_map_alx", (item, value) =>
            {
                item.AreaNamePos.x = value.GetValue();
            }).WithDefaultValue(0)
        ),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<SceneGroup>("Area Label Y", "scene_group_map_aly", (item, value) =>
            {
                item.AreaNamePos.y = value.GetValue();
            }).WithDefaultValue(0)
        ),
        ConfigurationManager.RegisterConfigType(
            new FloatConfigType<SceneGroup>("Area Radius", "scene_group_map_radius", (item, value) =>
            {
                item.Radius = value.GetValue();
            }).WithDefaultValue(0.2f)
        )
    ];
    
    public static readonly List<ConfigType> SceneGroupMapDirIn = [
        (NoteConfigType) "Make other maps lead to this one",
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<SceneGroup>("Enter Below", "scene_group_map_b", (item, value) =>
            {
                item.OverwriteEnter[(int)InventoryItemManager.SelectionDirection.Down] = value.GetValue();
            })
        ),
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<SceneGroup>("Enter Above", "scene_group_map_a", (item, value) =>
            {
                item.OverwriteEnter[(int)InventoryItemManager.SelectionDirection.Up] = value.GetValue();
            })
        ),
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<SceneGroup>("Enter Left", "scene_group_map_l", (item, value) =>
            {
                item.OverwriteEnter[(int)InventoryItemManager.SelectionDirection.Left] = value.GetValue();
            })
        ),
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<SceneGroup>("Enter Right", "scene_group_map_r", (item, value) =>
            {
                item.OverwriteEnter[(int)InventoryItemManager.SelectionDirection.Right] = value.GetValue();
            })
        )
    ];
    
    public static readonly List<ConfigType> SceneGroupMapDirOut = [
        (NoteConfigType) "Make this map lead to others",
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<SceneGroup>("Exit Below", "scene_group_map_b2", (item, value) =>
            {
                item.OverwriteExit[(int)InventoryItemManager.SelectionDirection.Down] = value.GetValue();
            })
        ),
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<SceneGroup>("Exit Above", "scene_group_map_a2", (item, value) =>
            {
                item.OverwriteExit[(int)InventoryItemManager.SelectionDirection.Up] = value.GetValue();
            })
        ),
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<SceneGroup>("Exit Left", "scene_group_map_l2", (item, value) =>
            {
                item.OverwriteExit[(int)InventoryItemManager.SelectionDirection.Left] = value.GetValue();
            })
        ),
        ConfigurationManager.RegisterConfigType(
            new StringConfigType<SceneGroup>("Exit Right", "scene_group_map_r2", (item, value) =>
            {
                item.OverwriteExit[(int)InventoryItemManager.SelectionDirection.Right] = value.GetValue();
            })
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
            new StringConfigType<CustomQuest>("Board Description", "quest_board_desc", (item, value) =>
            {
                item.WallDesc = value.GetValue();
            }).WithDefaultValue("Sample Board Description")
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
        (NoteConfigType)"Only apply to Red and Skill tools",
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
        (NoteConfigType) "Only apply to Red tools",
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