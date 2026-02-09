using System.Collections.Generic;
using Architect.Behaviour.Custom;
using UnityEngine;

namespace Architect.Events.Blocks.Outputs;

public class PngBlock : ScriptBlock
{
    private static readonly Color DefaultColor = new(0.9f, 0.2f, 0.2f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Custom PNG";

    protected override IEnumerable<(string, string)> OutputVars => [("Current Sprite", "Sprite")];

    private SpriteRenderer _renderer;

    public string Url;
    public bool Point;
    public float Ppu;

    public override void SetupReference()
    {
        var obj = new GameObject("[Architect] Custom PNG");
        obj.SetActive(false);

        _renderer = obj.AddComponent<SpriteRenderer>();
        
        var pngObj = obj.AddComponent<PngObject>();
        pngObj.url = Url;
        pngObj.point = Point;
        pngObj.ppu = Ppu;
        
        obj.SetActive(true);
    }

    protected override object GetValue(string id)
    {
        return _renderer.sprite;
    }
}