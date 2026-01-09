using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Architect.Editor;
using Architect.Events;
using Architect.Multiplayer.Ssmp.Data;
using Architect.Placements;
using Architect.Storage;
using Architect.Utils;
using SSMP.Api.Client;
using UnityEngine;

namespace Architect.Multiplayer.Ssmp;

public class ArchitectClientAddon : ClientAddon
{
    public IClientApi API;

    protected override string Name => "Architect";
    protected override string Version => Assembly.GetExecutingAssembly().GetName().Version.ToString();
    public override bool NeedsNetwork => true;
    public override uint ApiVersion => 1;
    
    private object _updateManager;
    private MethodInfo _setEnterSceneData;
    private MethodInfo _getCurrentAnimationClip;

    public override void Initialize(IClientApi clientApi)
    {
        API = clientApi;

        var netReceiver = clientApi.NetClient.GetNetworkReceiver<PacketId>(this,
            id => id.GetData());
        
        netReceiver.RegisterPacketHandler<ClearPacketData>(PacketId.Clear, HandleClear);
        netReceiver.RegisterPacketHandler<PlacePacketData>(PacketId.Place, HandlePlace);
        netReceiver.RegisterPacketHandler<ErasePacketData>(PacketId.Erase, HandleErase);
        netReceiver.RegisterPacketHandler<LockPacketData>(PacketId.Lock, HandleLock);
        netReceiver.RegisterPacketHandler<EventPacketData>(PacketId.Event, HandleEvent);
        netReceiver.RegisterPacketHandler<MovePacketData>(PacketId.Move, HandleMove);
        netReceiver.RegisterPacketHandler<TilePacketData>(PacketId.Tiles, HandleTiles);
        
        var updateManagerProperty = API.NetClient.GetType().GetProperty("UpdateManager");
        _updateManager = updateManagerProperty!.GetGetMethod().Invoke(API.NetClient, []);
        _setEnterSceneData = _updateManager.GetType().GetMethod("SetEnterSceneData");
        
        var animationManagerField = API.ClientManager.GetType().GetField("_animationManager", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        var animationManager = animationManagerField!.GetValue(API.ClientManager);
        _getCurrentAnimationClip = animationManager.GetType().GetMethod("GetCurrentAnimationClip",
            BindingFlags.Public | BindingFlags.Static);
    }

    private static void HandleClear(ClearPacketData packet)
    {
        ArchitectPlugin.Logger.LogInfo("Receiving Reset Packet");
        if (packet.SceneName == GameManager.instance.sceneName) ActionManager.ReceiveAction(new ResetRoom());
        else ResetRoom.Execute(packet.SceneName);
    }

    private static readonly List<PlacePacketData> PlaceInfo = [];
    private static string _lastPlaceGuid;

    private static void HandlePlace(PlacePacketData packet)
    {
        ArchitectPlugin.Logger.LogInfo("Receiving Place Packet");

        if (packet.Guid != _lastPlaceGuid)
        {
            _lastPlaceGuid = packet.Guid;
            PlaceInfo.Clear();
        }
        
        PlaceInfo.Add(packet);
        if (PlaceInfo.Count != packet.Length) return;

        PlaceInfo.Sort((o1, o2) => o1.Index.CompareTo(o2.Index));
        var json = ZipUtils.Unzip(PlaceInfo.Select(o => o.SerializedObjects)
            .Aggregate((a, b) => a.Concat(b).ToArray()));

        if (packet.IsFullScene)
        {
            var levelData = StorageManager.DeserializeLevel(json);

            if (packet.IsScriptOnly)
            {
                var scene = StorageManager.LoadScene(packet.SceneName);
                scene.ScriptBlocks.Clear();
                scene.ScriptBlocks.AddRange(levelData.ScriptBlocks);
                StorageManager.SaveScene(packet.SceneName, scene);
            }
            else
            {
                StorageManager.WipeScheduledEdits(packet.SceneName);
                StorageManager.SaveScene(packet.SceneName, levelData);
            }

            if (packet.SceneName == GameManager.instance.sceneName || packet.SceneName == StorageManager.GLOBAL)
            {
                PlacementManager.InvalidateScene();
                EditManager.ReloadRequired = true;
            }
        }
        else
        {
            var action = new PlaceObjects(StorageManager.DeserializePlacements(json));
            if (packet.SceneName == GameManager.instance.sceneName) ActionManager.ReceiveAction(action);
            else action.Execute(packet.SceneName);
        }
    }

    private static void HandleErase(ErasePacketData packet)
    {
        ArchitectPlugin.Logger.LogInfo("Receiving Erase Packet");
        if (packet.SceneName == GameManager.instance.sceneName)
        {
            ActionManager.ReceiveAction(new EraseObject(PlacementManager.GetLevelData().Placements
                .Where(p => packet.Removals.Remove(p.GetId())).ToList()));
        }
        else EraseObject.Execute(packet.SceneName, packet.Removals);
    }

    private static void HandleLock(LockPacketData packet)
    {
        ArchitectPlugin.Logger.LogInfo("Receiving Lock Packet");
        if (packet.SceneName == GameManager.instance.sceneName)
        {
            var obj = PlacementManager.GetLevelData().Placements
                .FirstOrDefault(p => p.GetId() == packet.Toggle);
            if (obj != null) ActionManager.ReceiveAction(new ToggleLock(obj));
        }
        else ToggleLock.Execute(packet.SceneName, packet.Toggle);
    }

    private static void HandleTiles(TilePacketData packet)
    {
        ArchitectPlugin.Logger.LogInfo("Receiving Tile Packet");

        var toggle = new ToggleTile(packet.Tiles, packet.Empty);
        
        if (packet.SceneName == GameManager.instance.sceneName) toggle.Execute();
        else toggle.Execute(packet.SceneName);
    }

    private static void HandleMove(MovePacketData packet)
    {
        ArchitectPlugin.Logger.LogInfo("Receiving Move Packet");
        if (packet.SceneName == GameManager.instance.sceneName)
        {
            List<(ObjectPlacement, Vector3, Vector3)> movements = [];

            foreach (var (id, newPos) in packet.Movements)
            {
                var o = PlacementManager.GetLevelData().Placements
                    .FirstOrDefault(o => o.GetId() == id);
                if (o == null) continue;
                movements.Add((o, newPos, o.GetPos()));
            }
            ActionManager.ReceiveAction(new MoveObjects(movements));
        }
        else MoveObjects.Execute(packet.SceneName, packet.Movements);
    }

    private static void HandleEvent(EventPacketData packet)
    {
        ArchitectPlugin.Logger.LogInfo("Receiving Event Packet");
        if (packet.SceneName != GameManager.instance.sceneName) return;
        EventManager.BroadcastMp(packet.Event);
    }

    public void RefreshRoom()
    {
        ArchitectPlugin.Logger.LogInfo("Refreshing Room");
        var ht = HeroController.instance.transform;
        var id = _getCurrentAnimationClip.Invoke(null, []);
        _setEnterSceneData
            .Invoke(_updateManager, [
                GameManager.instance.sceneName, 
                new SSMP.Math.Vector2(ht.position.x, ht.position.y),
                ht.GetScaleX() > 0,
                Convert.ToUInt16(id)
            ]);
    }
}