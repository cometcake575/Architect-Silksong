using BepInEx.Configuration;
using UnityEngine;

namespace Architect.Storage;

public static class Settings
{
    public static Keybind ToggleEditor;
    public static Keybind Rotate;
    public static Keybind UnsafeRotation;
    public static Keybind Flip;
    public static Keybind ScaleUp;
    public static Keybind ScaleDown;
    public static Keybind SavePrefab;
    public static Keybind LockAxis;
    public static Keybind Undo;
    public static Keybind Redo;
    public static Keybind MultiSelect;
    public static Keybind Copy;
    public static Keybind Paste;
    public static Keybind Preview;
    public static Keybind Overwrite;
    
    public static Keybind Blank;
    public static Keybind Cursor;
    public static Keybind Drag;
    public static Keybind Eraser;
    public static Keybind Pick;
    public static Keybind Reset;
    public static Keybind Lock;
    public static Keybind TileChanger;

    public static bool TestMode;
    public static bool ShowRespawnPoint;

    public static void Init(ConfigFile config)
    {
        ToggleEditor = new Keybind(config.Bind(
            "Keybinds",
            "EditToggle",
            KeyCode.E,
            "Toggles Edit Mode"
        ).Value);
        
        Rotate = new Keybind(config.Bind(
            "Keybinds",
            "Rotate",
            KeyCode.R,
            "Rotates the object on the cursor"
        ).Value);
        
        UnsafeRotation = new Keybind(config.Bind(
            "Keybinds",
            "UnsafeRotation",
            KeyCode.LeftAlt,
            "Allows rotating objects at any angle"
        ).Value);
        
        Flip = new Keybind(config.Bind(
            "Keybinds",
            "Flip",
            KeyCode.F,
            "Flips the object on the cursor"
        ).Value);
        
        ScaleUp = new Keybind(config.Bind(
            "Keybinds",
            "ScaleUp",
            KeyCode.Equals,
            "Increases the scale of the object on the cursor"
        ).Value);
        
        ScaleDown = new Keybind(config.Bind(
            "Keybinds",
            "ScaleDown",
            KeyCode.Minus,
            "Decreases the scale of the object on the cursor"
        ).Value);
        
        SavePrefab = new Keybind(config.Bind(
            "Keybinds",
            "SavePrefab",
            KeyCode.Return,
            "Saves the object on the cursor as a prefab"
        ).Value);
        
        LockAxis = new Keybind(config.Bind(
            "Keybinds",
            "LockAxis",
            KeyCode.RightShift,
            "Locks the current X or Y axis to the axis of the last placed object"
        ).Value);
        
        Undo = new Keybind(config.Bind(
            "Keybinds",
            "Undo",
            KeyCode.Z,
            "Undoes the last action"
        ).Value);
        
        Redo = new Keybind(config.Bind(
            "Keybinds",
            "Redo",
            KeyCode.Y,
            "Redoes the last action"
        ).Value);
        
        MultiSelect = new Keybind(config.Bind(
            "Keybinds",
            "MultiSelect",
            KeyCode.LeftControl,
            "Allows selecting multiple objects with the Drag tool"
        ).Value);
        
        Copy = new Keybind(config.Bind(
            "Keybinds",
            "Copy",
            KeyCode.C,
            "Copies the current selection of objects"
        ).Value);
        
        Paste = new Keybind(config.Bind(
            "Keybinds",
            "Paste",
            KeyCode.V,
            "Pastes the objects on the clipboard"
        ).Value);
        
        Preview = new Keybind(config.Bind(
            "Keybinds",
            "Preview",
            KeyCode.P,
            "Preview objects affected by the Object Anchor"
        ).Value);
        
        Overwrite = new Keybind(config.Bind(
            "Keybinds",
            "Overwrite",
            KeyCode.O,
            "Overwrites a clicked object with the one on your cursor"
        ).Value);
        
        Blank = new Keybind(config.Bind(
            "ToolHotkeys",
            "Blank",
            KeyCode.None,
            "Clears your current selected item"
        ).Value);
        
        Cursor = new Keybind(config.Bind(
            "ToolHotkeys",
            "Cursor",
            KeyCode.None,
            "Sets your current selected item to the Cursor tool"
        ).Value);
        
        Drag = new Keybind(config.Bind(
            "ToolHotkeys",
            "Drag",
            KeyCode.None,
            "Sets your current selected item to the Drag tool"
        ).Value);
        
        Eraser = new Keybind(config.Bind(
            "ToolHotkeys",
            "Eraser",
            KeyCode.None,
            "Sets your current selected item to the Eraser tool"
        ).Value);
        
        Pick = new Keybind(config.Bind(
            "ToolHotkeys",
            "Pick",
            KeyCode.None,
            "Sets your current selected item to the Pick tool"
        ).Value);
        
        Reset = new Keybind(config.Bind(
            "ToolHotkeys",
            "Reset",
            KeyCode.None,
            "Sets your current selected item to the Reset tool"
        ).Value);
        
        TileChanger = new Keybind(config.Bind(
            "ToolHotkeys",
            "TilemapChanger",
            KeyCode.None,
            "Sets your current selected item to the Tilemap Changer tool"
        ).Value);
        
        Lock = new Keybind(config.Bind(
            "ToolHotkeys",
            "Lock",
            KeyCode.None,
            "Locks an object in place so it cannot be edited until unlocked"
        ).Value);
        
        TestMode = config.Bind(
            "Options",
            "TestMode",
            false,
            "Stops the game from storing persistent data in such as enemies being killed"
        ).Value;
        
        ShowRespawnPoint = config.Bind(
            "Options",
            "ShowRespawnPoint",
            false,
            "Adds an indicator showing your current hazard respawn point"
        ).Value;
    }

    public class Keybind(KeyCode code)
    {
        public bool IsPressed => Input.GetKey(code);
        public bool WasPressed => Input.GetKeyDown(code);
    }
}