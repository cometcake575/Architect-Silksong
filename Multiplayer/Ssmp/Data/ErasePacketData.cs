using System.Collections.Generic;
using SSMP.Networking.Packet;

namespace Architect.Multiplayer.Ssmp.Data;

public class ErasePacketData : ScenePacketData
{
    public List<string> Removals;

    protected override void WriteExtData(IPacket packet)
    {
        packet.Write(Removals.Count);
        foreach (var r in Removals) packet.Write(r);
    }

    protected override void ReadExtData(IPacket packet)
    {
        Removals = [];
        var count = packet.ReadInt();
        for (var i = 0; i < count; i++)
        {
            Removals.Add(packet.ReadString());
        }
    }
}