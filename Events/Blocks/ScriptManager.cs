using System;
using System.Collections.Generic;
using Architect.Editor;
using Architect.Events.Blocks.Config.Types;
using Architect.Events.Blocks.Events;
using Architect.Events.Blocks.Objects;
using Architect.Events.Blocks.Operators;
using Architect.Events.Blocks.Outputs;
using Architect.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Architect.Events.Blocks;

public static class ScriptManager
{
    public static void Init()
    {
        EventBlocks.Init();
        OperatorBlocks.Init();
        ActionBlocks.Init();
        
        BlockTypes["object"] = () => new ObjectBlock { Type = "object" };
    }
    
    public static Start CurrentEventStart;
    
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
    
    public class EventStart : Start, IPointerDownHandler
    {
        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            if (!Block.IsValid) return;
            
            if (CurrentEventStart)
            {
                CurrentEventStart.img.color = CurrentEventStart.color;
                if (CurrentEventStart == this)
                {
                    CurrentEventStart = null;
                    return;
                } 
                CurrentEventStart = null;
            }
            
            CurrentEventStart = this;
            img.color = Color.cyan;
        }
    }
    
    public class EventEnd : MonoBehaviour, IPointerDownHandler
    {
        public ScriptBlock Block;
        public string id;
        
        public void OnPointerDown(PointerEventData eventData)
        {
            if (CurrentEventStart is not EventStart) return;
            CurrentEventStart.img.color = CurrentEventStart.color;
            
            if (!Block.IsValid || !CurrentEventStart.Block.IsValid) return;

            var eMap = CurrentEventStart.Block.EventMap;
            if (!eMap.ContainsKey(CurrentEventStart.id)) eMap[CurrentEventStart.id] = [];
            if (eMap[CurrentEventStart.id].Contains((Block.BlockId, id))) return;
            eMap[CurrentEventStart.id].Add((Block.BlockId, id));
            
            MakeLink(CurrentEventStart.Block, CurrentEventStart.id, Block, id, Connection.LinkType.Event);
            
            CurrentEventStart = null;
        }
    }
    
    public class VarStart : Start, IPointerDownHandler
    {
        public string type;
        
        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            if (!Block.IsValid) return;
            
            if (CurrentEventStart)
            {
                CurrentEventStart.img.color = CurrentEventStart.color;
                if (CurrentEventStart == this)
                {
                    CurrentEventStart = null;
                    return;
                } 
                CurrentEventStart = null;
            }
            
            CurrentEventStart = this;
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
            if (CurrentEventStart is not VarStart start) return;
            if (start.type != type) return;
            
            if (!Block.IsValid || !CurrentEventStart.Block.IsValid) return;

            var vMap = Block.VarMap;
            if (vMap.ContainsKey(id)) return;
            
            CurrentEventStart.img.color = CurrentEventStart.color;
            vMap[id] = (CurrentEventStart.Block.BlockId, CurrentEventStart.id);
            
            MakeLink(Block, id, CurrentEventStart.Block, CurrentEventStart.id, Connection.LinkType.Var);
            
            CurrentEventStart = null;
        }
    }
    
    public static void MakeLink(ScriptBlock sourceBlock, string sourceEvent, ScriptBlock block, string trigger, Connection.LinkType linkType)
    {
        var source = (RectTransform)sourceBlock.BlockInstance.LinkStarts[sourceEvent];
        var target = (RectTransform)block.BlockInstance.LinkEnds[trigger];

        if (!source || !target) return;

        var obj = Links[(sourceBlock.BlockId, sourceEvent, block.BlockId, trigger)] = new GameObject("Link")
        {
            transform = { parent = ScriptEditorUI.LinesParent.transform }
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

    public class Connection : Deletable
    {
        public RectTransform t1;
        public RectTransform t2;

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
            transform.localScale = new Vector3((dir.magnitude-t1.sizeDelta.x) / transform.parent.lossyScale.x, 1f, 1f);
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
    }

    public static void RegisterInputBlock<T>(string name, List<ConfigType> configGroup = null) where T : ScriptBlock, new()
    {
        var func = () => new T
        {
            Type = name, 
            Config = configGroup,
            Position = -ScriptEditorUI.BlocksParent.transform.localPosition
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
            Position = -ScriptEditorUI.BlocksParent.transform.localPosition
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
            Position = -ScriptEditorUI.BlocksParent.transform.localPosition
        };
        OutputBlocks.Add((func, name));
        BlockTypes[name] = func;
    }
}