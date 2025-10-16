using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Architect.Config.Types;
using Architect.Events;
using Architect.Objects;
using Architect.Objects.Categories;
using Architect.Objects.Placeable;
using Architect.Objects.Tools;
using Architect.Storage;
using Architect.Utils;
using MonoMod.RuntimeDetour;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Architect.Editor;

public static class EditorUI
{
    // Constants
    private const int ITEMS_PER_PAGE = 9;
    private const string FILLED_STAR = "★";
    private const string EMPTY_STAR = "☆";
    private const string BIN = "X";
    private const string NOTHING = " ";

    public static Text ResetRocketTime;

    private static readonly List<(Image, UIUtils.Label, UIUtils.Label)> GridIcons = [];
    private static readonly List<Image> HotbarIcons = [];

    private static readonly List<GameObject> DisableWhenPlaying = [];
    private static readonly List<GameObject> EnableWhenPlaying = [];
    private static GameObject _canvasObj;

    public static AbstractCategory CurrentCategory = Categories.All;
    private static int _pageIndex;
    private static List<SelectableObject> _categoryContents;

    private static UIUtils.Label _currentlySelected;
    private static UIUtils.Label _currentlySelectedDesc;

    public static UIUtils.Label ObjectIdLabel;

    public static InputField RotationText;
    public static InputField ScaleText;

    private static string _currentSearch = "";

    private static Button _configButton;
    private static Button _broadcastersButton;
    private static Button _receiversButton;
    
    private static AttributeType _currentOption = AttributeType.Config;

    public static void Init()
    {
        SetupCanvas();
        SetupLabels();
        SetupObjects();
        SetupCategories();
        SetupSearchBox();
        SetupPreciseSettings();
        SetupAttributeSettings();
        SetupHotbar();

        RefreshItem();
    }

    private static void SetupCanvas()
    {
        _canvasObj = new GameObject("[Architect] Editor Canvas");
        _canvasObj.SetActive(false);
        Object.DontDestroyOnLoad(_canvasObj);

        _canvasObj.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        _canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        _canvasObj.AddComponent<GraphicRaycaster>();
    }

    private static void SetupLabels()
    {
        var anchor = new Vector2(0.5f, 1);
        _currentlySelected = UIUtils.MakeLabel("Current Object Name", _canvasObj,
            new Vector3(0, -50, 0), anchor, anchor);
        _currentlySelectedDesc = UIUtils.MakeLabel("Current Object Description", _canvasObj,
            new Vector3(0, -70, 0), anchor, anchor);

        _currentlySelected.textComponent.alignment = TextAnchor.UpperCenter;
        _currentlySelectedDesc.textComponent.alignment = TextAnchor.UpperCenter;

        _currentlySelectedDesc.textComponent.fontSize = 10;
        DisableWhenPlaying.Add(_currentlySelectedDesc.gameObject);

        var currentScene = UIUtils.MakeLabel("Current Scene", _canvasObj,
            new Vector3(50, 45, 0), Vector2.zero, Vector2.zero);
        currentScene.textComponent.alignment = TextAnchor.LowerLeft;

        _ = new Hook(typeof(HeroController).GetMethod(nameof(HeroController.SceneInit)),
            (Action<HeroController> orig, HeroController self) =>
            {
                orig(self);
                currentScene.textComponent.text = "Scene: " + GameManager.instance.sceneName;
            });
        EnableWhenPlaying.Add(currentScene.gameObject);

        var bottomAnchor = new Vector2(0.5f, 0);
        ObjectIdLabel = UIUtils.MakeLabel("Object ID Description", _canvasObj,
            new Vector3(0, 45, 0), bottomAnchor, bottomAnchor);
        ObjectIdLabel.textComponent.fontSize = 10;
        ObjectIdLabel.textComponent.alignment = TextAnchor.LowerCenter;
    }

    private static void SetupCategories()
    {
        var anchor = new Vector2(1, 0);
        var position = new Vector2(-25, 135);
        foreach (var category in Categories.AllCategories)
        {
            position.y += 12;
            if (category.GetName() == null) continue;
            var (btn, label) = UIUtils.MakeTextButton(
                category.GetName(),
                category.GetName(),
                _canvasObj,
                position,
                anchor,
                anchor
            );
            btn.onClick.AddListener(() =>
            {
                _pageIndex = 0;
                CurrentCategory = category;
                RefreshCurrentPage();
            });
            DisableWhenPlaying.Add(label.gameObject);
            DisableWhenPlaying.Add(btn.gameObject);
        }
    }

    private static void SetupObjects()
    {
        var anchor = new Vector2(1, 0);
        for (var i = 2; i >= 0; i--)
        {
            for (var j = 2; j >= 0; j--)
            {
                var index = 8 - j - i * 3;

                var (btn, img, label) = UIUtils.MakeButtonWithImage("Option " + index, _canvasObj,
                    new Vector3(-25 - j * 40, 25 + i * 40), anchor, anchor, 96, 60);

                var (fav, favLabel) = UIUtils.MakeTextButton("Favourite " + index, NOTHING, _canvasObj,
                    new Vector3(-45 - j * 40, 25 + i * 40), anchor, anchor, false);

                btn.onClick.AddListener(() => SetItem(index));
                fav.onClick.AddListener(() => ToggleFavourite(index, favLabel));

                GridIcons.Add((img, label, favLabel));

                DisableWhenPlaying.Add(btn.gameObject);
                DisableWhenPlaying.Add(fav.gameObject);
                DisableWhenPlaying.Add(favLabel.gameObject);
            }
        }

        MakeToolButton(CursorObject.Instance, 230, 40);
        MakeToolButton(EraserObject.Instance, 190, 40);
        MakeToolButton(PickObject.Instance, 250, 0);
        
        MakeToolButton(DragObject.Instance, 150, 40);
        MakeToolButton(LockObject.Instance, 210, 0);
        MakeToolButton(TileChangerObject.Instance, 170, 0);
        MakeToolButton(ResetObject.Instance, 130, 0);

        var middle = new Vector2(0.5f, 0.5f);
        ResetRocketTime = UIUtils.MakeLabel(
            "Reset Time",
            _canvasObj,
            Vector3.zero,
            middle,
            middle).textComponent;
        ResetRocketTime.enabled = false;
        ResetRocketTime.alignment = TextAnchor.MiddleCenter;
        ResetRocketTime.fontSize = 60;
    }

    private static void MakeToolButton(ToolObject obj, int xShift, int yShift)
    {
        var anchor = new Vector2(1, 0);
        var (toolBtn, toolImg, _) = UIUtils.MakeButtonWithImage(obj.GetName(), _canvasObj,
            new Vector3(-25 - xShift, 25 + yShift), anchor, anchor, 96, 48);
        toolBtn.onClick.AddListener(() => SetItem(obj.Index));
        toolImg.sprite = obj.GetUISprite();

        DisableWhenPlaying.Add(toolBtn.gameObject);
    }

    public static void RefreshVisibility(bool editing, bool paused)
    {
        _canvasObj.SetActive(editing);
        if (editing)
        {
            RefreshCurrentTab(paused);
            
            foreach (var obj in DisableWhenPlaying) obj.SetActive(paused);
            foreach (var obj in EnableWhenPlaying) obj.SetActive(!paused);
        }
    }

    public static void WipeTabs()
    {
        DisableWhenPlaying.Remove(_configTab);
        DisableWhenPlaying.Remove(_receiverTab);
        DisableWhenPlaying.Remove(_broadcasterTab);
        Object.Destroy(_configTab);
        Object.Destroy(_receiverTab);
        Object.Destroy(_broadcasterTab);
    }

    public static void SetItem(int i)
    {
        var index = _pageIndex * ITEMS_PER_PAGE + i;
        if (_categoryContents.Count <= index) i = -99;

        EditManager.TryFindEmptySlot();
        
        EditManager.ClearAttributes();

        EditManager.CurrentlyFlipped = false;
        EditManager.SetRotation(0);
        EditManager.SetScale(1);
        
        var isPrefab = false;
        
        if (i < 0)
        {
            EditManager.CurrentObject = i switch
            {
                -1 => CursorObject.Instance,
                -2 => EraserObject.Instance,
                -3 => PickObject.Instance,
                -4 => DragObject.Instance,
                -5 => ResetObject.Instance,
                -6 => TileChangerObject.Instance,
                -7 => LockObject.Instance,
                _ => BlankObject.Instance
            };
        }
        else
        {
            var obj = _categoryContents[index];
            if (obj is PrefabObject prefab)
            {
                obj = prefab.PlaceableObject;

                EditManager.Broadcasters.AddRange(prefab.Placement.Broadcasters);
                EditManager.Receivers.AddRange(prefab.Placement.Receivers);
                foreach (var conf in prefab.Placement.Config)
                {
                    EditManager.Config[conf.GetTypeId()] = conf;
                }

                EditManager.SetScale(prefab.Placement.GetScale());
                EditManager.SetRotation(prefab.Placement.GetRotation());
                EditManager.CurrentlyFlipped = prefab.Placement.IsFlipped();

                isPrefab = true;
            }
            
            EditManager.CurrentObject = obj;
        }
        
        WipeTabs();
        RefreshAttributeControls(!isPrefab);
        RefreshItem();
    }

    public static void RefreshAttributeControls(bool useDefaultConfig)
    {
        var configBtn = false;
        var receiverBtn = false;
        var broadcasterBtn = false;
        if (EditManager.CurrentObject is PlaceableObject placeable)
        {
            if (useDefaultConfig)
            {
                foreach (var val in placeable.ConfigGroup.Select(configType => configType.GetDefaultValue())
                             .Where(val => val != null)) EditManager.Config[val.GetTypeId()] = val;
            }
            
            _currentOption = AttributeType.Config;
                
            configBtn = placeable.ConfigGroup.Count > 0;
            if (configBtn) SetupConfigTab(placeable.ConfigGroup);
            receiverBtn = placeable.ReceiverGroup.Count > 0;
            if (receiverBtn) SetupReceiverTab(placeable.ReceiverGroup);
            broadcasterBtn = placeable.BroadcasterGroup.Count > 0;
            if (broadcasterBtn) SetupBroadcasterTab(placeable.BroadcasterGroup);
        }

        _configButton.interactable = configBtn;
        _receiversButton.interactable = receiverBtn;
        _broadcastersButton.interactable = broadcasterBtn;
    }

    private static GameObject _configTab; 
    private static GameObject _broadcasterTab;
    private static GameObject _receiverTab;

    private static int _receiverCount;
    private static int _broadcasterCount;

    private static void SetupConfigTab(List<ConfigType> group)
    {
        _configTab = PrepareTab("Config Tab");
        _configButton.transform.SetAsLastSibling();

        var y = 20 + 12 * group.Count;
        foreach (var type in group)
        {
            var txt = UIUtils.MakeLabel("Config Title", _configTab, new Vector3(54, y),
                Vector2.zero, Vector2.zero).textComponent;
            txt.text = type.Name;
            txt.fontSize = 6;
            txt.alignment = TextAnchor.MiddleLeft;

            var (btn, _) =  UIUtils.MakeTextButton("Config Apply", "Apply", _configTab, 
                new Vector3(262, y), Vector2.zero, Vector2.zero);
            btn.interactable = false;

            var inp = type.CreateInput(_configTab, btn, new Vector3(142, y), 
                EditManager.Config.GetValueOrDefault(type.Id)?.SerializeValue());
            
            btn.onClick.AddListener(() =>
            {
                btn.interactable = false;
                var val = inp.GetValue();
                if (val.Length == 0) EditManager.Config.Remove(type.Id);
                else EditManager.Config[type.Id] = type.Deserialize(inp.GetValue());

                CursorManager.NeedsRefresh = true;
            });
            
            y -= 12;
        }
    }

    private static void SetupReceiverTab(List<EventReceiverType> group)
    {
        _receiverTab = PrepareTab("Receiver Tab");
        _receiverCount = 0;
        _receiversButton.transform.SetAsLastSibling();

        MakeEventTabLabel(_receiverTab, "Name", new Vector3(50, 235));
        MakeEventTabLabel(_receiverTab, "Trigger", new Vector3(142, 235));
        MakeEventTabLabel(_receiverTab, "Times", new Vector3(204, 235));
        MakeEventTabLabel(_receiverTab, "Add/Remove", new Vector3(252, 235));

        var (eventInput, _) = UIUtils.MakeTextbox("Receiver Event Input", _receiverTab, new Vector3(50, 220),
            Vector2.zero, Vector2.zero, 200, 25);

        var currentTriggerIndex = 0;
        var (btn, btnLabel) = UIUtils.MakeTextButton("Receiver Trigger Choice", group[currentTriggerIndex].Name, _receiverTab,
            new Vector3(142, 220), Vector2.zero, Vector2.zero, size: new Vector2(200, 25));
        btn.onClick.AddListener(() =>
        {
            currentTriggerIndex = (currentTriggerIndex + 1) % group.Count;
            btnLabel.textComponent.text = group[currentTriggerIndex].Name;
        });

        var (timesField, _) = UIUtils.MakeTextbox("Receiver Times", _receiverTab, new Vector3(204, 220),
            Vector2.zero, Vector2.zero, 80, 25);
        timesField.characterValidation = InputField.CharacterValidation.Integer;
        timesField.text = "1";
        
        var (addBtn, _) = UIUtils.MakeTextButton("Receiver Add", "+", _receiverTab,
            new Vector3(252, 220), Vector2.zero, Vector2.zero, size: new Vector2(30, 30));

        addBtn.interactable = false;
        eventInput.onValueChanged.AddListener(s => addBtn.interactable = s.Length > 0);
        
        addBtn.onClick.AddListener(() =>
        {
            var timesTxt = timesField.text;
            var times = timesTxt.Length == 0 ? 1 : Convert.ToInt32(timesTxt);

            var data = (eventInput.text, group[currentTriggerIndex].Id, times);
            eventInput.text = "";
            addBtn.interactable = false;
            AddReceiverToTab(data, group[currentTriggerIndex].Name);
            EditManager.Receivers.Add(data);
        });

        foreach (var data in EditManager.Receivers)
        {
            AddReceiverToTab(data, EventManager.GetReceiverType(data.Item2).Name);
        }
    }

    private static void AddReceiverToTab((string, string, int) data, string name)
    {
        var yPos = 207 - 12 * _receiverCount;
        var r1 = MakeEventTabLabel(_receiverTab, data.Item1, new Vector3(50, yPos));
        var r2 = MakeEventTabLabel(_receiverTab, name, new Vector3(142, yPos));
        var r3 = MakeEventTabLabel(_receiverTab, data.Item3.ToString(), new Vector3(204, yPos));

        var (addBtn, btnLabel) = UIUtils.MakeTextButton("Receiver Remove", "-", _receiverTab,
            new Vector3(252, yPos), Vector2.zero, Vector2.zero,
            size: new Vector2(30, 30));
        addBtn.onClick.AddListener(() =>
        {
            Object.Destroy(r1);
            Object.Destroy(r2);
            Object.Destroy(r3);
            Object.Destroy(addBtn.gameObject);
            Object.Destroy(btnLabel.gameObject);
            EditManager.Receivers.Remove(data);
        });

        _receiverCount++;
    }

    private static void SetupBroadcasterTab(List<string> group)
    {
        _broadcasterTab = PrepareTab("Broadcaster Tab");
        _broadcasterCount = 0;
        _broadcastersButton.transform.SetAsLastSibling();
        
        MakeEventTabLabel(_broadcasterTab, "Event", new Vector3(50, 235));
        MakeEventTabLabel(_broadcasterTab, "Name", new Vector3(142, 235));
        MakeEventTabLabel(_broadcasterTab, "Add/Remove", new Vector3(228, 235));

        var currentTriggerIndex = 0;
        var (btn, btnLabel) = UIUtils.MakeTextButton("Broadcaster Cause Choice", 
            group[currentTriggerIndex], _broadcasterTab, new Vector3(50, 220), 
            Vector2.zero, Vector2.zero, size: new Vector2(200, 25));
        btn.onClick.AddListener(() =>
        {
            currentTriggerIndex = (currentTriggerIndex + 1) % group.Count;
            btnLabel.textComponent.text = group[currentTriggerIndex];
        });

        var (eventInput, _) = UIUtils.MakeTextbox("Broadcaster Event Input", _broadcasterTab,
            new Vector3(142, 220), Vector2.zero, Vector2.zero, 200, 25);
        
        var (addBtn, _) = UIUtils.MakeTextButton("Broadcaster Add", "+", _broadcasterTab,
            new Vector3(228, 220), Vector2.zero, Vector2.zero, size: new Vector2(30, 30));

        addBtn.interactable = false;
        eventInput.onValueChanged.AddListener(s => addBtn.interactable = s.Length > 0);
        
        addBtn.onClick.AddListener(() =>
        {
            var data = (group[currentTriggerIndex], eventInput.text);
            eventInput.text = "";
            addBtn.interactable = false;
            AddBroadcasterToTab(data);
            EditManager.Broadcasters.Add(data);
        });

        foreach (var data in EditManager.Broadcasters) AddBroadcasterToTab(data);
    }

    private static void AddBroadcasterToTab((string, string) data)
    {
        var yPos = 207 - 12 * _broadcasterCount;
        var r2 = MakeEventTabLabel(_broadcasterTab, data.Item1, new Vector3(50, yPos));
        var r1 = MakeEventTabLabel(_broadcasterTab, data.Item2, new Vector3(142, yPos));

        var (addBtn, btnLabel) = UIUtils.MakeTextButton("Broadcaster Remove", "-", _broadcasterTab,
            new Vector3(228, yPos), Vector2.zero, Vector2.zero,
            size: new Vector2(30, 30));
        addBtn.onClick.AddListener(() =>
        {
            Object.Destroy(r1);
            Object.Destroy(r2);
            Object.Destroy(addBtn.gameObject);
            Object.Destroy(btnLabel.gameObject);
            EditManager.Broadcasters.Remove(data);
        });

        _broadcasterCount++;
    }

    private static GameObject MakeEventTabLabel(GameObject tab, string text, Vector3 pos)
    {
        var txt = UIUtils.MakeLabel(text, tab, pos,
            Vector2.zero, Vector2.zero);
        txt.textComponent.text = text;
        txt.textComponent.fontSize = 8;
        txt.textComponent.alignment = TextAnchor.MiddleCenter;
        txt.transform.SetAsFirstSibling();
        return txt.gameObject;
    }

    private static GameObject PrepareTab(string name)
    {
        var tab = new GameObject(name);
        tab.SetActive(false);
        
        var trans = tab.AddComponent<RectTransform>();
        trans.anchorMin = Vector2.zero;
        trans.anchorMax = Vector2.zero;
        trans.offsetMin = Vector2.zero;
        trans.offsetMax = Vector2.zero;
        trans.SetParent(_canvasObj.transform, false);
        trans.SetAsLastSibling();

        return tab;
    }

    private static void RefreshCurrentTab(bool paused)
    {
        if (_configTab) _configTab.SetActive(paused && _currentOption == AttributeType.Config);
        if (_broadcasterTab) _broadcasterTab.SetActive(paused && _currentOption == AttributeType.Events);
        if (_receiverTab) _receiverTab.SetActive(paused && _currentOption == AttributeType.Listeners);
    }

    private static void ToggleFavourite(int i, UIUtils.Label label)
    {
        var index = _pageIndex * ITEMS_PER_PAGE + i;
        if (_categoryContents.Count <= index) return;
        switch (_categoryContents[index])
        {
            case PlaceableObject placeable when FavouritesCategory.ToggleFavourite(placeable):
                label.textComponent.text = FILLED_STAR;
                label.textComponent.color = Color.yellow;
                break;
            case PlaceableObject:
                label.textComponent.text = EMPTY_STAR;
                label.textComponent.color = Color.white;
                break;
            case PrefabObject prefab:
                PrefabsCategory.RemovePrefab(prefab);
                RefreshCurrentPage();
                break;
        }
    }

    public static void RefreshItem()
    {
        var icon = HotbarIcons[EditManager.HotbarIndex];
        icon.sprite = EditManager.CurrentObject.GetUISprite();

        var rot = 0f;
        icon.transform.SetScaleX(1);
        icon.transform.SetScaleY(1);

        if (EditManager.CurrentObject is PlaceableObject placeable)
        {
            switch (placeable.GetUISprite().packingRotation)
            {
                case SpritePackingRotation.FlipHorizontal:
                    icon.transform.SetScaleX(-1);
                    break;
                case SpritePackingRotation.FlipVertical:
                    icon.transform.SetScaleY(-1);
                    break;
                case SpritePackingRotation.Rotate180:
                    rot += 180;
                    break;
            }

            rot += placeable.Rotation + placeable.ChildRotation + placeable.Tk2dRotation;
            
            icon.transform.SetScale2D(new Vector2(1.25f, 1.25f));
        }
        else icon.transform.SetScale2D(new Vector2(1, 1));

        icon.transform.SetRotationZ(rot);
        
        _currentlySelected.textComponent.text = EditManager.CurrentObject.GetName();
        _currentlySelectedDesc.textComponent.text = EditManager.CurrentObject.GetDescription();
    }

    public static void CompleteSetup()
    {
        FavouritesCategory.Favourites = StorageManager.LoadFavourites();
        PrefabsCategory.Prefabs = StorageManager.LoadPrefabs();

        RefreshCurrentPage();
    }

    public static void RefreshCurrentPage()
    {
        _categoryContents = CurrentCategory.GetObjects().Where(obj => obj.GetName()
            .IndexOf(_currentSearch, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

        for (var i = 0; i < ITEMS_PER_PAGE; i++)
        {
            var index = _pageIndex * ITEMS_PER_PAGE + i;
            var icon = GridIcons[i];
            if (_categoryContents.Count <= index)
            {
                icon.Item1.sprite = ArchitectPlugin.BlankSprite;
                icon.Item2.textComponent.text = "Unset";
                icon.Item3.textComponent.text = NOTHING;
            }
            else
            {
                var rot = 0f;
                
                PlaceableObject placeable;
                switch (_categoryContents[index])
                {
                    case PlaceableObject obj:
                        placeable = obj;
                        var favourite = FavouritesCategory.IsFavourite(placeable);
                        icon.Item3.textComponent.text = favourite ? FILLED_STAR : EMPTY_STAR;
                        icon.Item3.textComponent.color = favourite ? Color.yellow : Color.white;
                        break;
                    case PrefabObject prefab:
                        placeable = prefab.PlaceableObject;
                        icon.Item3.textComponent.text = BIN;
                        icon.Item3.textComponent.color = Color.red;
                        rot = prefab.Placement.GetRotation();
                        break;
                    default:
                        return;
                }
                icon.Item1.sprite = placeable.GetUISprite();
                icon.Item2.textComponent.text = "";
                
                icon.Item1.transform.SetScaleX(1);
                icon.Item1.transform.SetScaleY(1);
                switch (placeable.GetUISprite().packingRotation)
                {
                    case SpritePackingRotation.FlipHorizontal:
                        icon.Item1.transform.SetScaleX(-1);
                        break;
                    case SpritePackingRotation.FlipVertical:
                        icon.Item1.transform.SetScaleY(-1);
                        break;
                    case SpritePackingRotation.Rotate180:
                        rot += 180;
                        break;
                }

                icon.Item1.transform.SetRotation2D(rot + placeable.Rotation + 
                                                   placeable.ChildRotation + placeable.Tk2dRotation);
            }
        }
    }
    
    public static void Shift(int amount)
    {
        _pageIndex += amount;

        var num = (_categoryContents.Count - 1) / 9;
        if (_pageIndex > num) _pageIndex = 0;
        else if (_pageIndex < 0) _pageIndex = num;

        RefreshCurrentPage();
    }

    private static void SetupSearchBox()
    {
        var pos = new Vector3(-65, 131.25f);
        var anchor = new Vector2(1, 0);
        var (txt, label) = UIUtils.MakeTextbox("Search Box", _canvasObj, pos, anchor, anchor,
            300, 32);
        txt.onValueChanged.AddListener(s =>
        {
            _pageIndex = 0;
            _currentSearch = s;
            RefreshCurrentPage();
        });

        var placeholder = UIUtils.MakeLabel("Search Box Placeholder", _canvasObj, pos,
            anchor, anchor, 280).textComponent;
        placeholder.text = "Search...";
        placeholder.transform.localScale /= 3;
        placeholder.fontSize = 20;
        placeholder.alignment = TextAnchor.MiddleLeft;
        placeholder.color = Color.grey;
        placeholder.fontStyle = FontStyle.Italic;

        txt.placeholder = placeholder;

        DisableWhenPlaying.Add(txt.gameObject);
        DisableWhenPlaying.Add(label.gameObject);
        DisableWhenPlaying.Add(placeholder.gameObject);
    }

    private static void SetupPreciseSettings()
    {
        var anchor = new Vector2(1, 0);
        (RotationText, var rotText) = UIUtils.MakeTextbox("Rotation Box", _canvasObj, new Vector3(-65, 170)
            , anchor, anchor, 70, 32);

        RotationText.characterValidation = InputField.CharacterValidation.Decimal;
        RotationText.onValueChanged.AddListener(s =>
        {
            try
            {
                EditManager.CurrentRotation = Convert.ToSingle(s, CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                EditManager.SetRotation(0);
            }
            
            CursorManager.NeedsRefresh = true;
        });

        (ScaleText, var scaleText) = UIUtils.MakeTextbox("Scale Box", _canvasObj, new Vector3(-65, 190)
            , anchor, anchor, 70, 32);

        ScaleText.characterValidation = InputField.CharacterValidation.Decimal;
        ScaleText.onValueChanged.AddListener(s =>
        {
            try
            {
                EditManager.CurrentScale = Convert.ToSingle(s, CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                EditManager.SetScale(1);
            }

            CursorManager.NeedsRefresh = true;
        });

        var rotLabel = UIUtils.MakeLabel("Rotation Label", _canvasObj, new Vector3(-75, 170), anchor, anchor);
        rotLabel.textComponent.text = "Rotation: ";
        rotLabel.textComponent.fontSize = 8;
        rotLabel.textComponent.alignment = TextAnchor.MiddleLeft;

        var scaleLabel = UIUtils.MakeLabel("Scale Label", _canvasObj, new Vector3(-75, 190), anchor, anchor);
        scaleLabel.textComponent.text = "Scale: ";
        scaleLabel.textComponent.fontSize = 8;
        scaleLabel.textComponent.alignment = TextAnchor.MiddleLeft;

        DisableWhenPlaying.Add(RotationText.gameObject);
        DisableWhenPlaying.Add(rotText.gameObject);
        DisableWhenPlaying.Add(rotLabel.gameObject);
        DisableWhenPlaying.Add(ScaleText.gameObject);
        DisableWhenPlaying.Add(scaleText.gameObject);
        DisableWhenPlaying.Add(scaleLabel.gameObject);

        EditManager.SetRotation(0);
        EditManager.SetScale(1);
    }

    private static void SetupAttributeSettings()
    {
        var pos = new Vector3(50, 13.25f);
        _configButton = SetupAttributeButton(AttributeType.Config, "Config", pos);
        pos.x += 92;
        _broadcastersButton = SetupAttributeButton(AttributeType.Events, "Events", pos);
        pos.x += 92;
        _receiversButton = SetupAttributeButton(AttributeType.Listeners, "Listeners", pos);
    }
    
    private static Button SetupAttributeButton(AttributeType type, string name, Vector3 pos)
    {
        var size = new Vector2(260, 30);
        var (btn, label) = UIUtils.MakeTextButton(name + " Button", name, _canvasObj, pos, 
            Vector2.zero, Vector2.zero, size:size);
        DisableWhenPlaying.Add(btn.gameObject);
        DisableWhenPlaying.Add(label.gameObject);

        btn.onClick.AddListener(() => _currentOption = type);
        btn.interactable = false;

        return btn;
    }

    private static void SetupHotbar()
    {
        for (var i = -4; i < 5; i++)
        {
            var (btn, img, lbl) = UIUtils.MakeButtonWithImage("Hotbar Test", _canvasObj,
                new Vector3(i * 45, 35), new Vector2(0.5f, 0), new Vector2(0.5f, 0), 
                96, 48);
            btn.enabled = false;

            img.sprite = ArchitectPlugin.BlankSprite;
            HotbarIcons.Add(img);

            EnableWhenPlaying.Add(btn.gameObject);
            EnableWhenPlaying.Add(img.gameObject);
            EnableWhenPlaying.Add(lbl.gameObject);
        }
    }

    private enum AttributeType
    {
        Config,
        Events,
        Listeners
    }
}