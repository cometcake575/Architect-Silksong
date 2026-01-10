using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Architect.Config.Types;
using Architect.Events;
using Architect.Events.Blocks;
using Architect.Multiplayer;
using Architect.Objects;
using Architect.Objects.Categories;
using Architect.Objects.Placeable;
using Architect.Objects.Tools;
using Architect.Placements;
using Architect.Prefabs;
using Architect.Storage;
using Architect.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
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
    private static GameObject _mapUI;
    private static RectTransform _mapTransform;
    private static GameObject _scriptUI;
    
    public static AbstractCategory CurrentCategory = Categories.All;
    private static int _pageIndex;
    private static List<SelectableObject> _categoryContents;

    private static UIUtils.Label _currentlySelected;
    private static UIUtils.Label _currentlySelectedDesc;

    public static UIUtils.Label ObjectIdLabel;

    public static InputField RotationText;
    public static InputField ScaleText;

    private static string _currentSearch = "";

    private static (Button, UIUtils.Label) _configButton;
    private static (Button, UIUtils.Label) _broadcastersButton;
    private static (Button, UIUtils.Label) _receiversButton;
    
    private static (Button, UIUtils.Label) _mapButton;
    private static (Button, UIUtils.Label) _scriptButton;
    
    private static GameObject _shareLevelButton;
    private static GameObject _shareLevelLabel;
    private static GameObject _shareScriptButton;
    private static GameObject _shareScriptLabel;
    
    private static AttributeType _currentOption = AttributeType.Config;
    private static EditorType _currentType = EditorType.Map;
    
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

        _mapUI = new GameObject("Map Editor UI")
        {
            transform = { parent = _canvasObj.transform }
        };
        _mapUI.SetActive(false);
        _mapTransform = _mapUI.AddComponent<RectTransform>();
        _mapTransform.anchorMax = Vector2.one;
        _mapTransform.anchorMin = Vector2.zero;
        _mapTransform.offsetMax = Vector2.zero;
        _mapTransform.offsetMin = Vector2.zero;
        if (!Settings.LegacyEventSystem.Value) _mapTransform.anchoredPosition = new Vector2(0, 20);
        
        _scriptUI = new GameObject("Script Editor UI")
        {
            transform = { parent = _canvasObj.transform }
        };
        _scriptUI.SetActive(false);
        var st = _scriptUI.AddComponent<RectTransform>();
        st.anchorMax = Vector2.one;
        st.anchorMin = Vector2.zero;
        st.offsetMax = Vector2.zero;
        st.offsetMin = Vector2.zero;
        st.anchoredPosition = new Vector2(0, 20);
        
        ScriptEditorUI.Init(_scriptUI);
        
        _mapButton = SetupModeButton(EditorType.Map, "Map Editor", new Vector3(-200, 15));
        _scriptButton = SetupModeButton(EditorType.Script, "Script Editor", new Vector3(200, 15));
    }
    
    private static (Button, UIUtils.Label) SetupModeButton(EditorType type, string name, Vector3 pos)
    {
        var size = new Vector2(1150, 50);
        var (btn, label) = UIUtils.MakeTextButton(name + " Button", name, _canvasObj, pos, 
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), size:size);
        
        btn.onClick.AddListener(() =>
        {
            _currentType = type;
            Deletable.DeleteButton.SetActive(false);
        });
        label.textComponent.fontSize = 10;
        
        DisableWhenPlaying.Add(btn.gameObject);
        DisableWhenPlaying.Add(label.gameObject);
        
        return (btn, label);
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
        _currentlySelectedDesc.textComponent.verticalOverflow = VerticalWrapMode.Overflow;

        _currentlySelectedDesc.textComponent.fontSize = 10;

        var currentScene = UIUtils.MakeLabel("Current Scene", _canvasObj,
            new Vector3(50, 45, 0), Vector2.zero, Vector2.zero);
        currentScene.textComponent.alignment = TextAnchor.LowerLeft;

        typeof(HeroController).Hook(nameof(HeroController.SceneInit),
            (Action<HeroController> orig, HeroController self) =>
            {
                orig(self);
                currentScene.textComponent.text = "Scene: " + GameManager.instance.sceneName;
            });
        EnableWhenPlaying.Add(currentScene.gameObject);

        var bottomAnchor = new Vector2(0.5f, 0);
        ObjectIdLabel = UIUtils.MakeLabel("Object ID Description", _canvasObj,
            new Vector3(0, 100, 0), bottomAnchor, bottomAnchor);
        ObjectIdLabel.textComponent.fontSize = 10;
        ObjectIdLabel.textComponent.alignment = TextAnchor.LowerCenter;
    }

    private static (Button, UIUtils.Label) _legacyCategory;

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
                _mapUI,
                position,
                anchor,
                anchor
            );
            if (category is Category { Priority: < 0 }) _legacyCategory = (btn, label); 
            btn.onClick.AddListener(() =>
            {
                _pageIndex = 0;
                CurrentCategory = category;
                RefreshCurrentPage();
            });
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

                var (btn, img, label) = UIUtils.MakeButtonWithImage("Option " + index, _mapUI,
                    new Vector3(-25 - j * 40, 25 + i * 40), anchor, anchor, 96, 60);

                var (fav, favLabel) = UIUtils.MakeTextButton("Favourite " + index, NOTHING, _mapUI,
                    new Vector3(-45 - j * 40, 25 + i * 40), anchor, anchor, false);

                btn.onClick.AddListener(() => SetItem(index));
                fav.onClick.AddListener(() => ToggleFavourite(index, favLabel));

                GridIcons.Add((img, label, favLabel));
            }
        }

        MakeToolButton(CursorObject.Instance, 230, 40);
        MakeToolButton(EraserObject.Instance, 190, 40);
        MakeToolButton(PickObject.Instance, 150, 40);
        
        MakeToolButton(DragObject.Instance, 250, 0);
        MakeToolButton(LockObject.Instance, 210, 0);
        MakeToolButton(TileChangerObject.Instance, 170, 0);
        MakeToolButton(ResetObject.Instance, 130, 0);
        
        /*var (prefabBtn, prefabImg, _) = UIUtils.MakeButtonWithImage("Prefab Editor", _mapUI,
            new Vector3(-25, -45), new Vector2(1, 1), new Vector2(1, 1), 96, 48);
        
        prefabBtn.onClick.AddListener(PrefabManager.Toggle);
        prefabImg.sprite = ResourceUtils.LoadSpriteResource("prefab");*/

        var shareBtn = UIUtils.MakeTextButton(
            "Share Level", 
            $"Share Room ({CoopManager.Instance.Name})",
            _mapUI,
            new Vector3(-215, 95),
            new Vector2(1, 0),
            new Vector2(1, 0),
            size: new Vector2(250, 35)
            );
        _shareLevelButton = shareBtn.Item1.gameObject;
        _shareLevelLabel = shareBtn.Item2.gameObject;

        var shareScriptBtn = UIUtils.MakeTextButton(
            "Share Script", 
            $"Share Script ({CoopManager.Instance.Name})",
            _scriptUI,
            new Vector3(60, 20),
            new Vector2(0, 0),
            new Vector2(0, 0),
            size: new Vector2(250, 35)
            );
        _shareScriptButton = shareScriptBtn.Item1.gameObject;
        _shareScriptLabel = shareScriptBtn.Item2.gameObject;
        _shareScriptLabel.transform.parent = _shareScriptButton.transform;
        
        shareBtn.Item1.onClick.AddListener(() =>
        {
            if (!CoopManager.Instance.IsActive()) return;
            CoopManager.Instance.ShareScene(GameManager.instance.sceneName, false);
        });
        shareScriptBtn.Item1.onClick.AddListener(() =>
        {
            if (!CoopManager.Instance.IsActive()) return;
            CoopManager.Instance.ShareScene(ScriptManager.IsLocal ? 
                GameManager.instance.sceneName : StorageManager.GLOBAL, true);
        });

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

    private static Button MakeToolButton(ToolObject obj, int xShift, int yShift)
    {
        var anchor = new Vector2(1, 0);
        var (toolBtn, toolImg, _) = UIUtils.MakeButtonWithImage(obj.GetName(), _mapUI,
            new Vector3(-25 - xShift, 25 + yShift), anchor, anchor, 96, 48);
        toolBtn.onClick.AddListener(() => SetItem(obj.Index));
        toolImg.sprite = obj.GetUISprite();
        return toolBtn;
    }

    public static void RefreshVisibility(bool editing, bool paused)
    {
        _canvasObj.SetActive(editing);
        if (editing)
        {
            RefreshCurrentTab(paused);
            
            foreach (var obj in DisableWhenPlaying) obj.SetActive(paused);
            foreach (var obj in EnableWhenPlaying) obj.SetActive(!paused);

            if (paused)
            {
                SetupLegacy(Settings.LegacyEventSystem.Value);
                ScriptEditorUI.UpdateColour();
            }
            else
            {
                _mapUI.SetActive(false);
                _scriptUI.SetActive(false);
                Deletable.DeleteButton.SetActive(false);
                _currentType = EditorType.Map;
            }

            var share = paused && CoopManager.Instance.IsActive();
            _shareLevelButton.SetActive(share);
            _shareLevelLabel.SetActive(share);
            _shareScriptButton.SetActive(share);
            _shareScriptLabel.SetActive(share);
        }
    }

    private static void SetupLegacy(bool legacy)
    {
        _legacyCategory.Item2.gameObject.SetActive(legacy);
        _legacyCategory.Item1.gameObject.SetActive(legacy);

        _configButton.Item1.gameObject.SetActive(legacy);
        _configButton.Item2.gameObject.SetActive(legacy);
        _broadcastersButton.Item1.gameObject.SetActive(legacy);
        _broadcastersButton.Item2.gameObject.SetActive(legacy);
        _receiversButton.Item1.gameObject.SetActive(legacy);
        _receiversButton.Item2.gameObject.SetActive(legacy);

        _mapButton.Item1.gameObject.SetActive(!legacy);
        _mapButton.Item2.gameObject.SetActive(!legacy);
        _scriptButton.Item1.gameObject.SetActive(!legacy);
        _scriptButton.Item2.gameObject.SetActive(!legacy);

        if (_configTransform) _configTransform.anchoredPosition = new Vector2(0, legacy ? 0 : -20);
        _mapTransform.anchoredPosition = new Vector2(0, legacy ? 0 : 20);
        
        if (legacy)
        {
            _currentType = EditorType.Map;
            Deletable.DeleteButton.SetActive(false);
        }
        else
        {
            _currentOption = AttributeType.Config;
            if (CurrentCategory == Categories.Legacy)
            {
                _pageIndex = 0;
                CurrentCategory = Categories.All;
                RefreshCurrentPage();
            }
        }

        _mapUI.SetActive(_currentType == EditorType.Map);
        _scriptUI.SetActive(_currentType == EditorType.Script);
    }

    public static void WipeTabs()
    {
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

        CursorManager.ObjectChanged = true;
        
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

        _configButton.Item1.interactable = configBtn;
        _receiversButton.Item1.interactable = receiverBtn;
        _broadcastersButton.Item1.interactable = broadcasterBtn;
    }

    private static RectTransform _configTransform; 
    private static GameObject _configTab; 
    public static readonly List<(InputField, Action)> ConfigIds = []; 
    private static GameObject _broadcasterTab;
    private static GameObject _receiverTab;

    private static int _receiverCount;
    private static int _broadcasterCount;

    private static void SetupConfigTab(List<ConfigType> group)
    {
        (_configTab, _configTransform) = PrepareTab("Config Tab");
        ConfigIds.Clear();
        _configButton.Item1.transform.SetAsLastSibling();

        var y = 20 + 14 * group.Count;
        foreach (var type in group)
        {
            var txt = UIUtils.MakeLabel("Config Title", _configTab, new Vector3(54, y),
                Vector2.zero, Vector2.zero).textComponent;
            txt.text = type.Name;
            txt.fontSize = 8;
            txt.alignment = TextAnchor.MiddleLeft;

            var (btn, _) =  UIUtils.MakeTextButton("Config Apply", "Apply", _configTab, 
                new Vector3(262, y), Vector2.zero, Vector2.zero);
            btn.interactable = false;

            var inp = type.CreateInput(_configTab, btn, new Vector3(142, y), 
                EditManager.Config.GetValueOrDefault(type.Id)?.SerializeValue());

            if (inp is IdConfigElement element)
            {
                ConfigIds.Add((element.GetField(), Apply));
            }
            
            btn.onClick.AddListener(Apply);
            
            y -= 14;
            continue;

            void Apply()
            {
                btn.interactable = false;
                var val = inp.GetValue();
                if (val.Length == 0)
                    EditManager.Config.Remove(type.Id);
                else
                    EditManager.Config[type.Id] = type.Deserialize(inp.GetValue());

                CursorManager.ObjectChanged = true;
            }
        }
    }

    public class ChoiceButton : MonoBehaviour, IPointerClickHandler
    {
        public Action OnLeftClick;
        public Action OnRightClick;
        
        public void OnPointerClick(PointerEventData eventData)
        {
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                    OnLeftClick.Invoke();
                    break;
                case PointerEventData.InputButton.Right:
                    OnRightClick.Invoke();
                    break;
                case PointerEventData.InputButton.Middle:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private static void SetupReceiverTab(List<EventReceiverType> group)
    {
        _receiverTab = PrepareTab("Receiver Tab").Item1;
        _receiverCount = 0;
        _receiversButton.Item1.transform.SetAsLastSibling();

        MakeEventTabLabel(_receiverTab, "Name", new Vector3(50, 235));
        MakeEventTabLabel(_receiverTab, "Trigger", new Vector3(142, 235));
        MakeEventTabLabel(_receiverTab, "Times", new Vector3(204, 235));
        MakeEventTabLabel(_receiverTab, "Add/Remove", new Vector3(252, 235));

        var (eventInput, _) = UIUtils.MakeTextbox("Receiver Event Input", _receiverTab, new Vector3(50, 220),
            Vector2.zero, Vector2.zero, 200, 25);

        var currentTriggerIndex = 0;
        var (btn, btnLabel) = UIUtils.MakeTextButton("Receiver Trigger Choice", group[currentTriggerIndex].Name, _receiverTab,
            new Vector3(142, 220), Vector2.zero, Vector2.zero, size: new Vector2(200, 25));

        var cBtn = btn.gameObject.AddComponent<ChoiceButton>();
        
        cBtn.OnLeftClick += () =>
        {
            currentTriggerIndex = (currentTriggerIndex + 1) % group.Count;
            btnLabel.textComponent.text = group[currentTriggerIndex].Name;
        };
        cBtn.OnRightClick += () =>
        {
            currentTriggerIndex = (currentTriggerIndex - 1) % group.Count;
            if (currentTriggerIndex < 0) currentTriggerIndex += group.Count;
            btnLabel.textComponent.text = group[currentTriggerIndex].Name;
        };

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
        _broadcasterTab = PrepareTab("Broadcaster Tab").Item1;
        _broadcasterCount = 0;
        _broadcastersButton.Item1.transform.SetAsLastSibling();
        
        MakeEventTabLabel(_broadcasterTab, "Event", new Vector3(50, 235));
        MakeEventTabLabel(_broadcasterTab, "Name", new Vector3(142, 235));
        MakeEventTabLabel(_broadcasterTab, "Add/Remove", new Vector3(228, 235));

        var currentTriggerIndex = 0;
        var (btn, btnLabel) = UIUtils.MakeTextButton("Broadcaster Cause Choice", 
            group[currentTriggerIndex], _broadcasterTab, new Vector3(50, 220), 
            Vector2.zero, Vector2.zero, size: new Vector2(200, 25));
        
        var cBtn = btn.gameObject.AddComponent<ChoiceButton>();
        
        cBtn.OnLeftClick += () =>
        {
            currentTriggerIndex = (currentTriggerIndex + 1) % group.Count;
            btnLabel.textComponent.text = group[currentTriggerIndex];
        };
        cBtn.OnRightClick += () =>
        {
            currentTriggerIndex = (currentTriggerIndex - 1) % group.Count;
            if (currentTriggerIndex < 0) currentTriggerIndex += group.Count;
            btnLabel.textComponent.text = group[currentTriggerIndex];
        };

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

    private static (GameObject, RectTransform) PrepareTab(string name)
    {
        var tab = new GameObject(name);
        tab.SetActive(false);
        
        var trans = tab.AddComponent<RectTransform>();
        trans.anchorMin = Vector2.zero;
        trans.anchorMax = Vector2.zero;
        trans.offsetMin = Vector2.zero;
        trans.offsetMax = Vector2.zero;
        trans.SetParent(_mapUI.transform, false);
        trans.SetAsLastSibling();

        return (tab, trans);
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
        var txt = UIUtils.MakeTextbox("Search Box", _mapUI, pos, anchor, anchor,
            300, 32).Item1;
        txt.onValueChanged.AddListener(s =>
        {
            _pageIndex = 0;
            _currentSearch = s;
            RefreshCurrentPage();
        });

        var placeholder = UIUtils.MakeLabel("Search Box Placeholder", _mapUI, pos,
            anchor, anchor, 280).textComponent;
        placeholder.text = "Search...";
        placeholder.transform.localScale /= 3;
        placeholder.fontSize = 20;
        placeholder.alignment = TextAnchor.MiddleLeft;
        placeholder.color = Color.grey;
        placeholder.fontStyle = FontStyle.Italic;

        txt.placeholder = placeholder;
    }

    private static void SetupPreciseSettings()
    {
        var anchor = new Vector2(1, 0);
        RotationText = UIUtils.MakeTextbox("Rotation Box", _mapUI, new Vector3(-65, 170)
            , anchor, anchor, 70, 32).Item1;

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

        ScaleText = UIUtils.MakeTextbox("Scale Box", _mapUI, new Vector3(-65, 190)
            , anchor, anchor, 70, 32).Item1;

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

        var rotLabel = UIUtils.MakeLabel("Rotation Label", _mapUI, new Vector3(-75, 170), anchor, anchor);
        rotLabel.textComponent.text = "Rotation: ";
        rotLabel.textComponent.fontSize = 8;
        rotLabel.textComponent.alignment = TextAnchor.MiddleLeft;

        var scaleLabel = UIUtils.MakeLabel("Scale Label", _mapUI, new Vector3(-75, 190), anchor, anchor);
        scaleLabel.textComponent.text = "Scale: ";
        scaleLabel.textComponent.fontSize = 8;
        scaleLabel.textComponent.alignment = TextAnchor.MiddleLeft;

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
    
    private static (Button, UIUtils.Label) SetupAttributeButton(AttributeType type, string name, Vector3 pos)
    {
        var size = new Vector2(260, 30);
        var (btn, label) = UIUtils.MakeTextButton(name + " Button", name, _mapUI, pos, 
            Vector2.zero, Vector2.zero, size:size);

        btn.onClick.AddListener(() => _currentOption = type);
        btn.interactable = false;

        return (btn, label);
    }

    private static void SetupHotbar()
    {
        for (var i = -4; i < 5; i++)
        {
            var (btn, img, lbl) = UIUtils.MakeButtonWithImage("Hotbar Part", _canvasObj,
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

    private enum EditorType
    {
        Map,
        Script
    }
}