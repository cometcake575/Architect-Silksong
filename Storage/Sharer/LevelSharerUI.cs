using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Architect.Content.Preloads;
using Architect.Utils;
using GlobalEnums;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using UIUtils = Architect.Utils.UIUtils;

namespace Architect.Storage.Sharer;

public static class LevelSharerUI
{
    private const float MENU_FADE_SPEED = 3.2f;
    private const int LEVELS_PER_PAGE = 6;

    public static bool CurrentlyDownloading;
    
    private static int _index;
    private static List<Dictionary<string, string>> _orderedCurrentLevels;
    private static List<Dictionary<string, string>> _currentLevels;
    private static readonly List<(Text, Text, Text, GameObject, GameObject, GameObject, GameObject, Image)> DownloadChoices = [];

    [CanBeNull]
    public static string APIKey
    {
        get => _apiKey;
        set
        {
            StorageManager.SaveApiKey(value);
            _apiKey = value;
        }
    }
    
    [CanBeNull] private static string _apiKey = StorageManager.LoadApiKey();
    
    private static GameObject _canvasObj;
    private static GameObject _levelSharerObj;
    private static GameObject _backgroundObj;

    private static UIManager _uiManager;
    
    private static bool _viewing;

    private static InputField _descriptionInput;
    private static InputField _creatorInput;

    private static readonly List<Selectable> InteractableWhenLoggedIn = [];
    private static readonly List<Selectable> InteractableWhenLoggedOut = [];
    private static readonly List<Selectable> UninteractableWhenDownloading = [];
    
    public static void Init()
    {
        SetupCanvas();
        
        SetupBackground();
        SetupSwitchArea();
        SetupSearchArea();
        SetupLevelsArea();
        SetupUploadArea();
        SetupLoginArea();
        
        RefreshActiveOptions();
    }

    public static void Update()
    {
        if (!_uiManager)
        {
            _uiManager = Object.FindAnyObjectByType<UIManager>();
            if (!_uiManager) return;
        }
        _canvasObj.SetActive(_uiManager.menuState == MainMenuState.MAIN_MENU && PreloadManager.HasPreloaded);
    }

    private static void SetupCanvas()
    {
        _canvasObj = new GameObject("[Architect] Level Sharer Canvas");
        _canvasObj.SetActive(false);
        Object.DontDestroyOnLoad(_canvasObj);

        _canvasObj.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = _canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        
        _canvasObj.AddComponent<GraphicRaycaster>();

        _levelSharerObj = new GameObject("Level Sharer");
        _levelSharerObj.SetActive(false);
        
        var trans = _levelSharerObj.AddComponent<RectTransform>();
        trans.offsetMin = Vector2.zero;
        trans.offsetMax = Vector2.zero;
        
        trans.anchorMin = Vector2.zero;
        trans.anchorMax = Vector2.one;
        trans.SetParent(_canvasObj.transform, false);
    }

    private static void SetupBackground()
    {
        var bg = UIUtils.MakeImage("Background", _canvasObj, Vector3.zero,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(3000, 3000));
        bg.sprite = ResourceUtils.LoadSpriteResource("level_sharer_bg");
        _backgroundObj = bg.gameObject;
        _backgroundObj.SetActive(false);
        _backgroundObj.transform.SetAsFirstSibling();
    }
    
    private static void SetupSwitchArea()
    {
        var openEditor = ResourceUtils.LoadSpriteResource("level_sharer_open");
        var closeEditor = ResourceUtils.LoadSpriteResource("level_sharer_close");

        var (btn, img, _) = UIUtils.MakeButtonWithImage("Toggle Sharer UI", _canvasObj,
            new Vector3(-50, -50), new Vector2(1, 1), new Vector2(1, 1),
            220, 220);
        img.sprite = openEditor;
        
        UninteractableWhenDownloading.Add(btn);

        btn.onClick.AddListener(() =>
        {
            _viewing = !_viewing;
            if (_viewing)
            {
                img.sprite = closeEditor;
                _uiManager.StartCoroutine(FadeGameTitle());
                _uiManager.StartCoroutine(_uiManager.FadeOutCanvasGroup(_uiManager.mainMenuScreen));

                _ = PerformSearch();
            }
            else
            {
                img.sprite = openEditor;
                _levelSharerObj.SetActive(false);
                _backgroundObj.SetActive(false);
                _uiManager.UIGoToMainMenu();
            }
        });
    }
    
    private static IEnumerator FadeGameTitle()
    {
        var sprite = _uiManager.gameTitle;
        while (sprite.color.a > 0.0)
        {
            if (!_viewing) break;
            sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b,
                sprite.color.a - Time.unscaledDeltaTime * MENU_FADE_SPEED);
            yield return null;
        }

        sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, _viewing ? 0 : 1);

        if (_viewing)
        {
            _levelSharerObj.SetActive(true);
            _backgroundObj.SetActive(true);
        }

        yield return null;
    }

    private static void MakeTitleText(string text, int xPos)
    {
        var desc = UIUtils.MakeLabel(text, _levelSharerObj, new Vector2(xPos, -25),
            new Vector2(0.5f, 1), new Vector2(0.5f, 1)).textComponent;
        desc.text = text;
        desc.fontSize = 18;
        desc.alignment = TextAnchor.MiddleCenter;
    }

    private static InputField MakeTitleOption(string name, int xPos)
    {
        return UIUtils.MakeTextbox(name, _levelSharerObj, 
            new Vector2(xPos, -50), new Vector2(0.5f, 1), new Vector2(0.5f, 1), 
            600, 60, 30).Item1;
    }

    private static SortBy _currentSortBy = SortBy.MostDownloads;

    private static void SetupSearchArea()
    {
        MakeTitleText("Name/Description", -220);
        MakeTitleText("Creator", 0);
        MakeTitleText("Sort By", 190);

        _descriptionInput = MakeTitleOption("Description Input", -220);
        _creatorInput = MakeTitleOption("Creator Input", 0);
        var (sortBy, label) = UIUtils.MakeTextButton("Sort By Button", "Sort By", _levelSharerObj, 
            new Vector2(189, -50), new Vector2(0.5f, 1), new Vector2(0.5f, 1), 
            size:new Vector2(300, 60));
        label.textComponent.fontSize = 10;
        
        label.textComponent.text = GetSortByText(_currentSortBy);
        sortBy.onClick.AddListener(() =>
        {
            _currentSortBy = _currentSortBy == SortBy.Newest ? SortBy.MostDownloads : SortBy.Newest;
            label.textComponent.text = GetSortByText(_currentSortBy);
            _index = 0;
            RefreshCurrentLevels();
        });

        var (search, searchLabel) = UIUtils.MakeTextButton("Search Button", "Search", _levelSharerObj,
            new Vector2(0, -80), new Vector2(0.5f, 1), new Vector2(0.5f, 1),
            size: new Vector2(140, 60));
        searchLabel.textComponent.fontSize = 14;
        
        search.onClick.AddListener(() => Task.Run(PerformSearch));
    }

    private static Button MakeLoginButton(string text, Vector2 pos)
    {
        return UIUtils.MakeTextButton(text, text, _levelSharerObj, pos,
            Vector2.zero, Vector2.zero, size:new Vector2(85, 40)).Item1;
    }

    private static InputField MakeLoginBox(string name, Vector2 pos)
    {
        return UIUtils.MakeTextbox(name, _levelSharerObj, pos,
            Vector2.zero, Vector2.zero, 300, 40, 24).Item1;
    }

    private static void MakeLoginLabel(string name, Vector2 pos)
    {
        var txt = UIUtils.MakeLabel(name, _levelSharerObj, pos, Vector2.zero, Vector2.zero)
            .textComponent;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.text = name;
        txt.fontSize = 10;
    }

    private static void SetupLoginArea()
    {
        var error = UIUtils.MakeLabel("Login Error Message", _levelSharerObj, new Vector2(140, 15),
            Vector2.zero, Vector2.zero).textComponent;
        error.alignment = TextAnchor.MiddleCenter;
        error.fontSize = 8;

        MakeLoginLabel("Username", new Vector2(80, 65));
        MakeLoginLabel("Password", new Vector2(200, 65));

        var username = MakeLoginBox("Username Input", new Vector2(80, 50));
        var password = MakeLoginBox("Password Input", new Vector2(200, 50));

        InteractableWhenLoggedOut.Add(username);
        InteractableWhenLoggedOut.Add(password);

        password.contentType = InputField.ContentType.Password;

        var signUp = MakeLoginButton("Sign Up", new Vector2(100, 30));
        signUp.onClick.AddListener(() =>
            _ = SharerRequests.SendAuthRequest(username.text, password.text, "/create", error));
        InteractableWhenLoggedOut.Add(signUp);

        var logIn = MakeLoginButton("Log In", new Vector2(140, 30));
        logIn.onClick.AddListener(() =>
            _ = SharerRequests.SendAuthRequest(username.text, password.text, "/login", error));
        InteractableWhenLoggedOut.Add(logIn);

        var logOut = MakeLoginButton("Log Out", new Vector2(180, 30));
        logOut.onClick.AddListener(() =>
        {
            APIKey = null;
            RefreshActiveOptions();
        });
        InteractableWhenLoggedIn.Add(logOut);
    }

    internal static void RefreshActiveOptions()
    {
        var loggedOut = APIKey == null;
        foreach (var selectable in InteractableWhenLoggedOut) selectable.interactable = loggedOut && !CurrentlyDownloading;
        foreach (var selectable in InteractableWhenLoggedIn) selectable.interactable = !loggedOut && !CurrentlyDownloading;
        foreach (var selectable in UninteractableWhenDownloading) selectable.interactable = !CurrentlyDownloading;
    }

    internal static async Task PerformSearch()
    {
        var jsonResponse = await SharerRequests.SendSearchRequest(_descriptionInput.text, _creatorInput.text);
        _currentLevels = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(jsonResponse);
        _currentLevels.Reverse();
        _index = 0;
        RefreshCurrentLevels();
    }

    private static void SetupLevelsArea()
    {
        var status = UIUtils.MakeLabel("Download Status", _levelSharerObj, new Vector2(0, 40),
            new Vector2(0.5f, 0), new Vector2(0.5f, 0)).textComponent;
        status.alignment = TextAnchor.UpperCenter;
        status.text = "Warning!\n\n" +
                      "Downloading a level will overwrite any changes you have made yourself\n" +
                      "Downloading a save will overwrite save slot 4";

        var y = 135;
        for (var i = 0; i < LEVELS_PER_PAGE; i++)
        {
            var img = UIUtils.MakeImage($"Download Image {i}", _levelSharerObj, new Vector2(-300, y+25), 
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(200, 100));
            img.sprite = ArchitectPlugin.BlankSprite;
            img.preserveAspect = true;

            var count = UIUtils.MakeLabel($"Download Count {i}", _levelSharerObj, new Vector2(-210, y),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f)).textComponent;
            count.alignment = TextAnchor.UpperCenter;
            count.fontSize = 10;
            
            var name = UIUtils.MakeLabel($"Level Name {i}", _levelSharerObj, new Vector2(-130, y+20),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f)).textComponent;
            
            var desc = UIUtils.MakeLabel($"Level Desc {i}", _levelSharerObj, new Vector2(60, y+20),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), 440, 40,
                HorizontalWrapMode.Wrap).textComponent;
            desc.fontSize = 10;

            var (btn, label) = UIUtils.MakeTextButton($"Level Download {i}", "Download Level", 
                _levelSharerObj, new Vector2(320, y+55), 
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                size:new Vector2(160, 40));
            btn.gameObject.SetActive(false);
            label.gameObject.SetActive(false);
            UninteractableWhenDownloading.Add(btn);

            var (saveBtn, saveLabel) = UIUtils.MakeTextButton($"Save Download {i}", "Download Save", 
                _levelSharerObj, new Vector2(320, y+35), 
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                size:new Vector2(160, 40));
            saveBtn.gameObject.SetActive(false);
            saveLabel.gameObject.SetActive(false);
            UninteractableWhenDownloading.Add(saveBtn);
            
            DownloadChoices.Add((count, name, desc, btn.gameObject, label.gameObject,
                saveBtn.gameObject, saveLabel.gameObject, img));
            
            var k = i;
            
            btn.onClick.AddListener(() => _ = SharerRequests
                .DownloadLevel(_orderedCurrentLevels[_index * LEVELS_PER_PAGE + k]["level_id"], status));
            
            saveBtn.onClick.AddListener(() => _ = SharerRequests
                .DownloadSave(_orderedCurrentLevels[_index * LEVELS_PER_PAGE + k]["level_id"], status));
            
            y -= 70;
        }

        var (leftBtn, leftLab) = UIUtils.MakeTextButton("Arrow Left", "<=", _levelSharerObj, new Vector2(-370, 0),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), size:new Vector2(55, 55));
        leftLab.textComponent.fontSize = 10;
        var (rightBtn, rightLab) = UIUtils.MakeTextButton("Arrow Right", "=>", _levelSharerObj, new Vector2(370, 0),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), size:new Vector2(55, 55));
        rightLab.textComponent.fontSize = 10;
        
        leftBtn.onClick.AddListener(() =>
        {
            _index--;
            if (_index < 0) _index = _currentLevels.Count / LEVELS_PER_PAGE;
            RefreshCurrentLevels();
        });
        rightBtn.onClick.AddListener(() =>
        {
            _index = (_index + 1) % ((_currentLevels.Count-1) / LEVELS_PER_PAGE + 1);
            RefreshCurrentLevels();
        });
        
        UninteractableWhenDownloading.Add(leftBtn);
        UninteractableWhenDownloading.Add(rightBtn);
    }
    
    private static void RefreshCurrentLevels()
    {
        _orderedCurrentLevels = _currentSortBy == SortBy.MostDownloads ? 
            _currentLevels.OrderByDescending(c => Convert.ToInt32(c["downloads"])).ToList() 
            : _currentLevels;

        for (var i = 0; i < LEVELS_PER_PAGE; i++)
        {
            DownloadChoices[i].Item8.sprite = ArchitectPlugin.BlankSprite;

            var index = _index * LEVELS_PER_PAGE + i;
            if (_orderedCurrentLevels.Count > index)
            {
                var name = _orderedCurrentLevels[index]["level_name"] + " – " + _orderedCurrentLevels[index]["username"];
                DownloadChoices[i].Item1.text = "Downloads:\n" + _orderedCurrentLevels[index]["downloads"];
                DownloadChoices[i].Item2.text = name + new string(' ', Mathf.Max(0, 50 - name.Length));
                DownloadChoices[i].Item3.text = _orderedCurrentLevels[index]["level_desc"];
                DownloadChoices[i].Item4.SetActive(true);
                DownloadChoices[i].Item5.SetActive(true);

                var hasSave = _orderedCurrentLevels[index]["has_save"] == "true";
                DownloadChoices[i].Item6.SetActive(hasSave);
                DownloadChoices[i].Item7.SetActive(hasSave);
                
                ArchitectPlugin.Instance.StartCoroutine(GetSprite(DownloadChoices[i].Item8, _index,
                    _orderedCurrentLevels[index]["url"]));
            }
            else
            {
                DownloadChoices[i].Item1.text = "";
                DownloadChoices[i].Item2.text = "";
                DownloadChoices[i].Item3.text = "";
                DownloadChoices[i].Item4.SetActive(false);
                DownloadChoices[i].Item5.SetActive(false);
                DownloadChoices[i].Item6.SetActive(false);
                DownloadChoices[i].Item7.SetActive(false);
            }
        }
    }

    private static IEnumerator GetSprite(Image image, int pageIndex, string url)
    {
        var www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();
        if (_index != pageIndex) yield break;

        if (www.result == UnityWebRequest.Result.Success)
        {
            var tex = DownloadHandlerTexture.GetContent(www);
            image.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), default);
        }
    }

    private static Button MakeUploadButton(string text, Vector2 pos)
    {
        return UIUtils.MakeTextButton(text, text, _levelSharerObj, pos,
            new Vector2(1, 0), new Vector2(1, 0), size:new Vector2(85, 40)).Item1;
    }

    private static InputField MakeUploadBox(string name, Vector2 pos)
    {
        return UIUtils.MakeTextbox(name, _levelSharerObj, pos,
            new Vector2(1, 0), new Vector2(1, 0), 300, 40, 24).Item1;
    }

    private static void MakeUploadLabel(string name, Vector2 pos)
    {
        var txt = UIUtils.MakeLabel(name, _levelSharerObj, pos, 
                new Vector2(1, 0), new Vector2(1, 0))
            .textComponent;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.text = name;
        txt.fontSize = 10;
    }

    private static void SetupUploadArea()
    {
        var error = UIUtils.MakeLabel("Upload Error Message", _levelSharerObj, new Vector2(-110, 15),
            new Vector2(1, 0), new Vector2(1, 0)).textComponent;
        error.alignment = TextAnchor.MiddleCenter;
        error.fontSize = 8;

        var upload = MakeUploadButton("Upload", new Vector2(-30, 60));
        var delete = MakeUploadButton("Delete", new Vector2(-30, 40));

        var nameBox = MakeUploadBox("Level Name", new Vector2(-110, 90));
        var descBox = MakeUploadBox("Level Description", new Vector2(-110, 70));
        var iconBox = MakeUploadBox("Level Icon", new Vector2(-110, 50));
        var saveBox = MakeUploadBox("Level Save", new Vector2(-110, 30));
        saveBox.characterValidation = InputField.CharacterValidation.Integer;

        MakeUploadLabel("Level Name", new Vector2(-220, 90));
        MakeUploadLabel("Level Description", new Vector2(-220, 70));
        MakeUploadLabel("Icon URL (Optional)", new Vector2(-220, 50));
        MakeUploadLabel("Save File Index (Optional)", new Vector2(-220, 30));

        InteractableWhenLoggedIn.Add(upload);
        InteractableWhenLoggedIn.Add(delete);

        InteractableWhenLoggedIn.Add(nameBox);
        InteractableWhenLoggedIn.Add(descBox);
        InteractableWhenLoggedIn.Add(iconBox);
        InteractableWhenLoggedIn.Add(saveBox);

        upload.onClick.AddListener(() =>
        {
            int.TryParse(saveBox.text, out var saveNumber);
            _ = SharerRequests.UploadLevel(nameBox.text, descBox.text, iconBox.text, saveNumber, error);
        });
        delete.onClick.AddListener(() => _ = SharerRequests.DeleteLevel(nameBox.text, error));
    }

    private enum SortBy
    {
        MostDownloads,
        Newest
    }

    private static string GetSortByText(SortBy sort)
    {
        return sort switch
        {
            SortBy.MostDownloads => "Most Downloaded",
            SortBy.Newest => "Newest First",
            _ => ""
        };
    }
}