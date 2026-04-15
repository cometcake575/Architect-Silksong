using Architect.Events.Blocks.Config;

namespace Architect.Events.Blocks.Functions;

public static class FunctionManager
{
    public static void Init()
    {
        Category.Data.RegisterHiddenBlock<FunctionDefinitionBlock>("New Function", ConfigGroup.Functions);
    }
}