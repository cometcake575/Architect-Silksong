using System;
using System.Collections;
using Architect.Content.Preloads;
using Architect.Placements;
using Architect.Sharer.States;
using Architect.Storage;
using Architect.Utils;
using GlobalEnums;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Architect.Sharer;

public static class SharerManager
{
    // Appears when on Silksong main menu or sharer main menu
    public static GameObject OpenSharerBtn;
    // Appears on Silksong main menu
    public static GameObject EraseEditsBtn;
    // Appears when returnState is not null in the current MenuState
    public static GameObject ReturnBtn;

    private static bool _sharerOpen;
    
    private static GameObject _sharer;
    private static GameObject _states;
    
    private static MenuState _currentMenuState;
    public static MenuState HomeState;

    private static UIManager _uiManager;
    
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
        
        var bg = UIUtils.MakeImage("Background", _states, Vector3.zero,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(3000, 3000));
        bg.sprite = ResourceUtils.LoadSpriteResource("Sharer.bg");
        
        SetupToggleBtn();
        SetupReturnBtn();
        SetupResetBtn();
        
        HomeState = SetupMenuState<Home>("Home");
    }

    public static void Update()
    {
        if (!_uiManager)
        {
            _uiManager = Object.FindAnyObjectByType<UIManager>();
            if (!_uiManager) return;
        }
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

    private static void SetupToggleBtn()
    {
        var openEditor = ResourceUtils.LoadSpriteResource("Sharer.open");
        var closeEditor = ResourceUtils.LoadSpriteResource("Sharer.close");

        var (btn, img, _) = UIUtils.MakeButtonWithImage("Toggle Sharer UI", _sharer,
            new Vector3(-50, -50), new Vector2(1, 1), new Vector2(1, 1),
            220, 220);
        OpenSharerBtn = btn.gameObject;
        img.sprite = openEditor;

        btn.onClick.AddListener(ToggleSharer);
        return;

        void ToggleSharer()
        {
            _sharerOpen = !_sharerOpen;
            if (_sharerOpen)
            {
                img.sprite = closeEditor;
                _uiManager.StartCoroutine(FadeGameTitle());
                _uiManager.StartCoroutine(_uiManager.FadeOutCanvasGroup(_uiManager.mainMenuScreen));
            }
            else
            {
                img.sprite = openEditor;
                _states.SetActive(false);
                EraseEditsBtn.SetActive(true);
                _uiManager.UIGoToMainMenu();
            }
        }
    
        IEnumerator FadeGameTitle()
        {
            var sprite = _uiManager.gameTitle;
            while (sprite.color.a > 0.0)
            {
                if (!_sharerOpen) break;
                sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b,
                    sprite.color.a - Time.unscaledDeltaTime * 6.4f);
                yield return null;
            }

            sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, _sharerOpen ? 0 : 1);

            if (_sharerOpen)
            {
                _states.SetActive(true);
                EraseEditsBtn.SetActive(false);
                TransitionToState(HomeState);
            }

            yield return null;
        }
    }

    private static void SetupReturnBtn()
    {
        var returnIcon = ResourceUtils.LoadSpriteResource("Sharer.return", FilterMode.Point);

        var (btn, img, _) = UIUtils.MakeButtonWithImage("Return", _sharer,
            new Vector3(-50, -50), new Vector2(1, 1), new Vector2(1, 1),
            220, 220);
        ReturnBtn = btn.gameObject;
        ReturnBtn.SetActive(false);
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

        EraseEditsBtn = eraseBtn.gameObject;

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
        }
    }

    private class EraseBtn : MonoBehaviour, IPointerDownHandler
    {
        public Action OnClick;

        public void OnPointerDown(PointerEventData eventData) => OnClick();
    }
}