using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Architect.Editor;
using Architect.Events.Blocks.Events;
using Architect.Events.Blocks.Objects;
using Architect.Events.Blocks.Operators;
using Architect.Placements;
using Architect.Utils;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Architect.Events.Blocks;

[JsonConverter(typeof(ScriptBlockConverter))]
public abstract class ScriptBlock
{
    private static readonly Sprite FlowchartBlock = ResourceUtils.LoadSpriteResource(
        "Flowcharts.flowchart_block",
        border: new Vector4(10, 10, 10, 10)
    );
    
    public static readonly Dictionary<string, ScriptBlock> Blocks = [];
    
    public abstract IEnumerable<string> Inputs { get; }
    public abstract IEnumerable<string> Outputs { get; }

    protected abstract int InputCount { get; }
    protected abstract int OutputCount { get; }
    
    protected abstract Color Color { get; }
    protected abstract string Name { get; }

    public Dictionary<string, List<(string, string)>> EventMap = [];
    public string BlockId = Guid.NewGuid().ToString();

    protected abstract string Type { get; }
    
    public Vector2 Position;

    [CanBeNull] protected GameObject BlockObject;

    private bool _willDelete;

    public void Setup()
    {
        if (_willDelete) return;
        if (!SetupReference())
        {
            _willDelete = true;
            ScheduleDelete();
            return;
        }
        SetupBlock();
        Blocks[BlockId] = this;
    }

    public void SetupBlock()
    {
        var height = 50 * (1 + Mathf.Max(InputCount, OutputCount));
        
        var img = UIUtils.MakeImage(
            "Script Block",
            ScriptEditorUI.BlocksParent,
            Position,
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(300, height));
        BlockObject = img.gameObject;
        img.sprite = FlowchartBlock;
        img.type = Image.Type.Sliced;
        img.color = Color;

        var sbi = BlockObject.AddComponent<ScriptBlockInstance>();
        var txt = UIUtils.MakeLabel(
            Name,
            BlockObject,
            new Vector2(0, height / 2f + 15),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f));

        sbi.Block = this;
        txt.textComponent.fontSize = 20;
        txt.textComponent.alignment = TextAnchor.MiddleCenter;
        var rt = txt.GetComponent<RectTransform>();
        
        sbi.text = txt.textComponent;
    }

    public class ScriptBlockInstance : MonoBehaviour, IDragHandler, IBeginDragHandler
    {
        public ScriptBlock Block;
        public Text text;

        private Vector2 _offset;

        private void Update()
        {
            text.text = Block.Name;
        }

        public void OnDrag(PointerEventData eventData)
        {
            transform.position = eventData.position + _offset;
            Block.Position = transform.localPosition;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _offset = (Vector2)transform.position - eventData.position;
            transform.SetAsLastSibling();
        }
    }

    protected abstract bool SetupReference();

    protected void Event(string name)
    {
        if (EditManager.IsEditing) return;
        if (!EventMap.TryGetValue(name, out var targets)) return;
        foreach (var (block, trigger) in targets)
        {
            if (!Blocks.TryGetValue(block, out var b)) continue;
            b.Trigger(trigger);
        }
    }

    protected abstract void Trigger(string trigger);
    
    public void DestroyObject()
    {
        if (BlockObject) Object.Destroy(BlockObject);
    }

    public void ScheduleDelete() => ArchitectPlugin.Instance.StartCoroutine(Delete());

    private IEnumerator Delete()
    {
        DestroyObject();
        yield return null;
        if (PlacementManager.GetLevelData().ScriptBlocks.Contains(this)) 
            PlacementManager.GetLevelData().ScriptBlocks.Remove(this);
        foreach (var v in PlacementManager.GetLevelData().ScriptBlocks
                     .SelectMany(block => block.EventMap.Values))
        {
            v.RemoveAll(o => o.Item1 == BlockId);
        }
    }
    
    public class ScriptBlockConverter : JsonConverter<ScriptBlock>
    {
        public override void WriteJson(JsonWriter writer, ScriptBlock value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            
            // Position
            writer.WritePropertyName("pos");
            serializer.Serialize(writer, value.Position);
                
            writer.WritePropertyName("block_id");
            writer.WriteValue(value.BlockId);
                
            writer.WritePropertyName("events");
            serializer.Serialize(writer, value.EventMap);
            
            writer.WritePropertyName("type");
            writer.WriteValue(value.Type);

            if (value is ObjectBlock ob)
            {
                writer.WritePropertyName("object");
                writer.WriteValue(ob.TargetId);
            }

            writer.WriteEndObject();
        }

        public override ScriptBlock ReadJson(JsonReader reader, Type objectType, ScriptBlock existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            string type = null;
            string objectId = null;
            string blockId = null;
            Vector3 pos = default;
            Dictionary<string, List<(string, string)>> eventMap = [];

            while (reader.Read())
            {
                if (reader.Value is not string value) break;
                switch (value)
                {
                    case "type":
                        reader.ReadAsString();
                        type = reader.Value as string;
                        break;
                    case "block_id":
                        reader.ReadAsString();
                        blockId = reader.Value as string;
                        break;
                    case "object":
                        reader.ReadAsString();
                        objectId = reader.Value as string;
                        break;
                    case "pos":
                        reader.Read();
                        pos = serializer.Deserialize<Vector2>(reader);
                        break;
                    case "events":
                        reader.Read();
                        eventMap = serializer.Deserialize<Dictionary<string, List<(string, string)>>>(reader);
                        break;
                }
            }

            ScriptBlock o = type switch
            {
                "object" => new ObjectBlock(objectId),
                "if" => new IfBlock(),
                "start" => new StartBlock(),
                _ => throw new ArgumentOutOfRangeException()
            };
            o.BlockId = blockId;
            o.Position = pos;
            o.EventMap = eventMap;
            return o;
        }
    }
}
