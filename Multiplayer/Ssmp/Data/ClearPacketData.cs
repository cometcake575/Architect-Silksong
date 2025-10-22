using SSMP.Networking.Packet;

namespace Architect.Multiplayer.Ssmp.Data;


public class ClearPacketData : IPacketData
{
    public string SceneName;

    public void WriteData(IPacket packet)
    {
        packet.Write(SceneName);
    }

    public void ReadData(IPacket packet)
    {
        SceneName = packet.ReadString();
    }

    public bool IsReliable => true;
    public bool DropReliableDataIfNewerExists => false;
}