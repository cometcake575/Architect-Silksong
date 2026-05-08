using Architect.Events.Blocks.Config;

namespace Architect.Events.Blocks.Operators;

public static class OperatorBlocks
{
    public static void Init()
    {
        // If
        Category.Logic.RegisterBlock<IfBlock>("If");
        
        // Comparisons
        Category.Logic.RegisterBlock<CompareBlock>("Compare", ConfigGroup.Compare);
        Category.Logic.RegisterBlock<StringCompareBlock>("Compare Text", ConfigGroup.StringCompare);
        
        // Ordered/Random events
        Category.Logic.RegisterBlock<OrderBlock>("Ordered Events");
        Category.Logic.RegisterHiddenBlock<OrderBlock.TriggerBlock>("Order Trigger");
        
        Category.Logic.RegisterBlock<RandomEventBlock>("Random Event");
        Category.Logic.RegisterHiddenBlock<RandomEventBlock.TriggerBlock>("Random Trigger", ConfigGroup.RandomTrigger);
        
        // Counters
        Category.Logic.RegisterBlock<CounterBlock>("Counter", ConfigGroup.Counter);
        
        Category.Logic.RegisterBlock<OneByOneBlock>("Step Counter");
        Category.Logic.RegisterHiddenBlock<OneByOneBlock.TriggerBlock>("Step Trigger");
        
        // Constants
        Category.Data.RegisterBlock<ConstantNumBlock>("Constant Num", ConfigGroup.ConstantNum);
        Category.Data.RegisterBlock<ConstantBoolBlock>("Constant Bool", ConfigGroup.ConstantBool);
        Category.Data.RegisterBlock<ConstantTextBlock>("Constant Text", ConfigGroup.ConstantText);
        
        // Variables
        Category.Data.RegisterBlock<BoolVarBlock>("Variable (Bool)", ConfigGroup.BoolVar, BoolVarBlock.Init);
        Category.Data.RegisterBlock<NumVarBlock>("Variable (Number)", ConfigGroup.NumVar, NumVarBlock.Init);
        Category.Data.RegisterBlock<StringVarBlock>("Variable (Text)", ConfigGroup.StringVar, StringVarBlock.Init);
        
        // Convert
        Category.Data.RegisterBlock<ConvertBlock>("Convert");
        
        // Random
        Category.Data.RegisterBlock<RandomNumBlock>("Random Number", ConfigGroup.RandomNumber);
        Category.Data.RegisterBlock<RandomBoolBlock>("Random Bool");
        Category.Data.RegisterBlock<RandomTextBlock>("Random Text", ConfigGroup.RandomText);
        
        Category.Data.RegisterBlock<MathsBlock>("Operation", ConfigGroup.Maths);
        Category.Data.RegisterBlock<TrigBlock>("Trig Operation", ConfigGroup.Trig);
        Category.Data.RegisterBlock<NormaliseBlock>("Normalise", ConfigGroup.Normalise);
        Category.World.RegisterBlock<RaycastBlock>("Raycast", ConfigGroup.Raycast);
        
        Category.World.RegisterHiddenBlock<SceneNameBlock>("Scene Name");
        Category.World.RegisterHiddenBlock<GameplayBlock>("Gameplay");
        
        Category.Logic.RegisterBlock<AndBlock>("And");
        Category.Logic.RegisterBlock<OrBlock>("Or");
        Category.Logic.RegisterBlock<NotBlock>("Not");
        Category.Time.RegisterBlock<DelayBlock>("Delay", ConfigGroup.Delay);
        Category.Time.RegisterBlock<WaitUntilBlock>("Wait Until");
        Category.Time.RegisterBlock<LoopBlock>("Loop", ConfigGroup.Loop);
        Category.Time.RegisterBlock<TimeBlock>("Time", ConfigGroup.Time);
        Category.Time.RegisterBlock<DayBlock>("Day", ConfigGroup.Time);
        Category.Data.RegisterBlock<PersistentBoolBlock>("Persistent Data", ConfigGroup.Pd);
        Category.Data.RegisterBlock<PlayerDataBoolBlock>("PlayerData (Bool)", ConfigGroup.PdBool, PlayerDataBoolBlock.Init);
        Category.Data.RegisterBlock<PlayerDataIntBlock>("PlayerData (Int)", ConfigGroup.PdInt);
        Category.Data.RegisterBlock<PlayerDataFloatBlock>("PlayerData (Float)", ConfigGroup.PdFloat);
        Category.World.RegisterBlock<CustomNeedleBlock>("Custom Needle", ConfigGroup.CustomNeedle);
    }
}