using System.Collections;
using System.Collections.Generic;
using Architect.Sharer.Info;
using Architect.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Architect.Sharer.States;

public class Manage : MenuState
{
    public static Manage Instance;
    private readonly List<LevelDisplay> _displays = [];
    
    public override MenuState ReturnState => SharerManager.HomeState;

    public int page;

    private Button _leftBtn;
    private Button _rightBtn;
    
    public override void OnStart()
    {
        Instance = this;
        var binSprite = ResourceUtils.LoadSpriteResource("Sharer.bin");
        var editSprite = ResourceUtils.LoadSpriteResource("Sharer.pen");
        
        for (var y = 1; y >= -1; y--)
        {
            for (var x = -2; x <= 2; x++)
            {
                var listing = new GameObject($"Listing ({x},{y})");
                listing.transform.SetParent(transform, false);
                listing.RemoveOffset();
                
                var icon = UIUtils.MakeImage("Icon", listing,
                    new Vector2(x * 125, y * 160),
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(200, 200));
                icon.preserveAspect = true;
                icon.sprite = ArchitectPlugin.BlankSprite;

                var levelName = UIUtils.MakeLabel("Name", listing,
                    new Vector2(x * 125, y * 160 - 65),
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    maxWidth: 100).textComponent;
                levelName.alignment = TextAnchor.MiddleCenter;
                levelName.truncate = true;
                levelName.fontSize = 15;

                var controls = new GameObject("Controls");
                controls.transform.SetParent(listing.transform, false);
                controls.RemoveOffset();
                
                var (bin, binImg, _) = UIUtils.MakeButtonWithImage("Delete", controls,
                    new Vector2(x * 125 - 35, y * 160 - 35),
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    100,
                    100,
                    false);
                binImg.sprite = binSprite;
                
                var (edit, editImg, _) = UIUtils.MakeButtonWithImage("Edit", controls,
                    new Vector2(x * 125 + 35, y * 160 - 35),
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    100,
                    100,
                    false);
                editImg.sprite = editSprite;
                
                var display = new LevelDisplay(listing, icon, levelName);
                
                bin.onClick.AddListener(() => OpenDeleteUI(display.LevelInfo.LevelName));
                edit.onClick.AddListener(() =>
                {
                    LevelConfig.CurrentInfo = display.LevelInfo;
                    SharerManager.TransitionToState(Home.LevelConfig);
                });
                
                _displays.Add(display);
                
                listing.AddComponent<ShowButtons>().buttonParent = controls;
                controls.SetActive(false);
                listing.SetActive(false);
                
                icon.sprite = SharerManager.Placeholder;
                levelName.text = "Lorem Ipsum Dolor Sit";
            }
        }
        
        (_leftBtn, var leftLab) = UIUtils.MakeTextButton("Arrow Left", "<=", gameObject, 
            new Vector2(-375, 0),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), size:new Vector2(100, 100));
        leftLab.textComponent.fontSize = 15;
        leftLab.textComponent.alignment = TextAnchor.MiddleCenter;
        
        _leftBtn.onClick.AddListener(() =>
        {
            page--;
            StartCoroutine(RefreshPage());
        });
        
        (_rightBtn, var rightLab) = UIUtils.MakeTextButton("Arrow Right", "=>", gameObject,
            new Vector2(375, 0),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), size:new Vector2(100, 100));
        rightLab.textComponent.fontSize = 15;
        rightLab.textComponent.alignment = TextAnchor.MiddleCenter;
        
        _rightBtn.onClick.AddListener(() =>
        {
            page++;
            StartCoroutine(RefreshPage());
        });
        
        SetupDeleteUI();
    }

    private GameObject _deleteUI;
    private Text _changeTitle;
    private InputField _nameField;
    private Transform _bg;
    private Button _confirm;
    private string _currentDeleteName;
    
    private void SetupDeleteUI()
    {
        _deleteUI = new GameObject("Delete UI");
        _deleteUI.transform.SetParent(transform, false);
        _deleteUI.RemoveOffset();

        var bg = UIUtils.MakeImage("Bg", _deleteUI, Vector2.zero,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(10000, 10000));
        bg.sprite = ResourceUtils.LoadSpriteResource("Sharer.config_bg");
        _bg = bg.transform;

        var uiChild = new GameObject("Content");
        uiChild.transform.SetParent(_deleteUI.transform, false);
        uiChild.RemoveOffset();
        
        (_nameField, var cfTxt) = UIUtils.MakeTextbox("Name Input", uiChild, Vector2.zero,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), 1000, 80);
        cfTxt.transform.localScale = Vector3.one;
        cfTxt.textComponent.fontSize = 16;
        ((RectTransform)cfTxt.transform).sizeDelta /= 3;
        
        _changeTitle = UIUtils.MakeLabel("Title", uiChild, new Vector2(0, 140),
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
            _deleteUI.SetActive(false);
        });

        (_confirm, var confirmLabel) = UIUtils.MakeTextButton("Confirm", "Delete", uiChild, 
            new Vector2(70, -140),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            size: new Vector2(360, 80));
        confirmLabel.textComponent.fontSize = 18;
        _confirm.onClick.AddListener(() =>
        {
            cancel.interactable = false;
            _confirm.interactable = false;
            StartCoroutine(RequestManager.SendDeleteRequest(
                _currentDeleteName,
                b => StartCoroutine(OnComplete(b))));
        });
        
        _nameField.onValueChanged.AddListener(s =>
        {
            _confirm.interactable = s == _currentDeleteName;
        });
        
        _deleteUI.SetActive(false);

        return;

        IEnumerator OnComplete(bool b)
        {
            if (b) yield return RefreshPage();
            cancel.interactable = true;
            _confirm.interactable = true;
            _deleteUI.SetActive(false);
        }
    }

    private void OpenDeleteUI(string levelName)
    {
        _bg.SetAsFirstSibling();
        
        _currentDeleteName = levelName;

        _changeTitle.text = $"Enter '{levelName}' to confirm:";
        
        _nameField.text = "";
        _confirm.interactable = false;
        _deleteUI.SetActive(true);
    }

    public override void OnOpen()
    {
        page = 0;
        StartCoroutine(RefreshPage());
    }

    private IEnumerator RefreshPage()
    {
        _leftBtn.interactable = false;
        _rightBtn.interactable = false;
        
        yield return RequestManager.SearchLevels(new RequestManager.FilterInfo
        {
            KeyFilter = RequestManager.SharerKey,
            KeyMode = true
        }, 15, page, (success, levels, pages) =>
        {
            if (!success)
            {
                SharerManager.TransitionToState(SharerManager.HomeState);
                return;
            }

            _leftBtn.interactable = page > 0;
            _rightBtn.interactable = page < pages;
            
            var i = 0;
            foreach (var info in levels)
            {
                _displays[i].Apply(info);
                i++;
            }
            for (; i < _displays.Count; i++)
            {
                _displays[i].Apply(null);
            }
        });
    }
    
    public class LevelDisplay(GameObject parent, Image image, Text text)
    {
        public LevelInfo LevelInfo;
        
        public void Apply(LevelInfo info)
        {
            LevelInfo = info;
            
            if (info == null)
            {
                parent.SetActive(false);
                return;
            }
            
            parent.SetActive(true);
            text.text = info.LevelName;
            
            SharerManager.DoGetSprite(info.IconURL, image);
        }
    }

    public class ShowButtons : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public GameObject buttonParent;

        private void OnDisable()
        {
            buttonParent.SetActive(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            buttonParent.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            buttonParent.SetActive(false);
        }
    }
}