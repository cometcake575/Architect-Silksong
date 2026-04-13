namespace Architect.Events.Blocks.Functions;

public static class FunctionManager
{
    public static void Init()
    {
        Category.Functions.RegisterBlock<FunctionDefinitionBlock>("New Function");
    }
}