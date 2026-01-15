namespace Architect.Sharer.States;

public class Browse : MenuState
{
    public override MenuState ReturnState => SharerManager.HomeState;
    
}