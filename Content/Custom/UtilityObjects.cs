using System;
using System.Collections.Generic;
using System.Linq;
using Architect.Behaviour.Custom;
using Architect.Behaviour.Utility;
using Architect.Objects.Categories;
using Architect.Objects.Groups;
using Architect.Objects.Placeable;
using Architect.Utils;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Architect.Content.Custom;

public static class UtilityObjects
{
    private static readonly Dictionary<string, Func<GameObject, Disabler[]>> RemoverActions = [];
    
    public static void Init()
    {
        Categories.Utility.Add(CreateObjectAnchor());
        Categories.Utility.Add(CreateObjectSpinner());
        Categories.Utility.Add(CreateObjectSpawner());
        Categories.Utility.Add(CreateTriggerZone());
        Categories.Utility.Add(CreateInteraction());
        Categories.Utility.Add(CreateTimer());
        Categories.Utility.Add(CreateKeyListener());
        Categories.Utility.Add(CreateRelay());
        
        Categories.Utility.Add(CreateTextDisplay());
        Categories.Utility.Add(CreateChoiceDisplay());
        
        Categories.Utility.Add(CreatePlayerHook());
        Categories.Utility.Add(CreateEnemyBarrier());
        Categories.Utility.Add(CreateObjectRemover("enemy_remover", "Remove Enemy", 
                FindObjectsToDisable<HealthManager>, "Removes the nearest enemy.\n\n" +
                                                     "This should be placed at the enemy's spawn point, not its\n" +
                                                     "current position, or it will not work when exiting edit mode.")
            .WithConfigGroup(ConfigGroup.Generic)
            .WithReceiverGroup(ReceiverGroup.Generic));
        
        Categories.Utility.Add(CreateTeleportPoint());
        
        Categories.Utility.Add(CreateHazardRespawnPoint());
        Categories.Utility.Add(CreateObjectRemover("hrp_remover", "Remove Hazard Respawn Point",
                FindObjectsToDisable<HazardRespawnTrigger>, "Removes the nearest Hazard Respawn Point.")
            .WithConfigGroup(ConfigGroup.Generic)
            .WithReceiverGroup(ReceiverGroup.Generic));
        
        Categories.Utility.Add(CreateTransitionPoint());
        Categories.Utility.Add(CreateObjectRemover("door_remover", "Remove Transition", 
                FindObjectsToDisable<TransitionPoint>, "Removes the nearest door to another room.")
            .WithConfigGroup(ConfigGroup.Generic)
            .WithReceiverGroup(ReceiverGroup.Generic));
        
        Categories.Utility.Add(CreateBinoculars());
        Categories.Utility.Add(CreateCameraBorder());
        Categories.Utility.Add(CreateSceneBorderRemover());
        
        Categories.Utility.Add(CreateObjectRemover("collision_remover", "Remove Collider",
                disabler =>
                {
                    List<Collider2D> results1 = [];
                    List<Collider2D> results2 = [];
                    Physics2D.OverlapArea(
                        disabler.transform.position - Vector3.one,
                        disabler.transform.position + Vector3.one,
                        new ContactFilter2D(),
                        results1
                    );
                    Physics2D.OverlapPoint(
                        disabler.transform.position,
                        new ContactFilter2D(),
                        results2
                    );
                    return results1.Concat(results2)
                        .Select(p => p.gameObject.GetOrAddComponent<Disabler>()).ToArray();
                }, "Removes colliders touching this object.\n" +
                   "Trigger zones such as hazard respawn points do not count.")
            .WithConfigGroup(ConfigGroup.Generic)
            .WithReceiverGroup(ReceiverGroup.Generic));
        
        Categories.Utility.Add(CreateObjectRemover("render_remover", "Remove Renderer", 
                FindObjectsToDisable<Renderer>, "Removes the nearest renderer.")
            .WithConfigGroup(ConfigGroup.Generic)
            .WithReceiverGroup(ReceiverGroup.Generic));
        
        Categories.Utility.Add(CreateObjectRemover("object_remover", "Remove Object", o =>
            {
                var config = o.GetComponent<ObjectRemoverConfig>();
                GameObject point = null;

                if (config)
                    try
                    {
                        point = ObjectUtils.GetGameObjectFromArray(o.scene.GetRootGameObjects(), config.objectPath);
                    }
                    catch (ArgumentException) { }

                return point is not null ? [point.GetOrAddComponent<Disabler>()] : [];
            }, "Removes any vanilla objects from the game.\n\n" +
               "The path to an object can be found with tools such as Unity Explorer.")
            .WithConfigGroup(ConfigGroup.ObjectRemover)
            .WithReceiverGroup(ReceiverGroup.Generic));
        
        Categories.Utility.Add(CreateObjectRemover("room_remover", "Clear Room", o =>
                {
                    var clearer = o.GetComponent<RoomClearerConfig>();

                    if (!clearer) return [];

                    var objects = o.scene.GetRootGameObjects().Where(obj =>
                        !obj.name.StartsWith("[Architect]")
                        && !obj.name.StartsWith("_SceneManager")
                    );
            
                    if (clearer.removeOther)
                    {
                        if (!clearer.removeBenches) objects = objects.Where(obj => !obj.GetComponent<RestBench>());
                        if (!clearer.removeBlur) objects = objects.Where(obj => !obj.GetComponentInChildren<BlurPlane>());
                        if (!clearer.removeTransitions)
                            objects = objects.Where(obj => !obj.GetComponentInChildren<TransitionPoint>());
                        if (!clearer.removeMusic) objects = objects.Where(obj => !obj.GetComponent<MusicRegion>());
                    }
                    else
                    {
                        objects = objects.Where(obj =>
                            (obj.GetComponent<RestBench>() && clearer.removeBenches) ||
                            (obj.GetComponentInChildren<BlurPlane>() && clearer.removeBlur) ||
                            (obj.GetComponentInChildren<TransitionPoint>() && clearer.removeTransitions) ||
                            (obj.GetComponent<MusicRegion>() && clearer.removeMusic)
                        );
                    }

                    return objects.Select(obj => obj.GetOrAddComponent<Disabler>()).ToArray();
                }, "Removes all existing objects in a room,\n" +
                   "settings determine which objects are removed.")
            .WithConfigGroup(ConfigGroup.RoomClearer)
            .WithReceiverGroup(ReceiverGroup.Generic));
    }

    private static PlaceableObject CreateTransitionPoint()
    {
        var customDoor = new GameObject("Transition Point");

        Object.DontDestroyOnLoad(customDoor);
        customDoor.SetActive(false);

        CustomTransitionPoint.Init();

        customDoor.AddComponent<CustomTransitionPoint>();
        var point = customDoor.AddComponent<TransitionPoint>();
        point.nonHazardGate = true;

        var col = customDoor.AddComponent<BoxCollider2D>();
        col.size = new Vector2(3, 3);
        col.isTrigger = true;

        customDoor.layer = LayerMask.NameToLayer("TransitionGates");
        
        return new CustomObject("Transition Gate", "transition_gate", customDoor,
                description:"Creates a custom doorway to another room. Requires a target scene and door id.\n\n" +
                            "Disable 'Collision Trigger' and use the 'Transition' trigger to transition when\n" +
                            "an event is received instead of when touching the door.",
                sprite:ResourceUtils.LoadSpriteResource("door", ppu:33),
                preview:true)
            .WithConfigGroup(ConfigGroup.Transitions)
            .WithReceiverGroup(ReceiverGroup.Transitions);
    }

    private static PlaceableObject CreateCameraBorder()
    {
        CameraBorder.Init();

        var cameraBorder = new GameObject("Camera Border");
        cameraBorder.AddComponent<CameraBorder>();

        cameraBorder.layer = 10;
        cameraBorder.transform.position = new Vector3(0, 0, 0.1f);

        Object.DontDestroyOnLoad(cameraBorder);
        cameraBorder.SetActive(false);
        return new CustomObject("Camera Border", "camera_border", cameraBorder,
                description:"Stops the centre of the camera from passing a certain point.\n\n" +
                            "Toggling this object with the Disable and Enable\n" +
                            "triggers will toggle whether the border is active.\n\n" +
                            "Change the 'Active Mode' setting to control if the border is active everywhere,\n" +
                            "only when not using Binoculars, or only when using Binoculars.",
                sprite:ResourceUtils.LoadSpriteResource("camera_border"))
            .WithConfigGroup(ConfigGroup.CameraBorder);
    }

    private static PlaceableObject CreateSceneBorderRemover()
    {
        var sceneBorderRemover = new GameObject("Scene Border Remover");
        
        Object.DontDestroyOnLoad(sceneBorderRemover);
        sceneBorderRemover.SetActive(false);
        
        SceneBorderRemover.Init();
        sceneBorderRemover.AddComponent<SceneBorderRemover>();

        sceneBorderRemover.layer = 10;
        sceneBorderRemover.transform.position = new Vector3(0, 0, 0.1f);

        return new CustomObject("Scene Border Remover", "scene_border_remover", sceneBorderRemover,
                description:"Removes the borders of a room, making it possible\n" +
                            "to build out of bounds without needing to lock the camera.",
                sprite:ResourceUtils.LoadSpriteResource("scene_border_remover"),
                preview:true);
    }

    private static PlaceableObject CreateEnemyBarrier()
    {
        var enemyBarrier = new GameObject("Enemy Barrier");
        var heroOnly = LayerMask.NameToLayer("Hero Only");

        enemyBarrier.AddComponent<BoxCollider2D>().size = new Vector2(3.2f, 3.2f);
        enemyBarrier.layer = heroOnly;

        enemyBarrier.SetActive(false);
        Object.DontDestroyOnLoad(enemyBarrier);

        return new CustomObject("Enemy Barrier", "enemy_blocker", enemyBarrier,
            description:"A barrier that enemies cannot pass through, but the player can.\n\n" +
                        "This object is only a barrier, it does not function like terrain.",
            sprite:ResourceUtils.LoadSpriteResource("enemy_blocker", ppu:60))
            .WithConfigGroup(ConfigGroup.Stretchable);
    }

    private static PlaceableObject CreatePlayerHook()
    {
        var playerHook = new GameObject("Player Hook");
        playerHook.SetActive(false);
        Object.DontDestroyOnLoad(playerHook);
        
        PlayerHook.Init();

        playerHook.AddComponent<PlayerHook>();

        return new CustomObject("Player Hook", "player_hook", playerHook,
                description:"Can detect certain inputs or actions from the player such as jumping or landing,\n" +
                            "and perform certain triggers such as damaging or killing the player.",
                sprite:ResourceUtils.LoadSpriteResource("player_listener", FilterMode.Point, ppu:50))
            .WithBroadcasterGroup(BroadcasterGroup.PlayerHooks)
            .WithReceiverGroup(ReceiverGroup.PlayerHooks)
            .WithConfigGroup(ConfigGroup.Generic);
    }

    private static PlaceableObject CreateKeyListener()
    {
        var keyListener = new GameObject("Key Listener");

        keyListener.AddComponent<KeyListener>();

        keyListener.SetActive(false);
        Object.DontDestroyOnLoad(keyListener);

        return new CustomObject("Key Listener", "key_listener", keyListener,
            description:"Can listen and broadcast events when keys are pressed and released.\n\n" +
                        "The 'Key' option should be a Unity KeyCode, a list can be found on the Unity docs.",
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
                description: "Broadcasts the OnCall event when the Call trigger is run.\n\n" +
                             "Set a Relay ID for the Relay's active/inactive state to be saved when the room is reloaded.\n" +
                             "Relays with the same ID share the same state, even across multiple rooms.\n\n" +
                             "Enable 'Multiplayer Share' for the Relay's event to broadcast to others in multiplayer.")
            .WithConfigGroup(ConfigGroup.Relay)
            .WithBroadcasterGroup(BroadcasterGroup.Callable)
            .WithReceiverGroup(ReceiverGroup.Relay);
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
                description: "Displays a piece of text.\n\n" +
                             "Use <br> for a new line, <page> for a new page,\n" +
                             "and <hpage> for one where Hornet speaks.\n\n" +
                             "Use <color>, <b>, <i>, <s> and <u> to format text.\n" +
                             "For example: '<b><color=#FF0000>YOU</color></b>'")
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
                description: "Displays a piece of text.\n\n" +
                             "Use <br> for a new line.\n" +
                             "Use <color>, <b>, <i>, <s> and <u> to format text.\n" +
                             "For example: '<b><color=#FF0000>YOU</color></b>'")
            .WithReceiverGroup(ReceiverGroup.Displayable)
            .WithBroadcasterGroup(BroadcasterGroup.Choice)
            .WithConfigGroup(ConfigGroup.Choice);
    }

    private static PlaceableObject CreateObjectSpawner()
    {
        var duplicator = new GameObject("Object Duplicator");
        duplicator.SetActive(false);
        Object.DontDestroyOnLoad(duplicator);

        duplicator.AddComponent<ObjectDuplicator>();

        return new CustomObject("Object Spawner", "object_duplicator",
                duplicator,
                sprite: ResourceUtils.LoadSpriteResource("object_duplicator", FilterMode.Point),
                description: "Spawns a copy of a placed object." +
                             "Copies have the same settings as the original object.\n\n" +
                             "Find the ID of the object to copy using the Cursor tool.\n\n" +
                             "The original object should have 'Start Enabled' set to false.")
            .WithConfigGroup(ConfigGroup.Duplicator)
            .WithReceiverGroup(ReceiverGroup.Duplicator);
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
                description: "Broadcasts an event periodically.\n\n" +
                             "Start Delay is the time until the first call, " +
                             "Repeat Delay is the time between calls.\n\n" +
                             "Set 'Max Calls' for the timer to disable itself after a certain number of calls.")
            .WithConfigGroup(ConfigGroup.Timer)
            .WithBroadcasterGroup(BroadcasterGroup.Callable)
            .WithReceiverGroup(ReceiverGroup.Generic);
    }

    private static PlaceableObject CreateObjectAnchor()
    {
        var anchor = new GameObject("Object Anchor");
        anchor.SetActive(false);
        Object.DontDestroyOnLoad(anchor);
        
        ObjectAnchor.Init();
        anchor.AddComponent<ObjectAnchor>();

        return new CustomObject("Moving Object Anchor", "object_anchor",
                anchor,
                sprite: ResourceUtils.LoadSpriteResource("object_anchor", FilterMode.Point),
                description: "Used to move objects around, as if on a track.\n\n" +
                             "Find the ID of the object to move by clicking it with the Cursor tool.\n\n" +
                             "Hold the 'P' keybind to preview the movement of placed objects.",
                preview: true)
            .WithConfigGroup(ConfigGroup.ObjectAnchor)
            .WithReceiverGroup(ReceiverGroup.ObjectAnchor)
            .WithBroadcasterGroup(BroadcasterGroup.ObjectAnchor);
    }

    private static PlaceableObject CreateObjectSpinner()
    {
        var spinner = new GameObject("Object Spinner");

        spinner.AddComponent<ObjectSpinner>();

        spinner.SetActive(false);
        Object.DontDestroyOnLoad(spinner);

        return new CustomObject("Object Spinner", "object_spinner",
                spinner,
                sprite: ResourceUtils.LoadSpriteResource("object_spinner", FilterMode.Point),
                description: "Used to rotate objects at configurable speeds.\n" +
                             "May cause unusual behaviour with objects that are not intended to be rotated.\n\n" +
                             "Find the ID of the object to rotate by clicking it with the Cursor tool.\n\n" +
                             "Hold the 'P' keybind to preview the movement of placed objects.",
                preview: true)
            .WithConfigGroup(ConfigGroup.ObjectSpinner)
            .WithReceiverGroup(ReceiverGroup.ObjectSpinner);
    }

    private static PlaceableObject CreateTriggerZone()
    {
        var point = new GameObject("Trigger Zone");

        var collider = point.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(3.2f, 3.2f);

        point.AddComponent<TriggerZone>();

        point.SetActive(false);
        Object.DontDestroyOnLoad(point);

        return new CustomObject("Trigger Zone", "trigger_zone",
                point,
                sprite: ResourceUtils.LoadSpriteResource("trigger_zone", FilterMode.Point, ppu:10),
                description: "Can broadcast events when entered or exited.")
            .WithBroadcasterGroup(BroadcasterGroup.TriggerZone)
            .WithConfigGroup(ConfigGroup.TriggerZone);
    }

    private static PlaceableObject CreateInteraction()
    {
        CustomInteraction.Init();
        
        var point = new GameObject("Interaction");

        var collider = point.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(3.2f, 3.2f);

        point.AddComponent<CustomInteraction>();

        point.SetActive(false);
        Object.DontDestroyOnLoad(point);

        return new CustomObject("Interaction", "interaction",
                point,
                sprite: ResourceUtils.LoadSpriteResource("interaction", FilterMode.Point, ppu:10),
                description: "Hovering text appears when the player stands in the interaction's range,\n" +
                             "broadcasts an event when interacted with.")
            .WithConfigGroup(ConfigGroup.Interaction)
            .WithBroadcasterGroup(BroadcasterGroup.Interaction);
    }

    private static PlaceableObject CreateObjectRemover(string id, string name,
        [CanBeNull] Func<GameObject, Disabler[]> action, string desc)
    {
        var obj = new GameObject($"Object Remover ({id})");
        Object.DontDestroyOnLoad(obj);
        obj.SetActive(false);

        RemoverActions[id] = action;
        obj.AddComponent<ObjectRemover>().triggerName = id;

        var sprite = ResourceUtils.LoadSpriteResource(id, FilterMode.Point);
        obj.layer = 10;
        obj.transform.position = new Vector3(0, 0, -2);

        return new CustomObject(name, id, obj, desc, sprite:sprite, preview:true)
            .WithReceiverGroup(ReceiverGroup.Generic);
    }

    public static Disabler[] GetObjects(ObjectRemover editor)
    {
        return RemoverActions[editor.triggerName].Invoke(editor.gameObject);
    }

    private static Disabler[] FindObjectsToDisable<T>(GameObject disabler) where T : Component
    {
        var objects = disabler.scene.GetRootGameObjects()
            .Where(obj => !obj.name.StartsWith("[Architect] "))
            .SelectMany(root => root.GetComponentsInChildren<T>(true))
            .Select(obj => obj.gameObject);

        var lowest = float.MaxValue;
        GameObject point = null;
        foreach (var obj in objects)
        {
            var pos = obj.transform.position - disabler.transform.position;
            pos.z = 0;
            var dist = pos.sqrMagnitude;

            if (dist < lowest)
            {
                lowest = dist;
                point = obj;
            }
        }
        
        return point is not null && lowest <= 500 ? [point.gameObject.GetOrAddComponent<Disabler>()] : [];
    }

    private static PlaceableObject CreateHazardRespawnPoint()
    {
        var point = new GameObject("Hazard Respawn Point");

        point.SetActive(false);
        Object.DontDestroyOnLoad(point);

        point.AddComponent<HazardRespawnTrigger>().respawnMarker = point.AddComponent<CustomHazardRespawnMarker>();

        var collider = point.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;

        return new CustomObject("Hazard Respawn Point", "hazard_respawn_point", point, 
            sprite:ResourceUtils.LoadSpriteResource("hazard_respawn_point"),
            description:"A point that the player can respawn at after taking hazard damage.\n\n" +
                        "To use a custom hitbox set 'Contact Trigger' to false, then use a Trigger Zone and\n" +
                        "the Hazard Respawn Point's 'SetSpawn' trigger.",
            preview:true)
            .WithConfigGroup(ConfigGroup.HazardRespawn)
            .WithReceiverGroup(ReceiverGroup.HazardRespawn);
    }

    private static PlaceableObject CreateTeleportPoint()
    {
        var point = new GameObject("Teleport Point");

        point.SetActive(false);
        Object.DontDestroyOnLoad(point);

        return new CustomObject("Teleport Point", "telepoint", point, 
            sprite:ResourceUtils.LoadSpriteResource("telepoint"),
            description:"Teleports the player to this point when the 'Teleport' trigger is called.")
            .WithReceiverGroup(ReceiverGroup.TeleportPoint);
    }

    private static PlaceableObject CreateBinoculars()
    {
        var point = new GameObject("Binoculars");

        var col = point.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1.25f, 1.06f);

        point.AddComponent<SpriteRenderer>().sprite = ResourceUtils.LoadSpriteResource("binoculars");

        Binoculars.Init();
        var softTerrain = LayerMask.NameToLayer("Soft Terrain");
        point.layer = softTerrain;
        point.AddComponent<Binoculars>();

        point.SetActive(false);
        Object.DontDestroyOnLoad(point);

        return new CustomObject("Binoculars", "freecam", point,
                description:"Enables a Freecam mode when hit by the player,\n" +
                            "or when receiving the StartUsing trigger. Use the scroll wheel to zoom in/out.\n\n" +
                            "Useful for giving players a preview of a map.")    
            .WithConfigGroup(ConfigGroup.Binoculars)
            .WithReceiverGroup(ReceiverGroup.Binoculars)
            .WithBroadcasterGroup(BroadcasterGroup.Binoculars );
    }
}