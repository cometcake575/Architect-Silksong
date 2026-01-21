namespace Architect.Sharer.States;

public class Manage : MenuState
{
    public override MenuState ReturnState => SharerManager.HomeState;
    
    public override void OnOpen()
    {
        // TODO Set LevelConfig.CurrentInfo when opening a level
    }
}