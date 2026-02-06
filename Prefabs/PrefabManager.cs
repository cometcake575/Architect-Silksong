using System;
using System.Collections;
using System.Collections.Generic;
using Architect.Editor;
using Architect.Events.Blocks;
using Architect.Placements;
using Architect.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Architect.Prefabs;

public static class PrefabManager
{
    public static readonly Dictionary<string, LevelData> Prefabs = [];
    public static bool InPrefabScene;
    public static string Last;

    public static readonly Sprite PrefabIcon = ResourceUtils.LoadSpriteResource("prefab", ppu: 256);
    
    private static string _oldScene;
    private static Vector3 _oldPos;

    public static void Init()
    {
        PrefabObject.Init();
        
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
        
        InPrefabScene = !InPrefabScene;
        
        GameManager.instance.entryGateName = "";
        if (!InPrefabScene)
        {
            ScriptEditorUI.ToggleParent.SetActive(true);
            EditManager.NoclipPos = _oldPos;
            GameManager.instance.LoadScene(_oldScene);
        }
        else
        {
            foreach (var obj in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                foreach (var gate in obj.GetComponentsInChildren<TransitionPoint>()) 
                    gate.gameObject.SetActive(false);
            }
            ScriptManager.IsLocal = true;
            ScriptEditorUI.ToggleParent.SetActive(false);
            _oldScene = GameManager.instance.sceneName;
            _oldPos = HeroController.instance.transform.position;
            ArchitectPlugin.Instance.StartCoroutine(LoadScene($"Prefab_{prefabName}"));
        }
    }

    private static IEnumerator LoadScene(string sceneName)
    {
        var current = GameManager.instance.sceneName;
        
        var scene = SceneManager.CreateScene(sceneName);
        SceneManager.SetActiveScene(scene);

        var unload2 = SceneManager.UnloadSceneAsync(current);
        if (unload2 != null) while (!unload2.isDone) yield return null;

        var sm = SceneUtils.CreateSceneManager();
        
        sm.transform.position = new Vector3(100, 100, 1);
        sm.transform.localScale = Vector3.one;

        var sr = sm.AddComponent<SpriteRenderer>();
        sr.sprite = PrefabIcon;

        EditManager.NoclipPos.x = 100;
        EditManager.NoclipPos.y = 100;
        HeroController.instance.transform.SetPosition2D(new Vector2(100, 100));
        GameCameras.instance.mainCamera.transform.SetPosition2D(100, 100);
        GameCameras.instance.cameraTarget.transform.SetPosition2D(100, 100);
        
        GameCameras.instance.cameraTarget.SceneInit();
    }
}