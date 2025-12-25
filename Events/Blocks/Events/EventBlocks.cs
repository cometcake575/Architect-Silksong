using Architect.Events.Blocks.Config;

namespace Architect.Events.Blocks.Events;

public static class EventBlocks
{
    public static void Init()
    {
        ScriptManager.RegisterOutputBlock<StartBlock>("Start");
        ScriptManager.RegisterOutputBlock<TimerBlock>("Timer", ConfigGroup.Timer);
        ScriptManager.RegisterOutputBlock<KeyBlock>("Key Listener", ConfigGroup.KeyListener);
        ScriptManager.RegisterOutputBlock<PlayerBlock>("Player Listener");
        ScriptManager.RegisterOutputBlock<MultiplayerOutBlock>("Multiplayer Receive", ConfigGroup.MultiplayerOut);
    }
}