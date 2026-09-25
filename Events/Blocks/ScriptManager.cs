using System;
using System.Collections.Generic;
using System.Linq;
using Architect.Editor;
using Architect.Events.Blocks.Events;
using Architect.Events.Blocks.Functions;
using Architect.Events.Blocks.Objects;
using Architect.Events.Blocks.Operators;
using Architect.Events.Blocks.Outputs;
using Architect.Placements;
using Architect.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Architect.Events.Blocks;

public static class ScriptManager
{
    public static bool IsLocal
    {
        get;
        set
        {
            field = value;
            ScriptEditorUI.LocalParent.SetActive(value);
            ScriptEditorUI.GlobalParent.SetActive(!value);
            
            ScriptEditorUI.LocalBtnText.color = value ? Color.yellow : Color.white;
            ScriptEditorUI.GlobalBtnText.color = value ? Color.white : Color.yellow;

            if (ScriptEditorUI.Rainbow && ScriptEditorUI.Trans)
            {
                ScriptEditorUI.Rainbow.SetActive(value);
                ScriptEditorUI.Trans.SetActive(!value);
            }

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
        ActionBlocks.Init();
        OperatorBlocks.Init();
        foreach (var c in Category.Categories)
        {
            Category.All.Blocks.AddRange(c.Blocks);
        }

        FunctionManager.Init();
        
        BlockTypes["object"] = () => new ObjectBlock { Type = "object" };
    }
    
    public static Start CurrentStart;
    public static bool InSwapMode;
    
    public static readonly Dictionary<string, Func<ScriptBlock>> BlockTypes = [];

    public static Category CurrentCategory = Category.All;

    public static string Filter = string.Empty;
    public static IEnumerable<(Func<ScriptBlock>, string)> CurrentBlocks => CurrentCategory.Blocks.Where(c => 
            c.Item2.Contains(Filter, StringComparison.InvariantCultureIgnoreCase));

    public static readonly Dictionary<string, ScriptBlock> Blocks = [];
    
    public static readonly Dictionary<(string, string, string, string), GameObject> Links = [];

    public static readonly HashSet<string> SelectedBlockIds = [];

    public static void ClearSelection()
    {
        SelectedBlockIds.Clear();
    }

    public static void SetSelection(IEnumerable<string> ids)
    {
        SelectedBlockIds.Clear();
        foreach (var id in ids) SelectedBlockIds.Add(id);
    }

    public static bool IsSelected(string blockId) => SelectedBlockIds.Contains(blockId);

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
                        List<IEdit> edits = [];
                        
                        if (!Input.GetKey(KeyCode.LeftAlt))
                        {
                            edits.Add(new DisconnectScriptBlock(CurrentStart.Block, CurrentStart.id, Blocks[connection.Item1], connection.Item2,
                                Connection.LinkType.Event, IsLocal));
                        }

                        edits.Add(new ConnectScriptBlock(Block, id, Blocks[connection.Item1], connection.Item2,
                            Connection.LinkType.Event, IsLocal));
                        
                        ActionManager.ScriptActionManager.PerformAction(new MultiEdit(edits));
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

            ActionManager.ScriptActionManager.PerformAction(new ConnectScriptBlock(CurrentStart.Block, CurrentStart.id,
                Block, id, Connection.LinkType.Event, IsLocal));

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
            if (start.type != type && type != "Any" &&
                !(start.type == "Enemy" && type == "Object") &&
                !(start.type == "Object" && type == "Enemy")) return;

            if (!Block.IsValid || !CurrentStart.Block.IsValid) return;

            var vMap = Block.VarMap;
            if (vMap.ContainsKey(id)) return;

            CurrentStart.img.color = CurrentStart.color;

            ActionManager.ScriptActionManager.PerformAction(new ConnectScriptBlock(Block, id, CurrentStart.Block,
                CurrentStart.id, Connection.LinkType.Var, IsLocal));

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
            ActionManager.ScriptActionManager.PerformAction(new DisconnectScriptBlock(Blocks[sourceBlock], sourceEvent,
                Blocks[targetBlock], trigger, linkType, IsLocal));
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

    public static Vector3 BlockSpawnPos =>
        ScriptEditorUI.Blocks.transform.InverseTransformPoint(Screen.width / 2f, Screen.height / 2f, 0);

    public static PlaceScriptBlock AddToScript(ObjectPlacement obj)
    {
        IsLocal = true;
        
        EditorUI.DisplayHotbarText($"{obj.GetPlacementType().GetName()} added");

        var block = new ObjectBlock
        {
            TypeId = obj.GetPlacementType().GetId(),
            TargetId = obj.GetId(),
            Type = "object",
            Position = BlockSpawnPos
        };
        block.Setup(true);

        return new PlaceScriptBlock(block, true);
    }
}