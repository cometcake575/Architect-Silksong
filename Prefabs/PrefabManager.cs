using System;
using System.Collections;
using System.Collections.Generic;
using Architect.Content.Preloads;
using Architect.Editor;
using Architect.Events.Blocks;
using Architect.Placements;
using Architect.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Architect.Prefabs;

public static class PrefabManager
{
    public static readonly Dictionary<string, LevelData> Prefabs = [];
    public static bool InPrefabScene;
    public static string Last;

    public static readonly Sprite PrefabIcon = ResourceUtils.LoadSpriteResource("prefab");
    
    private static string _oldScene;
    private static Vector3 _oldPos;
    
    private static GameObject _sceneManager;
    
    public static void Init()
    {
        PrefabObject.Init();
        
        PreloadManager.RegisterPreload(new BasicPreload("Arborium_09", "_SceneManager", o =>
        {
            o.name = "[Architect] Scene Manager Preload";
            _sceneManager = o;
        }));
        
        typeof(GameManager).Hook(nameof(GameManager.LoadGame),
            (Action<GameManager, int, Action<bool>> orig, GameManager self, int saveSlot, Action<bool> callback) =>
            {
                InPrefabScene = false;
                ScriptEditorUI.ToggleParent.SetActive(true);
                orig(self, saveSlot, callback);
            });
    }
    
    public static void Toggle(string prefabName)
    {
        Last = prefabName;
        if (GameManager.instance.isPaused)
        {
            GameManager.instance.StartCoroutine(GameManager.instance.PauseGameToggle(false));
            GameManager.instance.SetPausedState(false);
        }

        GameManager.instance.entryGateName = "";
        if (InPrefabScene)
        {
            ScriptEditorUI.ToggleParent.SetActive(true);
            EditManager.NoclipPos = _oldPos;
            GameManager.instance.LoadScene(_oldScene);
        }
        else
        {
            ScriptManager.IsLocal = true;
            ScriptEditorUI.ToggleParent.SetActive(false);
            _oldScene = GameManager.instance.sceneName;
            _oldPos = HeroController.instance.transform.position;
            ArchitectPlugin.Instance.StartCoroutine(LoadScene($"Prefab_{prefabName}"));
        }

        InPrefabScene = !InPrefabScene;
    }

    private static IEnumerator LoadScene(string sceneName)
    {
        var current = GameManager.instance.sceneName;
        
        var scene = SceneManager.CreateScene(sceneName);
        SceneManager.SetActiveScene(scene);

        var unload2 =  SceneManager.UnloadSceneAsync(current);
        if (unload2 != null) while (!unload2.isDone) yield return null;

        var sm = Object.Instantiate(_sceneManager);
        sm.transform.position = new Vector3(100, 100, -5);
        sm.transform.localScale = new Vector3(0.05f, 0.05f);

        var sr = sm.AddComponent<SpriteRenderer>();
        sr.sprite = UIUtils.Square;
        
        sm.name = "_SceneManager";
        sm.GetComponent<CustomSceneManager>().borderPrefab = new GameObject("[Architect] Border Replacement");
        
        sm.AddComponent<HazardRespawnMarker>();
        
        sm.SetActive(true);
        
        EditManager.NoclipPos = new Vector2(100, 100);
        HeroController.instance.transform.SetPosition2D(new Vector3(100, 100));
        GameCameras.instance.mainCamera.transform.SetPosition2D(100, 100);
        GameCameras.instance.cameraTarget.transform.SetPosition2D(100, 100);
    }
}