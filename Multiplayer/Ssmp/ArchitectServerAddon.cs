using SSMP.Api.Server;

namespace Architect.Multiplayer.Ssmp;

public class ArchitectServerAddon : ServerAddon
{
    protected override string Name => "Architect";
    protected override string Version => "2.0.3";
    public override bool NeedsNetwork => true;
    
    public override void Initialize(IServerApi serverApi)
    {
        
    }
}