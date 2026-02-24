using Architect.Events.Blocks.Config;

namespace Architect.Events.Blocks.Operators;

public static class OperatorBlocks
{
    public static void Init()
    {
        BoolVarBlock.Init();
        NumVarBlock.Init();
        StringVarBlock.Init();
        
        ScriptManager.RegisterProcessBlock<CounterBlock>("Counter", ConfigGroup.Counter);
        ScriptManager.RegisterProcessBlock<IfBlock>("If");
        
        ScriptManager.RegisterProcessBlock<OrderBlock>("Ordered Events");
        ScriptManager.RegisterHiddenBlock<OrderBlock.TriggerBlock>("Order Trigger");
        
        ScriptManager.RegisterProcessBlock<RandomEventBlock>("Random Event");
        ScriptManager.RegisterHiddenBlock<RandomEventBlock.TriggerBlock>("Random Trigger", ConfigGroup.RandomTrigger);
        
        ScriptManager.RegisterProcessBlock<CompareBlock>("Compare", ConfigGroup.Compare);
        ScriptManager.RegisterProcessBlock<StringCompareBlock>("Compare Text", ConfigGroup.StringCompare);
        ScriptManager.RegisterProcessBlock<ConstantNumBlock>("Constant Num", ConfigGroup.ConstantNum);
        ScriptManager.RegisterProcessBlock<ConstantBoolBlock>("Constant Bool", ConfigGroup.ConstantBool);
        ScriptManager.RegisterProcessBlock<ConstantTextBlock>("Constant Text", ConfigGroup.ConstantText);
        ScriptManager.RegisterProcessBlock<BoolVarBlock>("Variable (Bool)", ConfigGroup.BoolVar);
        ScriptManager.RegisterProcessBlock<NumVarBlock>("Variable (Number)", ConfigGroup.NumVar);
        ScriptManager.RegisterProcessBlock<StringVarBlock>("Variable (Text)", ConfigGroup.StringVar);
        ScriptManager.RegisterProcessBlock<ConvertBlock>("Convert");
        ScriptManager.RegisterProcessBlock<RandomNumBlock>("Random Number", ConfigGroup.RandomNumber);
        ScriptManager.RegisterProcessBlock<RandomBoolBlock>("Random Bool");
        ScriptManager.RegisterProcessBlock<MathsBlock>("Operation", ConfigGroup.Maths);
        ScriptManager.RegisterProcessBlock<TrigBlock>("Trig Operation", ConfigGroup.Trig);
        ScriptManager.RegisterProcessBlock<NormaliseBlock>("Normalise", ConfigGroup.Normalise);
        ScriptManager.RegisterProcessBlock<RaycastBlock>("Raycast", ConfigGroup.Raycast);
        ScriptManager.RegisterProcessBlock<SceneNameBlock>("Scene Name");
        ScriptManager.RegisterProcessBlock<AndBlock>("And");
        ScriptManager.RegisterProcessBlock<OrBlock>("Or");
        ScriptManager.RegisterProcessBlock<NotBlock>("Not");
        ScriptManager.RegisterProcessBlock<DelayBlock>("Delay", ConfigGroup.Delay);
        ScriptManager.RegisterProcessBlock<WaitUntilBlock>("Wait Until");
        ScriptManager.RegisterProcessBlock<LoopBlock>("Loop", ConfigGroup.Loop);
        ScriptManager.RegisterProcessBlock<TimeBlock>("Time", ConfigGroup.Time);
        ScriptManager.RegisterProcessBlock<DayBlock>("Day", ConfigGroup.Time);
        ScriptManager.RegisterProcessBlock<PersistentBoolBlock>("Persistent Data", ConfigGroup.Pd);
        ScriptManager.RegisterProcessBlock<PlayerDataBoolBlock>("PlayerData (Bool)", ConfigGroup.PdBool);
        ScriptManager.RegisterProcessBlock<PlayerDataIntBlock>("PlayerData (Int)", ConfigGroup.PdInt);
        ScriptManager.RegisterProcessBlock<PlayerDataFloatBlock>("PlayerData (Float)", ConfigGroup.PdFloat);
    }
}