using Architect.Utils;
using UnityEngine;

namespace Architect.Workshop.Items;

public class CustomMapIcon : SpriteItem
{
    public string Scene = string.Empty;
    public int Mode;
    public string Text = string.Empty;
    public float FontSize = 12;
    public Vector2 Offset = Vector2.zero;
    public Color Colour = Color.white;
    
    public override void Register()
    {
        
        base.Register();
    }

    public override void Unregister()
    {
        
    }

    protected override void OnReadySprite()
    {
        
    }
}