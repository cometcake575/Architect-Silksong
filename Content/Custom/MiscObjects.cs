using Architect.Behaviour.Custom;
using Architect.Content.Preloads;
using Architect.Objects.Categories;
using Architect.Objects.Groups;
using Architect.Objects.Placeable;
using Architect.Utils;
using GlobalEnums;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Architect.Content.Custom;

public static class MiscObjects
{
    public static void Init()
    {
        Categories.Misc.AddStart(CreateTriangle());
        Categories.Misc.AddStart(CreateCircle());
        Categories.Misc.AddStart(CreateSquare());
        
        Categories.Misc.AddStart(CreateAsset<Mp4Object>("MP4", "custom_mp4", true)
            .WithConfigGroup(ConfigGroup.Mp4));
        
        Categories.Misc.AddStart(CreateAsset<WavObject>("WAV", "custom_wav", false)
            .WithConfigGroup(ConfigGroup.Wav)
            .WithReceiverGroup(ReceiverGroup.Playable));
        
        Categories.Misc.AddStart(CreateAsset<PngObject>("PNG", "custom_png", true)
            .WithConfigGroup(ConfigGroup.Png)
            .WithReceiverGroup(ReceiverGroup.Mp4));
        
        Categories.Misc.Add(CreateSilkSphere());
        
        Categories.Hazards.Add(CreateFrost());
        
        Categories.Platforming.Add(CreateWind());

        Categories.Hazards.Add(CreateCustomHazard("White Thorns", "white_thorns",
        [
            new Vector2(-3.672f, -1.265f),
            new Vector2(-3.011f, 0.066f),
            new Vector2(-1.469f, 0.777f),
            new Vector2(0.474f, 1.282f),
            new Vector2(2.353f, 0.813f),
            new Vector2(3.754f, -0.674f)
        ]));

        Categories.Hazards.Add(CreateWhiteSpikes());
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

    private static PlaceableObject CreateSilkSphere()
    {
        var sphere = new GameObject("Silk Sphere");
        
        SilkSphere.Init();
        
        sphere.SetActive(false);
        Object.DontDestroyOnLoad(sphere);

        sphere.transform.position = new Vector3(0, 0, 0.005f);

        sphere.AddComponent<SilkSphere>();
        sphere.AddComponent<SpriteRenderer>().sprite = ResourceUtils.LoadSpriteResource("silk_sphere", ppu:50);

        var col = sphere.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 2.2f;
        sphere.layer = LayerMask.NameToLayer("Enemies");

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
                             "unless a Frost Binding is active.")
            .WithConfigGroup(ConfigGroup.Frost);
    }

    private static PlaceableObject CreateAsset<T>(string name, string id, bool addRenderer) where T : MonoBehaviour
    {
        var asset = new GameObject("Custom Asset");

        if (addRenderer) asset.AddComponent<SpriteRenderer>().sprite = ArchitectPlugin.BlankSprite;
        asset.AddComponent<T>();
        Object.DontDestroyOnLoad(asset);
        asset.SetActive(false);

        return new CustomObject($"Custom {name}", id,
                asset,
                sprite: ResourceUtils.LoadSpriteResource(id, ppu: 300),
                description:
                $"Places a custom {name} in the game.\n\n" +
                "URL should be a direct download anyone can access\n" +
                "in order to work with the level sharer.")
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
}