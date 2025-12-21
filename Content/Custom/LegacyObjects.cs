using Architect.Behaviour.Custom;
using Architect.Behaviour.Utility;
using Architect.Objects.Categories;
using Architect.Objects.Groups;
using Architect.Objects.Placeable;
using Architect.Utils;
using UnityEngine;

namespace Architect.Content.Custom;

public static class LegacyObjects
{
    public static void Init()
    {
        Categories.Legacy.Add(CreateTimer());
        Categories.Legacy.Add(CreateKeyListener());
        Categories.Legacy.Add(CreateRelay());
        Categories.Legacy.Add(CreateTimeSlower());
        Categories.Legacy.Add(CreateAnimatorController());
        Categories.Legacy.Add(CreateTitleDisplay());
        Categories.Legacy.Add(CreateTextDisplay());
        Categories.Legacy.Add(CreateChoiceDisplay());
        Categories.Legacy.Add(CreatePlayerHook());
        Categories.Legacy.Add(CreatePlayerDataSetter());
        Categories.Legacy.Add(CreateCameraShaker());
    }

    private static PlaceableObject CreateTimer()
    {
        var timer = new GameObject("Timer");
        timer.SetActive(false);
        Object.DontDestroyOnLoad(timer);
        
        timer.AddComponent<Timer>();

        return new CustomObject("Timer", "timer",
                timer,
                sprite: ResourceUtils.LoadSpriteResource("timer", FilterMode.Point, ppu:10),
                description: "[LEGACY]" +
                             "\n\nBroadcasts an event periodically.")
            .WithConfigGroup(ConfigGroup.Timer)
            .WithBroadcasterGroup(BroadcasterGroup.Callable)
            .WithReceiverGroup(ReceiverGroup.Generic);
    }

    private static PlaceableObject CreateTitleDisplay()
    {
        var display = new GameObject("Title Display");
        display.SetActive(false);
        Object.DontDestroyOnLoad(display);
        
        display.AddComponent<TitleDisplay>();

        return new CustomObject("Title Display", "title_display",
                display,
                sprite: ResourceUtils.LoadSpriteResource("title_display", FilterMode.Point, ppu:64),
                description: "[LEGACY]\n\n" +
                             "Used to display a title to the player, such as area or boss titles.")
            .WithReceiverGroup(ReceiverGroup.Displayable)
            .WithConfigGroup(ConfigGroup.TitleDisplay);
    }

    private static PlaceableObject CreateTextDisplay()
    {
        var display = new GameObject("Text Display");
        display.SetActive(false);
        Object.DontDestroyOnLoad(display);
        
        display.AddComponent<TextDisplay>();

        return new CustomObject("Text Display", "text_display",
                display,
                sprite: ResourceUtils.LoadSpriteResource("text_display", FilterMode.Point, ppu:10),
                description: "[LEGACY]" +
                             "\n\nDisplays a piece of text.")
            .WithReceiverGroup(ReceiverGroup.Displayable)
            .WithConfigGroup(ConfigGroup.TextDisplay)
            .WithBroadcasterGroup(BroadcasterGroup.TextDisplay);
    }

    private static PlaceableObject CreateChoiceDisplay()
    {
        var display = new GameObject("Choice Display");
        display.SetActive(false);
        Object.DontDestroyOnLoad(display);
        
        display.AddComponent<ChoiceDisplay>();

        return new CustomObject("Choice Display", "choice_display",
                display,
                sprite: ResourceUtils.LoadSpriteResource("choice_display", FilterMode.Point, ppu:10),
                description: "[LEGACY]\n\n" +
                             "Displays a piece of text and prompts the player to choose Yes or No.")
            .WithReceiverGroup(ReceiverGroup.Displayable)
            .WithBroadcasterGroup(BroadcasterGroup.Choice)
            .WithConfigGroup(ConfigGroup.Choice);
    }

    private static PlaceableObject CreatePlayerHook()
    {
        var playerHook = new GameObject("Player Hook");
        playerHook.SetActive(false);
        Object.DontDestroyOnLoad(playerHook);
        
        PlayerHook.Init();

        playerHook.AddComponent<PlayerHook>();

        return new CustomObject("Player Hook", "player_hook", playerHook,
                description:"[LEGACY]\n\n" +
                            "Can detect certain inputs or actions from the player such as jumping or landing,\n" +
                            "and perform certain triggers such as damaging or killing the player.",
                sprite:ResourceUtils.LoadSpriteResource("player_listener", FilterMode.Point, ppu:64))
            .WithBroadcasterGroup(BroadcasterGroup.PlayerHooks)
            .WithReceiverGroup(ReceiverGroup.PlayerHooks)
            .WithConfigGroup(ConfigGroup.Generic);
    }

    private static PlaceableObject CreatePlayerDataSetter()
    {
        var dataSetter = new GameObject("PlayerData Setter");
        dataSetter.SetActive(false);
        Object.DontDestroyOnLoad(dataSetter);
        
        dataSetter.AddComponent<PlayerDataSetter>();

        return new CustomObject("PlayerData Hook", "player_data_setter", dataSetter,
                description:"[LEGACY]\n\n" +
                            "Sets or checks a PlayerData boolean value.",
                sprite:ResourceUtils.LoadSpriteResource("player_data_changer", FilterMode.Point, ppu:64))
            .WithReceiverGroup(ReceiverGroup.PlayerDataSetter)
            .WithConfigGroup(ConfigGroup.PlayerDataSetter)
            .WithBroadcasterGroup(BroadcasterGroup.Callable);
    }

    private static PlaceableObject CreateKeyListener()
    {
        var keyListener = new GameObject("Key Listener");

        keyListener.AddComponent<KeyListener>();

        keyListener.SetActive(false);
        Object.DontDestroyOnLoad(keyListener);

        return new CustomObject("Key Listener", "key_listener", keyListener,
            description:"[LEGACY]\n\n" +
                        "Can listen and broadcast events when keys are pressed and released.",
            sprite:ResourceUtils.LoadSpriteResource("key_listener", FilterMode.Point, ppu:10))
            .WithConfigGroup(ConfigGroup.KeyListener)
            .WithBroadcasterGroup(BroadcasterGroup.KeyListener);
    }

    private static PlaceableObject CreateRelay()
    {
        var relay = new GameObject("Relay");

        relay.AddComponent<Relay>();

        relay.SetActive(false);
        Object.DontDestroyOnLoad(relay);

        return new CustomObject("Relay", "relay",
                relay,
                sprite: ResourceUtils.LoadSpriteResource("relay", FilterMode.Point, ppu:10),
                description: "[LEGACY]\n\n" +
                             "Broadcasts the OnCall event when the Call trigger is run.")
            .WithConfigGroup(ConfigGroup.Relay)
            .WithBroadcasterGroup(BroadcasterGroup.Callable)
            .WithReceiverGroup(ReceiverGroup.Relay);
    }

    private static PlaceableObject CreateCameraShaker()
    {
        var cameraSpinner = new GameObject("Camera Shaker");
        cameraSpinner.AddComponent<CameraShaker>();
        
        cameraSpinner.transform.position = new Vector3(0, 0, 0.1f);

        Object.DontDestroyOnLoad(cameraSpinner);
        cameraSpinner.SetActive(false);
        return new CustomObject("Camera Shaker", "camera_shaker", cameraSpinner,
                description:"[LEGACY]\n\n" +
                            "Shakes the camera when the 'Shake' trigger is run.",
                sprite:ResourceUtils.LoadSpriteResource("camera_shaker"))
            .WithReceiverGroup(ReceiverGroup.CameraShaker)
            .WithConfigGroup(ConfigGroup.CameraShaker);
    }

    private static PlaceableObject CreateAnimatorController()
    {
        var animCtrl = new GameObject("Animator Controller");
        
        AnimPlayer.Init();

        Object.DontDestroyOnLoad(animCtrl);
        animCtrl.SetActive(false);

        animCtrl.AddComponent<AnimPlayer>();
        
        return new CustomObject("Animation Player", "anim_player", animCtrl,
                description:"[LEGACY]\n\n" +
                            "Makes Hornet perform a vanilla animation when the 'Play' trigger is called.",
                sprite:ResourceUtils.LoadSpriteResource("anim_ctrl", ppu:33))
            .WithReceiverGroup(ReceiverGroup.AnimPlayer)
            .WithConfigGroup(ConfigGroup.AnimPlayer)
            .WithBroadcasterGroup(BroadcasterGroup.Finishable);
    }

    private static PlaceableObject CreateTimeSlower()
    {
        TimeSlower.Init();
        
        var timeMng = new GameObject("Time Slower");
        
        Object.DontDestroyOnLoad(timeMng);
        timeMng.SetActive(false);

        timeMng.AddComponent<TimeSlower>();
        
        return new CustomObject("Time Slower", "time_manager", timeMng,
                description:"[LEGACY]\n\n" +
                            "Temporarily slows the game's speed when the 'SlowTime' trigger is run.",
                sprite:ResourceUtils.LoadSpriteResource("time_slower", ppu:33))
            .WithReceiverGroup(ReceiverGroup.TimeSlower)
            .WithConfigGroup(ConfigGroup.TimeSlower)
            .WithBroadcasterGroup(BroadcasterGroup.Finishable);
    }
}