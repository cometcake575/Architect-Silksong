namespace Architect.Sharer.States;

public class Upload : MenuState
{
    public override MenuState ReturnState => SharerManager.HomeState;

    public override void OnStart()
    {
        
    }
}