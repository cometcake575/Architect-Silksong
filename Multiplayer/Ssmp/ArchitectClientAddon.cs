using System.Reflection;
using System.Runtime.CompilerServices;
using Architect.Editor;
using Architect.Multiplayer.Ssmp.Data;
using SSMP.Api.Client;
using SSMP.Networking.Packet;

namespace Architect.Multiplayer.Ssmp;

public class ArchitectClientAddon : ClientAddon
{
    public IClientApi API;

    protected override string Name => "Architect";
    protected override string Version => Assembly.GetExecutingAssembly().GetName().Version.ToString();
    public override bool NeedsNetwork => true;

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public override void Initialize(IClientApi clientApi)
    {
        API = clientApi;

        var netReceiver = API.NetClient.GetNetworkReceiver<PacketId>(this, GetData);
        netReceiver.RegisterPacketHandler<ClearPacketData>(PacketId.Clear, HandleClear);
    }

    private void HandleClear(ClearPacketData packet)
    {
        if (packet.SceneName == GameManager.instance.sceneName) ActionManager.ReceiveAction(new ResetRoom());
        else ResetRoom.Execute(packet.SceneName);
    }

    public IPacketData GetData(PacketId packetId)
    {
        return packetId switch
        {
            //PacketId.Refresh => new ClearPacketData(),
            PacketId.Clear => new ClearPacketData(),
            //PacketId.Move => new ClearPacketData(),
            //PacketId.Erase => new ClearPacketData(),
            //PacketId.Tiles => new ClearPacketData(),
            //PacketId.Lock => new ClearPacketData(),
            //PacketId.Place => new ClearPacketData(),
            //PacketId.Event => new ClearPacketData(),
            _ => null
        };
    }
}