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
    public static Keybind SaveObject;
    public static Keybind LockAxis;
    public static Keybind Undo;
    public static Keybind Redo;
    public static Keybind MultiSelect;
    public static Keybind Copy;
    public static Keybind Paste;
    public static Keybind Preview;
    public static Keybind Overwrite;
    public static Keybind GrabId;
    public static Keybind StartLocked;
    public static Keybind StartScripted;
    
    public static Keybind Blank;
    public static Keybind Cursor;
    public static Keybind Drag;
    public static Keybind Eraser;
    public static Keybind Pick;
    public static Keybind Reset;
    public static Keybind Lock;
    public static Keybind TileChanger;

    public static ConfigEntry<bool> LegacyEventSystem;
    public static ConfigEntry<bool> TestMode;
    public static ConfigEntry<bool> ShowRespawnPoint;

    public static ConfigEntry<int> SaveSlot; 
    public static ConfigEntry<int> PreloadCount;
    
    public static ConfigEntry<Color> EditorBackgroundColour;

    public static void Init(ConfigFile config)
    {
        ToggleEditor = new Keybind(config.Bind(
            "Keybinds",
            "EditToggle",
            KeyCode.E,
            "Toggles Edit Mode"
        ));
        
        Rotate = new Keybind(config.Bind(
            "Keybinds",
            "Rotate",
            KeyCode.R,
            "Rotates the object on the cursor"
        ));
        
        UnsafeRotation = new Keybind(config.Bind(
            "Keybinds",
            "UnsafeRotation",
            KeyCode.LeftAlt,
            "Allows rotating objects at any angle"
        ));
        
        Flip = new Keybind(config.Bind(
            "Keybinds",
            "Flip",
            KeyCode.F,
            "Flips the object on the cursor"
        ));
        
        ScaleUp = new Keybind(config.Bind(
            "Keybinds",
            "ScaleUp",
            KeyCode.Equals,
            "Increases the scale of the object on the cursor"
        ));
        
        ScaleDown = new Keybind(config.Bind(
            "Keybinds",
            "ScaleDown",
            KeyCode.Minus,
            "Decreases the scale of the object on the cursor"
        ));
        
        SaveObject = new Keybind(config.Bind(
            "Keybinds",
            "SaveObject",
            KeyCode.Return,
            "Saves the object on the cursor as a saved object"
        ));
        
        LockAxis = new Keybind(config.Bind(
            "Keybinds",
            "LockAxis",
            KeyCode.RightShift,
            "Locks the current X or Y axis to the axis of the last placed object"
        ));
        
        Undo = new Keybind(config.Bind(
            "Keybinds",
            "Undo",
            KeyCode.Z,
            "Undoes the last action"
        ));
        
        Redo = new Keybind(config.Bind(
            "Keybinds",
            "Redo",
            KeyCode.Y,
            "Redoes the last action"
        ));
        
        MultiSelect = new Keybind(config.Bind(
            "Keybinds",
            "MultiSelect",
            KeyCode.LeftControl,
            "Allows selecting multiple objects with the Drag tool"
        ));
        
        Copy = new Keybind(config.Bind(
            "Keybinds",
            "Copy",
            KeyCode.C,
            "Copies the current selection of objects"
        ));
        
        Paste = new Keybind(config.Bind(
            "Keybinds",
            "Paste",
            KeyCode.V,
            "Pastes the objects on the clipboard"
        ));
        
        Preview = new Keybind(config.Bind(
            "Keybinds",
            "Preview",
            KeyCode.P,
            "Preview objects affected by the Object Anchor"
        ));
        
        Overwrite = new Keybind(config.Bind(
            "Keybinds",
            "Overwrite",
            KeyCode.O,
            "Overwrites a clicked object with the one on your cursor"
        ));
        
        GrabId = new Keybind(config.Bind(
            "Keybinds",
            "GrabId",
            KeyCode.I,
            "Sets the ID option of the object on the cursor to the selected object's ID"
        ));
        
        StartLocked = new Keybind(config.Bind(
            "Keybinds",
            "StartLocked",
            KeyCode.None,
            "Makes the placed object be locked instantly upon placing it"
        ));
        
        StartScripted = new Keybind(config.Bind(
            "Keybinds",
            "StartScripted",
            KeyCode.None,
            "Makes the placed object be added to the script instantly upon placing it"
        ));
        
        Blank = new Keybind(config.Bind(
            "ToolHotkeys",
            "Blank",
            KeyCode.None,
            "Clears your current selected item"
        ));
        
        Cursor = new Keybind(config.Bind(
            "ToolHotkeys",
            "Cursor",
            KeyCode.None,
            "Sets your current selected item to the Cursor tool"
        ));
        
        Drag = new Keybind(config.Bind(
            "ToolHotkeys",
            "Drag",
            KeyCode.None,
            "Sets your current selected item to the Drag tool"
        ));
        
        Eraser = new Keybind(config.Bind(
            "ToolHotkeys",
            "Eraser",
            KeyCode.None,
            "Sets your current selected item to the Eraser tool"
        ));
        
        Pick = new Keybind(config.Bind(
            "ToolHotkeys",
            "Pick",
            KeyCode.None,
            "Sets your current selected item to the Pick tool"
        ));
        
        Reset = new Keybind(config.Bind(
            "ToolHotkeys",
            "Reset",
            KeyCode.None,
            "Sets your current selected item to the Reset tool"
        ));
        
        TileChanger = new Keybind(config.Bind(
            "ToolHotkeys",
            "TilemapChanger",
            KeyCode.None,
            "Sets your current selected item to the Tilemap Changer tool"
        ));
        
        Lock = new Keybind(config.Bind(
            "ToolHotkeys",
            "Lock",
            KeyCode.None,
            "Locks an object in place so it cannot be edited until unlocked"
        ));
        
        TestMode = config.Bind(
            "Options",
            "TestMode",
            false,
            "Stops the game from storing persistent data in such as enemies being killed"
        );
        
        LegacyEventSystem = config.Bind(
            "Options",
            "LegacyEventSystem",
            false,
            "Enables the Legacy objects tab and the Events and Listeners tabs"
        );
        
        ShowRespawnPoint = config.Bind(
            "Options",
            "ShowRespawnPoint",
            false,
            "Adds an indicator showing your current hazard respawn point"
        );
        
        SaveSlot = config.Bind(
            "Options",
            "DownloadSlot",
            4,
            "The save slot to download save files from the level sharer into"
        );
        
        PreloadCount = config.Bind(
            "Options",
            "PreloadCount",
            4,
            "The maximum number of scenes that can be loaded at once during preloading"
        );

        EditorBackgroundColour = config.Bind(
            "Options",
            "EditorBackgroundColour",
            new Color(0.1f, 0.1f, 0.1f),
            "The background colour of the script editor"
        );
    }

    public class Keybind(ConfigEntry<KeyCode> code)
    {
        public bool IsPressed => Input.GetKey(code.Value);
        public bool WasPressed => Input.GetKeyDown(code.Value);
    }
}