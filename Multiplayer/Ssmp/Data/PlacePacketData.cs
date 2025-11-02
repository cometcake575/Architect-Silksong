using SSMP.Networking.Packet;

namespace Architect.Multiplayer.Ssmp.Data;

public class PlacePacketData : ScenePacketData
{
    public byte[] SerializedObjects;
    public string Guid;
    public int Index;
    public int Length;
    public bool IsFullScene;

    protected override void WriteExtData(IPacket packet)
    {
        packet.Write(IsFullScene);
        packet.Write(Length);
        packet.Write(Index);
        packet.Write(Guid);
        
        packet.Write(SerializedObjects.Length);
        foreach (var b in SerializedObjects) packet.Write(b);
    }

    protected override void ReadExtData(IPacket packet)
    {
        IsFullScene = packet.ReadBool();
        Length = packet.ReadInt();
        Index = packet.ReadInt();
        Guid = packet.ReadString();
        
        var count = packet.ReadInt();

        var bytes = new byte[count];
        for (var i = 0; i < count; i++) bytes[i] = packet.ReadByte();

        SerializedObjects = bytes;
    }
}