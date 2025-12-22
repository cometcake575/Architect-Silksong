using System;
using System.Collections.Generic;
using System.Linq;
using Architect.Behaviour.Utility;
using Architect.Config;
using Architect.Config.Types;
using Architect.Editor;
using Architect.Events;
using Architect.Events.Blocks;
using Architect.Events.Blocks.Objects;
using Architect.Objects.Placeable;
using Architect.Utils;
using Newtonsoft.Json;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Architect.Placements;

[JsonConverter(typeof(ObjectPlacementConverter))]
public class ObjectPlacement(
    PlaceableObject type, 
    Vector3 position, 
    string id, 
    bool flipped, 
    float rotation,
    float scale,
    bool locked,
    (string, string)[] broadcasters,
    (string, string, int)[] receivers,
    ConfigValue[] config)
{
    private static readonly Color DefaultColour = new (1, 1, 1, 0.5f);
    private static readonly Color DraggedColour = new (0.2f, 1, 0.2f, 0.5f);
    private static readonly Color HoverColour = new (0.2f, 0.2f, 1, 0.5f);
    
    private Color? _previewColour;
    
    private GameObject _previewObject;
    private SpriteRenderer _previewRenderer;

    public bool Touching(Vector3 mousePos)
    {
        if (!_previewObject || !_previewRenderer || !_previewRenderer.sprite) return false;

        var pos = EditManager.GetWorldPos(mousePos, offset:_previewObject.transform.position.z);
        
        var size = _previewRenderer.sprite.bounds.size;
        var width = size.x / 2 * Mathf.Abs(_previewObject.transform.GetScaleX());
        var height = size.y / 2 * Mathf.Abs(_previewObject.transform.GetScaleY());

        var objPos = _previewObject.transform.position;
        var objRotation = _previewObject.transform.rotation;

        var localPos = Quaternion.Inverse(objRotation) * (pos - objPos);

        return Mathf.Abs(localPos.x) <= width && Mathf.Abs(localPos.y) <= height;
    }

    public bool IsWithinZone(Vector2 pos1, Vector2 pos2)
    {
        if (!_previewObject || Locked) return false;
        
        var withinX = (_position.x > pos1.x && _position.x < pos2.x) || (_position.x < pos1.x && _position.x > pos2.x);
        var withinY = (_position.y > pos1.y && _position.y < pos2.y) || (_position.y < pos1.y && _position.y > pos2.y);
        return withinX && withinY;
    }

    public string GetId() => id;
    public PlaceableObject GetPlacementType() => type;
    public Vector3 GetPos() => _position;
    public bool IsFlipped() => flipped;
    public float GetRotation() => rotation;
    public float GetScale() => scale;

    public bool Locked = locked;

    public void ToggleLocked()
    {
        Locked = !Locked;
        if (_previewRenderer) _previewRenderer.color = _previewRenderer.color.Where(a: Locked ? 0.2f : 0.5f);
    }

    public readonly (string, string)[] Broadcasters = broadcasters;
    public readonly (string, string, int)[] Receivers = receivers;
    public readonly ConfigValue[] Config = config;

    private Vector3 _dragOffset;
    
    private Vector3 _position = position;
    private Vector3 _offset;
    private Vector3 _oldPos;
    
    public void SetDraggedColour()
    {
        if (_previewRenderer)
        {
            _previewColour = _previewRenderer.color;
            _previewRenderer.color = DraggedColour;
        }
    }

    public void SetHoverColour()
    {
        if (_previewRenderer)
        {
            _previewColour = _previewRenderer.color;
            _previewRenderer.color = HoverColour;
        }
    }

    public void ClearColour()
    {
        if (_previewRenderer) _previewRenderer.color = _previewColour ?? DefaultColour;
    }

    // Begins dragging the object, storing its old position and the offset from the cursor
    public void StartMove(Vector3 cursorWorldPos)
    {
        _dragOffset = _position - cursorWorldPos;
        _oldPos = _position;
    }

    // Finishes dragging the object, resetting the offset and returning its previous position for undo/redo
    public Vector3 FinishMove()
    {
        _dragOffset = Vector3.zero;
        return _oldPos;
    }

    // Updates the current position of the object as it is dragged
    public void Move(Vector3 newWorldPos)
    {
        _position = newWorldPos + _dragOffset;
        if (_previewObject) _previewObject.transform.position = (_position + _offset)
            .Where(z: _previewObject.transform.position.z);
    }

    public void PlaceGhost()
    {
        var rot = rotation + type.Rotation;
        
        _previewObject = new GameObject($"[Architect] {type.GetName()} ({id}) Preview")
            { transform = { localScale = type.LossyScale } };

        _previewRenderer = _previewObject.AddComponent<SpriteRenderer>();

        _previewRenderer.color = DefaultColour;
        
        _previewRenderer.sprite = type.Sprite;
        _previewObject.transform.localScale *= scale;
        _previewObject.transform.SetRotation2D(rot + type.ChildRotation + type.Tk2dRotation);

        if (type.Preview)
        {
            var preview = SpawnObject();
            if (preview)
            {
                preview.transform.SetParent(_previewObject.transform);

                foreach (var renderer in preview.GetComponentsInChildren<Renderer>())
                {
                    renderer.enabled = false;
                }

                foreach (var behaviour in preview.GetComponentsInChildren<PreviewableBehaviour>())
                {
                    behaviour.isAPreview = true;
                }
                
                preview.SetActive(true);
                preview.transform.localPosition = Vector3.zero;
                preview.transform.localScale = Vector3.one;
            }
        }

        _offset = PreviewUtils.FixPreview(_previewRenderer, type, flipped, rot, scale);
        _previewObject.transform.position = _position + _offset;

        _previewObject.AddComponent<PreviewObject>().offset = _offset;
        
        foreach (var config in Config.OrderBy(configVal => configVal.GetPriority()))
        {
            config.SetupPreview(_previewObject, ConfigurationManager.PreviewContext.Placement);
        }

        if (Locked) _previewRenderer.color = _previewRenderer.color.Where(a: 0.2f);

        PlacementManager.Objects[id] = _previewObject; 
    }

    public class PreviewObject : MonoBehaviour
    {
        public Vector3 offset;
    }

    public GameObject SpawnObject(Vector3 pos = default)
    {
        if (!type.Prefab)
        {
            ArchitectPlugin.Logger.LogError($"Error - Prefab of {type.GetName()} is missing, cannot spawn");
            return null;
        }

        if (pos == default) pos = _position;
        else pos.z = _position.z;
        var obj = Object.Instantiate(type.Prefab, pos, type.Prefab.transform.rotation);
        obj.name = $"[Architect] {type.GetName()} ({id})";

        FixId<int>(obj);
        FixId<bool>(obj);
        
        type.PostSpawnAction?.Invoke(obj);
        
        if (type.FlipAction != null) type.FlipAction.Invoke(obj, flipped);
        else if (flipped) obj.transform.SetScaleX(-obj.transform.GetScaleX());
        
        if (type.RotateAction != null) type.RotateAction.Invoke(obj, rotation);
        else obj.transform.SetRotation2D(rotation + obj.transform.GetRotation2D());
        
        if (type.ScaleAction != null) type.ScaleAction.Invoke(obj, scale);
        else obj.transform.localScale *= scale;

        foreach (var configVal in Config.Where(configVal => configVal.GetPriority() < 0)
                     .OrderBy(configVal => configVal.GetPriority())) configVal.Setup(obj);

        obj.SetActive(true);
        
        foreach (var receiver in Receivers)
        {
            var eri = obj.AddComponent<LegacyReceiver>();
            eri.eventName = receiver.Item1.ToLower();
            eri.ReceiverType = EventManager.GetReceiverType(receiver.Item2);
            eri.requiredCalls = receiver.Item3;
            
            EventManager.RegisterReceiver(eri);
        }

        foreach (var broadcaster in Broadcasters)
        {
            var ebi = obj.AddComponent<LegacyBroadcaster>();
            ebi.triggerName = broadcaster.Item1;
            ebi.eventName = broadcaster.Item2.ToLower();
        }

        foreach (var configVal in Config.Where(configVal => configVal.GetPriority() >= 0)
                     .OrderBy(configVal => configVal.GetPriority())) configVal.Setup(obj);
        return obj;
    }

    public void FixId<T>(GameObject obj) where T : IEquatable<T>
    {
        var comp = obj.GetComponent<PersistentItem<T>>();
        if (comp) comp.ItemData.ID = id;
    }

    public class ObjectPlacementConverter : JsonConverter<ObjectPlacement>
    {
        public override void WriteJson(JsonWriter writer, ObjectPlacement value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            WritePlacementInfo(writer, value, serializer);

            if (value.Broadcasters.Length > 0)
            {
                writer.WritePropertyName("events");
                serializer.Serialize(writer, value.Broadcasters);
            }

            if (value.Receivers.Length > 0)
            {
                writer.WritePropertyName("listeners");
                serializer.Serialize(writer, value.Receivers);
            }

            if (value.Config.Length > 0)
            {
                writer.WritePropertyName("config");
                serializer.Serialize(writer, value.Config.ToDictionary(c => c.GetTypeId(), c =>
                    c.SerializeValue()));
            }

            writer.WriteEndObject();
        }

        private static void WritePlacementInfo(JsonWriter writer, ObjectPlacement placement, JsonSerializer serializer)
        {
            writer.WritePropertyName("placement");

            writer.WriteStartObject();
            writer.WritePropertyName("id");
            writer.WriteValue(placement.GetPlacementType().GetId());
            writer.WritePropertyName("pid");
            writer.WriteValue(placement.GetId());
            writer.WritePropertyName("pos");
            serializer.Serialize(writer, placement.GetPos());

            writer.WritePropertyName("flipped");
            writer.WriteValue(placement.IsFlipped());

            writer.WritePropertyName("locked");
            writer.WriteValue(placement.Locked);

            writer.WritePropertyName("rotation");
            writer.WriteValue(placement.GetRotation());

            writer.WritePropertyName("scale");
            writer.WriteValue(placement.GetScale());

            writer.WriteEndObject();
        }

        public override ObjectPlacement ReadJson(JsonReader reader, Type objectType, ObjectPlacement existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            var id = "";
            string pid = null;
            var pos = Vector3.zero;
            var flipped = true;
            var rotation = 0f;
            var scale = 1f;
            var locked = false;

            (string, string)[] broadcasters = [];
            (string, string, int)[] receivers = [];
            ConfigValue[] config = [];

            reader.Read();
            while (reader.TokenType == JsonToken.PropertyName)
            {
                switch (reader.Value as string)
                {
                    case "placement":
                    {
                        reader.Read();
                        reader.Read();
                        while (reader.TokenType == JsonToken.PropertyName)
                        {
                            var key = reader.Value as string;
                            switch (key)
                            {
                                case "id":
                                    reader.ReadAsString();
                                    id = reader.Value as string;
                                    break;
                                case "pid":
                                    reader.ReadAsString();
                                    pid = reader.Value as string;
                                    break;
                                case "pos":
                                    reader.Read();
                                    pos = serializer.Deserialize<Vector3>(reader);
                                    break;
                                case "flipped":
                                    reader.ReadAsBoolean();
                                    flipped = (bool)reader.Value;
                                    break;
                                case "locked":
                                    reader.ReadAsBoolean();
                                    locked = (bool)reader.Value;
                                    break;
                                case "rotation":
                                    reader.ReadAsDouble();
                                    rotation = (float)(double)reader.Value;
                                    break;
                                case "scale":
                                    reader.ReadAsDouble();
                                    scale = (float)(double)reader.Value;
                                    break;
                            }

                            reader.Read();
                        }

                        break;
                    }
                    case "events":
                        reader.Read();
                        broadcasters = serializer.Deserialize<(string, string)[]>(reader);
                        break;
                    case "listeners":
                        reader.Read();
                        receivers = serializer.Deserialize<(string, string, int)[]>(reader);
                        break;
                    case "config":
                        reader.Read();
                        config = DeserializeConfig(serializer.Deserialize<Dictionary<string, string>>(reader));
                        break;
                }

                reader.Read();
            }

            pid ??= Guid.NewGuid().ToString()[..8];
            var placement = new ObjectPlacement(PlaceableObject.RegisteredObjects[id],
                pos, pid, flipped, rotation, scale, locked, broadcasters, receivers, config);

            return placement;
        }

        private static ConfigValue[] DeserializeConfig(Dictionary<string, string> data)
        {
            var config = new ConfigValue[data.Count];

            var i = 0;
            foreach (var pair in data)
            {
                config[i] = ConfigurationManager.DeserializeConfigValue(pair.Key, pair.Value);
                i++;
            }
            
            return config;
        }
    }

    public void Destroy()
    {
        if (_previewObject) Object.Destroy(_previewObject);
        PlacementManager.GetLevelData().Placements.Remove(this);
        foreach (var o in PlacementManager.GetLevelData().ScriptBlocks
            .Where(o => o is ObjectBlock block && block.TargetId == id)) o.ScheduleDelete();
        PlacementManager.Objects.Remove(id);
    }
}