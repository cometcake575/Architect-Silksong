using System.Collections;
using Architect.Content.Preloads;
using Architect.Editor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Architect.Prefabs;

public static class PrefabManager
{
    public static bool InPrefabScene;
    private static string _oldScene;
    private static Vector3 _oldPos;
    
    private static GameObject _sceneManager;
    
    public static void Init()
    {
        PreloadManager.RegisterPreload(new BasicPreload("Arborium_09", "_SceneManager", o =>
        {
            o.name = "[Architect] Scene Manager Preload";
            _sceneManager = o;
        }));
    }
    
    public static void Toggle()
    {
        if (GameManager.instance.isPaused)
        {
            GameManager.instance.StartCoroutine(GameManager.instance.PauseGameToggle(false));
            GameManager.instance.SetPausedState(false);
        }

        GameManager.instance.entryGateName = "";
        if (InPrefabScene)
        {
            EditManager.NoclipPos = _oldPos;
            GameManager.instance.LoadScene(_oldScene);
        }
        else
        {
            _oldScene = GameManager.instance.sceneName;
            _oldPos = HeroController.instance.transform.position;
            EditManager.NoclipPos = new Vector2(100, 100);
            HeroController.instance.transform.position = new Vector3(100, 100);
            GameCameras.instance.mainCamera.transform.position = new Vector3(100, 100);
            GameCameras.instance.cameraTarget.transform.position = new Vector3(100, 100);
            ArchitectPlugin.Instance.StartCoroutine(LoadScene("Architect_Prefab"));
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
        
        sm.name = "_SceneManager";
        sm.GetComponent<CustomSceneManager>().borderPrefab = new GameObject("[Architect] Border Replacement");
        
        sm.AddComponent<HazardRespawnMarker>();
        
        sm.SetActive(true);
    }
}