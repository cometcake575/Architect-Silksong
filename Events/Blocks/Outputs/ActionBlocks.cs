using Architect.Events.Blocks.Config;
using Architect.Events.Blocks.Objects;

namespace Architect.Events.Blocks.Outputs;

public static class ActionBlocks
{
    public static void Init()
    {
        Category.World.RegisterBlock<HpBlock>("Health Control", ConfigGroup.HealthHook);
        Category.World.RegisterBlock<InvulBlock>("Invulnerable Control", ConfigGroup.InvulHook, InvulBlock.Init);
        Category.World.RegisterBlock<SilkBlock>("Silk Control", ConfigGroup.SilkHook, SilkBlock.Init);
        Category.World.RegisterBlock<CurrencyBlock>("Currency Control", ConfigGroup.CurrencyHook);
        Category.World.RegisterBlock<StatusBlock>("Status Control");
        Category.World.RegisterBlock<QuestBlock>("Quest Control", ConfigGroup.QuestControl);
        Category.World.RegisterBlock<JournalEntryBlock>("Journal Control", ConfigGroup.EntryControl);
        Category.World.RegisterBlock<AchievementBlock>("Achievement Control", ConfigGroup.AchievementControl);
        Category.World.RegisterBlock<ToolBlock>("Tool Control", ConfigGroup.ToolControl);
        Category.World.RegisterBlock<CrestBlock>("Crest Control", ConfigGroup.CrestControl, CrestBlock.Init);
        Category.World.RegisterBlock<ItemBlock>("Item Control", ConfigGroup.ItemControl);
        Category.World.RegisterBlock<EnemyBlock>("Enemy Control", ConfigGroup.EnemyControl);
        Category.Visual.RegisterBlock<TextBlock>("Text Display", ConfigGroup.TextDisplay);
        Category.Visual.RegisterBlock<ChoiceBlock>("Choice Display", ConfigGroup.ChoiceDisplay);
        // ScriptManager.RegisterBlock<InputBlock>("Input Display", ConfigGroup.InputDisplay);
        Category.Visual.RegisterBlock<TitleBlock>("Title Display", ConfigGroup.TitleDisplay);
        Category.Visual.RegisterBlock<SongBlock>("Song Display", ConfigGroup.SongDisplay);
        Category.Visual.RegisterBlock<PngBlock>("Custom PNG", ConfigGroup.Png);
        Category.Visual.RegisterBlock<PowerupGetBlock>("Powerup Display", ConfigGroup.PowerupDisplay, PowerupGetBlock.Init);
        Category.Visual.RegisterBlock<ShakeCameraBlock>("Camera Shake", ConfigGroup.CameraShaker);
        Category.Visual.RegisterBlock<UIBlock>("UI Control", ConfigGroup.UIControl);
        Category.Visual.RegisterBlock<TimeSlowerBlock>("Time Slowdown", ConfigGroup.TimeSlower, TimeSlowerBlock.Init);
        Category.Visual.RegisterBlock<AnimatorBlock>("Animator Controller", ConfigGroup.AnimPlayer);
        Category.World.RegisterBlock<TransitionBlock>("Transition", ConfigGroup.Transition);
        Category.Visual.RegisterBlock<SetLightingBlock>("Set Lighting", ConfigGroup.Lighting, SetLightingBlock.Init);
        Category.World.RegisterBlock<SpawnObjectBlock>("Spawn Object", ConfigGroup.SpawnObject);
        Category.World.RegisterBlock<SpawnPrefabBlock>("Spawn Prefab", ConfigGroup.Prefab);
        Category.World.RegisterBlock<ObjectMoverBlock>("Move Object");
        //ScriptManager.RegisterBlock<EndingBlock>("Ending Control");
        
        Category.Visual.RegisterBlock<TravelBlock>("Travel UI", ConfigGroup.TravelUI, TravelBlock.Init);
        Category.Visual.RegisterHiddenBlock<TravelBlock.TravelLoc>("Travel Target", ConfigGroup.TravelUITarget);
        
        Category.Visual.RegisterBlock<ShopBlock>("Shop", init: ShopBlock.Init);
        Category.Visual.RegisterHiddenBlock<ShopBlock.ShopItemBlock>("Shop Item", ConfigGroup.ShopItem);
        
        /*Category.Visual.RegisterBlock<CollectionViewBlock>("Collection View", init: CollectionViewBlock.Init);
        Category.Visual.RegisterHiddenBlock<CollectionViewBlock.CollectionItemBlock>("Collection Item", ConfigGroup.CollectionItem);*/
        
        Category.Visual.RegisterBlock<QuestboardBlock>("Quest Board", init: QuestboardBlock.Init);
        Category.Visual.RegisterHiddenBlock<QuestboardBlock.QuestBlock>("Quest Item", ConfigGroup.QuestItem);
        
        LegacyTravelBlock.Init();
        Category.Visual.RegisterHiddenBlock<LegacyTravelBlock>("Travel List", ConfigGroup.TravelList);
        Category.Visual.RegisterHiddenBlock<TravelLoc>("Travel", ConfigGroup.Travel);
    }
}