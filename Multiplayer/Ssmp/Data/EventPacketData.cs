using SSMP.Networking.Packet;

namespace Architect.Multiplayer.Ssmp.Data;

public class EventPacketData : ScenePacketData
{
    public string Event;

    protected override void WriteExtData(IPacket packet)
    {
        packet.Write(Event);
    }

    protected override void ReadExtData(IPacket packet)
    {
        Event = packet.ReadString();
    }
}