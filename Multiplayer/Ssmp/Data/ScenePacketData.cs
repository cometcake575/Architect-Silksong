using SSMP.Networking.Packet;

namespace Architect.Multiplayer.Ssmp.Data;


public abstract class ScenePacketData : IPacketData
{
    public string SceneName;

    public void WriteData(IPacket packet)
    {
        packet.Write(SceneName);
        WriteExtData(packet);
    }

    public void ReadData(IPacket packet)
    { 
        SceneName = packet.ReadString();
        ReadExtData(packet);
    }

    protected abstract void WriteExtData(IPacket packet);

    protected abstract void ReadExtData(IPacket packet);

    public bool IsReliable => true;
    public bool DropReliableDataIfNewerExists => false;
}