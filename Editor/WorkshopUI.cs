using System;
using System.Collections.Generic;
using System.Linq;
using Architect.Sharer;
using Architect.Storage;
using Architect.Utils;
using Architect.Workshop;
using Architect.Workshop.Items;
using BepInEx;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Architect.Editor;

public static class WorkshopUI
{
    private static GameObject _configArea;
    private static GameObject _openArea;
    
    private static Text _title;
    private static Image _icon;
    private static InputField _idField;

    private static ScrollRect _scrollRect;
    private static InputField _search;

    private static readonly List<Listing> Listings = [];
    
    public static void Init(GameObject workshopUI)
    {
        WorkshopManager.Init();
        
        var bg = new GameObject("Background");
        bg.transform.SetParent(workshopUI.transform, false);
        bg.RemoveOffset();
        
        var bgImg = UIUtils.MakeImage(
            "Image",
            bg,
            new Vector2(0, 2510),
            new Vector2(0.5f, 0),
            new Vector2(0.5f, 0),
            new Vector2(10000, 10000));
        bgImg.color = new Color(0, 0, 0.15f);
        bgImg.sprite = UIUtils.Square;
        
        var label = UIUtils.MakeLabel(
            "Architect Text",
            bg,
            Vector2.zero,
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f));
        label.font = "TrajanPro-Bold";
        var bgTxt = label.textComponent;
        bgTxt.text = "architect";
        bgTxt.alignment = TextAnchor.MiddleCenter;
        bgTxt.fontSize = 50;
        label.transform.SetAsLastSibling();

        bgTxt.color = new Color(0.1f, 0.1f, 0.4f);

        _openArea = new GameObject("Open Area");
        _openArea.transform.SetParent(workshopUI.transform, false);
        _openArea.RemoveOffset();

        _configArea = new GameObject("Config Area");
        _configArea.transform.SetParent(workshopUI.transform, false);
        _configArea.RemoveOffset();
        _configArea.SetActive(false);

        _title = UIUtils.MakeLabel(
            "Title",
            _configArea,
            new Vector2(0, -75),
            new Vector2(0.5f, 1),
            new Vector2(0.5f, 1)).textComponent;

        var issue = UIUtils.MakeLabel(
            "Issue",
            _configArea,
            new Vector2(0, 75),
            new Vector2(0.5f, 0),
            new Vector2(0.5f, 0)).textComponent;
        issue.alignment = TextAnchor.MiddleCenter;
        
        var (delBtn, delLabel) = UIUtils.MakeTextButton(
            "Delete", "Delete", _configArea, new Vector2(-100, 35), 
            new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            size: new Vector2(260, 80));
        delLabel.textComponent.fontSize = 18;
        delBtn.onClick.AddListener(() =>
        {
            Delete();
            _configArea.SetActive(false);
            _openArea.SetActive(true);
            Refresh();
        });
        
        var (closeBtn, closeLabel) = UIUtils.MakeTextButton(
            "Close", "Close", _configArea, new Vector2(0, 35), 
            new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            size: new Vector2(260, 80));
        closeLabel.textComponent.fontSize = 18;
        closeBtn.onClick.AddListener(() =>
        {
            if (!_saved) Delete();
            _configArea.SetActive(false);
            _openArea.SetActive(true);
            Refresh();
        });
        
        var (savBtn, savLabel) = UIUtils.MakeTextButton(
            "Save", "Save", _configArea, new Vector2(100, 35), 
            new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            size: new Vector2(260, 80));
        savLabel.textComponent.fontSize = 18;
        
        _icon = UIUtils.MakeImage(
            "Preview",
            _configArea,
            new Vector2(-180, 40),
            new Vector2(1, 0.5f),
            new Vector2(1, 0.5f),
            new Vector2(200, 200));
        _icon.preserveAspect = true;
        (_idField, var idLabel) = UIUtils.MakeTextbox(
            "Id",
            _configArea,
            new Vector2(-180, -30),
        new Vector2(1, 0.5f),
        new Vector2(1, 0.5f),
            280,
            50,
            12
        );
        idLabel.transform.localScale = Vector3.one;
        ((RectTransform)idLabel.transform).sizeDelta /= 3;
        
        savBtn.onClick.AddListener(() =>
        {
            if (_idField.text.IsNullOrWhiteSpace() || _idField.text.StartsWith("Prefab"))
            {
                issue.text = "Invalid ID";
                return;
            }
            var clashItem = WorkshopManager.WorkshopData.Items.FirstOrDefault(i => i.Id == _idField.text);
            var item = WorkshopManager.WorkshopData.Items.FirstOrDefault(i => i.Id == _currentId);
            if (clashItem != null && clashItem != item)
            {
                issue.text = "Object with this ID exists";
                return;
            }
            if (item != null)
            {
                _currentId = _idField.text;
                item.Id = _idField.text;
                item.Unregister();
                item.Register();
                _saved = true;
                StorageManager.SaveWorkshopData();
                issue.text = "Saved Object";
            }
        });
        
        bg.transform.SetAsFirstSibling();

        (_scrollRect, var scroll, var bar) = UIUtils.MakeScrollView("Scroll",
            _openArea, 
            new Vector2(175, -40),
            new Vector2(200, 0),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(335, 350), 1520);
        bar.transform.localScale = Vector3.one;
        ((RectTransform)bar.transform).sizeDelta = new Vector3(20, 1);
        var handle = bar.transform.GetChild(1);
        handle.localScale = Vector3.one;
        ((RectTransform)handle).sizeDelta = Vector2.one;
        
        var vlg = scroll.GetComponent<VerticalLayoutGroup>();
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = false;
        vlg.childControlHeight = false;
        vlg.childControlWidth = false;
        vlg.spacing = 120;

        _scrollRect.verticalNormalizedPosition = 1;
        
        _scrollRect.gameObject.AddComponent<ScaleWithScreenSize>().padding = 150;
        bar.gameObject.AddComponent<ScaleWithScreenSize>().padding = 180;

        for (var i = 0; i < 15; i++)
        {
            var listing = new GameObject("Listing");
            listing.transform.SetParent(scroll.transform, false);
            listing.RemoveOffset();
            
            var icon = UIUtils.MakeImage("Icon", listing,
                new Vector2(70, -70),
                Vector2.zero, Vector2.zero,
                new Vector2(200, 200));
            icon.preserveAspect = true;
            
            var type = UIUtils.MakeLabel("Type Label", listing,
                new Vector2(180, -65),
                Vector2.zero, Vector2.zero).textComponent;
            type.fontSize = 18;
            
            var id = UIUtils.MakeLabel("ID Label", listing,
                new Vector2(180, -100),
                Vector2.zero, Vector2.zero).textComponent;
            id.fontSize = 16;

            var (btn, lbl) = UIUtils.MakeTextButton("Edit", "Edit", listing,
                new Vector2(152.5f, -95),
                Vector2.zero, Vector2.zero);
            lbl.textComponent.fontSize = 16;
            
            var listingComponent = listing.AddComponent<Listing>();
            listingComponent.obj = listing;
            listingComponent.icon = icon;
            listingComponent.type = type;
            listingComponent.id = id;
            Listings.Add(listingComponent);
            
            btn.onClick.AddListener(() =>
            {
                if (listingComponent.CurrentItem == null) return;
                Open(listingComponent.CurrentItem, false);
            });
        }

        (_search, var searchLabel) = UIUtils.MakeTextbox("Search", _openArea,
            new Vector2(175, -90),
            new Vector2(0.5f, 1), new Vector2(0.5f, 1),
            750, 65);
        searchLabel.textComponent.fontSize = 15;
        searchLabel.transform.localScale = Vector3.one;
        ((RectTransform)searchLabel.transform).sizeDelta /= 3;
        
        _search.onValueChanged.AddListener(_ =>
        {
            Search();
        });
        
        foreach (var (type, (pos, func)) in WorkshopManager.WorkshopItems)
        {
            var (addBtn, addLabel) = UIUtils.MakeTextButton(
                "Create", $"Create {type}", _openArea, pos, 
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                size: new Vector2(420, 80));
            addLabel.textComponent.fontSize = 18;
            
            addBtn.onClick.AddListener(() =>
            {
                issue.text = "";
                Open(func(Guid.NewGuid().ToString()[..8]), true);
            });
        }

        return;

        void Delete()
        {
            var item = WorkshopManager.WorkshopData.Items.FirstOrDefault(i => i.Id == _currentId);
            if (item != null) {
                item.Unregister();
                WorkshopManager.WorkshopData.Items.Remove(item);
            }
        }
    }

    public static void Refresh()
    {
        _scrollRect.verticalNormalizedPosition = 1;
        _search.text = "";
        Search();
    }

    private static void Search()
    {
        var i = 0;
        foreach (var item in WorkshopManager.WorkshopData.Items
                     .Where(o => o.Id.ToLower().Contains(_search.text.ToLower())))
        {
            Listings[i].Setup(item);
            i++;
            if (i >= 15) break;
        }

        for (; i < 15; i++)
        {
            Listings[i].Clear();
        }
    }

    private class Listing : MonoBehaviour
    {
        public GameObject obj;
        public Image icon;
        public Text type;
        public Text id;
        public WorkshopItem CurrentItem;
        
        public void Clear()
        {
            obj.SetActive(false);
        }

        public void Setup(WorkshopItem item)
        {
            CurrentItem = item;
            icon.sprite = item.GetIcon() ?? SharerManager.Placeholder;
            type.text = item.Type;
            id.text = item.Id;
            obj.SetActive(true);
        }
    }

    private class ScaleWithScreenSize : MonoBehaviour
    {
        public Canvas canvas;
        public RectTransform rt;
        public float padding;

        private void Awake()
        {
            canvas = GetComponentInParent<Canvas>();
            rt = GetComponent<RectTransform>();
        }
        
        private void LateUpdate()
        {
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, canvas.pixelRect.height / canvas.scaleFactor - padding);
        }
    }

    private static GameObject _configParent;
    private static string _currentId;
    
    public static void RefreshIcon(WorkshopItem item)
    {
        if (item.Id == _currentId)
        {
            _icon.sprite = item.GetIcon() ?? SharerManager.Placeholder;
        }
    }

    private static bool _saved;

    public static void Open(WorkshopItem item, bool isNew)
    {
        _saved = !isNew;
        if (isNew)
        {
            WorkshopManager.WorkshopData.Items.Add(item);
            item.Register();
        }
        
        _title.text = $"Creating {item.Type}";
        _currentId = item.Id;
        _idField.text = _currentId;
        RefreshIcon(item);
        
        _configArea.SetActive(true);
        _openArea.SetActive(false);
        if (_configParent) Object.Destroy(_configParent);

        _configParent = new GameObject("Config Parent");
        _configParent.transform.SetParent(_configArea.transform, false);
        _configParent.RemoveOffset();

        var y = (2 * item.Config.Length + item.Config.SelectMany(o => o).Count() - 3) * 6;
        foreach (var configGroup in item.Config)
        {
            foreach (var configType in configGroup)
            {
                var txt = UIUtils.MakeLabel(
                    "Config Title",
                    _configParent,
                    new Vector3(100, y),
                    new Vector2(0, 0.5f),
                    new Vector2(0, 0.5f)).textComponent;
                txt.text = configType.Name;
                txt.fontSize = 8;
                txt.alignment = TextAnchor.MiddleLeft;
                
                var (btn, _) = UIUtils.MakeTextButton(
                    "Config Apply",
                    "Apply",
                    _configParent, 
                    new Vector2(220, y),
                    new Vector2(0, 0.5f),
                    new Vector2(0, 0.5f));
                btn.interactable = false;
                
                var inp = configType.CreateInput(_configParent, btn, new Vector3(155, y), 
                    (isNew ?
                        configType.GetDefaultValue() :
                        item.CurrentConfig.GetValueOrDefault(configType.Id))?.SerializeValue());
                y -= 12;
                
                btn.onClick.AddListener(Apply);
                if (isNew) Apply();

                continue;
                
                void Apply()
                {
                    btn.interactable = false;
                    var val = inp.GetValue();
                    if (val.Length == 0)
                        item.CurrentConfig.Remove(configType.Id);
                    else
                    {
                        var cfg = configType.Deserialize(inp.GetValue());
                        cfg.Setup(item);
                        item.CurrentConfig[configType.Id] = cfg;
                    }
                }
            }

            y -= 24;
        }
    }
}