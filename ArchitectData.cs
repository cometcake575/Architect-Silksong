using System.Collections.Generic;
using BepInEx;
using UnityEngine;

namespace Architect;

public class ArchitectData
{
    public static ArchitectData Instance => 
        ArchitectPlugin.Instance.SaveData ?? (ArchitectPlugin.Instance.SaveData = new ArchitectData());

    public Dictionary<string, string> StringVariables = [];
    public Dictionary<string, float> FloatVariables = [];
    public Dictionary<string, bool> BoolVariables = [];

    public string CustomNeedle = string.Empty;
}

public class GlobalArchitectData
{
    public static GlobalArchitectData Instance => 
        ArchitectPlugin.Instance.GlobalData ?? (ArchitectPlugin.Instance.GlobalData = new GlobalArchitectData());
    
    public Dictionary<string, KeyCode> Keybinds = [];

    public string CurrentMap = "";

    public string MapLabel => CurrentMap.IsNullOrWhiteSpace() ? "Map Keybinds" : CurrentMap;
}
