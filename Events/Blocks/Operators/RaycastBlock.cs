using System.Collections.Generic;
using UnityEngine;

namespace Architect.Events.Blocks.Operators;

public class RaycastBlock : ScriptBlock
{
    protected override IEnumerable<string> Inputs => ["Raytrace"];
    protected override IEnumerable<(string, string)> InputVars => [
        Space,
        Space,
        ("X Pos", "Number"),
        ("Y Pos", "Number"),
        ("X Dir", "Number"),
        ("Y Dir", "Number")
    ];
    protected override IEnumerable<(string, string)> OutputVars => [
        ("Hit", "Boolean"), 
        ("X", "Number"), 
        ("Y", "Number"),
        ("Distance", "Number")
    ];

    private static readonly Color DefaultColor = new(0.9f, 0.7f, 0.3f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Raycast";

    protected override void Reset()
    {
        _hit = false;
        _xHit = 0;
        _yHit = 0;
        _dist = 0;
        MaxDistance = 10;
        Mode = 0;
    }

    public float MaxDistance = 10;
    public int Mode;
    private bool _hit;
    private float _xHit;
    private float _yHit;
    private float _dist;

    private static readonly int TerrainMask = LayerMask.GetMask("Terrain");
    private static readonly int EnemyMask = LayerMask.GetMask("Enemies");
    private static readonly int PlayerMask = LayerMask.GetMask("Player");
    protected override void Trigger(string trigger)
    {
        var raycast = Physics2D.Raycast(
            new Vector2(GetVariable<float>("X Pos"), GetVariable<float>("Y Pos")),
            new Vector2(GetVariable<float>("X Dir"), GetVariable<float>("Y Dir")),
            MaxDistance,
            Mode switch
            {
                0 => TerrainMask,
                1 => EnemyMask,
                _ => PlayerMask
            }
        );
        if (!raycast) Reset();
        else
        {
            _hit = true;
            _xHit = raycast.point.x;
            _yHit = raycast.point.y;
            _dist = raycast.distance;
        }
    }

    protected override object GetValue(string id)
    {
        return id switch
        {
            "Hit" => _hit,
            "X" => _xHit,
            "Y" => _yHit,
            "Distance" => _dist,
            _ => null
        };
    }
}