using Architect.Behaviour.Custom;
using Architect.Content.Preloads;
using Architect.Objects.Categories;
using Architect.Objects.Groups;
using Architect.Objects.Placeable;
using Architect.Utils;
using GlobalEnums;
using UnityEngine;
using UnityEngine.Video;
using Object = UnityEngine.Object;

namespace Architect.Content.Custom;

public static class MiscObjects
{
    public static void Init()
    {
        Categories.Misc.AddStart(CreateLine());
        Categories.Misc.AddStart(CreateTriangle());
        Categories.Misc.AddStart(CreateCircle());
        Categories.Misc.AddStart(CreateSquare());
        
        Categories.Effects.AddStart(CreateAsset<Mp4Object>("MP4", "custom_mp4", true, true)
            .WithConfigGroup(ConfigGroup.Mp4)
            .WithReceiverGroup(ReceiverGroup.Playable));
        
        Categories.Effects.AddStart(CreateAsset<WavObject>("WAV", "custom_wav", false, false)
            .WithConfigGroup(ConfigGroup.Wav)
            .WithReceiverGroup(ReceiverGroup.Wav)
            .WithInputGroup(InputGroup.Wav));
        
        Categories.Effects.AddStart(CreateAsset<PngObject>("PNG", "custom_png", true, false,
                "\n\nFrame Count options can be used to split a sprite sheet into an animation.\n" +
                "Broadcasts 'OnFinish' when the animation ends.")
            .WithConfigGroup(ConfigGroup.Png)
            .WithReceiverGroup(ReceiverGroup.Playable)
            .WithBroadcasterGroup(BroadcasterGroup.Finishable));
        
        Categories.Misc.Add(CreateSilkSphere());
        
        Categories.Hazards.Add(CreateFrost());
        
        Categories.Platforming.Add(CreateWind());
        Categories.Platforming.Add(CreateBumper());

        Categories.Hazards.Add(CreateCustomHazard("White Thorns", "white_thorns",
        [
            new Vector2(-3.672f, -1.265f),
            new Vector2(-3.011f, 0.066f),
            new Vector2(-1.469f, 0.777f),
            new Vector2(0.474f, 1.282f),
            new Vector2(2.353f, 0.813f),
            new Vector2(3.754f, -0.674f)
        ]));

        Categories.Hazards.Add(CreateJellyEgg());
        Categories.Hazards.Add(CreateWhiteSpikes());
    }

    private static PlaceableObject CreateJellyEgg()
    {
        var obj = new GameObject("Jelly Egg Bomb");
        obj.SetActive(false);
        Object.DontDestroyOnLoad(obj);

        obj.AddComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;

        var je = obj.AddComponent<Behaviour.Custom.JellyEgg>();
        PreloadManager.RegisterPreload(new BasicPreload("Bone_East_14", "Gas Explosion Recycle M",
            o =>
            {
                je.explosion = o;
            }, true));

        var sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = ResourceUtils.LoadSpriteResource("jelly_egg_bomb", ppu:64);

        var col = obj.AddComponent<CircleCollider2D>();
        col.radius = 0.6f;
        col.isTrigger = true;
        
        obj.layer = LayerMask.NameToLayer("Terrain");
        
        obj.transform.SetPositionZ(0.0065f);

        return new CustomObject("Jelly Egg Bomb", "jelly_egg", obj, 
                description:"Setting 'Regen Time' to 0 or more will make the\n" +
                            "egg automatically regenerate after the configured number of seconds.")
            .WithRotationGroup(RotationGroup.All)
            .WithConfigGroup(ConfigGroup.JellyEgg);
    }

    private static PlaceableObject CreateWhiteSpikes()
    {
        var plate = new GameObject("Moving Spikes")
        {
            transform = { localPosition = new Vector3(0, 0, 0.05f) }
        };
        
        plate.SetActive(false);
        Object.DontDestroyOnLoad(plate);
        
        plate.AddComponent<SpriteRenderer>().sprite = ResourceUtils.LoadSpriteResource("Spikes.spikes_plate", ppu: 64);

        var spikes = new GameObject("Moving Part")
        {
            transform =
            {
                parent = plate.transform,
                localPosition = new Vector3(0, 0, 0.05f)
            }
        };
        
        WhiteSpikes.Init();

        spikes.AddComponent<WhiteSpikes>();
        spikes.AddComponent<SpriteRenderer>();

        var damager = new GameObject("Damager")
        {
            transform = { parent = spikes.transform },
            layer = LayerMask.NameToLayer("Attack")
        };
        
        var collider = damager.AddComponent<BoxCollider2D>();
        collider.offset = new Vector2(0, 2.1f);
        collider.size = new Vector2(2, 1.1f);

        var ef1 = damager.AddComponent<TinkEffect>();
        
        damager.AddComponent<DamageHero>().hazardType = HazardType.SPIKES;
        damager.AddComponent<DamageEnemies>().damageDealt = 20;

        var terrain = new GameObject("Terrain")
        {
            transform = { parent = spikes.transform },
            layer = LayerMask.NameToLayer("Terrain")
        };
        
        var terrainCol = terrain.AddComponent<BoxCollider2D>();
        terrainCol.offset = new Vector2(0, -0.7f);
        terrainCol.size = new Vector2(1.5f, 4.5f);

        var ef2 = terrain.AddComponent<TinkEffect>();
        
        PreloadManager.RegisterPreload(new BasicPreload(
            "Greymoor_06", "Greymoor_windmill_cog (1)/GameObject/dustpen_trap_shine0000", o =>
            {
                var blockEffect = o.GetComponent<TinkEffect>().blockEffect;
                ef1.blockEffect = blockEffect;
                ef2.blockEffect = blockEffect;
            }));
        
        return new CustomObject("Moving Spikes", "wp_trap_spikes", plate, 
                sprite:ResourceUtils.LoadSpriteResource("Spikes.spikes_icon", ppu:64))
            .WithConfigGroup(ConfigGroup.WhiteSpikes)
            .WithRotationGroup(RotationGroup.Four);
    }

    public static PlaceableObject CreateCustomHazard(string name, string id, Vector2[] points)
    {
        var obj = new GameObject(name);
        obj.SetActive(false);
        Object.DontDestroyOnLoad(obj);
        
        obj.transform.SetPositionZ(0.01f);

        var col = obj.AddComponent<EdgeCollider2D>();
        col.isTrigger = true;

        col.points = points;

        obj.AddComponent<CustomDamager>().damageAmount = 1;
        obj.AddComponent<SpriteRenderer>().sprite = ResourceUtils.LoadSpriteResource(id, ppu:64);

        return new CustomObject(name, id, obj)
            .WithRotationGroup(RotationGroup.All)
            .WithConfigGroup(ConfigGroup.Decorations);
    }

    private static PlaceableObject CreateWind()
    {
        Wind.Init();

        var windObj = new GameObject("Wind")
        {
            layer = LayerMask.NameToLayer("Terrain")
        };

        var collider = windObj.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(10, 10);
        collider.isTrigger = true;

        windObj.AddComponent<Wind>();

        windObj.SetActive(false);
        Object.DontDestroyOnLoad(windObj);
        
        return new CustomObject("Wind", "wind_zone",
                windObj,
                sprite: ResourceUtils.LoadSpriteResource("wind", FilterMode.Point, ppu:3.2f),
                description: "Applies a force to players and some objects when inside the wind's hitbox.")
            .WithRotationGroup(RotationGroup.All)
            .WithConfigGroup(ConfigGroup.Wind);
    }

    public static PlaceableObject CreateDreamBlock()
    {
        DreamBlock.Init();

        var obj = new GameObject("Dream Block")
        {
            layer = LayerMask.NameToLayer("Terrain")
        };
        Object.DontDestroyOnLoad(obj);
        obj.SetActive(false);

        obj.AddComponent<BoxCollider2D>().size *= 10;

        var col = obj.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size *= 9.8f;
        
        obj.AddComponent<DreamBlock>();

        obj.AddComponent<SpriteRenderer>().sprite =
            ResourceUtils.LoadSpriteResource("Dream.dream_block", FilterMode.Point);

        obj.transform.SetPositionZ(0.01f);

        return new CustomObject("Dream Block", "dream_block", obj)
            .WithConfigGroup(ConfigGroup.DreamBlock);
    }

    private static PlaceableObject CreateSilkSphere()
    {
        var sphere = new GameObject("Silk Sphere");
        
        SilkSphere.Init();
        
        sphere.SetActive(false);
        Object.DontDestroyOnLoad(sphere);

        sphere.transform.position = new Vector3(0, 0, 0.005f);

        sphere.AddComponent<SilkSphere>();
        sphere.AddComponent<HarpoonHook>();
        sphere.AddComponent<SpriteRenderer>().sprite = ResourceUtils.LoadSpriteResource("silk_sphere", ppu:50);

        var col = sphere.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 2.2f;
        sphere.layer = LayerMask.NameToLayer("Interactive Object");

        return new CustomObject("Silk Sphere", "silk_sphere",
                sphere,
                description: "An infinite source of Silk.")
            .WithRotationGroup(RotationGroup.All);
    }

    private static PlaceableObject CreateFrost()
    {
        var frost = new GameObject("Frost Marker");
        
        FrostMarker.Init();
        
        frost.SetActive(false);
        Object.DontDestroyOnLoad(frost);

        frost.AddComponent<FrostMarker>();

        return new CustomObject("Frost", "frost_marker",
                frost,
                sprite:ResourceUtils.LoadSpriteResource("cold_icon", ppu:50),
                description: "When enabled, the player will take frost damage over time.\n\n" +
                             "If the player has the Faydown Cloak then frost will not affect them,\n" +
                             "unless a Frost Binding is active.\n\n" +
                             "The player begins freezing after (100/frost speed) seconds,\n" +
                             "and hits are 1.75 seconds apart.")
            .WithConfigGroup(ConfigGroup.Frost);
    }

    private static PlaceableObject CreateAsset<T>(string name, string id, bool addRenderer, 
        bool addVideo, string extDesc = "") where T : MonoBehaviour
    {
        var asset = new GameObject("Custom Asset");

        if (addRenderer) asset.AddComponent<SpriteRenderer>().sprite = ArchitectPlugin.BlankSprite;
        if (addVideo) asset.AddComponent<VideoPlayer>();
        
        asset.AddComponent<T>();
        Object.DontDestroyOnLoad(asset);
        asset.SetActive(false);

        return new CustomObject($"Custom {name}", id,
                asset,
                sprite: ResourceUtils.LoadSpriteResource(id, ppu: 300),
                description:
                $"Places a custom {name} in the game.\n\n" +
                "URL should be a direct download anyone can access\n" +
                "in order to work with the level sharer." + extDesc)
            .WithRotationGroup(RotationGroup.All);
    }

    private static PlaceableObject CreateSquare()
    {
        var square = CreateShape("square");

        var collider = square.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(10, 10);

        return new CustomObject("Coloured Square", "coloured_square", square, 
                "A square that can be coloured or given a hitbox for custom collision.\n\n" +
                "RGBA colour values should be between 0 and 1.")
            .WithConfigGroup(ConfigGroup.Colours)
            .WithRotationGroup(RotationGroup.All);
    }

    private static PlaceableObject CreateCircle()
    {
        var circle = CreateShape("circle");

        var collider = circle.AddComponent<PolygonCollider2D>();
        collider.isTrigger = true;

        var points = new Vector2[24];
        for (var i = 0; i < 24; i++)
        {
            var angle = 2 * Mathf.PI * i / 24;
            var x = Mathf.Cos(angle) * 5;
            var y = Mathf.Sin(angle) * 5;
            points[i] = new Vector2(x, y);
        }

        collider.pathCount = 1;
        collider.SetPath(0, points);

        return new CustomObject("Coloured Circle", "coloured_circle", circle, 
                "A circle that can be coloured or given a hitbox for custom collision." +
                "\n\nRGBA colour values should be between 0 and 1.")
            .WithConfigGroup(ConfigGroup.Colours)
            .WithRotationGroup(RotationGroup.All);
    }
    
    public static readonly Material LineMaterial = new(Shader.Find("Sprites/Default"));

    private static PlaceableObject CreateLine()
    {
        var line = new GameObject("Shape (Line)");

        line.SetActive(false);
        Object.DontDestroyOnLoad(line);

        line.AddComponent<LineObject>();

        var lr = line.AddComponent<LineRenderer>();
        lr.material = LineMaterial;

        var collider = line.AddComponent<EdgeCollider2D>();
        collider.isTrigger = true;

        return new CustomObject("Coloured Line Point", "coloured_line", line, 
                "A point that can be combined with others to form lines that\n" +
                "can be coloured or given a hitbox for custom collision.\n\n" +
                "Follows the config options of the first point of its ID that is placed." +
                "\n\nRGBA colour values should be between 0 and 1.",
                sprite: ResourceUtils.LoadSpriteResource("line_point", ppu:200))
            .WithConfigGroup(ConfigGroup.Line)
            .WithRotationGroup(RotationGroup.All);
    }

    private static PlaceableObject CreateTriangle()
    {
        var triangle = CreateShape("triangle");

        var collider = triangle.AddComponent<EdgeCollider2D>();
        collider.isTrigger = true;
        collider.points =
        [
            new Vector2(-5, -4.17f),
            new Vector2(0, 4.45f),
            new Vector2(5, -4.17f),
            new Vector2(-5, -4.17f)
        ];

        return new CustomObject("Coloured Triangle", "coloured_triangle", triangle, 
                "A triangle that can be coloured or given a hitbox for custom collision." +
                "\n\nRGBA colour values should be between 0 and 1.")
            .WithConfigGroup(ConfigGroup.Colours)
            .WithRotationGroup(RotationGroup.All);
    }

    private static GameObject CreateShape(string name)
    {
        var sprite = ResourceUtils.LoadSpriteResource(name);

        var point = new GameObject("Shape (" + name + ")");
        point.transform.localScale /= 3;

        point.AddComponent<SpriteRenderer>().sprite = sprite;

        point.SetActive(false);
        Object.DontDestroyOnLoad(point);

        return point;
    }

    private static PlaceableObject CreateBumper()
    {
        Bumper.Init();
        
        var bumper = new GameObject("Bumper")
        {
            layer = LayerMask.NameToLayer("Terrain")
        };
        
        bumper.SetActive(false);
        Object.DontDestroyOnLoad(bumper);
        
        bumper.transform.position = new Vector3(0, 0, 0.005f);
        
        bumper.AddComponent<HarpoonHook>();
        bumper.AddComponent<Bumper>();
        bumper.AddComponent<SpriteRenderer>().sprite = Bumper.NormalIcon;

        var col = bumper.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.7f;

        var damager = new GameObject("Damager")
        {
            transform = { parent = bumper.transform },
            layer = LayerMask.NameToLayer("Attack")
        };
        
        var damageCol = damager.AddComponent<CircleCollider2D>();
        damageCol.isTrigger = true;
        damageCol.radius = 0.7f;
        
        damager.AddComponent<DamageHero>().hazardType = HazardType.COAL_SPIKES;
        damager.AddComponent<DamageEnemies>().damageDealt = 20;
        
        return new CustomObject("Bumper", "celeste_bumper", bumper)
            .WithConfigGroup(ConfigGroup.Bumpers)
            .WithReceiverGroup(ReceiverGroup.Bumpers);
    }
}