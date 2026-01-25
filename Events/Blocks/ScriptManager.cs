using System;
using System.Collections.Generic;
using Architect.Editor;
using Architect.Events.Blocks.Config.Types;
using Architect.Events.Blocks.Events;
using Architect.Events.Blocks.Objects;
using Architect.Events.Blocks.Operators;
using Architect.Events.Blocks.Outputs;
using Architect.Objects.Tools;
using Architect.Placements;
using Architect.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Architect.Events.Blocks;

public static class ScriptManager
{
    private static bool _local;

    public static bool IsLocal
    {
        get => _local;
        set
        {
            _local = value;
            ScriptEditorUI.LocalParent.SetActive(value);
            ScriptEditorUI.GlobalParent.SetActive(!value);

            if (CurrentStart)
            {
                CurrentStart.img.color = CurrentStart.color;
                CurrentStart = null;
                InSwapMode = false;
            }
        }
    }
    
    public static void Init()
    {
        EventBlocks.Init();
        OperatorBlocks.Init();
        ActionBlocks.Init();
        
        BlockTypes["object"] = () => new ObjectBlock { Type = "object" };
    }
    
    public static Start CurrentStart;
    public static bool InSwapMode;
    
    public static readonly List<(Func<ScriptBlock>, string)> InputBlocks = [];
    
    public static readonly List<(Func<ScriptBlock>, string)> ProcessBlocks = [];
    
    public static readonly List<(Func<ScriptBlock>, string)> OutputBlocks = [];
    
    public static readonly Dictionary<string, Func<ScriptBlock>> BlockTypes = [];

    public static BlockType CurrentType = BlockType.Output;
    
    public static List<(Func<ScriptBlock>, string)> CurrentBlocks
    {
        get
        {
            return CurrentType switch
            {
                BlockType.Input => InputBlocks,
                BlockType.Process => ProcessBlocks,
                BlockType.Output => OutputBlocks,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    public enum BlockType
    {
        Input,
        Process,
        Output
    }
    
    public static readonly Dictionary<string, ScriptBlock> Blocks = [];
    
    public static readonly Dictionary<(string, string, string, string), GameObject> Links = [];

    public class Start : MonoBehaviour
    {
        public Image img;
        public Color color;
        public ScriptBlock Block;
        public string id;
    }

    private static readonly Color Orange = new(0.9f, 0.7f, 0);
    
    public class EventStart : Start, IPointerDownHandler
    {
        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            if (!Block.IsValid) return;
            
            if (CurrentStart)
            {
                CurrentStart.img.color = CurrentStart.color;
                if (CurrentStart == this)
                {
                    CurrentStart = null;
                    return;
                }

                if (InSwapMode)
                {
                    foreach (var connection in CurrentStart.Block.EventMap[CurrentStart.id].ToArray())
                    {
                        if (!Input.GetKey(KeyCode.LeftAlt))
                        {
                            DestroyLink(CurrentStart.Block.BlockId, CurrentStart.id, connection.Item1, connection.Item2,
                                Connection.LinkType.Event);
                        }

                        var eMap = Block.EventMap;
                        if (!eMap.ContainsKey(id)) eMap[id] = [];
                        if (eMap[id].Contains((connection.Item1, connection.Item2))) continue;
                        eMap[id].Add((connection.Item1, connection.Item2));
                        MakeLink(Block, id, Blocks[connection.Item1], connection.Item2, Connection.LinkType.Event);
                    }

                    CurrentStart = null;
                    return;
                }

                CurrentStart = null;
            }
            
            CurrentStart = this;
            InSwapMode = Input.GetKey(KeyCode.LeftAlt);
            img.color = InSwapMode ? Orange : Color.cyan;
        }
    }
    
    public class EventEnd : MonoBehaviour, IPointerDownHandler
    {
        public ScriptBlock Block;
        public string id;
        
        public void OnPointerDown(PointerEventData eventData)
        {
            if (CurrentStart is not EventStart || InSwapMode) return;
            CurrentStart.img.color = CurrentStart.color;
            
            if (!Block.IsValid || !CurrentStart.Block.IsValid) return;

            var eMap = CurrentStart.Block.EventMap;
            if (!eMap.ContainsKey(CurrentStart.id)) eMap[CurrentStart.id] = [];
            if (eMap[CurrentStart.id].Contains((Block.BlockId, id))) return;
            eMap[CurrentStart.id].Add((Block.BlockId, id));
            
            MakeLink(CurrentStart.Block, CurrentStart.id, Block, id, Connection.LinkType.Event);
            
            CurrentStart = null;
        }
    }
    
    public class VarStart : Start, IPointerDownHandler
    {
        public string type;
        
        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            if (!Block.IsValid) return;
            
            if (CurrentStart)
            {
                CurrentStart.img.color = CurrentStart.color;
                if (CurrentStart == this)
                {
                    CurrentStart = null;
                    return;
                } 
                CurrentStart = null;
            }
            
            CurrentStart = this;
            InSwapMode = false;
            img.color = Color.cyan;
        }
    }
    
    public class VarEnd : MonoBehaviour, IPointerDownHandler
    {
        public ScriptBlock Block;
        public string id;

        public string type;
        
        public void OnPointerDown(PointerEventData eventData)
        {
            if (CurrentStart is not VarStart start) return;
            if (start.type != type) return;
            
            if (!Block.IsValid || !CurrentStart.Block.IsValid) return;

            var vMap = Block.VarMap;
            if (vMap.ContainsKey(id)) return;
            
            CurrentStart.img.color = CurrentStart.color;
            vMap[id] = (CurrentStart.Block.BlockId, CurrentStart.id);
            
            MakeLink(Block, id, CurrentStart.Block, CurrentStart.id, Connection.LinkType.Var);
            
            CurrentStart = null;
        }
    }
    
    public static void MakeLink(ScriptBlock sourceBlock, string sourceEvent, ScriptBlock block, string trigger, Connection.LinkType linkType)
    {
        if (!sourceBlock.BlockInstance.LinkStarts.TryGetValue(sourceEvent, out var start)) return;
        var source = (RectTransform)start;
        if (!block.BlockInstance.LinkEnds.TryGetValue(trigger, out var end)) return;
        var target = (RectTransform)end;

        if (!source || !target) return;

        var obj = Links[(sourceBlock.BlockId, sourceEvent, block.BlockId, trigger)] = new GameObject("Link")
        {
            transform = { parent = ScriptEditorUI.Lines.transform }
        };
        
        obj.AddComponent<RectTransform>().sizeDelta = new Vector2(1, 4);
        var lr = obj.AddComponent<Connection>();
        lr.linkType = linkType;
        lr.t1 = source;
        lr.t2 = target;

        lr.sourceBlock = sourceBlock.BlockId;
        lr.sourceEvent = sourceEvent;
        lr.targetBlock = block.BlockId;
        lr.trigger = trigger;

        var img = obj.AddComponent<Image>();
        img.sprite = UIUtils.Square;
        lr.img = img;
    }

    public static void DestroyLink(string sourceBlock, string sourceEvent, string block, string trigger,
        Connection.LinkType linkType)
    {
        if (!Links.ContainsKey((sourceBlock, sourceEvent, block, trigger))) return;
        
        if (!Links.Remove((sourceBlock, sourceEvent, block, trigger), out var value)) return;
        if (linkType == Connection.LinkType.Event)
        {
            var map = Blocks[sourceBlock].EventMap;
            map[sourceEvent].Remove((block, trigger));
        } else Blocks[sourceBlock].VarMap.Remove(sourceEvent);
        Object.Destroy(value);
    }

    public class Connection : Deletable, IPointerEnterHandler, IPointerExitHandler
    {
        public RectTransform t1;
        public RectTransform t2;

        public Image img;

        public string sourceBlock;
        public string targetBlock;
        public string sourceEvent;
        public string trigger;

        public LinkType linkType;
    
        public void Update()
        {
            if (!t1 || !t2) return;
            var midpoint = (t1.position + t2.position) / 2f;
            
            transform.position = midpoint;
            
            var dir = t1.position - t2.position;
            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
            transform.localScale = new Vector3(dir.magnitude / transform.parent.lossyScale.x - t1.sizeDelta.x / 2, 1f, 1f);
        }

        public override void Delete()
        {
            DestroyLink(sourceBlock, sourceEvent, targetBlock, trigger, linkType);
        }

        public enum LinkType
        {
            Event,
            Var
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            img.color = Color.cyan;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            img.color = Color.white;
        }
    }

    public static void RegisterInputBlock<T>(string name, List<ConfigType> configGroup = null) where T : ScriptBlock, new()
    {
        var func = () => new T
        {
            Type = name, 
            Config = configGroup,
            Position = -ScriptEditorUI.ScriptParent.transform.localPosition
        };
        InputBlocks.Add((func, name));
        BlockTypes[name] = func;
    }

    public static void RegisterProcessBlock<T>(string name, List<ConfigType> configGroup = null) where T : ScriptBlock, new()
    {
        var func = () => new T
        {
            Type = name, 
            Config = configGroup,
            Position = -ScriptEditorUI.ScriptParent.transform.localPosition
        };
        ProcessBlocks.Add((func, name));
        BlockTypes[name] = func;
    }

    public static void RegisterOutputBlock<T>(string name, List<ConfigType> configGroup = null) where T : ScriptBlock, new()
    {
        var func = () => new T
        {
            Type = name, 
            Config = configGroup,
            Position = -ScriptEditorUI.ScriptParent.transform.localPosition
        };
        OutputBlocks.Add((func, name));
        BlockTypes[name] = func;
    }

    public static void RegisterHiddenBlock<T>(string name, List<ConfigType> configGroup = null) where T : ScriptBlock, new()
    {
        BlockTypes[name] = () => new T
        {
            Type = name, 
            Config = configGroup,
            Position = -ScriptEditorUI.ScriptParent.transform.localPosition
        };
    }

    public static void AddToScript(ObjectPlacement obj)
    {
        var wasLocal = IsLocal;
        if (!wasLocal) IsLocal = true;

        EditorUI.ObjectIdLabel.textComponent.text = $"{obj.GetPlacementType().GetName()} added";
        ArchitectPlugin.Instance.StartCoroutine(CursorObject.ClearCursorInfoLabel());

        var block = new ObjectBlock
        {
            TypeId = obj.GetPlacementType().GetId(),
            TargetId = obj.GetId(),
            Type = "object"
        };
        block.Setup(true);
        PlacementManager.GetLevelData().ScriptBlocks.Add(block);

        if (!wasLocal) IsLocal = false;
    }
}