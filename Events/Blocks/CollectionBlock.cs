using System.Collections.Generic;
using Architect.Utils;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace Architect.Events.Blocks;

public abstract class CollectionBlock<T>(int maxChildren = -1) : ScriptBlock 
    where T : CollectionBlock<T>.ChildBlock, new()
{
    public int MaxChildren = maxChildren;
    public List<ChildBlock> Children = [];

    protected override Dictionary<string, string> SerializeExtraData()
    {
        Dictionary<string, string> d = [];
        d["children"] = JsonConvert.SerializeObject(Children);
        return d;
    }

    protected override void DeserializeExtraData(Dictionary<string, string> data)
    {
        Children = JsonConvert.DeserializeObject<List<ChildBlock>>(data["children"], Sbc);
    }

    protected override void SetupBlock(bool newBlock, int width, int height)
    {
        base.SetupBlock(newBlock, width, height);
        
        if (!BlockObject) return;
        var img = UIUtils.MakeImage(
            "Lower Seg",
            BlockObject,
            new Vector2(0, 10),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(width, height));
        img.sprite = FlowchartBlock;
        img.type = Image.Type.Sliced;
        img.color = Color;
        
        #region Test
        var o = new T
        {
            Type = "Random Trigger",
            Position = Position
        };
        Children.Add(o);
        #endregion

        foreach (var child in Children)
        {
            ScriptManager.Blocks[child.BlockId] = child;
            child.Group = Children;
            child.SetupBlock(newBlock);
            if (!child.BlockObject) continue;
            child.BlockObject.transform.parent = BlockObject.transform;
        }
    }

    public abstract class ChildBlock : ScriptBlock
    {
        protected override string Name => null;
        public List<ChildBlock> Group;

        public override void Delete()
        {
            Group.Remove(this);
            base.Delete();
        }
    }
}
