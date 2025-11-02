using System.Collections.Generic;
using SSMP.Networking.Packet;

namespace Architect.Multiplayer.Ssmp.Data;

public class TilePacketData : ScenePacketData
{
    public List<(int, int)> Tiles;
    public bool Empty;

    protected override void WriteExtData(IPacket packet)
    {
        packet.Write(Empty);
        packet.Write(Tiles.Count);
        foreach (var r in Tiles)
        {
            packet.Write(r.Item1);
            packet.Write(r.Item2);
        }
    }

    protected override void ReadExtData(IPacket packet)
    {
        Empty = packet.ReadBool();
        Tiles = [];
        var count = packet.ReadInt();
        for (var i = 0; i < count; i++)
        {
            Tiles.Add((packet.ReadInt(), packet.ReadInt()));
        }
    }
}