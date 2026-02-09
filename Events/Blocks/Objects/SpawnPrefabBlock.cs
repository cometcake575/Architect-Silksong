using System;
using System.Collections.Generic;
using Architect.Prefabs;
using BepInEx;
using UnityEngine;

namespace Architect.Events.Blocks.Objects;

public class SpawnPrefabBlock : ScriptBlock
{
    protected override IEnumerable<string> Inputs => ["Spawn"];
    protected override IEnumerable<(string, string)> InputVars => [("X", "Number"), ("Y", "Number")];

    protected override Color Color => ObjectBlock.ValidColor;
    protected override string Name => "Spawn Prefab";

    protected override void Reset()
    {
        Prefab = "";
        OffsetX = 0;
        OffsetY = 0;
    }

    public string Prefab = "";
    public float OffsetX;
    public float OffsetY;

    protected override void Trigger(string trigger)
    {
        if (Prefab.IsNullOrWhiteSpace()) return;

        var prefab = new GameObject($"[Architect] Prefab Spawner {Guid.NewGuid()}")
        {
            transform = { position = new Vector3(
                GetVariable<float>("X") + OffsetX, 
                GetVariable<float>("Y") + OffsetY) }
        }.AddComponent<Prefab>();
        prefab.id = Prefab;
    }
}
