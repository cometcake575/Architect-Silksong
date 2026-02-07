using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using QuickWarp;
using UnityEngine;

namespace Architect.Utils;

public static class QuickWarpHook
{
    private static QuickWarpGUI _gui;
    private static FieldInfo _areaNames;
    private static FieldInfo _scenesByArea;
    private static FieldInfo _transitionsByScene;
    private static FieldInfo _respawnsByScene;

    public static bool Init()
    {
        _gui = Resources.FindObjectsOfTypeAll<QuickWarpGUI>().FirstOrDefault();
        if (!_gui) return false;

        _areaNames = typeof(QuickWarpGUI).GetField("_areaNames",
            BindingFlags.NonPublic | BindingFlags.Instance);
        _scenesByArea = typeof(Warp).GetField("_scenes_by_area", 
            BindingFlags.NonPublic | BindingFlags.Static);
        _transitionsByScene = typeof(Warp).GetField("_transitions_by_scene", 
            BindingFlags.NonPublic | BindingFlags.Static);
        _respawnsByScene = typeof(Warp).GetField("_respawns_by_scene", 
            BindingFlags.NonPublic | BindingFlags.Static);
        
        return true;
    }

    private static void RegisterGroup(string groupName)
    {
        var names = ((string[])_areaNames.GetValue(_gui)).ToList();
        var scenesByArea = (Dictionary<string, List<string>>)_scenesByArea.GetValue(null);

        scenesByArea[groupName] = [];
        
        names.Add(groupName);
        _areaNames.SetValue(_gui, names.ToArray());
    }

    private static void UnregisterGroup(string groupName)
    {
        var names = ((string[])_areaNames.GetValue(_gui)).ToList();
        names.Remove(groupName);
        var scenesByArea = (Dictionary<string, List<string>>)_scenesByArea.GetValue(null);

        scenesByArea.Remove(groupName);
        
        _areaNames.SetValue(_gui, names.ToArray());
    }

    public static void RegisterScene(string groupName, string sceneName)
    {
        var scenesByArea = (Dictionary<string, List<string>>)_scenesByArea.GetValue(null);
        if (!scenesByArea.ContainsKey(groupName)) RegisterGroup(groupName);
        scenesByArea[groupName].Add(sceneName);
        ((Dictionary<string, List<string>>)_transitionsByScene.GetValue(null))[sceneName] = ["_SceneManager"];
        ((Dictionary<string, List<string>>)_respawnsByScene.GetValue(null))[sceneName] = [];
    }

    public static void UnregisterScene(string groupName, string sceneName)
    {
        var scenesByArea = (Dictionary<string, List<string>>)_scenesByArea.GetValue(null);
        if (scenesByArea.ContainsKey(groupName))
        {
            scenesByArea[groupName].Remove(sceneName);
            if (scenesByArea[groupName].IsNullOrEmpty()) UnregisterGroup(groupName);
        }
    }
}