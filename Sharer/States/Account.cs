using System.Collections;
using Architect.Sharer.Info;
using Architect.Utils;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Architect.Sharer.States;

public class Account : MenuState
{
    public static UserInfo CurrentUserInfo;
    
    public override MenuState ReturnState => SharerManager.HomeState;

    private Text _username;
    private Text _desc;
    private Image _pfp;

    public override void OnStart()
    {
        var userTitle = UIUtils.MakeLabel("Username Title", gameObject,
            new Vector2(0, 150),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            maxWidth: 400).textComponent;
        userTitle.text = "Username:";
        userTitle.fontSize = 22;
        userTitle.alignment = TextAnchor.UpperLeft;
        
        var descTitle = UIUtils.MakeLabel("Desc Title", gameObject,
            new Vector2(0, 50),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            maxWidth: 400).textComponent;
        descTitle.text = "About Me:";
        descTitle.fontSize = 22;
        descTitle.alignment = TextAnchor.UpperLeft;
        
        _username = UIUtils.MakeLabel("Username", gameObject,
            new Vector2(20, 120),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            maxWidth: 400).textComponent;
        _username.fontSize = 16;
        _username.alignment = TextAnchor.UpperLeft;
        
        _desc = UIUtils.MakeLabel("Desc", gameObject,
            new Vector2(20, -40),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            maxWidth: 400,
            maxHeight: 200).textComponent;
        _desc.fontSize = 16;
        _desc.alignment = TextAnchor.UpperLeft;
        _desc.horizontalOverflow = HorizontalWrapMode.Wrap;

        _pfp = UIUtils.MakeImage("Profile Picture", gameObject,
            new Vector2(170, 130),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(200, 200));
        
        var (btn, label) = UIUtils.MakeTextButton("Sign Out", "Sign Out", gameObject,
            new Vector2(0, -180),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            size: new Vector2(420, 80));
        label.textComponent.fontSize = 20;
        label.textComponent.horizontalOverflow = HorizontalWrapMode.Wrap;
        
        btn.onClick.AddListener(SignOut);
    }

    public override void OnOpen()
    {
        ArchitectPlugin.Instance.StartCoroutine(SetToMainUser());
    }

    public IEnumerator SetToMainUser()
    {
        if (RequestManager.SharerKey == null)
        {
            SharerManager.TransitionToState(ReturnState);
            CurrentUserInfo = null;
            yield break;
        }

        if (!(CurrentUserInfo?.UsingToken ?? false) ||
            CurrentUserInfo?.UserID != RequestManager.SharerKey)
        {
            _username.text = "";
            _desc.text = "";
            _pfp.sprite = ArchitectPlugin.BlankSprite;

            CurrentUserInfo = new UserInfo(RequestManager.SharerKey, true);
            yield return CurrentUserInfo.Setup();

            if (!CurrentUserInfo.IsSetup) yield break;
            _username.text = CurrentUserInfo.Username;
            _desc.text = CurrentUserInfo.Description;
            ArchitectPlugin.Instance.StartCoroutine(GetSprite(CurrentUserInfo.PfpUrl));
        }
    }

    private IEnumerator GetSprite(string url)
    {
        var www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();
        
        if (www.result == UnityWebRequest.Result.Success)
        {
            var tex = DownloadHandlerTexture.GetContent(www);
            _pfp.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), default);
        }
    }

    private static void SignOut()
    {
        RequestManager.SharerKey = null;
        SharerManager.TransitionToState(SharerManager.HomeState);
    }
}