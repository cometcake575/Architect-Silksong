using System;
using System.Collections.Generic;
using System.Linq;
using Architect.Behaviour.Custom;
using Architect.Behaviour.Utility;
using Architect.Objects.Categories;
using Architect.Objects.Groups;
using Architect.Objects.Placeable;
using Architect.Utils;
using BepInEx;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Architect.Content.Custom;

public static class UtilityObjects
{
    private static readonly Dictionary<string, Func<GameObject, string, Disabler[]>> RemoverActions = [];
    
    public static void Init()
    {
        Categories.Utility.Add(CreateItem());
        Categories.Utility.Add(CreateVoider());
        Categories.Utility.Add(CreatePlasmifier());
        
        Categories.Utility.Add(CreateObjectAnchor());
        Categories.Utility.Add(CreateObjectSpinner());
        Categories.Utility.Add(CreateObjectMover());
        Categories.Utility.Add(CreateObjectSpawner());
        Categories.Utility.Add(CreateObjectColourer());
        Categories.Utility.Add(CreateTriggerZone());
        Categories.Utility.Add(CreateInteraction());
        Categories.Utility.Add(CreateFakePerformance());
        
        Categories.Utility.Add(CreateWalkTarget());
        Categories.Utility.Add(CreateDarkness());
        Categories.Utility.Add(CreateMemoryToggle());
        
        Categories.Utility.Add(CreateEnemyBarrier());
        Categories.Utility.Add(CreateVignetteDisabler());
        Categories.Utility.Add(CreateObjectRemover("enemy_remover", "Disable Enemy", 
                FindObjectsToDisable<HealthManager>, "Removes the nearest enemy.\n\n" +
                                                     "This should be placed at the enemy's spawn point, not its\n" +
                                                     "current position, or it will not work when exiting edit mode.")
            .WithConfigGroup(ConfigGroup.Disabler)
            .WithReceiverGroup(ReceiverGroup.Generic));
        
        Categories.Utility.Add(CreateTeleportPoint());
        
        Categories.Utility.Add(CreateHazardRespawnPoint());
        Categories.Utility.Add(CreateObjectRemover("hrp_remover", "Disable Hazard Respawn Point",
                FindObjectsToDisable<HazardRespawnTrigger>, "Removes the nearest Hazard Respawn Point.")
            .WithConfigGroup(ConfigGroup.Disabler)
            .WithReceiverGroup(ReceiverGroup.Generic));
        
        Categories.Utility.Add(CreateTransitionPoint());
        Categories.Utility.Add(CreateObjectRemover("door_remover", "Disable Transition", 
                FindObjectsToDisable<TransitionPoint>, "Removes the nearest door to another room.")
            .WithConfigGroup(ConfigGroup.Generic)
            .WithReceiverGroup(ReceiverGroup.Generic));
        
        Categories.Utility.Add(CreateBinoculars());
        Categories.Utility.Add(CreateCameraBorder());
        Categories.Utility.Add(CreateCameraRotator());
        Categories.Utility.Add(CreateSceneBorderRemover());
        
        Categories.Utility.Add(CreateObjectRemover("collision_remover", "Disable Collider",
                (disabler, filter) =>
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
                        .Where(i => i.name.Contains(filter) && i.gameObject.scene == disabler.scene)
                        .Select(p => p.gameObject.GetOrAddComponent<Disabler>()).ToArray();
                }, "Removes colliders touching this object.\n" +
                   "Trigger zones such as hazard respawn points do not count.")
            .WithConfigGroup(ConfigGroup.Remover)
            .WithReceiverGroup(ReceiverGroup.Generic));
        
        Categories.Utility.Add(CreateObjectRemover("render_remover", "Disable Renderer", 
                FindObjectsToDisable<Renderer>, "Removes the nearest renderer.")
            .WithConfigGroup(ConfigGroup.Remover)
            .WithReceiverGroup(ReceiverGroup.Generic));
        
        Categories.Utility.Add(CreateObjectRemover("object_remover", "Disable Object", (o, _) =>
            {
                var config = o.GetComponent<ObjectRemoverConfig>();
                GameObject point = null;

                if (config)
                    try
                    {
                        point = ObjectUtils.GetGameObjectFromArray(o.scene.GetRootGameObjects(), config.objectPath);
                    }
                    catch (ArgumentException) { }

                if (point && point.GetComponent<Enabler>()) return [];
                return point is not null ? [point.GetOrAddComponent<Disabler>()] : [];
            }, "Removes any vanilla objects from the game.\n\n" +
               "The path to an object can be found with tools such as Unity Explorer.")
            .WithConfigGroup(ConfigGroup.ObjectRemover)
            .WithReceiverGroup(ReceiverGroup.Generic));
        
        Categories.Utility.Add(CreateObjectEnabler());
        
        Categories.Utility.Add(CreateObjectRemover("room_remover", "Clear Room", (o, filter) =>
                {
                    var clearer = o.GetComponent<RoomClearerConfig>();

                    if (!clearer) return [];

                    var objects = o.scene.GetRootGameObjects().Where(obj =>
                        !obj.name.StartsWith("[Architect]")
                        && !obj.name.StartsWith("_SceneManager")
                        && !obj.GetComponent<CustomTransitionPoint>()
                        && !obj.GetComponent<WorldRumblePreventWhileActive>()
                        && !obj.GetComponentInChildren<SceneAdditiveLoadConditional>()
                        && obj.name.Contains(filter)
                    );

                    if (clearer.removeMusic && filter.IsNullOrWhiteSpace()) 
                        clearer.gameObject.AddComponent<MusicController>();
            
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
        
        Categories.Effects.Add(CreateRoar());
    }

    private static PlaceableObject CreateItem()
    {
        CustomPickup.Init();
        
        var pickup = new GameObject("Pickup Spawner");
        Object.DontDestroyOnLoad(pickup);
        pickup.SetActive(false);

        pickup.AddComponent<CustomPickup>();
        
        return new CustomObject("Item", "item_pickup", pickup, 
                "A collectible item such as a Rosary String, Shard Bundle, Tool or Key.\n" +
                "Some items may act strangely.\n\n" +
                "Disabling 'Ignore Limit' will cause the item to disappear if the player normally cannot get more,\n" +
                "such as if the item is a tool the player already has.\n\n" +
                "A list of valid item IDs can be found in the Architect guide.",
                sprite:ResourceUtils.LoadSpriteResource("item_pickup", ppu:64))
            .WithConfigGroup(ConfigGroup.Item)
            .WithReceiverGroup(ReceiverGroup.Item)
            .WithBroadcasterGroup(BroadcasterGroup.Item);
    }

    private static PlaceableObject CreateRoar()
    {
        var roar = new GameObject("Roar Spawner");
        Object.DontDestroyOnLoad(roar);
        roar.SetActive(false);

        roar.AddComponent<RoarEffect>();
        
        return new CustomObject("Roar Effect", "roar_effect", roar,
                sprite:ResourceUtils.LoadSpriteResource("roar", ppu:64))
            .WithConfigGroup(ConfigGroup.Roar)
            .WithReceiverGroup(ReceiverGroup.Roar);
    }

    private static PlaceableObject CreateVoider()
    {
        var threader = new GameObject("Voider");
        Object.DontDestroyOnLoad(threader);
        threader.SetActive(false);

        threader.AddComponent<BlackThreader>();
        
        return new CustomObject("Black Threader", "black_threader", threader, 
                "Makes an enemy become black threaded, can either make the enemy start threaded\n" +
                "or require the 'Activate' trigger to be run.\n\n" +
                "Enemies will pick one void attack to use,\n" +
                "the chance of each attack is configurable.",
                sprite:ResourceUtils.LoadSpriteResource("black_threader", ppu:64))
            .WithConfigGroup(ConfigGroup.BlackThreader)
            .WithReceiverGroup(ReceiverGroup.BlackThreader);
    }

    private static PlaceableObject CreatePlasmifier()
    {
        var plasmifier = new GameObject("Plasmifier");
        Object.DontDestroyOnLoad(plasmifier);
        plasmifier.SetActive(false);

        plasmifier.AddComponent<Plasmifier>();
        
        return new CustomObject("Plasmifier", "plasmifier", plasmifier, 
                "Makes an enemy become plasmified, can either make the enemy start plasmified\n" +
                "or require the 'Activate' trigger to be run.\n\n" +
                "Plasmified enemies will heal every 0.75 seconds they are not damaged.",
                sprite:ResourceUtils.LoadSpriteResource("plasmifier", ppu:64))
            .WithConfigGroup(ConfigGroup.Plasmifier)
            .WithReceiverGroup(ReceiverGroup.Plasmifier);
    }

    private static PlaceableObject CreateWalkTarget()
    {
        var target = new GameObject("Walk Target");

        Object.DontDestroyOnLoad(target);
        target.SetActive(false);

        target.AddComponent<WalkTarget>();
        
        return new CustomObject("Walk Target", "walk_target", target,
                description:"Forces the player to walk to this spot when the 'Start' trigger is called.\n" +
                            "Animation and speed can be customised.\n\n" +
                            "Can be cancelled with the 'Cancel' trigger.\n" +
                            "Calls the 'OnFinish' event when the player arrives at the target.",
                sprite:ResourceUtils.LoadSpriteResource("walk_target", ppu:33))
            .WithReceiverGroup(ReceiverGroup.WalkTarget)
            .WithConfigGroup(ConfigGroup.WalkTarget)
            .WithBroadcasterGroup(BroadcasterGroup.Finishable);
    }

    private static PlaceableObject CreateDarkness()
    {
        Darkness.Init();
        
        var dark = new GameObject("Darkness");
        
        Object.DontDestroyOnLoad(dark);
        dark.SetActive(false);

        dark.AddComponent<Darkness>();
        
        return new CustomObject("Darkness", "darkness", dark,
                description:"Makes the room darker, reducing how far the player can see.\n" +
                            "Placing 2 darkness objects at once will increase the effect.",
                sprite:ResourceUtils.LoadSpriteResource("darkness"))
            .WithConfigGroup(ConfigGroup.Generic);
    }

    private static PlaceableObject CreateMemoryToggle()
    {
        MapStateHook.Init();
        
        var msh = new GameObject("Memory Toggler");
        
        Object.DontDestroyOnLoad(msh);
        msh.SetActive(false);

        msh.AddComponent<MapStateHook>();
        
        return new CustomObject("Memory Toggler", "memory_toggle", msh,
                description:"Controls whether the current room is considered a memory.\n\n" +
                            "Rosaries, Shell Shards and Items cannot spawn in memories\n" +
                            "and dying does not leave a cocoon.",
                sprite:ResourceUtils.LoadSpriteResource("memory_toggle", ppu:64))
            .WithConfigGroup(ConfigGroup.MapStateHook);
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

    private static PlaceableObject CreateCameraRotator()
    {
        CameraRotator.Init();

        var cameraSpinner = new GameObject("Camera Rotator");
        cameraSpinner.AddComponent<CameraRotator>();

        cameraSpinner.transform.position = new Vector3(0, 0, 0.1f);

        Object.DontDestroyOnLoad(cameraSpinner);
        cameraSpinner.SetActive(false);
        return new CustomObject("Camera Rotator", "camera_rotator", cameraSpinner,
                description:"Rotates the camera to the angle of this object.\n\n" +
                            "Can be used in combination with the Object Spinner\n" +
                            "to rotate the camera over time.",
                sprite:ResourceUtils.LoadSpriteResource("camera_spinner"));
    }

    private static PlaceableObject CreateSceneBorderRemover()
    {
        var sceneBorderRemover = new GameObject("Scene Border Remover");
        
        Object.DontDestroyOnLoad(sceneBorderRemover);
        sceneBorderRemover.SetActive(false);
        
        SceneBorderRemover.Init();
        sceneBorderRemover.AddComponent<SceneBorderRemover>();

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

    private static PlaceableObject CreateVignetteDisabler()
    {
        VignetteDisabler.Init();
        var vignetteDisabler = new GameObject("Vignette Disabler");

        vignetteDisabler.SetActive(false);
        Object.DontDestroyOnLoad(vignetteDisabler);

        vignetteDisabler.AddComponent<VignetteDisabler>();

        return new CustomObject("Disable Vignette", "vignette_disabler", vignetteDisabler,
            description:"Disables the Vignette effect.", preview: true,
            sprite:ResourceUtils.LoadSpriteResource("vignette_disabler", FilterMode.Point));
    }

    private static PlaceableObject CreateFakePerformance()
    {
        FakePerformanceRegion.Init();
        
        var relay = new GameObject("Fake Needolin");

        relay.AddComponent<FakePerformanceRegion>();

        relay.SetActive(false);
        Object.DontDestroyOnLoad(relay);

        return new CustomObject("Fake Needolin Performance", "fake_performance",
                relay,
                sprite: ResourceUtils.LoadSpriteResource("fake_performance", FilterMode.Point, ppu:64),
                description: "Acts like the Needolin is playing at this object's position when it is active.")
            .WithConfigGroup(ConfigGroup.FakePerformance);
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
                description: "Spawns a copy of a placed object.\n" +
                             "Copies have the same settings as the original object.\n\n" +
                             "Find the ID of the object to copy using the Cursor tool.\n\n" +
                             "The original object should have 'Start Enabled' set to false.")
            .WithConfigGroup(ConfigGroup.Duplicator)
            .WithReceiverGroup(ReceiverGroup.Duplicator);
    }

    private static PlaceableObject CreateObjectColourer()
    {
        var colourer = new GameObject("Object Colourer");
        colourer.SetActive(false);
        Object.DontDestroyOnLoad(colourer);

        colourer.AddComponent<ObjectColourer>();

        return new CustomObject("Object Colourer", "object_colourer",
                colourer,
                sprite: ResourceUtils.LoadSpriteResource("object_colourer", FilterMode.Point),
                description: "Changes the colour of an object.\n" +
                             "This works with most objects, but not all of them.\n\n" +
                             "Find the ID of the object to copy using the Cursor tool.")
            .WithConfigGroup(ConfigGroup.Colourer)
            .WithReceiverGroup(ReceiverGroup.Colourer);
    }

    private static PlaceableObject CreateObjectAnchor()
    {
        var anchor = new GameObject("Object Anchor");
        anchor.SetActive(false);
        Object.DontDestroyOnLoad(anchor);
        
        ObjectAnchor.Init();
        anchor.AddComponent<ObjectAnchor>();

        return new CustomObject("Object Anchor", "object_anchor",
                anchor,
                sprite: ResourceUtils.LoadSpriteResource("object_anchor", FilterMode.Point),
                description: "Used to move objects.\n\n" +
                             "Set Move Speed above 0 for the anchor to move linearly.\n\n" +
                             "Set the Parent ID to make the anchor follow the parent,\n" +
                             "or set it to a Track Point to make the anchor move along the track.\n\n" +
                             "Find the ID of the object and parent by clicking them with the Cursor tool\n" +
                             "or holding the 'I' key and clicking the object.",
                preview: true)
            .WithConfigGroup(ConfigGroup.ObjectAnchor)
            .WithReceiverGroup(ReceiverGroup.ObjectAnchor)
            .WithBroadcasterGroup(BroadcasterGroup.ObjectAnchor);
    }

    private static PlaceableObject CreateObjectMover()
    {
        var mover = new GameObject("Object Mover");
        mover.SetActive(false);
        Object.DontDestroyOnLoad(mover);
        
        mover.AddComponent<ObjectMover>();

        return new CustomObject("Object Mover", "object_mover",
                mover,
                sprite: ResourceUtils.LoadSpriteResource("object_mover", FilterMode.Point),
                description: "Teleports an object to another position relative to itself, the mover, the player\n" +
                             "or another object. Position Source ID (an Object ID) overrides Position Source.\n\n" +
                             "This should be used for individual teleports.\n" +
                             "To move an object as if on a track, use the Object Anchor.\n" +
                             "To rotate an object over time, use the Object Spinner.")
            .WithConfigGroup(ConfigGroup.ObjectMover)
            .WithReceiverGroup(ReceiverGroup.ObjectMover);
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

        var bc = point.AddComponent<BoxCollider2D>();
        bc.isTrigger = true;
        bc.size = new Vector2(3.2f, 3.2f);

        var cc = point.AddComponent<PolygonCollider2D>();
        cc.isTrigger = true;

        var points = new Vector2[24];
        for (var i = 0; i < 24; i++)
        {
            var angle = 2 * Mathf.PI * i / 24;
            var x = Mathf.Cos(angle) * 1.6f;
            var y = Mathf.Sin(angle) * 1.6f;
            points[i] = new Vector2(x, y);
        }

        cc.pathCount = 1;
        cc.SetPath(0, points);
        cc.enabled = false;

        point.AddComponent<TriggerZone>();

        point.SetActive(false);
        Object.DontDestroyOnLoad(point);

        return new CustomObject("Trigger Zone", "trigger_zone",
                point,
                sprite: TriggerZone.SquareZone,
                description: "Can broadcast events when entered or exited.\n\n" +
                             "'Activator' mode detects Shakra Rings, Kratt and Beastlings,\n" +
                             "a trigger layer can be set to only detect specific objects.")
            .WithBroadcasterGroup(BroadcasterGroup.TriggerZone)
            .WithConfigGroup(ConfigGroup.TriggerZones);
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
        [CanBeNull] Func<GameObject, string, Disabler[]> action, string desc)
    {
        var obj = new GameObject($"Object Remover ({id})");
        Object.DontDestroyOnLoad(obj);
        obj.SetActive(false);

        RemoverActions[id] = action;
        obj.AddComponent<ObjectRemover>().triggerName = id;

        var sprite = ResourceUtils.LoadSpriteResource(id, FilterMode.Point);
        obj.transform.position = new Vector3(0, 0, -2);

        return new CustomObject(name, id, obj, desc, sprite:sprite, preview:true)
            .WithReceiverGroup(ReceiverGroup.Generic);
    }

    private static PlaceableObject CreateObjectEnabler()
    {
        var obj = new GameObject("Object Enabler");
        Object.DontDestroyOnLoad(obj);
        obj.SetActive(false);

        obj.AddComponent<ObjectEnabler>();

        var sprite = ResourceUtils.LoadSpriteResource("object_enabler", FilterMode.Point);
        obj.transform.position = new Vector3(0, 0, -2);

        return new CustomObject("Enable Object", "object_enabler", obj, 
                "Enables a disabled object.\n\n" +
                "The path to an object can be found with tools such as Unity Explorer.", sprite:sprite, preview:true)
            .WithReceiverGroup(ReceiverGroup.Generic)
            .WithConfigGroup(ConfigGroup.ObjectEnabler);
    }

    public static Disabler[] GetObjects(ObjectRemover editor)
    {
        return RemoverActions[editor.triggerName].Invoke(editor.gameObject, editor.filter);
    }

    private static Disabler[] FindObjectsToDisable<T>(GameObject disabler, string filter) where T : Component
    {
        var objects = disabler.scene.GetRootGameObjects()
            .Where(obj => !obj.name.StartsWith("[Architect] ") && obj.name.Contains(filter) &&
                          !obj.GetComponent<CustomTransitionPoint>())
            .SelectMany(root => root.GetComponentsInChildren<T>(true))
            .Select(obj => obj.gameObject);

        var or = disabler.GetComponent<ObjectRemover>();
        if (or && or.all)
        {
            return objects.Select(o => o.GetOrAddComponent<Disabler>()).ToArray();
        }
        
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