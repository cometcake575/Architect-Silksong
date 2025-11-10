using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Architect.Multiplayer.Ssmp.Data;
using Architect.Placements;
using Architect.Storage;
using Architect.Utils;
using Newtonsoft.Json;
using SSMP.Api.Client;
using SSMP.Api.Server;
using SSMP.Networking.Packet;
using UnityEngine;

namespace Architect.Multiplayer.Ssmp;

public class SsmpManager : CoopManager
{
    private const int SPLIT_SIZE = 600;
    
    private readonly ArchitectClientAddon _clientAddon;

    public override string Name => "SSMP";
    
    public SsmpManager()
    {
        _clientAddon = new ArchitectClientAddon();
        ClientAddon.RegisterAddon(_clientAddon);
        ServerAddon.RegisterAddon(new ArchitectServerAddon());
    }

    private void SendPacket(PacketId id, IPacketData data)
    {
        _clientAddon.API.NetClient.GetNetworkSender<PacketId>(_clientAddon).SendSingleData(id, data);
    }
    
    public override bool IsActive()
    {
        return _clientAddon.API.NetClient.IsConnected;
    }

    public override void ResetRoom(string room)
    {
        ArchitectPlugin.Logger.LogInfo("Sending Reset Packet");
        SendPacket(PacketId.Clear, new ClearPacketData { SceneName = room });
    }

    public override void MoveObjects(string room, List<(string, Vector3)> movements)
    {
        ArchitectPlugin.Logger.LogInfo("Sending Move Packet");
        SendPacket(PacketId.Move, new MovePacketData
        {
            SceneName = room,
            Movements = movements
        });
    }

    public override void EraseObjects(string room, List<string> ids)
    {
        ArchitectPlugin.Logger.LogInfo("Sending Erase Packet");
        SendPacket(PacketId.Erase, new ErasePacketData
        {
            SceneName = room,
            Removals = ids
        });
    }

    public override void ToggleTiles(string room, List<(int, int)> tiles, bool empty)
    {
        ArchitectPlugin.Logger.LogInfo("Sending Tiles Packet");
        SendPacket(PacketId.Tiles, new TilePacketData
        {
            SceneName = room,
            Tiles = tiles,
            Empty = empty
        });
    }

    public override void ToggleLock(string room, string id)
    {
        ArchitectPlugin.Logger.LogInfo("Sending Lock Packet");
        SendPacket(PacketId.Lock, new LockPacketData
        {
            SceneName = room,
            Toggle = id
        });
    }

    public override void PlaceObjects(string room, List<ObjectPlacement> placements)
    {
        ArchitectPlugin.Logger.LogInfo("Sending Place Packet");
        
        var json = StorageManager.SerializePlacements(placements);
        var bytes = Split(ZipUtils.Zip(json), SPLIT_SIZE);

        Task.Run(() => SendSplitPlaceData(bytes, room, false));
    }

    public override void ShareScene(string room)
    {
        ArchitectPlugin.Logger.LogInfo("Sending Scene Packet");
        
        var json = StorageManager.SerializeLevel(PlacementManager.GetLevelData(), Formatting.None);
        var bytes = Split(ZipUtils.Zip(json), SPLIT_SIZE);
        
        Task.Run(() => SendSplitPlaceData(bytes, room, true));
    }

    private async Task SendSplitPlaceData(byte[][] bytes, string room, bool isFullScene)
    {
        var guid = Guid.NewGuid().ToString();

        var i = 0;
        var length = bytes.Length;
        foreach (var byteGroup in bytes)
        {
            SendPacket(PacketId.Place, new PlacePacketData
            {
                SceneName = room,
                SerializedObjects = byteGroup,
                Index = i,
                Length = length,
                Guid = guid,
                IsFullScene = isFullScene
            });
            i++;
            await Task.Delay(100);
        }
    }

    public override void RefreshRoom()
    {
        _clientAddon.RefreshRoom();
    }

    public static byte[][] Split(byte[] array, int size)
    {
        var count = Mathf.CeilToInt((float)array.Length / size);
        var bytes = new byte[count][];

        for (var i = 0; i < count; i++) bytes[i] = array.Skip(i * size).Take(size).ToArray();

        return bytes;
    }

    public override void ShareEvent(string room, string name)
    {
        ArchitectPlugin.Logger.LogInfo("Sending Event Packet");
        SendPacket(PacketId.Event, new EventPacketData
        {
            SceneName = room,
            Event = name
        });
    }
}