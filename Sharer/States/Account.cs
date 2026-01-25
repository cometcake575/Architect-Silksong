using System.Collections;
using Architect.Sharer.Info;
using Architect.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
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
        _username.gameObject.AddComponent<UserConfig>();
        
        _desc = UIUtils.MakeLabel("Desc", gameObject,
            new Vector2(20, -40),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            maxWidth: 400,
            maxHeight: 200).textComponent;
        _desc.fontSize = 16;
        _desc.alignment = TextAnchor.UpperLeft;
        _desc.horizontalOverflow = HorizontalWrapMode.Wrap;
        _desc.gameObject.AddComponent<DescConfig>();

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
        StartCoroutine(SetToMainUser());
        _configUI.SetActive(false);
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
        SharerManager.DoGetSprite(CurrentUserInfo.PfpUrl, _pfp);
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
        protected abstract bool IsBig { get; }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            Instance.ShowOptionType(Title, Id, Current, MaxLength, IsBig);
        }
    }

    public class UserConfig : OpenConfig
    {
        protected override string Title => "Change Username:";
        protected override string Id => "username";
        protected override string Current => CurrentUserInfo.Username;
        protected override int MaxLength => 20;
        protected override bool IsBig => false;
    }

    public class DescConfig : OpenConfig
    {
        protected override string Title => "Change About Me:";
        protected override string Id => "description";
        protected override string Current => CurrentUserInfo.Description;
        protected override int MaxLength => 1000;
        protected override bool IsBig => true;
    }

    public class PfpConfig : OpenConfig
    {
        protected override string Title => "Change Icon:";
        protected override string Id => "icon_url";
        protected override string Current => CurrentUserInfo.PfpUrl;
        protected override int MaxLength => 1000;
        protected override bool IsBig => false;
    }

    private GameObject _configUI;
    private Text _changeTitle;
    private InputField _changeField;
    private RectTransform _changeLabel;
    private string _changeType;
    private Transform _bg;

    private void SetupConfigUI()
    {
        _configUI = new GameObject("Config UI");
        _configUI.transform.SetParent(gameObject.transform, false);
        _configUI.RemoveOffset();

        var bg = UIUtils.MakeImage("Bg", _configUI, Vector2.zero,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(10000, 10000));
        bg.sprite = ResourceUtils.LoadSpriteResource("Sharer.config_bg");
        _bg = bg.transform;

        var uiChild = new GameObject("Content");
        uiChild.transform.SetParent(_configUI.transform, false);
        uiChild.RemoveOffset();
        
        (_changeField, var cfTxt) = UIUtils.MakeTextbox("Change", uiChild, Vector2.zero,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), 1000, 80);
        cfTxt.transform.localScale = Vector3.one;
        cfTxt.textComponent.fontSize = 16;
        _changeLabel = (RectTransform)cfTxt.transform;
        _changeLabel.sizeDelta /= 3;
        
        _changeTitle = UIUtils.MakeLabel("Type", uiChild, new Vector2(0, 140),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f))
            .textComponent;
        _changeTitle.fontSize = 20;
        _changeTitle.alignment = TextAnchor.MiddleCenter;
        
        var (cancel, cancelLabel) = UIUtils.MakeTextButton("Cancel", "Cancel", uiChild, 
            new Vector2(-70, -140),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            size: new Vector2(360, 80));
        cancelLabel.textComponent.fontSize = 18;
        cancel.onClick.AddListener(() =>
        {
            _configUI.SetActive(false);
        });

        var (confirm, confirmLabel) = UIUtils.MakeTextButton("Confirm", "Confirm", uiChild, 
            new Vector2(70, -140),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            size: new Vector2(360, 80));
        confirmLabel.textComponent.fontSize = 18;
        confirm.onClick.AddListener(() =>
        {
            cancel.interactable = false;
            confirm.interactable = false;
            StartCoroutine(RequestManager.SendChangeRequest(
                _changeType, 
                _changeField.text,
                b => StartCoroutine(OnComplete(b))));
        });

        return;

        IEnumerator OnComplete(bool b)
        {
            if (b) yield return SetToMainUser();
            cancel.interactable = true;
            confirm.interactable = true;
            _configUI.SetActive(false);
        }
    }

    private void ShowOptionType(string changeTitle, string changeType, string current, int maxLength, bool isBig)
    {
        _changeField.characterLimit = maxLength;
        ((RectTransform)_changeField.transform).sizeDelta = new Vector2(1000, isBig ? 620 : 80);
        _changeLabel.sizeDelta = new Vector2(325, isBig ? 200 : 26);
        _changeField.lineType = isBig ? InputField.LineType.MultiLineNewline : InputField.LineType.SingleLine;
        _changeField.textComponent.alignment = isBig ? TextAnchor.UpperLeft : TextAnchor.MiddleLeft;
        
        _changeTitle.text = changeTitle;
        _changeField.text = current;
        _changeType = changeType;
        
        _configUI.SetActive(true);
        
        _bg.SetAsFirstSibling();
    }
}