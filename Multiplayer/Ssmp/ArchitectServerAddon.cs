using System.Collections.Generic;
using System.Reflection;
using Architect.Multiplayer.Ssmp.Data;
using SSMP.Api.Command.Server;
using SSMP.Api.Server;
using SSMP.Networking.Packet;

namespace Architect.Multiplayer.Ssmp;

public class ArchitectServerAddon : ServerAddon
{
    private static ArchitectServerAddon _instance;
    
    public IServerApi API;
    public EditorLockType LockType = EditorLockType.Unlocked;
    
    protected override string Name => "Architect";
    protected override string Version => Assembly.GetExecutingAssembly().GetName().Version.ToString();
    public override bool NeedsNetwork => true;
    public override uint ApiVersion => 1;

    public override void Initialize(IServerApi serverApi)
    {
        _instance = this;
        API = serverApi;
        
        var netReceiver = serverApi.NetServer.GetNetworkReceiver<PacketId>(this, 
            id => id.GetData());
        
        netReceiver.RegisterPacketHandler<ClearPacketData>(PacketId.Clear, (id, packet) => 
            Share(id, packet, PacketId.Clear));
        netReceiver.RegisterPacketHandler<PlacePacketData>(PacketId.Place, (id, packet) => 
            Share(id, packet, PacketId.Place));
        netReceiver.RegisterPacketHandler<ErasePacketData>(PacketId.Erase, (id, packet) => 
            Share(id, packet, PacketId.Erase));
        netReceiver.RegisterPacketHandler<LockPacketData>(PacketId.Lock, (id, packet) => 
            Share(id, packet, PacketId.Lock));
        netReceiver.RegisterPacketHandler<EventPacketData>(PacketId.Event, (id, packet) => 
            Share(id, packet, PacketId.Event));
        netReceiver.RegisterPacketHandler<MovePacketData>(PacketId.Move, (id, packet) => 
            Share(id, packet, PacketId.Move));
        netReceiver.RegisterPacketHandler<TilePacketData>(PacketId.Tiles, (id, packet) => 
            Share(id, packet, PacketId.Tiles));
        netReceiver.RegisterPacketHandler<LockEditorPacketData>(PacketId.LockEditor, (id, _) => SendLockInfo(id));
        
        serverApi.CommandManager.RegisterCommand(new SetEditPermCommand());
    }

    private void Share(ushort id, IPacketData packet, PacketId packetId)
    {
        var sender = API.NetServer.GetNetworkSender<PacketId>(this);
        List<ushort> ids = [];
        foreach (var player in API.ServerManager.Players) if (player.Id != id) ids.Add(player.Id);
        sender.SendSingleData(packetId, packet, ids.ToArray());
    }

    private class SetEditPermCommand : IServerCommand
    {
        public string Trigger => "/architect";
        public string[] Aliases => [];
        
        public void Execute(ICommandSender commandSender, string[] arguments)
        {
            if (arguments.Length == 2)
            {
                switch (arguments[1])
                {
                    case "lock":
                        _instance.LockType = EditorLockType.Locked;
                        _instance.ShareLock();
                        commandSender.SendMessage("Editor locked for all players");
                        return;
                    case "unlock":
                        _instance.LockType = EditorLockType.Unlocked;
                        _instance.ShareLock();
                        commandSender.SendMessage("Editor unlocked");
                        return;
                    case "authlock":
                        _instance.LockType = EditorLockType.AdminsOnly;
                        _instance.ShareLock();
                        commandSender.SendMessage("Editor locked for unauthorised players");
                        return;
                }
            }
            commandSender.SendMessage("Invalid usage: </architect> <lock|unlock|authlock>" +
                                      "\n'lock' to lock edit mode, 'unlock' to unlock, " +
                                      "'authlock' to lock for unauthorised players.");
        }
        
        public bool AuthorizedOnly => true;
    }

    private void SendLockInfo(ushort id)
    {
        var packet = new LockEditorPacketData();
        
        var sender = API.NetServer.GetNetworkSender<PacketId>(this);
        if (LockType == EditorLockType.AdminsOnly)
        {
            packet.LockType = API.ServerManager.GetPlayer(id)!.IsAuthorized
                ? EditorLockType.Unlocked
                : EditorLockType.Locked;
        } else packet.LockType = LockType;
        sender.SendSingleData(PacketId.LockEditor, packet, id);
    }

    private void ShareLock()
    {
        foreach (var player in API.ServerManager.Players) SendLockInfo(player.Id);
    }
}