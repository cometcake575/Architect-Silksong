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
    public override uint ApiVersion => 1;

    public override void Initialize(IServerApi serverApi)
    {
        API = serverApi;
        
        var netReceiver = serverApi.NetServer.GetNetworkReceiver<PacketId>(this, 
            id => id.GetData());
        
        netReceiver.RegisterPacketHandler<ClearPacketData>(PacketId.Clear, (id, packet) => 
            Share(id, packet, PacketId.Clear));
        netReceiver.RegisterPacketHandler<PlacePacketData>(PacketId.Place, (id, packet) => 
            Share(id, packet, PacketId.Place));
        netReceiver.RegisterPacketHandler<ErasePacketData>(PacketId.Erase, (id, packet) => 
            Share(id, packet, PacketId.Erase));
        netReceiver.RegisterPacketHandler<LockPacketData>(PacketId.Lock, (id, packet) => 
            Share(id, packet, PacketId.Lock));
        netReceiver.RegisterPacketHandler<EventPacketData>(PacketId.Event, (id, packet) => 
            Share(id, packet, PacketId.Event));
        netReceiver.RegisterPacketHandler<MovePacketData>(PacketId.Move, (id, packet) => 
            Share(id, packet, PacketId.Move));
        netReceiver.RegisterPacketHandler<TilePacketData>(PacketId.Tiles, (id, packet) => 
            Share(id, packet, PacketId.Tiles));
    }

    private void Share(ushort id, IPacketData packet, PacketId packetId)
    {
        var sender = API.NetServer.GetNetworkSender<PacketId>(this);
        List<ushort> ids = [];
        foreach (var player in API.ServerManager.Players) if (player.Id != id) ids.Add(player.Id);
        sender.SendSingleData(packetId, packet, ids.ToArray());
    }
}