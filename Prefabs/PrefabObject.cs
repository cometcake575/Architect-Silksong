using System.Collections.Generic;
using Architect.Behaviour.Utility;
using Architect.Objects.Placeable;
using Architect.Placements;
using Architect.Storage;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Architect.Prefabs;

public class PrefabObject : PlaceableObject
{
    private static GameObject _prefabObject;

    public readonly string Name; 
    
    public static void Init()
    {
        _prefabObject = new GameObject("[Architect] Prefab Spawner");
        _prefabObject.SetActive(false);
        Object.DontDestroyOnLoad(_prefabObject);
        _prefabObject.AddComponent<Prefab>();
    }
    
    public PrefabObject(string name) : base(
        $"{name} (Prefab)",
        $"prefab_{name}",
        "References a prefab, changes to the prefab will\n" +
        "update every copy of this object.\n\n" +
        "Hold Left Alt when placing to disconnect the objects from the prefab.",
        preview: true,
        sprite: PrefabManager.PrefabIcon)
    {
        FinishSetup(_prefabObject);
        PostSpawnAction = Setup;
        Name = name;
        ConfigGroup = Objects.Groups.ConfigGroup.Generic;
        ReceiverGroup = Objects.Groups.ReceiverGroup.Prefab;
        DisableTransformations = true;
        HideUISprite = true;
    }

    private void Setup(GameObject self)
    {
        self.GetComponent<Prefab>().id = Name;
    }
}

public class Prefab : PreviewableBehaviour
{
    public string id;
    public List<GameObject> spawns = [];

    private void Start()
    {
        // Turn red and don't preview if inside own prefab scene to indicate to user this will not work
        if (GameManager.instance.sceneName == $"Prefab_{id}")
        {
            GetComponentInParent<SpriteRenderer>().color = Color.red;
            return;
        }
        
        if (!PrefabManager.Prefabs.TryGetValue(id, out var o)) 
            o = PrefabManager.Prefabs[id] = StorageManager.LoadScene($"Prefab_{id}");
        foreach (var placement in o.Placements)
        {
            if (isAPreview)
            {
                placement.PlaceGhost(
                    placement.GetPos() + transform.position - new Vector3(100, 100),
                    false
                ).transform.SetParent(transform, true);
            }
            else
            {
                var obj = placement.SpawnObject(
                    placement.GetPos() + transform.position - new Vector3(100, 100),
                    name
                );
                if (obj)
                {
                    PlacementManager.Objects[placement.GetId() + name] = obj;
                    PlacementManager.OnPlace?.Invoke(
                        placement.GetPlacementType().GetId(), 
                        placement.GetId() + name,
                        obj);
                    spawns.Add(obj);
                }
            }
        }

        foreach (var block in o.ScriptBlocks)
        {
            block.Clone(name).Setup(false);
        }
    }
}
