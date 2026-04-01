using Architect.Utils;
using UnityEngine;

namespace Architect.Workshop.Items;

public class StatusEffect : WorkshopItem
{
    private static readonly Sprite Sprite = ResourceUtils.LoadSpriteResource("effect", FilterMode.Point);
    
    public override void Register()
    {
        
    }

    public override void Unregister()
    {
        
    }

    public override Sprite GetIcon() => Sprite;
}