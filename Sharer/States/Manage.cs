namespace Architect.Sharer.States;

public class Manage : MenuState
{
    public override MenuState ReturnState => SharerManager.HomeState;
    
}