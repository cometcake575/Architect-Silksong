using System;
using System.Collections;
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
using GlobalEnums;
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
    private static GameObject _workshopUI;
    
    private static GameObject _editorTypeButtons;
    private static GameObject _configTypeButtons;
    private static GameObject _modern;
    private static GameObject _categories;
    private static GameObject _universalOptions;
    public static GameObject PositionOptions;
    
    public static AbstractCategory CurrentCategory = Categories.All;
    public static int PageIndex;
    private static List<SelectableObject> _categoryContents;

    private static UIUtils.Label _currentlySelected;
    private static UIUtils.Label _currentlySelectedDesc;

    public static UIUtils.Label ObjectIdLabel;

    public static InputField RotationText;
    public static InputField ScaleText;
    public static InputField ZText;
    public static InputField PosXText;
    public static InputField PosYText;

    private static string _currentSearch = "";

    private static (Button, UIUtils.Label) _configButton;
    private static (Button, UIUtils.Label) _broadcastersButton;
    private static (Button, UIUtils.Label) _receiversButton;
    
    private static GameObject _shareLevelButton;
    private static GameObject _shareLevelLabel;
    private static GameObject _shareScriptButton;
    private static GameObject _shareScriptLabel;
    
    private static AttributeType _currentOption = AttributeType.Config;
    public static EditorType CurrentType = EditorType.Map;
    
    public static void Setup()
    {
        SetupCanvas();
        SetupLabels();
        SetupObjects();
        SetupSearchBox();
        SetupPreciseSettings();
        SetupEditorSettings();
        SetupAttributeSettings();
        SetupHotbar();
        SetupLayers();

        SetupEditBlockers();

        RefreshItem();
    }

    public static void DisplayHotbarText(string text)
    {
        ObjectIdLabel.textComponent.text = text;
        ArchitectPlugin.Instance.StartCoroutine(CursorObject.ClearCursorInfoLabel());
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
        
        _modern = new GameObject("Modern Elements")
        {
            transform = { parent = _mapTransform }
        };
        _modern.RemoveOffset();
        _categories = new GameObject("Categories")
        {
            transform = { parent = _mapTransform }
        };
        _categories.RemoveOffset();
        _universalOptions = new GameObject("Universal Options")
        {
            transform = { parent = _canvasObj.transform }
        };
        _universalOptions.RemoveOffset();
        
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
        
        _workshopUI = new GameObject("Workshop UI")
        {
            transform = { parent = _canvasObj.transform }
        };
        _workshopUI.SetActive(false);
        var wt = _workshopUI.AddComponent<RectTransform>();
        wt.anchorMax = Vector2.one;
        wt.anchorMin = Vector2.zero;
        wt.offsetMax = Vector2.zero;
        wt.offsetMin = Vector2.zero;
        wt.anchoredPosition = new Vector2(0, 60);
        
        WorkshopUI.Init(_workshopUI);
    }

    private static UIUtils.Label _inTestMode;

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
        _inTestMode = UIUtils.MakeLabel("Test Mode", _canvasObj,
            new Vector3(-50, 45, 0), new Vector2(1, 0), new Vector2(1, 0));
        currentScene.textComponent.alignment = TextAnchor.LowerLeft;
        _inTestMode.textComponent.alignment = TextAnchor.LowerRight;

        typeof(HeroController).Hook(nameof(HeroController.SceneInit),
            (Action<HeroController> orig, HeroController self) =>
            {
                orig(self);
                currentScene.textComponent.text = "Scene: " + GameManager.instance.sceneName;
            });
        EnableWhenPlaying.Add(currentScene.gameObject);
        EnableWhenPlaying.Add(_inTestMode.gameObject);

        var bottomAnchor = new Vector2(0.5f, 0);
        ObjectIdLabel = UIUtils.MakeLabel("Object ID Description", _canvasObj,
            new Vector3(0, 100, 0), bottomAnchor, bottomAnchor);
        ObjectIdLabel.textComponent.fontSize = 10;
        ObjectIdLabel.textComponent.alignment = TextAnchor.LowerCenter;
    }

    private static (Button, UIUtils.Label) _legacyCategory;

    public static void SetupCategories()
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
                _categories,
                position,
                anchor,
                anchor
            );
            if (category.GetName() == "Legacy") _legacyCategory = (btn, label);
            btn.onClick.AddListener(() =>
            {
                PageIndex = 0;
                CurrentCategory = category;
                DoRefreshCurrentPage();
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

        SetupPrefabButton();

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
            new Vector3(0, 20),
            new Vector2(0.5f, 0),
            new Vector2(0.5f, 0),
            size: new Vector2(250, 35)
            );
        _shareScriptButton = shareScriptBtn.Item1.gameObject;
        _shareScriptLabel = shareScriptBtn.Item2.gameObject;
        _shareScriptLabel.transform.parent = _shareScriptButton.transform;
        
        shareBtn.Item1.onClick.AddListener(() =>
        {
            if (!CoopManager.Instance.IsActive()) return;
            CoopManager.Instance.ShareScene(GameManager.instance.sceneName, false, PlacementManager.GetLevelData());
        });
        shareScriptBtn.Item1.onClick.AddListener(() =>
        {
            if (!CoopManager.Instance.IsActive()) return;
            CoopManager.Instance.ShareScene(ScriptManager.IsLocal ? 
                GameManager.instance.sceneName : StorageManager.GLOBAL, true,
                ScriptManager.IsLocal ? PlacementManager.GetLevelData() : PlacementManager.GetGlobalData());
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

    private static void SetupPrefabButton()
    {
        var (prefabBtn, prefabImg, _) = UIUtils.MakeButtonWithImage("Prefab Editor", _modern,
            new Vector3(-25, -45), new Vector2(1, 1), new Vector2(1, 1), 96, 48);
        
        var openPrefab = new GameObject("Open Prefab")
        {
            transform = { parent = _modern.transform }
        };
        var rt = openPrefab.AddComponent<RectTransform>();
        rt.anchorMax = Vector2.one;
        rt.anchorMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.offsetMin = Vector2.zero;
        openPrefab.SetActive(false);

        var textbox = UIUtils.MakeTextbox("Name", openPrefab, new Vector2(0, 0),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), 300, 30).Item1;

        var prompt = UIUtils.MakeLabel("Prompt", openPrefab, new Vector2(0, 20),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        prompt.textComponent.fontSize = 12;
        prompt.textComponent.text = "Enter Prefab Name:";
        prompt.textComponent.alignment = TextAnchor.MiddleCenter;
            
        UIUtils.MakeTextButton("Cancel", "Cancel", openPrefab, new Vector2(-25, -20),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            size: new Vector2(100, 30)).Item1.onClick.AddListener(() => openPrefab.SetActive(false));
        UIUtils.MakeTextButton("Confirm", "Confirm", openPrefab, new Vector2(25, -20), 
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            size: new Vector2(100, 30)).Item1.onClick.AddListener(() =>
        {
            if (textbox.text.Length == 0) return;
            openPrefab.SetActive(false);
            PrefabManager.Toggle(textbox.text);
        });

        var img = UIUtils.MakeImage("Background", openPrefab, Vector2.zero,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(400, 300));
        img.sprite = UIUtils.Square;
        img.color = new Color(0.2f, 0.2f, 0.2f);
        img.transform.SetAsFirstSibling();
        
        prefabBtn.onClick.AddListener(() =>
        {
            if ((Input.GetKey(KeyCode.LeftAlt) && PrefabManager.Last != null) || PrefabManager.InPrefabScene)
            {
                openPrefab.SetActive(false);
                PrefabManager.Toggle(PrefabManager.Last);
            }
            else
            {
                textbox.text = "";
                openPrefab.SetActive(true);
            }
        });
        
        prefabImg.sprite = PrefabManager.PrefabIcon;
        var (btn, _) = UIUtils.MakeTextButton(
            "Prefabs",
            "Prefabs",
            _mapUI,
            new Vector2(-25, -70),
            new Vector2(1, 1),
            new Vector2(1, 1)
        );
        btn.onClick.AddListener(() =>
        {
            PageIndex = 0;
            CurrentCategory = PrefabsCategory.Instance;
            DoRefreshCurrentPage();
        });
    }

    private static void MakeToolButton(ToolObject obj, int xShift, int yShift)
    {
        var anchor = new Vector2(1, 0);
        var (toolBtn, toolImg, _) = UIUtils.MakeButtonWithImage(obj.GetName(), _mapUI,
            new Vector3(-25 - xShift, 25 + yShift), anchor, anchor, 96, 48);
        toolBtn.onClick.AddListener(() => SetItem(obj.Index));
        toolImg.sprite = obj.GetUISprite();
    }

    public static void RefreshVisibility(bool editing, bool paused)
    {
        _canvasObj.SetActive(editing);
        if (editing)
        {
            _inTestMode.textComponent.text = Settings.TestMode.Value ? "Test Mode" : "";
            
            RefreshConfigTabs(paused);
            
            foreach (var obj in DisableWhenPlaying) obj.SetActive(paused);
            foreach (var obj in EnableWhenPlaying) obj.SetActive(!paused);
            
            if (paused && UIManager.instance.uiState == UIState.PAUSED) UIManager.instance.uiState = UIState.OPTIONS;

            if (paused)
            {
                SetupLegacy(Settings.LegacyEventSystem.Value);
                ScriptEditorUI.UpdateColour();

                _mapUI.SetActive(CurrentType == EditorType.Map);
                _scriptUI.SetActive(CurrentType == EditorType.Script);
                _workshopUI.SetActive(CurrentType == EditorType.Workshop);
            }
            else
            {
                _mapUI.SetActive(false);
                _scriptUI.SetActive(false);
                _workshopUI.SetActive(false);
                Deletable.DeleteButton.SetActive(false);
                CurrentType = EditorType.Map;
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

        _configTypeButtons.SetActive(legacy);
        _editorTypeButtons.SetActive(!legacy);

        _mapTransform.anchoredPosition = new Vector2(0, legacy ? 0 : 20);
        
        if (legacy)
        {
            CurrentType = EditorType.Map;
            UIManager.instance.uiState = UIState.PAUSED;
            Deletable.DeleteButton.SetActive(false);
        }
        else
        {
            _currentOption = AttributeType.Config;
            if (CurrentCategory == Categories.Legacy)
            {
                PageIndex = 0;
                CurrentCategory = Categories.All;
                DoRefreshCurrentPage();
            }
        }
    }
    
    public static void SetItem(int i)
    {
        var index = PageIndex * ITEMS_PER_PAGE + i;
        if (_categoryContents.Count <= index) i = -99;

        EditManager.TryFindEmptySlot();
        
        EditManager.ClearAttributes();

        EditManager.CurrentlyFlipped = false;
        EditManager.SetRotation(0);
        EditManager.SetZ(0);
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
            
            switch (obj)
            {
                case PreloadObject { Loaded: false }:
                    return;
                case SavedObject saved:
                {
                    obj = saved.PlaceableObject;

                    EditManager.Broadcasters.AddRange(saved.Placement.Broadcasters);
                    EditManager.Receivers.AddRange(saved.Placement.Receivers);
                    foreach (var conf in saved.Placement.Config)
                    {
                        EditManager.Config[conf.GetTypeId()] = conf;
                    }

                    EditManager.SetScale(saved.Placement.GetScale());
                    EditManager.SetRotation(saved.Placement.GetRotation());
                    EditManager.SetZ(saved.Placement.GetPos().z);
                    EditManager.CurrentlyFlipped = saved.Placement.IsFlipped();

                    isPrefab = true;
                    break;
                }
                case PlaceableObject placeable:
                    if (placeable is PrefabObject prefab)
                    {
                        if (Input.GetKey(KeyCode.LeftAlt) && !PrefabManager.InPrefabScene)
                        {
                            PrefabManager.Toggle(prefab.Name);
                            return;
                        }
                    }
                    EditManager.SetZ(placeable.ZPosition);
                    break;
            }

            EditManager.CurrentObject = obj;
        }
        
        RefreshAttributeControls(!isPrefab);
        RefreshItem();
    }

    public static void RefreshAttributeControls(bool useDefaultConfig)
    {
        Object.Destroy(_configTab);
        Object.Destroy(_receiverTab);
        Object.Destroy(_broadcasterTab);
        
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
            
            _editBlockers.transform.SetAsFirstSibling();
        }

        _configButton.Item1.interactable = configBtn;
        _receiversButton.Item1.interactable = receiverBtn;
        _broadcastersButton.Item1.interactable = broadcasterBtn;
    }

    private static GameObject _configTab; 
    public static readonly List<(InputField, Action)> ConfigIds = []; 
    private static GameObject _broadcasterTab;
    private static GameObject _receiverTab;

    private static int _receiverCount;
    private static int _broadcasterCount;

    private static void SetupConfigTab(List<ConfigType> group)
    {
        (_configTab, _) = PrepareTab("Config Tab");
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
                if (!GameManager.instance.isPaused) CursorManager.NeedsRefresh = true;
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
        trans.SetParent(_canvasObj.transform, false);
        trans.SetAsFirstSibling();

        return (tab, trans);
    }

    private static GameObject _editBlockers;

    private static void SetupEditBlockers()
    {
        _editBlockers = new GameObject("Edit Blockers")
        {
            transform = { parent = _canvasObj.transform }
        };
        _editBlockers.RemoveOffset();
        
        SetupEditBlocker(new Vector2(0, 0.5f), 1150);
        SetupEditBlocker(new Vector2(1, 0.5f), 500);
    }

    private static void SetupEditBlocker(Vector2 anchors, float width)
    {
        var img = UIUtils.MakeImage("Edit Blocker", _editBlockers, Vector2.zero, 
            anchors, anchors, new Vector2(width, 10000));
        img.raycastTarget = false;
        img.sprite = UIUtils.Square;
        img.color = new Color(0.1f, 0.1f, 0.1f, 0.6f);
    }
    
    private static void RefreshConfigTabs(bool paused)
    {
        var show = paused || EditManager.ConfigOpen;
        
        _hotbar.SetActive(!show);
        
        _editBlockers.SetActive(EditManager.ConfigOpen && !paused);

        _universalOptions.SetActive(show);
        if (_configTab) _configTab.SetActive(show && _currentOption == AttributeType.Config);
        if (_broadcasterTab) _broadcasterTab.SetActive(show && _currentOption == AttributeType.Events);
        if (_receiverTab) _receiverTab.SetActive(show && _currentOption == AttributeType.Listeners);
    }

    private static void ToggleFavourite(int i, UIUtils.Label label)
    {
        var index = PageIndex * ITEMS_PER_PAGE + i;
        if (_categoryContents.Count <= index) return;
        switch (_categoryContents[index])
        {
            case PrefabObject:
                break;
            case PlaceableObject placeable when FavouritesCategory.ToggleFavourite(placeable):
                label.textComponent.text = FILLED_STAR;
                label.textComponent.color = Color.yellow;
                break;
            case PlaceableObject:
                label.textComponent.text = EMPTY_STAR;
                label.textComponent.color = Color.white;
                break;
            case SavedObject prefab:
                SavedCategory.RemovePrefab(prefab);
                DoRefreshCurrentPage();
                break;
        }
    }

    public static void RefreshItem()
    {
        RefreshItem(EditManager.HotbarIndex);
        
        _currentlySelected.textComponent.text = EditManager.CurrentObject.GetName();
        _currentlySelectedDesc.textComponent.text = EditManager.CurrentObject.GetDescription();

        ScaleText.enabled = !(EditManager.CurrentObject?.DisableTransformations ?? true);
        ZText.enabled = !(EditManager.CurrentObject?.DisableTransformations ?? true);
        RotationText.enabled = !(EditManager.CurrentObject?.DisableTransformations ?? true);
    }

    public static void RefreshItem(int index)
    {
        if (index == EditManager.ACTIVE_OBJECT_INDEX) return;
        
        var icon = HotbarIcons[index];
        icon.sprite = EditManager.HotbarCurrentObject[index].GetUISprite();
        var cfg = EditManager.HotbarConfig[index].Values.FirstOrDefault(c => c.GetTypeId() == "png_url");
        if (cfg != null)
        {
            CustomAssetManager.DoLoadSprite(cfg.SerializeValue(), true, 100, 1, 1, sprites =>
            {
                icon.sprite = sprites[0];
            });
        }

        var rot = 0f;
        icon.transform.SetScaleX(1.25f);
        icon.transform.SetScaleY(1.25f);

        if (EditManager.HotbarCurrentObject[index] is PlaceableObject placeable)
        {
            switch (placeable.GetUISprite().packingRotation)
            {
                case SpritePackingRotation.FlipHorizontal:
                    icon.transform.SetScaleX(-1.25f);
                    break;
                case SpritePackingRotation.FlipVertical:
                    icon.transform.SetScaleY(-1.25f);
                    break;
                case SpritePackingRotation.Rotate180:
                    rot += 180;
                    break;
            }

            rot += placeable.Rotation + placeable.ChildRotation + placeable.Tk2dRotation;
        }
        else icon.transform.localScale = new Vector3(1, 1, 1);

        icon.transform.SetRotationZ(rot);
    }

    private static int _refreshRoutineId;

    public static void DoRefreshCurrentPage()
    {
        _refreshRoutineId++;
        ArchitectPlugin.Instance.StartCoroutine(RefreshCurrentPage(_refreshRoutineId));
    }

    private static IEnumerator RefreshCurrentPage(int id)
    {
        _categoryContents = CurrentCategory.GetObjects().Where(obj => obj.GetName()
            .IndexOf(_currentSearch, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

        for (var i = 0; i < ITEMS_PER_PAGE; i++)
        {
            var index = PageIndex * ITEMS_PER_PAGE + i;
            var icon = GridIcons[i];
            if (_categoryContents.Count <= index)
            {
                icon.Item1.sprite = ArchitectPlugin.BlankSprite;
                icon.Item2.textComponent.text = "Unset";
                icon.Item2.textComponent.fontSize = 14;
                icon.Item3.textComponent.text = NOTHING;
            }
            else
            {
                var rot = 0f;
                
                PlaceableObject placeable;
                switch (_categoryContents[index])
                {
                    case PrefabObject obj:
                        placeable = obj;
                        icon.Item3.textComponent.text = "";
                        break;
                    case PlaceableObject obj:
                        placeable = obj;
                        var favourite = FavouritesCategory.IsFavourite(placeable);
                        icon.Item3.textComponent.text = favourite ? FILLED_STAR : EMPTY_STAR;
                        icon.Item3.textComponent.color = favourite ? Color.yellow : Color.white;
                        break;
                    case SavedObject prefab:
                        placeable = prefab.PlaceableObject;
                        icon.Item3.textComponent.text = BIN;
                        icon.Item3.textComponent.color = Color.red;
                        rot = prefab.Placement.GetRotation();
                        break;
                    default:
                        continue;
                }

                if (placeable is PreloadObject { Loaded: false })
                {
                    yield return placeable.EnsureLoaded();
                }

                if (id != _refreshRoutineId) yield break;
                
                icon.Item1.gameObject.SetActive(placeable is not PrefabObject);
                icon.Item1.sprite = _categoryContents[index].GetUISprite();

                var text = "";
                if (placeable is PrefabObject)
                {
                    icon.Item1.gameObject.SetActive(false);
                    icon.Item2.textComponent.fontSize = 32;

                    var name = placeable.GetName()[..^9].ToArray();
                    name[0] = char.ToUpper(name[0]);
                    text = string.Concat(name.Where(c => c is >= 'A' and <= 'Z' or >= '0' and <= '9'));
                    if (text.Length > 4) text = text[..4];
                }
                else
                {
                    icon.Item1.gameObject.SetActive(true);
                    icon.Item2.textComponent.fontSize = 14;
                }
                icon.Item2.textComponent.text = text;
                
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
        PageIndex += amount;

        if (_categoryContents == null) return;
        var num = (_categoryContents.Count - 1) / 9;
        if (PageIndex > num) PageIndex = 0;
        else if (PageIndex < 0) PageIndex = num;

        DoRefreshCurrentPage();
    }

    private static void SetupSearchBox()
    {
        var pos = new Vector3(-65, 131.25f);
        var anchor = new Vector2(1, 0);
        var txt = UIUtils.MakeTextbox("Search Box", _mapUI, pos, anchor, anchor,
            300, 32).Item1;
        txt.onValueChanged.AddListener(s =>
        {
            PageIndex = 0;
            _currentSearch = s;
            DoRefreshCurrentPage();
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
        PositionOptions = new GameObject("Position")
        {
            transform = { parent = _universalOptions.transform }
        };
        PositionOptions.SetActive(false);
        PositionOptions.RemoveOffset();
        
        var anchor = new Vector2(1, 0);
        (RotationText, var rl) = UIUtils.MakeTextbox("Rotation Box", _universalOptions, new Vector3(-65, 190)
            , anchor, anchor, 70, 32);
        rl.textComponent.raycastTarget = false;

        RotationText.characterValidation = InputField.CharacterValidation.Decimal;
        RotationText.onValueChanged.AddListener(s =>
        {
            try
            {
                EditManager.CurrentRotation = Convert.ToSingle(s, CultureInfo.InvariantCulture);
            }
            catch (FormatException) {}
            
            CursorManager.NeedsRefresh = true;
        });

        (ScaleText, var sl) = UIUtils.MakeTextbox("Scale Box", _universalOptions, new Vector3(-65, 210)
            , anchor, anchor, 70, 32);
        sl.textComponent.raycastTarget = false;

        ScaleText.characterValidation = InputField.CharacterValidation.Decimal;
        ScaleText.onValueChanged.AddListener(s =>
        {
            try
            {
                EditManager.CurrentScale = Convert.ToSingle(s, CultureInfo.InvariantCulture);
            }
            catch (FormatException) {}

            CursorManager.NeedsRefresh = true;
        });

        (ZText, var zl) = UIUtils.MakeTextbox("Offset Box", _universalOptions, new Vector3(-65, 170)
            , anchor, anchor, 70, 32);
        zl.textComponent.raycastTarget = false;

        ZText.characterValidation = InputField.CharacterValidation.Decimal;
        ZText.onValueChanged.AddListener(s =>
        {
            try
            {
                EditManager.CurrentZ = Convert.ToSingle(s, CultureInfo.InvariantCulture);
            }
            catch (FormatException) {}

            CursorManager.NeedsRefresh = true;
        });

        (PosXText, var pxl) = UIUtils.MakeTextbox("X Box", PositionOptions, new Vector3(-65, 137.5f)
            , anchor, anchor, 70, 32);
        pxl.textComponent.raycastTarget = false;

        PosXText.characterValidation = InputField.CharacterValidation.Decimal;
        PosXText.onValueChanged.AddListener(_ => CursorManager.NeedsRefresh = true);

        (PosYText, var pyl) = UIUtils.MakeTextbox("Y Box", PositionOptions, new Vector3(-65, 122.5f)
            , anchor, anchor, 70, 32);
        pyl.textComponent.raycastTarget = false;

        PosYText.characterValidation = InputField.CharacterValidation.Decimal;
        PosYText.onValueChanged.AddListener(_ => CursorManager.NeedsRefresh = true);

        var zLabel = UIUtils.MakeLabel("Z Label", _universalOptions, new Vector3(-75, 170), anchor, anchor);
        zLabel.textComponent.text = "Z Position: ";
        zLabel.textComponent.fontSize = 8;
        zLabel.textComponent.alignment = TextAnchor.MiddleLeft;
        zLabel.textComponent.raycastTarget = false;

        var rotLabel = UIUtils.MakeLabel("Rotation Label", _universalOptions, new Vector3(-75, 190), anchor, anchor);
        rotLabel.textComponent.text = "Rotation: ";
        rotLabel.textComponent.fontSize = 8;
        rotLabel.textComponent.alignment = TextAnchor.MiddleLeft;
        rotLabel.textComponent.raycastTarget = false;

        var scaleLabel = UIUtils.MakeLabel("Scale Label", _universalOptions, new Vector3(-75, 210), anchor, anchor);
        scaleLabel.textComponent.text = "Scale: ";
        scaleLabel.textComponent.fontSize = 8;
        scaleLabel.textComponent.alignment = TextAnchor.MiddleLeft;
        scaleLabel.textComponent.raycastTarget = false;

        var posLabel = UIUtils.MakeLabel("Pos Label", PositionOptions, new Vector3(-75, 130), anchor, anchor);
        posLabel.textComponent.text = "Position: ";
        posLabel.textComponent.fontSize = 8;
        posLabel.textComponent.alignment = TextAnchor.MiddleLeft;
        posLabel.textComponent.raycastTarget = false;

        EditManager.SetRotation(0);
        EditManager.SetScale(1);
        EditManager.SetZ(0);
    }

    private static void SetupEditorSettings()
    {
        _editorTypeButtons = new GameObject("Editor Type Buttons")
        {
            transform = { parent = _canvasObj.transform }
        };
        _editorTypeButtons.RemoveOffset();
        
        SetupModeButton(EditorType.Map, "Map Editor", new Vector3(-263.5f, 15));
        SetupModeButton(EditorType.Script, "Script Editor", new Vector3(0, 15));
        SetupModeButton(EditorType.Workshop, "Workshop", new Vector3(263.5f, 15));
    }
    
    private static void SetupModeButton(EditorType type, string name, Vector3 pos)
    {
        var size = new Vector2(765, 50);
        var (btn, label) = UIUtils.MakeTextButton(name + " Button", name, _editorTypeButtons, pos, 
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), size:size);
        label.textComponent.raycastTarget = false;
        
        btn.onClick.AddListener(() =>
        {
            CurrentType = type;
            Deletable.DeleteButton.SetActive(false);
        });
        label.textComponent.fontSize = 10;
        
        DisableWhenPlaying.Add(btn.gameObject);
        DisableWhenPlaying.Add(label.gameObject);
    }

    private static void SetupAttributeSettings()
    {
        _configTypeButtons = new GameObject("Config Type Buttons")
        {
            transform = { parent = _canvasObj.transform }
        };
        _configTypeButtons.RemoveOffset();
        
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
        var (btn, label) = UIUtils.MakeTextButton(name + " Button", name, _configTypeButtons, pos, 
            Vector2.zero, Vector2.zero, size:size);
        label.textComponent.raycastTarget = false;

        btn.onClick.AddListener(() => _currentOption = type);
        btn.interactable = false;
        
        DisableWhenPlaying.Add(btn.gameObject);
        DisableWhenPlaying.Add(label.gameObject);

        return (btn, label);
    }

    private static GameObject _hotbar;

    private static void SetupHotbar()
    {
        _hotbar = new GameObject("Hotbar")
        {
            transform = { parent = _canvasObj.transform }
        };
        _hotbar.SetActive(false);
        _hotbar.RemoveOffset();
        
        for (var i = -4; i < 5; i++)
        {
            var (btn, img, _) = UIUtils.MakeButtonWithImage("Hotbar Part", _hotbar,
                new Vector3(i * 45, 35), new Vector2(0.5f, 0), new Vector2(0.5f, 0), 
                96, 48);
            btn.enabled = false;

            img.sprite = ArchitectPlugin.BlankSprite;
            HotbarIcons.Add(img);
        }
    }

    public static Text LayerName;
    public static Text LayerToggle;

    private static void SetupLayers()
    {
        var layerParent = new GameObject("Layers")
        {
            transform = { parent = _modern.transform }
        };
        layerParent.RemoveOffset().anchoredPosition = new Vector2(0, -20);
        
        LayerName = UIUtils.MakeLabel("Layer Name", layerParent,
            new Vector2(0, 45), new Vector2(0.5f, 0), new Vector2(0.5f, 0)).textComponent;
        LayerName.fontSize = 14;
        LayerName.text = $"Layer\n{EditManager.Layer}";
        LayerName.alignment = TextAnchor.MiddleCenter;
        
        var (bsa, sa) = UIUtils.MakeTextButton("Show All", "Show All", layerParent,
            new Vector2(75, 55), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            size: new Vector2(120, 35));
        sa.textComponent.fontSize = 10;
        
        var (bha, ha) = UIUtils.MakeTextButton("Hide All", "Hide All", layerParent,
            new Vector2(75, 35), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            size: new Vector2(120, 35));
        ha.textComponent.fontSize = 10;
        
        bsa.onClick.AddListener(() =>
        {
            EditManager.ShowLayersByDefault = true;
            EditManager.FlippedLayers.Clear();
            EditManager.Layer = EditManager.Layer;
        });
        
        bha.onClick.AddListener(() =>
        {
            EditManager.ShowLayersByDefault = false;
            EditManager.FlippedLayers.Clear();
            EditManager.Layer = EditManager.Layer;
        });

        var vl = UIUtils.MakeLabel("Visible Label", layerParent,
            new Vector2(-75, 52.5f), new Vector2(0.5f, 0), new Vector2(0.5f, 0)).textComponent;
        vl.fontSize = 10;
        vl.text = "Always Show";
        vl.alignment = TextAnchor.MiddleCenter;
        
        var (vBtn, vt) = UIUtils.MakeTextButton("Visible", "X", layerParent,
            new Vector2(-75, 35), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            size: new Vector2(35, 35));
        vBtn.onClick.AddListener(() =>
        {
            if (!EditManager.FlippedLayers.Remove(EditManager.Layer))
                EditManager.FlippedLayers.Add(EditManager.Layer);
            EditManager.Layer = EditManager.Layer;
        });
        LayerToggle = vt.textComponent;
        LayerToggle.fontSize = 10;
        
        var (lBtn, lt) = UIUtils.MakeTextButton("Left", "<", layerParent,
            new Vector2(-35, 45), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            size: new Vector2(35, 35));
        lt.textComponent.fontSize = 10;
        lBtn.onClick.AddListener(() => EditManager.Layer = Math.Max(0, EditManager.Layer - 1));
        
        var (rBtn, rt) = UIUtils.MakeTextButton("Right", ">", layerParent,
            new Vector2(35, 45), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            size: new Vector2(35, 35));
        rt.textComponent.fontSize = 10;
        rBtn.onClick.AddListener(() => EditManager.Layer++);
        
        DisableWhenPlaying.Add(layerParent);
    }

    private enum AttributeType
    {
        Config,
        Events,
        Listeners
    }

    public enum EditorType
    {
        Map,
        Script,
        Workshop
    }
}