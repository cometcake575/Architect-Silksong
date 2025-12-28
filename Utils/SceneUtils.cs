using System;
using System.Collections;
using Architect.Content.Preloads;
using TeamCherry.Localization;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Architect.Utils;

public static class SceneUtils
{
    private static GameObject _sceneManager;
    
    public static void Init()
    {
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
                    ArchitectPlugin.Instance.StartCoroutine(CreateScene(destScene));
                } else orig(self, destScene);
            });
        
        typeof(SaveSlotBackgrounds).Hook(nameof(SaveSlotBackgrounds.GetBackground),
            (Func<SaveSlotBackgrounds, SaveStats, SaveSlotBackgrounds.AreaBackground> orig, SaveSlotBackgrounds self,
                SaveStats stats) =>
            {
                var o = orig(self, stats);
                return new SaveSlotBackgrounds.AreaBackground
                {
                    BackgroundImage = o.BackgroundImage,
                    Act3BackgroundImage = o.Act3BackgroundImage,
                    NameOverride = new LocalisedString
                    {
                        Key = "UwU",
                        Sheet = "ArchitectMod"
                    },
                    Act3OverlayOptOut = o.Act3OverlayOptOut
                };
            }, typeof(SaveStats));
    }

    private static IEnumerator CreateScene(string sceneName)
    {
        var current = GameManager.instance.sceneName;

        if (current == sceneName)
        {
            GameManager.instance.LoadScene("Belltown");
            current = "Belltown";
            yield return new WaitForSeconds(10);
        }
        var scene = SceneManager.CreateScene(sceneName);
        SceneManager.SetActiveScene(scene);

        var unload2 =  SceneManager.UnloadSceneAsync(current);
        if (unload2 != null) while (!unload2.isDone) yield return null;

        var sm = Object.Instantiate(_sceneManager);
        sm.AddComponent<HazardRespawnMarker>();
        sm.name = "_SceneManager";
        sm.SetActive(true);
    }
}