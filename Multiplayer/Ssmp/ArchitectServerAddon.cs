using System.Collections.Generic;
using System.Reflection;
using Architect.Multiplayer.Ssmp.Data;
using SSMP.Api.Server;
using SSMP.Networking.Packet;

namespace Architect.Multiplayer.Ssmp;

public class ArchitectServerAddon : ServerAddon
{
    public IServerApi API;
    
    protected override string Name => "Architect";
    protected override string Version => Assembly.GetExecutingAssembly().GetName().Version.ToString();
    public override bool NeedsNetwork => true;
    
    public override void Initialize(IServerApi serverApi)
    {
        API = serverApi;
        
        var netReceiver = serverApi.NetServer.GetNetworkReceiver<PacketId>(this, GetData);
        
        netReceiver.RegisterPacketHandler<ClearPacketData>(PacketId.Clear, Share);
    }

    private void Share(ushort id, IPacketData packet)
    {
        var sender = API.NetServer.GetNetworkSender<PacketId>(this);
        List<ushort> ids = [];
        foreach (var player in API.ServerManager.Players) if (player.Id != id) ids.Add(player.Id);
        sender.SendSingleData(PacketId.Clear, packet, ids.ToArray());
    }

    public IPacketData GetData(PacketId packetId)
    {
        return packetId switch
        {
            //PacketId.Refresh => new ClearPacketData(),
            PacketId.Clear => new ClearPacketData(),
            //PacketId.Move => new ClearPacketData(),
            //PacketId.Erase => new ClearPacketData(),
            //PacketId.Tiles => new ClearPacketData(),
            //PacketId.Lock => new ClearPacketData(),
            //PacketId.Place => new ClearPacketData(),
            //PacketId.Event => new ClearPacketData(),
            _ => null
        };
    }
}