using System;
using System.Collections.Generic;
using System.Linq;
using Architect.Behaviour.Utility;
using Architect.Config;
using Architect.Config.Types;
using Architect.Editor;
using Architect.Events.Blocks;
using Architect.Events.Blocks.Events;
using Architect.Events.Blocks.Operators;
using Architect.Events.Blocks.Outputs;
using Architect.Objects.Placeable;
using Architect.Placements;
using Architect.Storage;
using Architect.Utils;
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

    public PrefabObject(string name) : 
        this(name, PrefabManager.Prefabs[name] = StorageManager.LoadScene($"Prefab_{name}"))
    {
        
    }
    
    public PrefabObject(string name, LevelData data) : base(
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

        RefreshConfig(data);
        
        ReceiverGroup = Objects.Groups.ReceiverGroup.Prefab;
        OutputGroup = Objects.Groups.OutputGroup.Generic;
        RotateAction = Rotate;
        ScaleAction = Scale;
    }

    public void RefreshConfig(LevelData data)
    {
        var blocks = data.ScriptBlocks.OfType<ConstantBlock>().ToArray();
        foreach (var b in blocks)
        {
            foreach (var c in b.CurrentConfig) c.Value.Setup(b);
        }

        ConfigGroup = blocks
            .Where(b => b.Public)
            .Select(b => b.GetConfigType())
            .Concat(Objects.Groups.ConfigGroup.Prefab).ToList();
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

                var newPos = placement.GetPos() + pos - new Vector3(100, 100);
                newPos = (newPos - pos) * EditManager.CurrentScale + pos;
                newPos = newPos.RotatePointAroundPivot(pos, EditManager.CurrentRotation);
                rePlacement.SetRotation(rePlacement.GetRotation() + EditManager.CurrentRotation);
                rePlacement.SetScale(rePlacement.GetScale() * EditManager.CurrentScale);
                rePlacement.Move(newPos);
            }
            EditManager.RegisterLastPos(pos);
            ActionManager.PerformAction(new PlaceObjects(placements));

            List<ScriptBlock> clones = [];
            foreach (var block in o.ScriptBlocks)
            {
                var clone = block.Clone(id);
                var wasLocal = ScriptManager.IsLocal;
                
                if (!wasLocal) ScriptManager.IsLocal = true;
                
                PlacementManager.GetLevelData().ScriptBlocks.Add(clone);
                clone.Setup(true);
                clones.Add(clone);
                
                if (!wasLocal) ScriptManager.IsLocal = false;
            } 
            foreach (var clone in clones) clone.LateSetup();
        } else base.Click(mousePosition, first);
    }

    private void Setup(GameObject self)
    {
        self.GetComponent<Prefab>().id = Name;
    }

    private static void Rotate(GameObject self, float rot)
    {
        self.GetComponent<Prefab>().rot = rot;
    }

    private static void Scale(GameObject self, float scale)
    {
        self.GetComponent<Prefab>().scale = scale;
    }
}

public class Prefab : PreviewableBehaviour
{
    public string id;
    public List<GameObject> spawns = [];
    private readonly Dictionary<string, ReceiveBlock> _receivers = [];
    private readonly Dictionary<string, string> _constants = [];

    public int visibility;

    public float scale;
    public float rot;

    public void Destroy()
    {
        foreach (var spawn in spawns) Destroy(spawn);
    }

    public void ApplyConfig(string block, string value)
    {
        _constants[block] = value;
    }

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
                if (visibility == 0 || (visibility == 1 && !placement.Locked)) placement.PlaceGhost(
                    placement.GetPos() + transform.position - new Vector3(100, 100),
                    false,
                    name
                ).transform.SetParent(transform, true);
            }
            else
            {
                var pos = placement.GetPos() + transform.position - new Vector3(100, 100);
                pos = (pos - transform.position) * scale + transform.position;
                pos = pos.RotatePointAroundPivot(transform.position, rot);
                var obj = placement.SpawnObject(
                    pos,
                    name,
                    rot,
                    scale
                );
                PlacementManager.PrefabPlacements[placement.GetId() + name] = placement;
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
            var clone = block.Clone(name);
            clone.Setup(false);
            switch (clone)
            {
                case BroadcastBlock bb:
                    if (!bb.Local) continue;
                    bb.ActualEventName = ((BroadcastBlock)block).EventName;
                    bb.TargetPrefab = gameObject;
                    break;
                case ReceiveBlock rb:
                    if (!rb.Local) continue;
                    rb.ActualEventName = ((ReceiveBlock)block).EventName;
                    _receivers[rb.ActualEventName] = rb;
                    break;
                case ConstantBlock cb:
                    if (!cb.Public) continue;
                    if (_constants.TryGetValue(block.BlockId, out var value)) cb.Load(value);
                    break;
            }
        }

        if (isAPreview)
        {
            transform.SetRotation2D(rot);
            transform.localScale *= scale;
        }
    }

    public void Receive(string eName)
    {
        if (!_receivers.TryGetValue(eName, out var receiver)) return;
        receiver.Event("OnReceive");
    }
}
