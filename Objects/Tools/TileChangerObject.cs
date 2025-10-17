using System.Collections.Generic;
using System.Linq;
using Architect.Editor;
using Architect.Placements;
using UnityEngine;

namespace Architect.Objects.Tools;

public class TileChangerObject() : ToolObject("tile_changer", Storage.Settings.TileChanger, -6)
{
    public static readonly TileChangerObject Instance = new();

    private static readonly List<(int, int)> TileFlips = [];
    
    private static (int, int) _lastPos = (-1, -1);
    private static bool _lastEmpty;
    
    public override string GetName()
    {
        return "Tilemap Editor";
    }

    public override string GetDescription()
    {
        return "Click on the tilemap to add or remove a tile.\n\n" +
               "Does not work out of bounds as the tilemap is limited to the room.";
    }

    public override void Click(Vector3 mousePosition, bool first)
    {
        var map = PlacementManager.GetTilemap();
        if (!map || !map.GetTileAtPosition(EditManager.GetWorldPos(mousePosition), out var x, out var y)) return;

        var pos = (x, y);
        if (_lastPos == pos && !first) return;
        _lastPos = pos;

        var empty = map.GetTile(x, y, 0) == -1;
        if (first) _lastEmpty = empty;
        else if (_lastEmpty != empty) return;

        if (empty) map.SetTile(x, y, 0, 0);
        else map.ClearTile(x, y, 0);
        map.Build();
        
        TileFlips.Add(pos);
    }

    public override void Release()
    {
        ActionManager.PerformAction(new ToggleTile(TileFlips.ToList(), !_lastEmpty));
        TileFlips.Clear();
    }
}