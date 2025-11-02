using SSMP.Networking.Packet;

namespace Architect.Multiplayer.Ssmp.Data;

public class LockPacketData : ScenePacketData
{
    public string Toggle;

    protected override void WriteExtData(IPacket packet)
    {
        packet.Write(Toggle);
    }

    protected override void ReadExtData(IPacket packet)
    {
        Toggle = packet.ReadString();
    }
}