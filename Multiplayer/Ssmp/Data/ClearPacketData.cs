using SSMP.Networking.Packet;

namespace Architect.Multiplayer.Ssmp.Data;


public class ClearPacketData : ScenePacketData
{
    protected override void WriteExtData(IPacket packet) {}

    protected override void ReadExtData(IPacket packet) {}
}