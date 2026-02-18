using System.Collections.Generic;
using System.Linq;
using Architect.Utils;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace Architect.Events.Blocks;

public abstract class LinkedBlock : ScriptBlock
{
    public abstract void AddExtraIds(List<string> ids);
}

public abstract class CollectionBlock<T> : LinkedBlock
    where T : CollectionBlock<T>.ChildBlock, new()
{
    protected ChildrenGroup Children = new();

    protected abstract string ChildName { get; }
    protected virtual int MaxChildren => -1;
    protected abstract bool NeedsGap { get; }
    
    protected override Dictionary<string, string> SerializeExtraData()
    {
        Dictionary<string, string> d = [];
        d["children"] = JsonConvert.SerializeObject(Children.Blocks);
        return d;
    }

    protected override void DeserializeExtraData(Dictionary<string, string> data)
    {
        Children = new ChildrenGroup
        {
            Blocks = JsonConvert.DeserializeObject<List<ChildBlock>>(data["children"], Sbc)
        };
    }

    protected override void SetupBlock(bool newBlock, int width, int height)
    {
        base.SetupBlock(newBlock, width, height);
        if (!BlockObject) return;

        var addArea = new GameObject("Add Area");
        addArea.transform.SetParent(BlockObject.transform, false);
        var addTrans = addArea.RemoveOffset();

        var (btn, btnImg, _) = UIUtils.MakeButtonWithImage("Add Button", addArea,
            Vector2.zero,
            new Vector2(0.5f, 0),
            new Vector2(0.5f, 0), 80, 80, false);
        btnImg.sprite = AddSprite;
        btn.onClick.AddListener(() =>
        {
            if (MaxChildren > 0 && Children.Blocks.Count >= MaxChildren) return;
            if (ScriptManager.BlockTypes[ChildName]() is not ChildBlock child) return;

            child.Group = Children;
            ScriptManager.Blocks[child.BlockId] = child;
            child.SetupBlock(true);
            Children.Blocks.Add(child);

            foreach (var cfg in child.CurrentConfig.Values) cfg.Setup(child);

            if (!child.BlockObject) return;

            child.BlockObject.transform.SetParent(BlockObject.transform, true);
            Children.OrderChildren();
        });

        Children.AddBlock = addTrans;
        Children.MaxChildren = MaxChildren;
        Children.Height = height;
        Children.Parent = this;
        Children.NeedsGap = NeedsGap;
    }

    public override void Setup(bool visual, bool newBlock = false)
    {
        base.Setup(visual, newBlock);
        
        foreach (var child in Children.Blocks)
        {
            ScriptManager.Blocks[child.BlockId] = child;
            child.Group = Children;
            
            if (visual && BlockObject)
            {
                child.SetupBlock(newBlock);
                if (!child.BlockObject) return;
                child.BlockObject.transform.SetParent(BlockObject.transform, true);
            }
            foreach (var cfg in child.CurrentConfig.Values) cfg.Setup(child);
        }
        if (visual) Children.OrderChildren();
        else foreach (var child in Children.Blocks) child.SetupReference();
    }

    public override void Delete()
    {
        foreach (var child in Children.Blocks.ToArray()) child.Delete();
        base.Delete();
    }

    public override void LateSetup()
    {
        base.LateSetup();
        
        foreach (var child in Children.Blocks)
        {
            child.LateSetup();
        }
    }

    public override void AddExtraIds(List<string> ids)
    {
        ids.AddRange(Children.Blocks.Select(child => child.BlockId));
    }

    public abstract class ChildBlock : LinkedBlock
    {
        protected override string Name => null;
        public ChildrenGroup Group;
        public int BlockHeight;

        protected override void SetupBlock(bool newBlock, int width, int height)
        {
            if (ConfigCount >= 3) height += 25; 
            base.SetupBlock(newBlock, width, height);
            BlockInstance.overrideDrag = Group.Parent.BlockInstance;
            BlockHeight = height;

            if (Group.NeedsGap)
            {
                var img = UIUtils.MakeImage(
                    "Connector",
                    BlockObject,
                    new Vector2(25, 40),
                    new Vector2(0, 1),
                    new Vector2(0, 1),
                    new Vector2(50, 170));
                img.sprite = FlowchartBlock;
                img.type = Image.Type.Sliced;
                img.color = Color;
            }
        }

        public override void Delete()
        {
            Group.Remove(this);
            base.Delete();
        }

        public override void AddExtraIds(List<string> ids)
        {
            ids.Add(Group.Parent.BlockId);
            foreach (var child in Group.Children) ids.Add(child.BlockId);
        }
    }

    public class ChildrenGroup
    {
        public ScriptBlock Parent;
        public List<ChildBlock> Blocks = [];
        public bool NeedsGap;
        public IEnumerable<T> Children => Blocks.Where(o => o is T).Cast<T>();
        
        public RectTransform AddBlock;
        public int MaxChildren;
        public int Height;

        public void Remove(ChildBlock block)
        {
            Blocks.Remove(block);
            OrderChildren();
        }

        public void OrderChildren()
        {
            float y = -Height;
            var ng = NeedsGap && Blocks.Count > 0;
            if (!NeedsGap) y += 25;
            var i = 0;
            foreach (var o in Blocks)
            {
                if (ng)
                {
                    if (i > 0) y -= 37.5f;
                    y -= 40;
                    y -= o.BlockHeight / 2f;
                }
                if (o.BlockObject) o.BlockObject.transform.localPosition = new Vector3(0, y + (NeedsGap ? 12.5f : 0));
                if (ng) y -= o.BlockHeight / 2f;
                else y -= o.BlockHeight;
                i++;
            }
            AddBlock.SetLocalPositionY(y + (ng ? 84 : NeedsGap ? 122.5f : 197.5f));
            AddBlock.SetAsLastSibling();
            
            if (MaxChildren > 0) AddBlock.gameObject.SetActive(Blocks.Count < MaxChildren);
        }
    }
}
