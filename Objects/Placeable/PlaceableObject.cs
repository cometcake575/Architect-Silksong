using System;
using System.Collections.Generic;
using System.Linq;
using Architect.Config.Types;
using Architect.Editor;
using Architect.Events;
using Architect.Objects.Groups;
using Architect.Placements;
using Architect.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace Architect.Objects.Placeable;

public abstract class PlaceableObject : SelectableObject
{
    public static readonly Dictionary<string, PlaceableObject> RegisteredObjects = [];
    
    public readonly bool Preview;
    public readonly Action<GameObject> PostSpawnAction;

    public GameObject Prefab;
    
    public Sprite Sprite;
    private readonly Sprite _uiSprite;
    
    public Vector3 Offset;
    public Vector3 ChildOffset;
    public Vector3 ParentScale;
    public Vector3 LossyScale;
    public float Rotation;
    public float ChildRotation;
    public int Tk2dRotation;
    public float ZPosition;

    public bool FlipX;
    
    private readonly string _name;
    private readonly string _description;
    private readonly string _id;

    [CanBeNull] public Action<GameObject, bool> FlipAction;
    [CanBeNull] public Action<GameObject, float> RotateAction;
    [CanBeNull] public Action<GameObject, float> ScaleAction;

    private RotationGroup _rotationGroup = RotationGroup.None;
    
    public List<EventReceiverType> ReceiverGroup = Groups.ReceiverGroup.Generic;
    public List<ConfigType> ConfigGroup = Groups.ConfigGroup.Visible;
    public List<string> BroadcasterGroup = [];

    protected PlaceableObject(
        string name, 
        string id,
        string description = null,
        Action<GameObject> postSpawnAction = null,
        bool preview = false,
        Sprite sprite = null,
        Sprite uiSprite = null)
    {
        _name = name;
        _id = id;
        _description = description;

        PostSpawnAction = postSpawnAction;
        Preview = preview;
        
        Sprite = sprite;
        _uiSprite = uiSprite;

        RegisteredObjects[id] = this;
    }

    public override Sprite GetUISprite()
    {
        return _uiSprite ?? Sprite;
    }

    protected void FinishSetup(GameObject prefab)
    {
        Prefab = prefab;

        var setZ = Prefab.GetComponent<SetZ>();
        if (setZ)
        {
            var pos = Prefab.transform.position;
            pos.z = setZ.z;
            Prefab.transform.position = pos;
        }
        
        ZPosition = Prefab.transform.position.z;
        
        Prefab.RemoveComponent<BlackThreadState>();
        
        ParentScale = Prefab.transform.lossyScale;
        if (Sprite) LossyScale = ParentScale;
        else Sprite = RetrieveSprite();
    }

    public PlaceableObject WithRotationGroup(RotationGroup group)
    {
        _rotationGroup = group;
        return this;
    }

    public PlaceableObject WithReceiverGroup(List<EventReceiverType> group)
    {
        ReceiverGroup = group;
        return this;
    }

    public PlaceableObject WithConfigGroup(List<ConfigType> group)
    {
        ConfigGroup = group;
        return this;
    }

    public PlaceableObject WithBroadcasterGroup(List<string> group)
    {
        BroadcasterGroup = group;
        return this;
    }

    public PlaceableObject DoFlipX() {
        FlipX = true;
        return this;
    }

    public RotationGroup GetRotationGroup() => _rotationGroup;

    public override string GetName() => _name;

    public string GetId() => _id;

    public override string GetDescription()
    {
        return _description;
    }

    public override void Click(Vector3 mousePosition, bool first)
    {
        if (!first) return;

        var pos = EditManager.GetWorldPos(mousePosition, true);
        pos.z = ZPosition;
        
        var obj = PreparePlacement(pos);
        
        EditManager.RegisterLastPos(pos);
        
        ActionManager.PerformAction(new PlaceObject([obj]));
    }

    public ObjectPlacement PreparePlacement(Vector3 pos)
    {
        var hover = EditManager.HoveredObject;
        string id;
        if (hover != null)
        {
            pos = hover.GetPos();
            id = hover.GetId();
            ActionManager.PerformAction(new EraseObject([hover]));
            EditManager.HoveredObject = null;
        }
        else id = Guid.NewGuid().ToString()[..8];
        
        return new ObjectPlacement(
            this, 
            pos,
            id,
            EditManager.CurrentlyFlipped,
            EditManager.CurrentRotation,
            EditManager.CurrentScale,
            false,
            EditManager.Broadcasters.ToArray(),
            EditManager.Receivers.ToArray(),
            EditManager.Config.Values.ToArray()
        );
    }

    #region Automatically Preparing Sprite

    public class SpriteSource : MonoBehaviour;
    
    private Sprite RetrieveSprite()
    {
        var spriteRenderer = Prefab.GetComponent<SpriteRenderer>();
        if (spriteRenderer)
        {
            Rotation = Prefab.transform.rotation.eulerAngles.z;
            LossyScale = ParentScale;
            return spriteRenderer.sprite;
        }

        var sprite = Prefab.GetComponent<tk2dSprite>();
        if (sprite)
        {
            Rotation = Prefab.transform.rotation.eulerAngles.z;
            LossyScale = ParentScale;
            return PrepareSpriteWithTk2D(sprite, false);
        }

        var sprSource = Prefab.GetComponentInChildren<SpriteSource>(true);
        var source = sprSource?.gameObject ?? Prefab;
        
        var cSprite = source.GetComponentInChildren<tk2dSprite>();
        if (cSprite)
        {
            ChildOffset = cSprite.gameObject.transform.position - Prefab.transform.position;
            ChildRotation = cSprite.gameObject.transform.eulerAngles.z;
            LossyScale = cSprite.gameObject.transform.lossyScale;
            return PrepareSpriteWithTk2D(cSprite, true);
        }

        var cSpriteRenderer = source.GetComponentInChildren<SpriteRenderer>();
        if (!cSpriteRenderer) return null;

        ChildOffset = cSpriteRenderer.gameObject.transform.position - Prefab.transform.position;
        ChildRotation = cSpriteRenderer.gameObject.transform.eulerAngles.z;
        LossyScale = cSpriteRenderer.gameObject.transform.lossyScale;
        
        return cSpriteRenderer.sprite;
    }
    
    private Sprite PrepareSpriteWithTk2D(tk2dSprite sprite, bool child)
    {
        var animator = sprite.gameObject.GetComponent<tk2dSpriteAnimator>();
        var def = sprite.CurrentSprite;

        if (animator)
        {
            var clip = animator.DefaultClip;
            if (clip != null)
            {
                var frame = clip.frames[0];
                def = frame.spriteCollection.spriteDefinitions[frame.spriteId];
            }
        }

        if (def.flipped != tk2dSpriteDefinition.FlipMode.None)
        {
            Tk2dRotation += 90;
        }

        var center = def.GetBounds().center;

        if (child) ChildOffset += center;
        else Offset += center;
        
        return PreviewUtils.ConvertFrom2DToolkit(def,
            Mathf.Abs(1 / (sprite.scale.x * sprite.GetCurrentSpriteDef().texelSize.x)));
    }
    #endregion

    #region Object Manipulation Overrides
    public PlaceableObject WithFlipAction(Action<GameObject, bool> flipAction)
    {
        FlipAction = flipAction;
        return this;
    }

    public PlaceableObject WithRotateAction(Action<GameObject, float> rotateAction)
    {
        RotateAction = rotateAction;
        return this;
    }

    public PlaceableObject WithScaleAction(Action<GameObject, float> scaleAction)
    {
        ScaleAction = scaleAction;
        return this;
    }
    #endregion
}
