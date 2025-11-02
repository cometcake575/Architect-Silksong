using System.Collections.Generic;
using SSMP.Networking.Packet;
using UnityEngine;

namespace Architect.Multiplayer.Ssmp.Data;

public class MovePacketData : ScenePacketData
{
    public List<(string, Vector3)> Movements;

    protected override void WriteExtData(IPacket packet)
    {
        packet.Write(Movements.Count);
        foreach (var r in Movements)
        {
            packet.Write(r.Item1);
            packet.Write(r.Item2.x);
            packet.Write(r.Item2.y);
            packet.Write(r.Item2.z);
        }
    }

    protected override void ReadExtData(IPacket packet)
    {
        Movements = [];
        var count = packet.ReadInt();
        for (var i = 0; i < count; i++)
        {
            Movements.Add((packet.ReadString(),
                new Vector3(packet.ReadFloat(), packet.ReadFloat(), packet.ReadFloat())));
        }
    }
}