using Architect.Objects.Placeable;
using UnityEngine;

namespace Architect.Utils;

public static class PreviewUtils
{
    public static Vector3 FixPreview(SpriteRenderer renderer, PlaceableObject type, bool flipped, float rotation, float scale)
    {
        if (type.Tk2dRotation / 90 % 2 != 0)
        {
            renderer.flipY = flipped != type.FlipX;
            renderer.flipX = type.LossyScale.x < 0;
        }
        else
        {
            renderer.flipX = flipped != type.FlipX;
            renderer.flipY = type.LossyScale.y < 0;
        }

        var childOffset = type.ChildOffset;

        childOffset.x /= type.LossyScale.x;
        childOffset.y /= type.LossyScale.y;
        
        var offset = type.Offset + childOffset;
        
        offset.x *= type.ParentScale.x;
        offset.y *= type.ParentScale.y;
        
        if (flipped) offset.x = -offset.x;

        return Quaternion.Euler(0, 0, rotation) * (offset * scale);
    }

    public static Sprite ConvertFrom2DToolkit(tk2dSpriteDefinition def, float ppu)
    {
        if (def.material.mainTexture is not Texture2D texture)
        {
            var mainTexture = def.material.mainTexture;
            texture = new Texture2D(mainTexture.width, mainTexture.height, TextureFormat.RGBA32, false);
 
            var currentRT = RenderTexture.active;
 
            var renderTexture = new RenderTexture(mainTexture.width, mainTexture.height, 32);
            Graphics.Blit(mainTexture, renderTexture);
 
            RenderTexture.active = renderTexture;
            texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture.Apply();
            RenderTexture.active = currentRT;
        }
        
        var minX = def.uvs[0].x;
        var minY = def.uvs[0].y;
        var maxX = def.uvs[0].x;
        var maxY = def.uvs[0].y;
        for (var i = 1; i < def.uvs.Length; i++)
        {
            minX = Mathf.Min(minX, def.uvs[i].x);
            minY = Mathf.Min(minY, def.uvs[i].y);
            maxX = Mathf.Max(maxX, def.uvs[i].x);
            maxY = Mathf.Max(maxY, def.uvs[i].y);
        }

        var x = minX * texture.width;
        var y = minY * texture.height;
        var width = (maxX - minX) * texture.width;
        var height = (maxY - minY) * texture.height;

        return Sprite.Create(texture, new Rect(x, y, width, height), new Vector2(0.5f, 0.5f), ppu);
    }
}