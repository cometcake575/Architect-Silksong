using System.Collections.Generic;
using Architect.Placements;
using SSMP.Api.Client;
using SSMP.Api.Server;
using UnityEngine;

namespace Architect.Multiplayer.Ssmp;

public class SsmpManager : CoopManager
{
    private static ArchitectClientAddon _clientAddon;
    
    protected override void Setup()
    {
        _clientAddon = new ArchitectClientAddon();
        ClientAddon.RegisterAddon(_clientAddon);
        ServerAddon.RegisterAddon(new ArchitectServerAddon());
    }

    public override bool IsActive()
    {
        return _clientAddon.API.NetClient.IsConnected;
    }

    public override void ResetRoom(string room)
    {
        
    }

    public override void MoveObjects(string room, List<(string, Vector3)> movements)
    {
        
    }

    public override void EraseObjects(string room, List<string> ids)
    {
        
    }

    public override void ToggleTiles(string room, List<(int, int)> tiles)
    {
        
    }

    public override void ToggleLock(string room, string id)
    {
        
    }

    public override void PlaceObjects(string room, List<ObjectPlacement> placements)
    {
        
    }

    public override void ShareEvent(string room, string name)
    {
        
    }
}