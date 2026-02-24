using Architect.Events.Blocks.Config;
using Architect.Events.Blocks.Objects;

namespace Architect.Events.Blocks.Outputs;

public static class ActionBlocks
{
    public static void Init()
    {
        TimeSlowerBlock.Init();
        PowerupGetBlock.Init();
        LegacyTravelBlock.Init();
        TravelBlock.Init();
        ShopBlock.Init();
        QuestboardBlock.Init();
        SetLightingBlock.Init();
        
        ScriptManager.RegisterInputBlock<HpBlock>("Health Control", ConfigGroup.HealthHook);
        ScriptManager.RegisterInputBlock<SilkBlock>("Silk Control", ConfigGroup.SilkHook);
        ScriptManager.RegisterInputBlock<CurrencyBlock>("Currency Control", ConfigGroup.CurrencyHook);
        ScriptManager.RegisterInputBlock<StatusBlock>("Status Control");
        ScriptManager.RegisterInputBlock<QuestBlock>("Quest Control", ConfigGroup.QuestControl);
        ScriptManager.RegisterInputBlock<JournalEntryBlock>("Journal Control", ConfigGroup.EntryControl);
        ScriptManager.RegisterInputBlock<ToolBlock>("Tool Control", ConfigGroup.ToolControl);
        ScriptManager.RegisterInputBlock<ItemBlock>("Item Control", ConfigGroup.ItemControl);
        ScriptManager.RegisterInputBlock<EnemyBlock>("Enemy Control", ConfigGroup.EnemyControl);
        ScriptManager.RegisterInputBlock<TextBlock>("Text Display", ConfigGroup.TextDisplay);
        ScriptManager.RegisterInputBlock<ChoiceBlock>("Choice Display", ConfigGroup.ChoiceDisplay);
        // ScriptManager.RegisterInputBlock<InputBlock>("Input Display", ConfigGroup.InputDisplay);
        ScriptManager.RegisterInputBlock<TitleBlock>("Title Display", ConfigGroup.TitleDisplay);
        ScriptManager.RegisterInputBlock<NeedolinBlock>("Song Display", ConfigGroup.SongDisplay);
        ScriptManager.RegisterProcessBlock<PngBlock>("Custom PNG", ConfigGroup.Png);
        ScriptManager.RegisterInputBlock<PowerupGetBlock>("Powerup Display", ConfigGroup.PowerupDisplay);
        ScriptManager.RegisterInputBlock<ShakeCameraBlock>("Camera Shake", ConfigGroup.CameraShaker);
        ScriptManager.RegisterInputBlock<UIBlock>("UI Control", ConfigGroup.UIControl);
        ScriptManager.RegisterInputBlock<TimeSlowerBlock>("Time Slowdown", ConfigGroup.TimeSlower);
        ScriptManager.RegisterInputBlock<AnimatorBlock>("Animator Controller", ConfigGroup.AnimPlayer);
        ScriptManager.RegisterInputBlock<BroadcastBlock>("Broadcast", ConfigGroup.Broadcast);
        ScriptManager.RegisterInputBlock<MultiplayerInBlock>("Multiplayer Event", ConfigGroup.MultiplayerIn);
        ScriptManager.RegisterInputBlock<TransitionBlock>("Transition", ConfigGroup.Transition);
        ScriptManager.RegisterInputBlock<SetLightingBlock>("Set Lighting", ConfigGroup.Lighting);
        ScriptManager.RegisterInputBlock<SpawnPrefabBlock>("Spawn Prefab", ConfigGroup.Prefab);
        
        ScriptManager.RegisterInputBlock<TravelBlock>("Travel UI", ConfigGroup.TravelUI);
        ScriptManager.RegisterHiddenBlock<TravelBlock.TravelLoc>("Travel Target", ConfigGroup.TravelUITarget);
        
        ScriptManager.RegisterInputBlock<ShopBlock>("Shop");
        ScriptManager.RegisterHiddenBlock<ShopBlock.ShopItemBlock>("Shop Item", ConfigGroup.ShopItem);
        
        ScriptManager.RegisterInputBlock<QuestboardBlock>("Quest Board");
        ScriptManager.RegisterHiddenBlock<QuestboardBlock.QuestBlock>("Quest Item", ConfigGroup.QuestItem);
        
        ScriptManager.RegisterHiddenBlock<LegacyTravelBlock>("Travel List", ConfigGroup.TravelList);
        ScriptManager.RegisterHiddenBlock<TravelLoc>("Travel", ConfigGroup.Travel);
    }
}