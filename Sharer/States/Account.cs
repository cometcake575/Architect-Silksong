using System.Collections;
using Architect.Sharer.Info;
using Architect.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Architect.Sharer.States;

public class Account : MenuState
{
    public static Account Instance;
    
    public static UserInfo CurrentUserInfo;
    
    public override MenuState ReturnState => SharerManager.HomeState;

    private Text _username;
    private Text _desc;
    private Image _pfp;

    public override void OnStart()
    {
        Instance = this;
        
        var userTitle = UIUtils.MakeLabel("Username Title", gameObject,
            new Vector2(0, 150),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            maxWidth: 400).textComponent;
        userTitle.text = "Username:";
        userTitle.fontSize = 22;
        userTitle.alignment = TextAnchor.UpperLeft;
        userTitle.gameObject.AddComponent<UserConfig>();
        
        var descTitle = UIUtils.MakeLabel("Desc Title", gameObject,
            new Vector2(0, 50),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            maxWidth: 400).textComponent;
        descTitle.text = "About Me:";
        descTitle.fontSize = 22;
        descTitle.alignment = TextAnchor.UpperLeft;
        descTitle.gameObject.AddComponent<DescConfig>();
        
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
        _pfp.gameObject.AddComponent<PfpConfig>();
        
        var (btn, label) = UIUtils.MakeTextButton("Sign Out", "Sign Out", gameObject,
            new Vector2(0, -180),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            size: new Vector2(420, 80));
        label.textComponent.fontSize = 20;
        label.textComponent.horizontalOverflow = HorizontalWrapMode.Wrap;
        
        btn.onClick.AddListener(SignOut);

        SetupConfigUI();
    }
    
    public override void OnOpen()
    {
        if (CurrentUserInfo == null || CurrentUserInfo.UserID != RequestManager.SharerKey)
        {
            ArchitectPlugin.Instance.StartCoroutine(SetToMainUser());
        }
    }

    public IEnumerator SetToMainUser()
    {
        if (RequestManager.SharerKey == null)
        {
            SharerManager.TransitionToState(ReturnState);
            CurrentUserInfo = null;
            yield break;
        }

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

    public abstract class OpenConfig : MonoBehaviour, IPointerClickHandler
    {
        protected abstract string Title { get; }
        protected abstract string Id { get; }
        protected abstract string Current { get; }
        protected abstract int MaxLength { get; }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            Instance.ShowOptionType(Title, Id, Current, MaxLength);
        }
    }

    public class UserConfig : OpenConfig
    {
        protected override string Title => "Change Username:";
        protected override string Id => "username";
        protected override string Current => CurrentUserInfo.Username;
        protected override int MaxLength => 20;
    }

    public class DescConfig : OpenConfig
    {
        protected override string Title => "Change About Me:";
        protected override string Id => "description";
        protected override string Current => CurrentUserInfo.Description;
        protected override int MaxLength => 1000;
    }

    public class PfpConfig : OpenConfig
    {
        protected override string Title => "Change Icon:";
        protected override string Id => "icon_url";
        protected override string Current => CurrentUserInfo.PfpUrl;
        protected override int MaxLength => 1000;
    }

    private GameObject _configUI;
    private Text _changeTitle;
    private InputField _changeField;
    private string _changeType;

    private void SetupConfigUI()
    {
        _configUI = new GameObject("Config UI");
        _configUI.transform.SetParent(gameObject.transform, false);
        _configUI.RemoveOffset();
        _configUI.SetActive(false);

        var bg = UIUtils.MakeImage("Bg", _configUI, Vector2.zero,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(10000, 10000));
        bg.sprite = ResourceUtils.LoadSpriteResource("Sharer.config_bg");

        (_changeField, var cftxt) = UIUtils.MakeTextbox("Change", _configUI, new Vector2(0, 50),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), 1000, 80);

        _changeTitle = UIUtils.MakeLabel("Type", _configUI, new Vector2(0, 100),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f))
            .textComponent;
        _changeTitle.fontSize = 20;
        
        var cancel = UIUtils.MakeTextButton("Cancel", "Cancel", _configUI, new Vector2(50, -40),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            size: new Vector2(420, 80)).Item1;
        cancel.onClick.AddListener(() =>
        {
            _configUI.SetActive(false);
        });

        var confirm = UIUtils.MakeTextButton("Confirm", "Confirm", _configUI, new Vector2(-50, -40),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            size: new Vector2(420, 80)).Item1;
        
        confirm.onClick.AddListener(() =>
        {
            cancel.interactable = false;
            confirm.interactable = false;
            ArchitectPlugin.Instance.StartCoroutine(RequestManager.SendChangeRequest(
                _changeType, 
                _changeField.text,
                b => ArchitectPlugin.Instance.StartCoroutine(OnComplete(b))));
        });

        bg.transform.SetAsFirstSibling();

        return;

        IEnumerator OnComplete(bool b)
        {
            if (b) yield return SetToMainUser();
            cancel.interactable = true;
            confirm.interactable = true;
            _configUI.SetActive(false);
        }
    }

    private void ShowOptionType(string changeTitle, string changeType, string current, int maxLength)
    {
        _changeTitle.text = changeTitle;
        _changeField.text = current;
        _changeField.characterLimit = maxLength;
        _changeType = changeType;
        
        _configUI.SetActive(true);
    }
}