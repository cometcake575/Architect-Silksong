using GlobalEnums;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Architect.Config;
using Architect.Config.Types;
using Architect.Content.Preloads;
using Architect.Multiplayer;
using Architect.Objects;
using Architect.Objects.Categories;
using Architect.Objects.Groups;
using Architect.Objects.Placeable;
using Architect.Objects.Tools;
using Architect.Placements;
using Architect.Storage;
using Architect.Utils;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Architect.Editor;

public static class EditManager
{
    public static bool IsEditing;
    public static bool ReloadRequired;
    public static bool IgnoreControlRelinquished;
    
    private static float _lastEditToggle;

    private static int _hotbarIndex;

    public static int HotbarIndex
    {
        get => _hotbarIndex;
        set
        {
            _hotbarIndex = value;

            EditorUI.WipeTabs();
            EditorUI.RefreshAttributeControls(false);
            EditorUI.RefreshItem();

            EditorUI.RotationText.text = CurrentRotation.ToString(CultureInfo.InvariantCulture);
            EditorUI.ScaleText.text = CurrentScale.ToString(CultureInfo.InvariantCulture);
            
            CursorManager.NeedsRefresh = true;
            CursorManager.ObjectChanged = true;
        }
    }

    private static readonly SelectableObject[] HotbarCurrentObject =
        [CursorObject.Instance, BlankObject.Instance, BlankObject.Instance,
        BlankObject.Instance, BlankObject.Instance, BlankObject.Instance,
        BlankObject.Instance, BlankObject.Instance, BlankObject.Instance];
    
    private static readonly bool[] HotbarCurrentlyFlipped = [false, false, false, false, false, false, false, false, false];
    private static readonly float[] HotbarCurrentRotation = [0, 0, 0, 0, 0, 0, 0, 0, 0];
    private static readonly float[] HotbarCurrentScale = [1, 1, 1, 1, 1, 1, 1, 1, 1];

    private static readonly List<(string, string, int)>[] HotbarReceivers = [[], [], [], [], [], [], [], [], []];
    private static readonly List<(string, string)>[] HotbarBroadcasters = [[], [], [], [], [], [], [], [], []];
    private static readonly Dictionary<string, ConfigValue>[] HotbarConfig = [[], [], [], [], [], [], [], [], []];
    
    public static SelectableObject CurrentObject
    {
        get => HotbarCurrentObject[_hotbarIndex];
        set => HotbarCurrentObject[_hotbarIndex] = value;
    }

    public static bool CurrentlyFlipped
    {
        get => HotbarCurrentlyFlipped[_hotbarIndex];
        set => HotbarCurrentlyFlipped[_hotbarIndex] = value;
    }

    public static float CurrentRotation
    {
        get => HotbarCurrentRotation[_hotbarIndex];
        set => HotbarCurrentRotation[_hotbarIndex] = value;
    }
    
    public static float CurrentScale
    {
        get => HotbarCurrentScale[_hotbarIndex];
        set => HotbarCurrentScale[_hotbarIndex] = value;
    }

    public static List<(string, string, int)> Receivers => HotbarReceivers[_hotbarIndex];
    public static List<(string, string)> Broadcasters => HotbarBroadcasters[_hotbarIndex];
    public static Dictionary<string, ConfigValue> Config => HotbarConfig[_hotbarIndex]; 
    
    private static Vector3 _posToLoad;
    private static bool _loadPos;

    private static string _lockArea;
    
    private static Vector3 _noclipPos;
    
    private static float _lastX;
    private static float _lastY;
    
    private static string _validLastPosScene;
    
    private static bool HasValidLastPos
    {
        get => _validLastPosScene == GameManager.instance.sceneName;
        set => _validLastPosScene = value ? GameManager.instance.sceneName : "";
    }

    public static ObjectPlacement HoveredObject;

    public static void Init()
    {
        typeof(GameManager).Hook("EnterHero", OnSceneLoad);
        
        typeof(InputHandler).Hook("SetCursorVisible", EnableCursor);
        
        typeof(QuitToMenu).Hook("Start", (Func<QuitToMenu, IEnumerator> orig, QuitToMenu self) =>
            {
                IsEditing = false;
                return orig(self); 
            });
        
        typeof(HeroController).Hook(nameof(HeroController.CanTakeDamage), BlockAction);
        
        typeof(HeroController).Hook(nameof(HeroController.CanTakeDamageIgnoreInvul), BlockAction);

        typeof(HeroController).Hook(nameof(HeroController.CanOpenInventory), BlockAction);

        typeof(HeroController).Hook(nameof(HeroController.CanDash), BlockAction);
        
        typeof(HeroController).Hook(nameof(HeroController.CanHarpoonDash), BlockAction);

        typeof(PersistentBoolItem).Hook("Awake", (Action<PersistentBoolItem> orig, PersistentBoolItem self) =>
        {
            if (self.gameObject.name.StartsWith("[Architect] ") && Settings.TestMode.Value)
            {
                self.OnGetSaveState += (out bool value) => value = false;
            }
            orig(self);
        });
        
        SetupGroupSelectionBox();
    }

    private static bool BlockAction(Func<HeroController, bool> orig, HeroController self) => !IsEditing && orig(self);

    public static void TryFindEmptySlot()
    {
        if (!Input.GetKey(KeyCode.LeftAlt)) return;
        
        if (CurrentObject == BlankObject.Instance) return;
        
        for (var i = _hotbarIndex; i < 9; i++)
        {
            if (HotbarCurrentObject[i] == BlankObject.Instance)
            {
                HotbarIndex = i;
                return;
            }
        }
        
        for (var i = 0; i < _hotbarIndex; i++)
        {
            if (HotbarCurrentObject[i] == BlankObject.Instance)
            {
                HotbarIndex = i;
                return;
            }
        }
    }

    public static void SetRotation(float rotation)
    {
        EditorUI.RotationText.text = (rotation % 360).ToString(CultureInfo.InvariantCulture);
        CursorManager.NeedsRefresh = true;
    }

    public static void SetScale(float scale)
    {
        EditorUI.ScaleText.text = Mathf.Max(scale, 0.1f).ToString(CultureInfo.InvariantCulture);
        CursorManager.NeedsRefresh = true;
    }
    
    public static void Update()
    {
        // Update UI
        var paused = GameManager.instance.isPaused;
        var actions = InputHandler.Instance.inputActions;

        if (!paused && (!HeroController.instance.controlReqlinquished || IgnoreControlRelinquished) &&
            !_loadPos && !HeroController.instance.cState.dead &&
            HeroController.instance.transitionState == HeroTransitionState.WAITING_TO_TRANSITION)
        {
            if (Settings.ToggleEditor.WasPressed) ToggleEditor();
            else if (ReloadRequired) ReloadScene();
        }

        EditorUI.RefreshVisibility(IsEditing, paused);

        if (paused)
        {
            var left = actions.Left.WasPressed;
            var right = actions.Right.WasPressed;
            if (left != right) EditorUI.Shift(right ? 1 : -1);
        }
        
        // Noclip
        if (IsEditing || _loadPos) DoNoclip(actions, paused);
        
        if (!IsEditing) return;
        
        PlayerData.instance.isInvincible = true;
        
        HeroController.instance.ResetHardLandingTimer();

        if (!paused)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) HotbarIndex = 0;
            else if (Input.GetKeyDown(KeyCode.Alpha2)) HotbarIndex = 1;
            else if (Input.GetKeyDown(KeyCode.Alpha3)) HotbarIndex = 2;
            else if (Input.GetKeyDown(KeyCode.Alpha4)) HotbarIndex = 3;
            else if (Input.GetKeyDown(KeyCode.Alpha5)) HotbarIndex = 4;
            else if (Input.GetKeyDown(KeyCode.Alpha6)) HotbarIndex = 5;
            else if (Input.GetKeyDown(KeyCode.Alpha7)) HotbarIndex = 6;
            else if (Input.GetKeyDown(KeyCode.Alpha8)) HotbarIndex = 7;
            else if (Input.GetKeyDown(KeyCode.Alpha9)) HotbarIndex = 8;

            foreach (var (keybind, index) in ToolObject.Keybinds)
            {
                if (keybind.WasPressed) EditorUI.SetItem(index);
            }
        }

        // Applying transformation and saving prefabs
        var clearHover = HoveredObject != null;
        if (CurrentObject is PlaceableObject placeable)
        {
            if (!paused) ApplyEditChanges(placeable);
            
            if (Settings.SavePrefab.WasPressed)
            {
                PrefabsCategory.AddPrefab(new PrefabObject(placeable.PreparePlacement(Vector3.zero)));
                if (EditorUI.CurrentCategory == PrefabsCategory.Instance) EditorUI.RefreshCurrentPage();
            }

            if (!paused && (Settings.Overwrite.IsPressed || Settings.GrabId.IsPressed))
            {
                var newObj = PlacementManager.FindObject(Input.mousePosition);
                if (HoveredObject != newObj)
                {
                    if (clearHover) HoveredObject.ClearColour();

                    newObj?.SetHoverColour();
                    HoveredObject = newObj;
                }

                clearHover = false;
            }
        }
        
        if (clearHover)
        {
            HoveredObject.ClearColour();
            HoveredObject = null;
        }

        // Checks left click input
        var b1 = Input.GetMouseButtonDown(0);
        var c1 = Input.GetMouseButtonDown(1);
        var b2 = Input.GetMouseButton(0);

        // Refresh the group selection box if group selection is active
        if (_groupSelecting) RefreshGroupSelection(!b2 || paused || Settings.Preview.IsPressed);

        // Checks if the object is the drag tool
        var dragObj = CurrentObject is DragObject;
        // If objects are selected
        if (SelectedObjects.Count > 0)
        {
            // If the selection should not persist, release it
            if (Settings.Preview.IsPressed || (CurrentObject is not EraserObject && !dragObj)) 
                StopIfDragging(true);
            
            // Only runs if actively dragging objects
            else if (_dragging)
            {
                // If left click was released, stop dragging, let go of objects if multi select not pressed
                if (!b2) StopIfDragging(!Settings.MultiSelect.IsPressed);
                // Stop dragging if the game is paused, and release objects
                else if (paused) StopIfDragging(true);
                // If the dragging should continue, update it
                else UpdateDragging();
            }
        }

        // Only runs if selecting the drag object and not currently dragging objects
        if (dragObj && !_dragging)
        {
            // Copy
            if (Settings.Copy.WasPressed) CopySelection();
            // Paste
            if (Settings.Paste.WasPressed) PasteToSelection();
        }
        
        // Reset room code
        if (!b2 || paused) ResetObject.RestartDelay();
        
        if (paused) return;
        
        // Click/release code based on input
        if (b1 || b2) CurrentObject.Click(Input.mousePosition, b1);
        else if (Input.GetMouseButtonUp(0)) CurrentObject.Release();
        if (c1) CurrentObject.RightClick(Input.mousePosition);
        
        // Undo/Redo code
        if (Settings.Undo.WasPressed) ActionManager.UndoLast();
        if (Settings.Redo.WasPressed) ActionManager.RedoLast();
    }

    // Placement is copied object, Vector3 is offset from cursor when copied
    private static readonly List<(ObjectPlacement, Vector3)> CopiedObjects = [];

    private static void CopySelection()
    {
        CopiedObjects.Clear();
        var worldPos = GetWorldPos(Input.mousePosition);
        foreach (var obj in SelectedObjects)
        {
            CopiedObjects.Add((obj, worldPos - obj.GetPos()));
        }
    }

    private static void PasteToSelection()
    {
        StopIfDragging(true);
        
        var wp = GetWorldPos(Input.mousePosition);

        var converts = new Dictionary<string, string>();
        foreach (var (obj, _) in CopiedObjects)
        {
            converts[obj.GetId()] = Guid.NewGuid().ToString()[..8];
        }

        List<ObjectPlacement> newPlacements = [];
        
        foreach (var (obj, offset) in CopiedObjects)
        {
            var updatedConfig = new List<ConfigValue>();

            foreach (var conf in obj.Config)
                switch (conf)
                {
                    case StringConfigValue value:
                    {
                        var val = value.GetValue();
                        updatedConfig.Add(converts.TryGetValue(val, out var convert)
                            ? ConfigurationManager.DeserializeConfigValue(conf.GetTypeId(), convert)
                            : conf);
                        break;
                    }
                    case IdConfigValue value:
                    {
                        var val = value.GetValue();
                        updatedConfig.Add(converts.TryGetValue(val, out var convert)
                            ? ConfigurationManager.DeserializeConfigValue(conf.GetTypeId(), convert)
                            : conf);
                        break;
                    }
                    default:
                        updatedConfig.Add(conf);
                        break;
                }

            newPlacements.Add(new ObjectPlacement(
                obj.GetPlacementType(),
                wp - offset,
                converts[obj.GetId()],
                obj.IsFlipped(),
                obj.GetRotation(),
                obj.GetScale(),
                false,
                obj.Broadcasters,
                obj.Receivers,
                updatedConfig.ToArray()
            ));
        }

        ActionManager.PerformAction(new PlaceObjects(newPlacements));
        foreach (var obj in newPlacements)
        {
            ToggleSelectedObject(obj, false);
        }
    }

    private static void EnableCursor(Action<InputHandler, bool> orig, InputHandler self, bool value)
    {
        if (IsEditing)
        {
            if (!GameManager.instance.isPaused && CurrentObject is PlaceableObject)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.None;
            }
            else orig(self, true);
            return;
        }
        
        orig(self, value);
    }

    private static void ApplyEditChanges(PlaceableObject placeable)
    {
        if (Settings.Flip.WasPressed)
        {
            CurrentlyFlipped = !CurrentlyFlipped;
            CursorManager.NeedsRefresh = true;
        }
        if (Settings.ScaleUp.IsPressed) SetScale(CurrentScale + Time.deltaTime);
        if (Settings.ScaleDown.IsPressed) SetScale(CurrentScale - Time.deltaTime);

        var group = placeable.GetRotationGroup();
        if (Settings.UnsafeRotation.IsPressed) group = RotationGroup.All;

        float i;
        if (group == RotationGroup.All && Settings.Rotate.IsPressed) i = Time.deltaTime * 60;
        else if (Settings.Rotate.WasPressed)
            i = group switch
            {
                RotationGroup.Vertical => 180,
                RotationGroup.Three => 90,
                RotationGroup.Four => 90,
                RotationGroup.Eight => 45,
                _ => 0
            };
        else return;
        if (Input.GetKey(KeyCode.LeftShift)) i = -i;

        var rot = CurrentRotation + i;
        if (group == RotationGroup.Three && Mathf.Approximately(rot, 180)) rot = 270;
        SetRotation(rot);
    }

    private static void ToggleEditor()
    {
        if (!PreloadManager.HasPreloaded) return;
        if (Time.time - _lastEditToggle < 1) return;

        IgnoreControlRelinquished = false;
        
        _noclipPos = HeroController.instance.transform.position;
        
        if (IsEditing && _dragging) StopIfDragging(true);
        if (IsEditing && Input.GetMouseButton(0)) CurrentObject.Release();

        _lastEditToggle = Time.time;
        IsEditing = !IsEditing;
        if (!IsEditing) PlayerData.instance.isInvincible = false;
        else
        {
            ScreenFaderUtils.SetColour(Color.clear);
            GameCameras.instance.HUDIn();
        }
        ReloadScene();
        
        StorageManager.SaveScene(GameManager.instance.sceneName, PlacementManager.GetLevelData());
        StorageManager.SaveScene(StorageManager.GLOBAL, PlacementManager.GetGlobalData());
    }
    
    #region Scene Reloading
    private static void OnSceneLoad(Action<GameManager> orig, GameManager self)
    {
        if (_loadPos)
        {
            self.entryGateName = null;
            HeroController.instance.transform.position = _posToLoad;
            _loadPos = false;
            _noclipPos = _posToLoad;

            if (CoopManager.Instance.IsActive()) CoopManager.Instance.RefreshRoom();
        }

        if (HoveredObject != null)
        {
            HoveredObject.ClearColour();
            HoveredObject = null;
        }
        
        SelectedObjects.Clear();
        _groupSelecting = false;
        _groupSelectionBox.gameObject.SetActive(false);

        orig(self);
        
        if (_lockArea != null)
        {
            var areaObj = ObjectUtils
                .GetGameObjectFromArray(SceneManager.GetActiveScene().GetRootGameObjects(), _lockArea);
            _lockArea = null;
            if (!areaObj) return;
            var area = areaObj.GetComponent<CameraLockArea>();
            if (area) GameCameras.instance.cameraController.LockToArea(area);
        }
    }

    // Reloads the current scene, in order to refresh objects or update edit mode
    // Stores the player's current position in _posToLoad in order to keep them in the same place
    private static void ReloadScene()
    {
        ReloadRequired = false;
        IgnoreControlRelinquished = false;
        
        _lockArea = GameCameras.instance.cameraController.CurrentLockArea?.transform.GetPath();
        
        _loadPos = true;
        _posToLoad = HeroController.instance.transform.position;
        GameManager.instance.SaveLevelState();
        GameManager.instance.LoadScene(GameManager.instance.sceneName);
    }
    #endregion
    
    private static void DoNoclip(HeroActions actions, bool paused)
    {
        var up = actions.Up.IsPressed;
        var down = actions.Down.IsPressed;
        var left = actions.Left.IsPressed;
        var right = actions.Right.IsPressed;

        var speed = actions.Dash.IsPressed ? 35 : 20;
        if (!_loadPos)
        {
            if (!paused && up != down) _noclipPos += (up ? Vector3.up : Vector3.down) * (Time.deltaTime * speed);
            if (!paused && left != right) _noclipPos += (left ? Vector3.left : Vector3.right) * (Time.deltaTime * speed);
        }

        if (HeroController.instance.transitionState == HeroTransitionState.WAITING_TO_TRANSITION)
            HeroController.instance.transform.position = _noclipPos;
        else _noclipPos = HeroController.instance.transform.position;
    }

    public static void RegisterLastPos(Vector2 pos)
    {
        _lastX = pos.x;
        _lastY = pos.y;
        HasValidLastPos = true;
    }

    public static Vector3 GetWorldPos(Vector3 mousePosition, bool lockAxisApplies = false, float offset = 0)
    {
        var cam = GameCameras.instance.cameraController.cam;
        mousePosition.z = -cam.transform.position.z + offset;
        var pos = cam.ScreenToWorldPoint(mousePosition);
        
        if (lockAxisApplies && Settings.LockAxis.IsPressed && HasValidLastPos)
        {
            if (Math.Abs(_lastX - pos.x) > Math.Abs(_lastY - pos.y)) pos.y = _lastY;
            else pos.x = _lastX;
        }

        return pos;
    }

    public static void ClearAttributes()
    {
        Broadcasters.Clear();
        Receivers.Clear();
        Config.Clear();
    }

    public static readonly List<ObjectPlacement> SelectedObjects = [];

    public static void ToggleSelectedObject(ObjectPlacement placement, bool startDragging)
    {
        if (Settings.Preview.IsPressed) return;
        
        if (SelectedObjects.Contains(placement))
        {
            SelectedObjects.Remove(placement);
            placement.ClearColour();
            return;
        }
        
        SelectedObjects.Add(placement);
        placement.SetDraggedColour();
        
        if (startDragging) BeginDragging();
    }
    
    private static bool _dragging;
    
    private static bool _groupSelecting;
    private static Vector3 _groupSelectionCorner;
    private static GroupSelectionBox _groupSelectionBox;

    private static readonly int Color1 = Shader.PropertyToID("_Color");

    public static void StartGroupSelect()
    {
        _groupSelectionCorner = GetWorldPos(Input.mousePosition);
        _groupSelecting = true;

        _groupSelectionBox.gameObject.SetActive(true);
        _groupSelectionBox.transform.position = _groupSelectionCorner;
        _groupSelectionBox.width = 0;
        _groupSelectionBox.height = 0;
        _groupSelectionBox.UpdateOutline();
    }

    private static void StopGroupSelect(Vector3 worldPos)
    {
        _groupSelectionBox.gameObject.SetActive(false);

        foreach (var obj in PlacementManager.GetLevelData().Placements
                     .Where(obj => obj.IsWithinZone(_groupSelectionCorner, worldPos)))
            ToggleSelectedObject(obj, false);

        _groupSelecting = false;
    }
    
    private static void RefreshGroupSelection(bool release)
    {
        var wp = GetWorldPos(Input.mousePosition);
        if (release)
        {
            StopGroupSelect(wp);
            return;
        }

        _groupSelectionBox.width = wp.x - _groupSelectionCorner.x;
        _groupSelectionBox.height = wp.y - _groupSelectionCorner.y;
        _groupSelectionBox.UpdateOutline();
    }

    public static void BeginDragging()
    {
        _dragging = true;

        var worldPos = GetWorldPos(Input.mousePosition);
        foreach (var obj in SelectedObjects)
        {
            obj.StartMove(worldPos);
        }
    }

    private static void UpdateDragging()
    {
        var worldPos = GetWorldPos(Input.mousePosition);
        foreach (var obj in SelectedObjects) obj.Move(worldPos);
    }

    public static void StopIfDragging(bool release)
    {
        // Placement, new pos, old pos
        List<(ObjectPlacement, Vector3, Vector3)> movements = [];

        foreach (var obj in SelectedObjects)
        {
            movements.Add((obj, obj.GetPos(), obj.FinishMove()));
            if (release) obj.ClearColour();
        }

        if (_dragging)
        {
            ActionManager.PerformAction(new MoveObjects(movements));
            _dragging = false;
        }

        if (release) SelectedObjects.Clear();
    }

    private static void SetupGroupSelectionBox()
    {
        var box = new GameObject("[Architect] Group Selection Box");
        Object.DontDestroyOnLoad(box);
        box.SetActive(false);

        var particleMaterial = new Material(Shader.Find("Sprites/Default"));
        particleMaterial.SetColor(Color1, new Color(0, 1, 0, 0.3f));
        box.AddComponent<LineRenderer>().material = particleMaterial;

        _groupSelectionBox = box.AddComponent<GroupSelectionBox>();
    }
}