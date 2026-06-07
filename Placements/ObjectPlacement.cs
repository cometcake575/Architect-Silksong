using System;
using System.Collections.Generic;
using System.Linq;
using Architect.Behaviour.Fixers;
using Architect.Config;
using Architect.Config.Types;
using Architect.Editor;
using Architect.Events;
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
    int layer,
    (string, string)[] broadcasters,
    (string, string, int)[] receivers,
    ConfigValue[] config)
{
    public string ID = id;
    
    private GameObject _previewObject;
    private PreviewUtils.Preview _preview;

    public bool Touching(Vector3 mousePos)
    {
        if (!_preview) return false;

        var pos = EditManager.GetWorldPos(mousePos, offset:_previewObject.transform.position.z);

        return _preview.Touching(pos);
    }

    public bool IsWithinZone(Vector2 pos1, Vector2 pos2)
    {
        if (!_previewObject || IsLocked()) return false;
        
        var withinX = (_position.x > pos1.x && _position.x < pos2.x) || (_position.x < pos1.x && _position.x > pos2.x);
        var withinY = (_position.y > pos1.y && _position.y < pos2.y) || (_position.y < pos1.y && _position.y > pos2.y);
        return withinX && withinY;
    }

    private float _rotation = rotation;
    private float _scale = scale;
    private bool _flipped = flipped;

    public string GetId() => ID;
    public PlaceableObject GetPlacementType() => type;
    public Vector3 GetPos() => _position;
    public bool IsFlipped() => _flipped;
    public float GetRotation() => _rotation;
    public void SetRotation(float rot) => _rotation = rot;
    public bool GetFlipped() => _flipped;
    public void SetFlipped(bool flip) => _flipped = flip;
    public void SetScale(float sc) => _scale = sc;
    public float GetScale() => _scale;

    public bool Locked = locked;

    private bool IsLocked()
    {
        return Locked || !IsCurrentLayer();
    }

    public bool IsCurrentLayer() => _layer == EditManager.Layer;
    
    private bool ShowCurrentLayer() => IsCurrentLayer() || 
                                       EditManager.FlippedLayers.Contains(_layer) != EditManager.ShowLayersByDefault;
    
    public void SetLayer(int layer) => _layer = layer;

    public void ToggleLocked()
    {
        Locked = !Locked;
        RefreshColour();
    }

    private bool _hovered;
    private bool _dragged;

    public void RefreshColour()
    {
        if (!_preview) return;

        var r = 1f;
        var g = 1f;
        var b = 1f;
        if (_dragged)
        {
            r *= 0.2f;
            b *= 0.2f;
        } else if (_hovered)
        {
            r *= 0.2f;
            g *= 0.2f;
        }
        _preview.Settings = new PreviewUtils.PreviewSettings(r, g, b, ShowCurrentLayer() ? IsLocked() ? 0.2f : 0.5f : 0);
    }

    public readonly (string, string)[] Broadcasters = broadcasters;
    public readonly (string, string, int)[] Receivers = receivers;
    public readonly ConfigValue[] Config = config;

    private Vector3 _dragOffset;
    
    private Vector3 _position = position;
    private Vector3 _offset;
    private Vector3 _oldPos;

    private int _layer = layer;

    public void SetDraggedColour()
    {
        _dragged = true;
        RefreshColour();
    }
    
    public void SetHoverColour()
    {
        _hovered = true;
        RefreshColour();
    }

    public void ClearDraggedColour()
    {
        _dragged = false;
        RefreshColour();
    }
    
    public void ClearHoverColour()
    {
        _hovered = false;
        RefreshColour();
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

    private bool _spawningPreview;

    public GameObject PlaceGhost(Vector3 pos = default, bool store = true, string extraId = null)
    {
        if (pos == default) pos = _position;
        
        _spawningPreview = true;
        var obj = SpawnObject(pos, extraId);
        _previewObject = obj;
        
        if (type.SpritePreview)
        {
            _previewObject = PreviewUtils.MakeSpritePreview(
                _previewObject, 
                type, 
                _flipped, _rotation, _scale, 
                out _offset);
            _previewObject.transform.position = pos + _offset;
            _previewObject.AddComponent<PreviewObject>().offset = _offset;
        }
        
        _preview = _previewObject.AddComponent<PreviewUtils.Preview>();
        _preview.Setup(type);
        
        if (store) PlacementManager.Objects[ID] = _previewObject;
        
        RefreshColour();
        _spawningPreview = false;
        
        obj.SetActive(true);
            
        foreach (var rb2d in _previewObject.GetComponentsInChildren<Rigidbody2D>(true))
        {
            rb2d.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        foreach (var configVal in Config.OrderBy(configVal => configVal.GetPriority())) 
            configVal.SetupPreview(_previewObject, ConfigurationManager.PreviewContext.Placement);
        
        return _previewObject;
    }

    public class PreviewObject : MonoBehaviour
    {
        public Vector3 offset;
    }

    public GameObject SpawnObject(Vector3 pos = default, string extraId = null, float extraRot = 0,
        float extraScale = 1, bool extraFlip = false)
    {
        if (!type.Prefab)
        {
            ArchitectPlugin.Logger.LogError($"Error - Prefab of {type.GetName()} is missing, cannot spawn");
            return null;
        }

        var cId = ID;
        if (extraId != null) cId += extraId;

        if (pos == default) pos = _position;
        else pos.z = _position.z;
        var wasPrefabActive = type.Prefab.activeSelf;
        type.Prefab.SetActive(false);
        var obj = Object.Instantiate(type.Prefab, pos, type.Prefab.transform.rotation);
        if (wasPrefabActive) type.Prefab.SetActive(true);
        obj.name = $"[Architect] {type.GetName()} ({cId})";

        FixId<int>(obj, cId);
        FixId<bool>(obj, cId);
        
        if (_spawningPreview) obj.AddComponent<MiscFixers.PreviewState>();
        
        type.PostSpawnAction?.Invoke(obj);

        if (type.FlipAction != null) type.FlipAction.Invoke(obj, _flipped != extraFlip);
        else if (_flipped != extraFlip) obj.transform.SetScaleX(-obj.transform.GetScaleX());

        if (type.RotateAction != null) type.RotateAction.Invoke(obj, _rotation + extraRot);
        else obj.transform.SetRotation2D(_rotation + obj.transform.GetRotation2D() + extraRot);

        if (type.ScaleAction != null) type.ScaleAction.Invoke(obj, _scale * extraScale);
        else obj.transform.localScale *= _scale * extraScale;

        foreach (var configVal in Config.Where(configVal => configVal.GetPriority() < 0)
                     .OrderBy(configVal => configVal.GetPriority())) configVal.Setup(obj, extraId);

        if (!_spawningPreview)
        {
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
        }

        foreach (var configVal in Config.Where(configVal => configVal.GetPriority() >= 0)
                     .OrderBy(configVal => configVal.GetPriority())) configVal.Setup(obj, extraId);

        return obj;
    }

    private static void FixId<T>(GameObject obj, string cId) where T : IEquatable<T>
    {
        var comp = obj.GetComponent<PersistentItem<T>>();
        if (comp) comp.ItemData.ID = cId;
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

            writer.WritePropertyName("layer");
            writer.WriteValue(placement._layer);

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
            var layer = 0;
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
                                case "layer":
                                    reader.ReadAsInt32();
                                    layer = (int)reader.Value;
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
            if (!PlaceableObject.RegisteredObjects.TryGetValue(id, out var obj)) return null;
            var placement = new ObjectPlacement(obj,
                pos, pid, flipped, rotation, scale, locked, layer, broadcasters, receivers, config);

            return placement;
        }

        private static ConfigValue[] DeserializeConfig(Dictionary<string, string> data)
        {
            List<ConfigValue> config = [];
            try
            {
                config.AddRange(data
                    .Select(pair => ConfigurationManager.DeserializeConfigValue(pair.Key, pair.Value))
                    .Where(cfg => cfg != null));

                return config.ToArray();
            }
            catch (Exception)
            {
                return [];
            }
        }
    }

    public void Destroy()
    {
        if (_previewObject) Object.Destroy(_previewObject);
        PlacementManager.GetLevelData().Placements.Remove(this);
        PlacementManager.Objects.Remove(ID);
    }
}