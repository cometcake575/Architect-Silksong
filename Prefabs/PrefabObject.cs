using System;
using System.Collections.Generic;
using Architect.Behaviour.Utility;
using Architect.Config.Types;
using Architect.Editor;
using Architect.Events.Blocks;
using Architect.Objects.Placeable;
using Architect.Placements;
using Architect.Storage;
using Newtonsoft.Json;
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
    }

    public override void Click(Vector3 mousePosition, bool first)
    {
        if (Input.GetKey(KeyCode.LeftAlt) && first)
        {
            var id = $"-{Guid.NewGuid().ToString()[..8]}";
            
            if (!PrefabManager.Prefabs.TryGetValue(Name, out var o)) 
                o = PrefabManager.Prefabs[Name] = StorageManager.LoadScene($"Prefab_{Name}");

            List<ObjectPlacement> placements = [];
            
            var pos = EditManager.GetWorldPos(mousePosition, true);
            pos.z = 0;
            
            foreach (var placement in o.Placements)
            {
                var rePlacement = JsonConvert.DeserializeObject<ObjectPlacement>(JsonConvert.SerializeObject(placement), 
                    StorageManager.Opc);
                rePlacement.ID += id;
                
                placements.Add(rePlacement);
                foreach (var cfg in rePlacement.Config)
                {
                    if (cfg is IdConfigValue icv)
                    {
                        icv.Value += id;
                    }
                }
                
                rePlacement.Move(placement.GetPos() + pos - new Vector3(100, 100));
            }
            EditManager.RegisterLastPos(pos);
            ActionManager.PerformAction(new PlaceObjects(placements));
            
            foreach (var block in o.ScriptBlocks)
            {
                var clone = block.Clone(id);
                var wasLocal = ScriptManager.IsLocal;
                
                if (!wasLocal) ScriptManager.IsLocal = true;
                
                PlacementManager.GetLevelData().ScriptBlocks.Add(clone);
                clone.Setup(true);
                
                if (!wasLocal) ScriptManager.IsLocal = false;
            } 
        } else base.Click(mousePosition, first);
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
                    false,
                    name
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
