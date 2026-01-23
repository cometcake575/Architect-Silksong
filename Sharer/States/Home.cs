using Architect.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Architect.Sharer.States;

public class Home : MenuState
{
    public static MenuState Manage;
    
    private static Button _manageBtn;
    private static Button _uploadBtn;
    
    private Text _txt;
    
    public override void OnStart()
    {
        var img = UIUtils.MakeImage("Title", gameObject, new Vector2(0, 95),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(1030, 1030));
        img.preserveAspect = true;
        img.sprite = ResourceUtils.LoadSpriteResource("Sharer.title");
        
        var (btn, label) = UIUtils.MakeTextButton("Account", "Account", gameObject, 
            new Vector2(0, -40),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), 
            size: new Vector2(420, 80));
        _txt = label.textComponent;
        _txt.fontSize = 20;
        
        var login = SharerManager.SetupMenuState<Login>("Login");
        var account = SharerManager.SetupMenuState<Account>("Account");
        
        btn.onClick.AddListener(() =>
        {
            SharerManager.TransitionToState(RequestManager.SharerKey == null ? login : account);
        });
        
        MakeButton<Browse>("Browse Levels", -80);
        _uploadBtn = MakeButton<LevelConfig>("Upload", -120).Item2;
        (Manage, _manageBtn) = MakeButton<Manage>("Manage Levels", -160);
    }

    public override void OnOpen()
    {
        LevelConfig.CurrentInfo = null;
    }

    private (MenuState, Button) MakeButton<T>(string stateName, float y) where T : MenuState
    {
        var (btn, label) = UIUtils.MakeTextButton(stateName, stateName, gameObject, 
            new Vector2(0, y), 
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), 
            size: new Vector2(420, 85));
        label.textComponent.fontSize = 20;
        
        var state = SharerManager.SetupMenuState<T>(stateName);
        
        btn.onClick.AddListener(() =>
        {
            SharerManager.TransitionToState(state);
        });
        return (state, btn);
    }

    private void Update()
    {
        _txt.text = RequestManager.SharerKey == null ? "Log In / Sign Up" : "Account";
        _manageBtn.interactable = RequestManager.SharerKey != null;
        _uploadBtn.interactable = RequestManager.SharerKey != null;
    }
}