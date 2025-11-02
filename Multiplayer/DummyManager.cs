using System.Collections.Generic;
using Architect.Placements;
using UnityEngine;

namespace Architect.Multiplayer;

public class DummyManager : CoopManager
{
    public override string Name => "Dummy";
    
    public override bool IsActive() => false;

    public override void ResetRoom(string room) { }
    
    public override void ShareEvent(string room, string name) { }

    public override void MoveObjects(string room, List<(string, Vector3)> _) { }

    public override void EraseObjects(string room, List<string> _) { }
    
    public override void ToggleLock(string room, string _) { }
    
    public override void ToggleTiles(string room, List<(int, int)> tiles, bool empty) { }

    public override void PlaceObjects(string room, List<ObjectPlacement> _) { }
    
    public override void ShareScene(string room) { }
}