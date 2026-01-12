using System;
using System.Collections;
using Architect.Content.Preloads;
using Architect.Placements;
using Architect.Storage;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Architect.Utils;

public static class SceneUtils
{
    private static GameObject _sceneManager;
    private static GameObject _borderPrefab;

    public static void Init()
    {
        _borderPrefab = new GameObject("[Architect] Border Replacement");
        _borderPrefab.SetActive(false);
        Object.DontDestroyOnLoad(_borderPrefab);
        
        PreloadManager.RegisterPreload(new BasicPreload("Arborium_09", "_SceneManager", o =>
        {
            o.name = "[Architect] Scene Manager Preload";
            _sceneManager = o;
        }));
        
        typeof(GameManager).Hook(nameof(GameManager.LoadScene),
            (Action<GameManager, string> orig, GameManager self, string destScene) =>
            {
                if (destScene.StartsWith("Architect_"))
                {
                    StorageManager.SaveScene(destScene, PlacementManager.GetLevelData());
                    StorageManager.SaveScene(StorageManager.GLOBAL, PlacementManager.GetGlobalData());
                    
                    PersistentAudioManager.OnLeaveScene();
                    PersistentAudioManager.QueueSceneEntry();
                    self.startedOnThisScene = false;
                    self.nextSceneName = destScene;
                    self.LastSceneLoad = new SceneLoad(self, new GameManager.SceneLoadInfo
                    {
                        SceneName = destScene
                    });
                    ArchitectPlugin.Instance.StartCoroutine(LoadScene(destScene));
                    return;
                }
                orig(self, destScene);
            });
        
        // Not working
        typeof(ScenePreloader).Hook(nameof(ScenePreloader.SpawnPreloader),
            (Action<string, LoadSceneMode> orig, string sceneName, LoadSceneMode mode) =>
            {
                if (sceneName.StartsWith("Architect_"))
                {
                    ArchitectPlugin.Instance.StartCoroutine(LoadScene(sceneName));
                    return;
                }
                orig(sceneName, mode);
            });
    }

    public static IEnumerator LoadScene(string sceneName)
    {
        var current = GameManager.instance.sceneName;

        if (current == sceneName)
        {
            var temp = SceneManager.CreateScene("Temp");
            SceneManager.SetActiveScene(temp);
            yield return SceneManager.UnloadSceneAsync(current);
            current = "Temp";
        }
        
        var scene = SceneManager.CreateScene(sceneName);
        SceneManager.MoveGameObjectToScene(CreateSceneManager(), scene);
        SceneManager.SetActiveScene(scene);

        yield return SceneManager.UnloadSceneAsync(current);
    }

    public static GameObject CreateSceneManager()
    {
        var sm = Object.Instantiate(_sceneManager);
        sm.name = "_SceneManager";
        sm.GetComponent<CustomSceneManager>().borderPrefab = _borderPrefab;
        sm.AddComponent<HazardRespawnMarker>();
        sm.SetActive(true);
        return sm;
    }
}