using SSMP.Api.Client;

namespace Architect.Multiplayer.Ssmp;

public class ArchitectClientAddon : ClientAddon
{
    public IClientApi API;

    protected override string Name => "Architect";
    protected override string Version => "2.0.3";
    public override bool NeedsNetwork => true;
    
    public override void Initialize(IClientApi clientApi)
    {
        API = clientApi;
    }
}