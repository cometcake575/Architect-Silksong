using Architect.Events.Blocks.Config;

namespace Architect.Events.Blocks.Operators;

public static class OperatorBlocks
{
    public static void Init()
    {
        BoolVarBlock.Init();
        NumVarBlock.Init();
        
        ScriptManager.RegisterProcessBlock<IfBlock>("If");
        ScriptManager.RegisterProcessBlock<CounterBlock>("Counter", ConfigGroup.Counter);
        ScriptManager.RegisterProcessBlock<ConstantNumBlock>("Constant Num", ConfigGroup.ConstantNum);
        ScriptManager.RegisterProcessBlock<ConstantBoolBlock>("Constant Bool", ConfigGroup.ConstantBool);
        ScriptManager.RegisterProcessBlock<BoolVarBlock>("Variable (Bool)", ConfigGroup.BoolVar);
        ScriptManager.RegisterProcessBlock<NumVarBlock>("Variable (Number)", ConfigGroup.NumVar);
        ScriptManager.RegisterProcessBlock<RandomNumBlock>("Random Number", ConfigGroup.RandomNumber);
        ScriptManager.RegisterProcessBlock<RandomBoolBlock>("Random Bool");
        ScriptManager.RegisterProcessBlock<CompareBlock>("Compare", ConfigGroup.Compare);
        ScriptManager.RegisterProcessBlock<MathsBlock>("Operation", ConfigGroup.Maths);
        ScriptManager.RegisterProcessBlock<AndBlock>("And");
        ScriptManager.RegisterProcessBlock<OrBlock>("Or");
        ScriptManager.RegisterProcessBlock<NotBlock>("Not");
        ScriptManager.RegisterProcessBlock<DelayBlock>("Delay", ConfigGroup.Delay);
        ScriptManager.RegisterProcessBlock<PlayerDataBoolBlock>("PlayerData (Bool)", ConfigGroup.PdBool);
        ScriptManager.RegisterProcessBlock<PlayerDataIntBlock>("PlayerData (Int)", ConfigGroup.PdInt);
        ScriptManager.RegisterProcessBlock<PlayerDataFloatBlock>("PlayerData (Float)", ConfigGroup.PdFloat);
    }
}