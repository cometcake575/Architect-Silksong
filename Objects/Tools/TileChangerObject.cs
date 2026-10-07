using System;
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
               "Does not work out of bounds as the tilemap is limited to the room.\n\n" +
               "Hold Left Alt for flood fill.";
    }

    public override void Click(Vector3 mousePosition, bool first)
    {
        var map = PlacementManager.GetTilemap();
        if (!map || !map.GetTileAtPosition(EditManager.GetWorldPos(mousePosition), out var x, out var y)) return;

        var pos = (x, y);
        if (_lastPos == pos && !first) return;
        _lastPos = pos;

        var empty = map.GetTile(x, y, 0) == -1;
        if (first)
        {
            _lastEmpty = empty;
            
            if (Input.GetKey(KeyCode.LeftAlt))
            {
                HashSet<(int, int)> seen = [pos];
                Queue<(int, int)> tiles = [];
                tiles.Enqueue(pos);
                while (tiles.TryDequeue(out var current))
                {
                    (x, y) = current;
                    if (map.GetTile(x, y, 0) == -1 != empty) continue;
                    
                    if (empty) map.SetTile(x, y, 0, 0);
                    else map.ClearTile(x, y, 0);
                    TileFlips.Add(current);

                    if (x - 1 >= 0)
                    {
                        var left = (x - 1, y);
                        if (seen.Add(left)) tiles.Enqueue(left);
                    }

                    if (x + 1 < map.width)
                    {
                        var right = (x + 1, y);
                        if (seen.Add(right)) tiles.Enqueue(right);
                    }

                    if (y - 1 >= 0)
                    {
                        var down = (x, y - 1);
                        if (seen.Add(down)) tiles.Enqueue(down);
                    }

                    if (y + 1 < map.height)
                    {
                        var up = (x, y + 1);
                        if (seen.Add(up)) tiles.Enqueue(up);
                    }
                }

                map.Build();
                return;
            }
        }
        else if (_lastEmpty != empty) return;
        
        try
        {
            if (empty) map.SetTile(x, y, 0, 0);
            else map.ClearTile(x, y, 0);
            map.Build();
        }
        catch (Exception)
        {
            // Out of bounds
            return;
        }

        TileFlips.Add(pos);
    }

    public override void Release()
    {
        ActionManager.SceneActionManager.PerformAction(new ToggleTile(TileFlips.ToList(), !_lastEmpty));
        TileFlips.Clear();
    }
}