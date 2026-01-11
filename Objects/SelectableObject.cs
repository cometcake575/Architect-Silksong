using JetBrains.Annotations;
using UnityEngine;

namespace Architect.Objects;

public abstract class SelectableObject
{
    public abstract string GetName();

    [CanBeNull]
    public abstract string GetDescription();
    
    public abstract void Click(Vector3 mousePosition, bool first);

    public virtual void RightClick(Vector3 mousePosition) { }

    public virtual void Release() { }

    public abstract Sprite GetUISprite();
    
    public bool DisableTransformations;
}