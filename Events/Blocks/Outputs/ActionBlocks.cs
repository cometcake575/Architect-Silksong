using Architect.Events.Blocks.Config;

namespace Architect.Events.Blocks.Outputs;

public static class ActionBlocks
{
    public static void Init()
    {
        TimeSlowerBlock.Init();
        
        ScriptManager.RegisterInputBlock<HpBlock>("Health Control", ConfigGroup.HealthHook);
        ScriptManager.RegisterInputBlock<SilkBlock>("Silk Control", ConfigGroup.SilkHook);
        ScriptManager.RegisterInputBlock<StatusBlock>("Status Control");
        ScriptManager.RegisterInputBlock<TextBlock>("Text Display", ConfigGroup.TextDisplay);
        ScriptManager.RegisterInputBlock<ChoiceBlock>("Choice Display", ConfigGroup.ChoiceDisplay);
        ScriptManager.RegisterInputBlock<TitleBlock>("Title Display", ConfigGroup.TitleDisplay);
        ScriptManager.RegisterInputBlock<PowerupGetBlock>("Powerup Display", ConfigGroup.PowerupDisplay);
        ScriptManager.RegisterInputBlock<ShakeCameraBlock>("Camera Shake", ConfigGroup.CameraShaker);
        ScriptManager.RegisterInputBlock<TimeSlowerBlock>("Time Slowdown", ConfigGroup.TimeSlower);
        ScriptManager.RegisterInputBlock<AnimatorBlock>("Animator Controller", ConfigGroup.AnimPlayer);
        ScriptManager.RegisterInputBlock<BroadcastBlock>("Broadcast", ConfigGroup.Broadcast);
        ScriptManager.RegisterInputBlock<MultiplayerInBlock>("Multiplayer Event", ConfigGroup.MultiplayerIn);
    }
}