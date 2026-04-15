using System.Collections.Generic;
using Architect.Behaviour.Custom;
using UnityEngine;

namespace Architect.Events.Blocks.Outputs;

public class PngBlock : ScriptBlock
{
    
    
    protected override string Name => "Custom PNG";

    protected override IEnumerable<(string, string)> OutputVars => [("Current Sprite", "Sprite")];

    private SpriteRenderer _renderer;

    public string Url;
    public bool Point;
    public float Ppu;

    public override void SetupReference()
    {
        var obj = new GameObject("[Architect] Custom PNG")
            { transform = { localPosition = new Vector3(-9999, -9999) } };
        obj.SetActive(false);

        _renderer = obj.AddComponent<SpriteRenderer>();
        
        var pngObj = obj.AddComponent<PngObject>();
        pngObj.url = Url;
        pngObj.point = Point;
        pngObj.ppu = Ppu;
        
        obj.SetActive(true);
    }

    public override object GetValue(string id)
    {
        return _renderer.sprite;
    }
}