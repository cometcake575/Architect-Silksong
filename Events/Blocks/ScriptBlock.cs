using System;
using System.Collections.Generic;
using System.Linq;
using Architect.Editor;
using Architect.Events.Blocks.Config;
using Architect.Events.Blocks.Config.Types;
using Architect.Events.Blocks.Objects;
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
    private static readonly Sprite Input = ResourceUtils.LoadSpriteResource(
        "Flowcharts.input", ppu: 15, filterMode: FilterMode.Point
    );
    private static readonly Sprite Output = ResourceUtils.LoadSpriteResource(
        "Flowcharts.output", ppu: 15, filterMode: FilterMode.Point
    );

    protected virtual IEnumerable<string> Inputs => [];
    protected virtual IEnumerable<string> Outputs => [];
    protected virtual IEnumerable<(string, string)> OutputVars => [];
    protected virtual IEnumerable<(string, string)> InputVars => [];

    public List<ConfigType> Config;
    protected Dictionary<string, ConfigValue> CurrentConfig = [];
    
    protected virtual int InputCount => Inputs.Count();
    protected virtual int OutputCount => Outputs.Count();
    protected int InputVarCount => InputVars.Count();
    protected int OutputVarCount => OutputVars.Count();
    protected int ConfigCount => Config?.Count ?? 0;
    
    protected abstract Color Color { get; }
    protected abstract string Name { get; }

    public Dictionary<string, List<(string, string)>> EventMap = [];
    public Dictionary<string, (string, string)> VarMap = [];
    public string BlockId = Guid.NewGuid().ToString();

    public string Type;

    public virtual bool IsValid => true;
    
    public Vector2 Position;

    [CanBeNull] protected GameObject BlockObject;
    public ScriptBlockInstance BlockInstance;

    public void Setup(bool visual, bool newBlock = false)
    {
        ScriptManager.Blocks[BlockId] = this;
        if (visual) SetupBlock(newBlock);
        else
        {
            foreach (var cfg in CurrentConfig.Values) cfg.Setup(this);
            SetupReference();
        }
    }
    
    public void LateSetup()
    {
        if (!EditManager.IsEditing) return;
        if (!IsValid) return;
        foreach (var (source, links) in EventMap)
        {
            foreach (var (block, trigger) in links) 
                ScriptManager.MakeLink(this, source, ScriptManager.Blocks[block], trigger, 
                    ScriptManager.Connection.LinkType.Event);
        }

        foreach (var (source, (block, trigger)) in VarMap)
        {
            ScriptManager.MakeLink(this, source, ScriptManager.Blocks[block], trigger,
                ScriptManager.Connection.LinkType.Var);
        }
    }

    protected T GetVariable<T>(string id)
    {
        var (blockId, targetId) = VarMap[id];
        return (T)ScriptManager.Blocks[blockId].GetValue(targetId);
    }

    [CanBeNull] protected virtual object GetValue(string id) => null;

    protected void SetupBlock(bool newBlock)
    {
        var height = 50 * (1 + Math.Max(ConfigCount/2, Math.Max(InputCount, OutputCount)));
        var width = 50 * Math.Max(5, 1 + OutputVarCount + InputVarCount);

        var img = UIUtils.MakeImage(
            "Script Block",
            ScriptEditorUI.BlocksParent,
            Position,
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(width, height));
        BlockObject = img.gameObject;
        img.sprite = FlowchartBlock;
        img.type = Image.Type.Sliced;
        img.color = Color;

        BlockInstance = BlockObject.AddComponent<ScriptBlockInstance>();
        var txt = UIUtils.MakeLabel(
            Name,
            BlockObject,
            new Vector2(10, height / 2f + 15),
            new Vector2(0, 0.5f),
            new Vector2(0, 0.5f));

        BlockInstance.Block = this;
        ((RectTransform)txt.transform).pivot = new Vector2(0, 0.5f);
        txt.textComponent.fontSize = 20;
        txt.textComponent.alignment = TextAnchor.MiddleLeft;
        BlockInstance.text = txt.textComponent;
        BlockInstance.img = img;

        // Config
        if (Config != null)
        {
            var y = 0;

            var configArea = new GameObject("Config")
            {
                transform =
                {
                    parent = BlockObject.transform,
                    localScale = new Vector3(1.8f, 1.8f)
                }
            };
            var lp = configArea.AddComponent<RectTransform>();
            lp.anchorMax = Vector2.one;
            lp.anchorMin = new Vector2(0, 1);
            lp.offsetMax = Vector2.zero;
            lp.offsetMin = Vector2.zero;

            foreach (var type in Config)
            {
                var uiImg = UIUtils.MakeImage(
                    "Image",
                    configArea,
                    new Vector2(0, y),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(250, 25)
                );
                uiImg.color = new Color(0, 0, 0, 0.5f);
                uiImg.sprite = UIUtils.Square;

                var cfgTxt = UIUtils.MakeLabel("Config Title", configArea, new Vector3(105, y),
                    Vector2.zero, Vector2.zero).textComponent;
                cfgTxt.text = type.Name;
                cfgTxt.fontSize = 8;
                cfgTxt.alignment = TextAnchor.MiddleLeft;

                var (btn, _) = UIUtils.MakeTextButton("Config Apply", "Apply", configArea,
                    new Vector3(175, y), Vector2.zero, Vector2.zero);
                btn.interactable = false;

                if (newBlock && type.GetDefaultValue() != null) CurrentConfig[type.Id] = type.GetDefaultValue();
                var inp = type.CreateInput(configArea, btn, new Vector3(130, y),
                    CurrentConfig.GetValueOrDefault(type.Id)?.SerializeValue());

                btn.onClick.AddListener(Apply);

                uiImg.transform.SetAsFirstSibling();

                y -= 14;
                continue;

                void Apply()
                {
                    btn.interactable = false;
                    var val = inp.GetValue();
                    if (val.Length == 0)
                        CurrentConfig.Remove(type.Id);
                    else
                        CurrentConfig[type.Id] = type.Deserialize(inp.GetValue());
                }
            }

            lp.anchoredPosition = new Vector2(0, -25);
        }

        // Event inputs
        var i = 0;
        foreach (var input in Inputs)
        {
            i++;
            var inputImg = UIUtils.MakeImage(
                "Input",
                BlockObject,
                new Vector2(0, i * -50),
                new Vector2(0, 1),
                new Vector2(0, 1),
                new Vector2(40, 40));

            var inputTxt = UIUtils.MakeLabel(
                input,
                BlockObject,
                new Vector2(-55, i * -50),
                new Vector2(0, 1),
                new Vector2(0, 1));
            inputTxt.textComponent.text = this is ObjectBlock ? EventManager.GetReceiverType(input).Name : input;
            inputTxt.textComponent.alignment = TextAnchor.MiddleRight;

            inputImg.sprite = Input;
            inputImg.color = Color;

            var eEnd = inputImg.gameObject.AddComponent<ScriptManager.EventEnd>();
            eEnd.id = input;
            eEnd.Block = this;

            BlockInstance.LinkEnds[eEnd.id] = eEnd.transform;
        }

        // Event outputs
        i = 0;
        foreach (var output in Outputs)
        {
            i++;
            var outputImg = UIUtils.MakeImage(
                "Output",
                BlockObject,
                new Vector2(0, i * -50),
                new Vector2(1, 1),
                new Vector2(1, 1),
                new Vector2(40, 40));

            var outputTxt = UIUtils.MakeLabel(
                output,
                BlockObject,
                new Vector2(55, i * -50),
                new Vector2(1, 1),
                new Vector2(1, 1));

            outputTxt.textComponent.text = output;
            outputTxt.textComponent.alignment = TextAnchor.MiddleLeft;

            outputImg.sprite = Output;
            outputImg.color = Color;

            var eStart = outputImg.gameObject.AddComponent<ScriptManager.EventStart>();
            eStart.img = outputImg;
            eStart.color = Color;
            eStart.id = output;
            eStart.Block = this;

            BlockInstance.LinkStarts[eStart.id] = eStart.transform;
        }

        // Variable inputs
        i = InputVarCount;
        foreach (var (input, type) in InputVars)
        {
            var inputImg = UIUtils.MakeImage(
                "Input Var",
                BlockObject,
                new Vector2(i * -50, 0),
                new Vector2(1, 1),
                new Vector2(1, 1),
                new Vector2(40, 40));

            var inputTxt = UIUtils.MakeLabel(
                input,
                BlockObject,
                new Vector2(i * -50, 45),
                new Vector2(1, 1),
                new Vector2(1, 1));

            inputTxt.textComponent.text = input + "\n" + type;
            inputTxt.textComponent.alignment = TextAnchor.MiddleCenter;

            inputImg.sprite = Input;
            inputImg.color = Color;
            inputImg.transform.SetRotation2D(270);
            i--;

            var eEnd = inputImg.gameObject.AddComponent<ScriptManager.VarEnd>();
            eEnd.id = input;
            eEnd.type = type;
            eEnd.Block = this;

            BlockInstance.LinkStarts[eEnd.id] = eEnd.transform;
        }

        // Variable outputs
        i = OutputVarCount;
        foreach (var (output, type) in OutputVars)
        {
            var outputImg = UIUtils.MakeImage(
                "Output Var",
                BlockObject,
                new Vector2(i * -50, 0),
                new Vector2(1, 0),
                new Vector2(1, 0),
                new Vector2(40, 40));

            var outputTxt = UIUtils.MakeLabel(
                output,
                BlockObject,
                new Vector2(i * -50, -45),
                new Vector2(1, 0),
                new Vector2(1, 0));

            outputTxt.textComponent.text =
                (this is ObjectBlock ? EventManager.GetOutputType(output).Name : output) + "\n" + type;
            outputTxt.textComponent.alignment = TextAnchor.MiddleCenter;

            outputImg.sprite = Output;
            outputImg.color = Color;
            outputImg.transform.SetRotation2D(270);
            i--;

            var eStart = outputImg.gameObject.AddComponent<ScriptManager.VarStart>();
            eStart.id = output;
            eStart.Block = this;
            eStart.img = outputImg;
            eStart.color = Color;
            eStart.type = type;

            BlockInstance.LinkEnds[eStart.id] = eStart.transform;
        }
    }

    public class ScriptBlockInstance : Deletable, IDragHandler, IBeginDragHandler
    {
        public ScriptBlock Block;
        public Text text;
        public Image img;

        public readonly Dictionary<string, Transform> LinkStarts = [];
        public readonly Dictionary<string, Transform> LinkEnds = [];

        private Vector2 _offset;

        private void Update()
        {
            text.text = Block.Name;
            img.color = Block.Color;
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

        public override void Delete()
        {
            foreach (var (sourceEvent, target) in Block.EventMap)
            {
                foreach (var (targetBlock, targetEvent) in target.ToArray())
                {
                    ScriptManager.DestroyLink(Block.BlockId, sourceEvent, targetBlock, targetEvent, ScriptManager.Connection.LinkType.Event);
                }
            }

            foreach (var (sourceEvent, (targetBlock, targetEvent)) in Block.VarMap.ToArray())
            {
                ScriptManager.DestroyLink(Block.BlockId, sourceEvent, targetBlock, targetEvent,
                    ScriptManager.Connection.LinkType.Var);
            }

            foreach (var block in ScriptManager.Blocks.Values)
            {
                foreach (var (sourceEvent, target) in block.EventMap)
                {
                    foreach (var (_, targetEvent) in target
                                 .Where(o => o.Item1 == Block.BlockId).ToArray())
                    {
                        ScriptManager.DestroyLink(block.BlockId, sourceEvent, Block.BlockId, targetEvent, ScriptManager.Connection.LinkType.Event);
                    }
                }

                foreach (var (sourceEvent, (id, targetEvent)) in block.VarMap.ToArray())
                {
                    if (id != Block.BlockId) continue;
                    ScriptManager.DestroyLink(block.BlockId, sourceEvent, Block.BlockId, targetEvent,
                        ScriptManager.Connection.LinkType.Var);
                }
            }
            ScriptManager.Blocks.Remove(Block.BlockId);
            PlacementManager.GetLevelData().ScriptBlocks.Remove(Block);
            Destroy(gameObject);
        }
    }

    protected virtual Dictionary<string, string> SerializeExtraData() => [];

    protected virtual void DeserializeExtraData(Dictionary<string, string> data) { }

    protected virtual void SetupReference() { }

    public void Event(string name)
    {
        if (EditManager.IsEditing) return;
        if (!EventMap.TryGetValue(name, out var targets)) return;
        foreach (var (block, trigger) in targets.ToArray())
        {
            if (!ScriptManager.Blocks.TryGetValue(block, out var b)) continue;
            if (b.IsValid) b.Trigger(trigger);
        }
    }

    protected virtual void Trigger(string trigger) { }
    
    public void DestroyObject()
    {
        if (BlockObject) Object.Destroy(BlockObject);
    }

    public void Delete()
    {
        if (!BlockObject) return;
        BlockObject.GetComponent<ScriptBlockInstance>().Delete();
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
                
            writer.WritePropertyName("vars");
            serializer.Serialize(writer, value.VarMap);
            
            writer.WritePropertyName("type");
            writer.WriteValue(value.Type);

            if (value.CurrentConfig.Count > 0)
            {
                writer.WritePropertyName("config");
                serializer.Serialize(writer, value.CurrentConfig.Values
                    .ToDictionary(c => c.GetTypeId(), c =>
                    c.SerializeValue()));
            }
            
            writer.WritePropertyName("ext");
            serializer.Serialize(writer, value.SerializeExtraData());

            if (value is ObjectBlock ob)
            {
                writer.WritePropertyName("object");
                writer.WriteValue(ob.TargetId);
                
                writer.WritePropertyName("object_type");
                writer.WriteValue(ob.TypeId);
            }

            writer.WriteEndObject();
        }

        public override ScriptBlock ReadJson(JsonReader reader, Type objectType, ScriptBlock existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            string type = null;
            string blockId = null;
            Vector3 pos = default;
            Dictionary<string, List<(string, string)>> eventMap = [];
            Dictionary<string, (string, string)> varMap = [];
            Dictionary<string, string> ext = [];
            Dictionary<string, ConfigValue> config = [];

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
                    case "pos":
                        reader.Read();
                        pos = serializer.Deserialize<Vector2>(reader);
                        break;
                    case "events":
                        reader.Read();
                        eventMap = serializer.Deserialize<Dictionary<string, List<(string, string)>>>(reader);
                        break;
                    case "vars":
                        reader.Read();
                        varMap = serializer.Deserialize<Dictionary<string, (string, string)>>(reader);
                        break;
                    case "ext":
                        reader.Read();
                        ext = serializer.Deserialize<Dictionary<string, string>>(reader);
                        break;
                    case "config":
                        reader.Read();
                        config = DeserializeConfig(serializer.Deserialize<Dictionary<string, string>>(reader));
                        break;
                }
            }

            var o = type == "object" ? new ObjectBlock() : ScriptManager.BlockTypes[type!]();
            o.DeserializeExtraData(ext);
            o.Type = type;
            o.BlockId = blockId;
            o.Position = pos;
            o.EventMap = eventMap;
            o.VarMap = varMap;
            o.CurrentConfig = config;
            return o;
        }

        private static Dictionary<string, ConfigValue> DeserializeConfig(Dictionary<string, string> data)
        {
            var config = new Dictionary<string, ConfigValue>();

            foreach (var pair in data)
            {
                config[pair.Key] = ConfigurationManager.DeserializeConfigValue(pair.Key, pair.Value);
            }
            
            return config;
        }
    }
}
