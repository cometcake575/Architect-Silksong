using Architect.Multiplayer.Ssmp.Data;
using SSMP.Networking.Packet;

namespace Architect.Multiplayer.Ssmp;

public enum PacketId
{
    Clear,
    Move,
    Erase,
    Tiles,
    Lock,
    Place,
    Event
}

public static class PacketIdExtension
{
    public static IPacketData GetData(this PacketId packetId)
    {
        return packetId switch
        {
            PacketId.Clear => new ClearPacketData(),
            PacketId.Move => new MovePacketData(),
            PacketId.Erase => new ErasePacketData(),
            PacketId.Tiles => new TilePacketData(),
            PacketId.Lock => new LockPacketData(),
            PacketId.Place => new PlacePacketData(),
            PacketId.Event => new EventPacketData(),
            _ => null
        };
    }
}
