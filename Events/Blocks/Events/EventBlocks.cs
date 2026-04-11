using Architect.Events.Blocks.Config;
using Architect.Events.Blocks.Outputs;

namespace Architect.Events.Blocks.Events;

public static class EventBlocks
{
    public static void Init()
    {
        Category.Events.RegisterBlock<StartBlock>("Start");
        Category.Events.RegisterBlock<TimerBlock>("Timer", ConfigGroup.Timer);
        Category.Events.RegisterBlock<EveryFrameBlock>("Every Frame");
        Category.Events.RegisterBlock<KeyBlock>("Key Listener", ConfigGroup.KeyListener);
        Category.Events.RegisterBlock<ReceiveBlock>("Receive", ConfigGroup.Receive);
        Category.Events.RegisterBlock<BroadcastBlock>("Broadcast", ConfigGroup.Broadcast);
        Category.Events.RegisterBlock<MultiplayerOutBlock>("Multiplayer Receive", ConfigGroup.MultiplayerOut);
        Category.Events.RegisterBlock<MultiplayerInBlock>("Multiplayer Event", ConfigGroup.MultiplayerIn);
        
        Category.World.RegisterHiddenBlock<PlayerBlock>("Player Listener");
        
        Category.World.RegisterBlock<StateBlock>("Player State");
        Category.World.RegisterBlock<ActionBlock>("Player Movement");
        Category.World.RegisterBlock<AttackBlock>("Player Attacks");
        Category.World.RegisterBlock<NeedolinBlock>("Needolin Control", ConfigGroup.Needolin, NeedolinBlock.Init);
    }
}