namespace Architect.Sharer.States;

public class Account : MenuState
{
    public override MenuState ReturnState => SharerManager.HomeState;

    public override void OnStart()
    {
        
    }
}