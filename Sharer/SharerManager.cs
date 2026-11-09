using System;
using System.Collections;
using Architect.Content.Preloads;
using Architect.Placements;
using Architect.Sharer.States;
using Architect.Storage;
using Architect.Utils;
using Architect.Workshop;
using GlobalEnums;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Architect.Sharer;

public static class SharerManager
{
    // Appears when on Silksong main menu or sharer main menu
    public static GameObject OpenSharerBtn;
    // Appears on Silksong main menu
    private static GameObject _eraseEditsBtn;
    // Appears when returnState is not null in the current MenuState
    public static Button ReturnBtn;
    
    public static bool SharerOpen;
    
    private static GameObject _sharer;
    private static GameObject _states;
    
    private static MenuState _currentMenuState;
    public static MenuState HomeState;

    private static UIManager _uiManager;

    public static readonly Sprite Placeholder = ResourceUtils.LoadSpriteResource("Sharer.placeholder", FilterMode.Point);
    
    private static Image _bg;
    
    public static void Init()
    {
        _sharer = new GameObject("[Architect] Level Sharer");
        _sharer.SetActive(false);
        Object.DontDestroyOnLoad(_sharer);

        _sharer.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = _sharer.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        _sharer.AddComponent<GraphicRaycaster>();

        _states = new GameObject("States");
        _states.SetActive(false);
        _states.RemoveOffset();
        _states.transform.SetParent(_sharer.transform, true);
        
        _bg = UIUtils.MakeImage("Background", _states, Vector3.zero,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(3000, 3000));
        _bg.sprite = ResourceUtils.LoadSpriteResource("Sharer.bg");
        
        SetupToggleBtn();
        SetupReturnBtn();
        SetupResetBtn();
        
        HomeState = SetupMenuState<Home>("Home");
        
        typeof(UIManager).Hook(nameof(UIManager.Awake),
            (Action<UIManager> orig, UIManager self) =>
            {
                orig(self);
                _uiManager = self;
                SetupSharerBtn();
            });
        
        typeof(UIManager).Hook(nameof(UIManager.UIGoToMainMenu),
            (Action<UIManager> orig, UIManager self) =>
            {
                orig(self);
                AdjustNavigations();
            });
    }

    public static void Update()
    {
        if (!_uiManager) return;
        _sharer.SetActive(_uiManager.menuState == MainMenuState.MAIN_MENU && PreloadManager.HasPreloaded);
    }

    public static void TransitionToState(MenuState state)
    {
        if (_currentMenuState) _currentMenuState.Close();
        state.Open();
        _currentMenuState = state;
    }

    public static MenuState SetupMenuState<T>(string name) where T : MenuState
    {
        var obj = new GameObject(name);
        obj.RemoveOffset();
        obj.transform.SetParent(_states.transform, false);
        obj.SetActive(false);
        return obj.AddComponent<T>();
    }

    private static MenuButton _sharerBtn;

    private static void SetupSharerBtn()
    {
        var optionsBtn = _uiManager.mainMenuButtons.optionsButton;
        
        var sharerBtnObj = Object.Instantiate(optionsBtn.gameObject, _uiManager.mainMenuButtons.transform);
        _sharerBtn = sharerBtnObj.GetComponent<MenuButton>();
        sharerBtnObj.name = "SharerButton";
        
        sharerBtnObj.transform.SetAsLastSibling();
        sharerBtnObj.transform.SetSiblingIndex(optionsBtn.transform.GetSiblingIndex() + 1);

        var textUI = sharerBtnObj.GetComponent<AutoLocalizeTextUI>();
        textUI.textKey = "Level Sharer";
        textUI.sheetTitle = "ArchitectMod";
        textUI.OnValidate();
        textUI.RefreshTextFromLocalization();
        
        var et = sharerBtnObj.GetComponent<EventTrigger>();
        et.triggers.Clear();
        AddTrigger(EventTriggerType.Submit);
        AddTrigger(EventTriggerType.PointerClick);
        
        AdjustNavigations();
        return;

        void AddTrigger(EventTriggerType triggerType)
        {
            var entry = new EventTrigger.Entry
            {
                eventID = triggerType
            };
            entry.callback.AddListener(_ =>
            {
                SharerOpen = true;
                _uiManager.StartCoroutine(FadeGameTitle());
                _uiManager.StartCoroutine(_uiManager.FadeOutCanvasGroup(_uiManager.mainMenuScreen));
            });
            et.triggers.Add(entry);
        }
    
        IEnumerator FadeGameTitle()
        {
            var sprite = _uiManager.gameTitle;
            while (sprite.color.a > 0.0)
            {
                if (!SharerOpen) break;
                sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b,
                    sprite.color.a - Time.unscaledDeltaTime * 6.4f);
                yield return null;
            }

            sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, SharerOpen ? 0 : 1);
            
            if (SharerOpen)
            {
                _bg.color = Color.clear;
                _states.SetActive(true);
                OpenSharerBtn.SetActive(true);
                _eraseEditsBtn.SetActive(true);
                TransitionToState(HomeState);

                var i = 0f;
                while (i < 1)
                {
                    yield return null;
                    i += Time.deltaTime * 5;
                    _bg.color = Color.Lerp(Color.clear, Color.white, Mathf.Clamp01(i));
                }

                _bg.color = Color.white;
            }

            yield return null;
        }
    }

    public static void AdjustNavigations()
    {
        var optionsBtn = _uiManager.mainMenuButtons.optionsButton;
        var achievementsBtn = _uiManager.mainMenuButtons.achievementsButton;
        
        var sharerNav = _sharerBtn.navigation;
        sharerNav.mode = Navigation.Mode.Automatic;
        _sharerBtn.originalNavigation = sharerNav;
        _sharerBtn.navigation = sharerNav;

        var optionsNav = optionsBtn.navigation;
        optionsNav.mode = Navigation.Mode.Automatic;
        optionsBtn.originalNavigation = optionsNav;
        optionsBtn.navigation = optionsNav;

        var achievementsNav = achievementsBtn.navigation;
        achievementsNav.mode = Navigation.Mode.Automatic;
        achievementsBtn.originalNavigation = achievementsNav;
        achievementsBtn.navigation = achievementsNav;
    }

    private static void SetupToggleBtn()
    {
        var (btn, img, _) = UIUtils.MakeButtonWithImage("Toggle Sharer UI", _sharer,
            new Vector3(-50, -50), new Vector2(1, 1), new Vector2(1, 1),
            220, 220);
        OpenSharerBtn = btn.gameObject;
        OpenSharerBtn.SetActive(false);
        img.sprite = ResourceUtils.LoadSpriteResource("Sharer.close");

        btn.onClick.AddListener(ToggleSharer);
        return;

        void ToggleSharer()
        {
            SharerOpen = false;
            
            _states.SetActive(false);
            OpenSharerBtn.SetActive(false);
            _eraseEditsBtn.SetActive(false);
            _uiManager.UIGoToMainMenu();
        }
    }

    private static void SetupReturnBtn()
    {
        var returnIcon = ResourceUtils.LoadSpriteResource("Sharer.return", FilterMode.Point);

        var (btn, img, _) = UIUtils.MakeButtonWithImage("Return", _sharer,
            new Vector3(-50, -50), new Vector2(1, 1), new Vector2(1, 1),
            220, 220);
        ReturnBtn = btn;
        ReturnBtn.gameObject.SetActive(false);
        img.sprite = returnIcon;

        btn.onClick.AddListener(GoToReturnState);
        return;
        
        void GoToReturnState()
        {
            if (!_currentMenuState || !_currentMenuState.ReturnState) return;
            TransitionToState(_currentMenuState.ReturnState);
        }
    }

    private static void SetupResetBtn()
    {
        var (eraseBtn, eraseImg, _) = UIUtils.MakeButtonWithImage("Erase Edits", _sharer,
            new Vector3(-50, -140), new Vector2(1, 1), new Vector2(1, 1),
            220, 220);
        var eraseAll = ResourceUtils.LoadSpriteResource("erase_all");
        var eraseAll3 = ResourceUtils.LoadSpriteResource("erase_all_3");
        var eraseAll2 = ResourceUtils.LoadSpriteResource("erase_all_2");
        var eraseAll1 = ResourceUtils.LoadSpriteResource("erase_all_1");

        _eraseEditsBtn = eraseBtn.gameObject;
        _eraseEditsBtn.SetActive(false);

        eraseImg.sprite = eraseAll;

        eraseBtn.gameObject.AddComponent<EraseBtn>().OnClick +=
            () => ArchitectPlugin.Instance.StartCoroutine(DoErase());
        return;

        IEnumerator DoErase()
        {
            var time = Time.time;

            while (Time.time - time < 3)
            {
                yield return null;
                if (!Input.GetMouseButton(0))
                {
                    eraseImg.sprite = eraseAll;
                    yield break;
                }
                var t = Time.time - time;
                eraseImg.sprite = t > 2 ? eraseAll1 : t > 1 ? eraseAll2 : eraseAll3;
            }

            eraseImg.sprite = eraseAll;
            StorageManager.WipeLevelData();
            PlacementManager.InvalidateScene();
            WorkshopManager.LoadWorkshop(new WorkshopData());
        }
    }

    private class EraseBtn : MonoBehaviour, IPointerDownHandler
    {
        public Action OnClick;

        public void OnPointerDown(PointerEventData eventData) => OnClick();
    }

    public class SpriteURL : MonoBehaviour;

    public static void DoGetSprite(string url, Image apply)
    {
        var spriteURL = apply.gameObject.GetOrAddComponent<SpriteURL>();
        spriteURL.StopAllCoroutines();
        spriteURL.StartCoroutine(GetSprite(url, apply));
    }
    
    private static IEnumerator GetSprite(string url, Image apply)
    {
        apply.sprite = ArchitectPlugin.BlankSprite;
        var www = UnityWebRequestTexture.GetTexture(url);
        www.timeout = 30;
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            var tex = DownloadHandlerTexture.GetContent(www);
            if (tex.width < 1 || tex.height < 1) apply.sprite = Placeholder;
            else apply.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), default);
        }
        else apply.sprite = Placeholder;
    }
}