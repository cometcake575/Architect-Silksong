using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Architect.Editor;
using Architect.Utils;
using MonoMod.RuntimeDetour;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Architect.Content.Preloads;

public static class PreloadManager
{
    private static GameObject _canvasObj;
    public static bool HasPreloaded;

    private static RectTransform _preloadBar;
    private static float _preloadBarPoint;
    
    private static int _activePreloads;
    private static int _finishedPreloads;
    private static float _secondsSinceLastSet;

    private static readonly Dictionary<string, List<(string, IPreload)>> ToPreload = [];
    
    public static void Init()
    {
        AudioListener.pause = true;
        
        // Waits for the GameManager to be ready before preloading
        typeof(GameManager).Hook("Awake", DoPreload);
        
        SetupCanvas();
    }

    private static void DoPreload(Action<GameManager> orig, GameManager self)
    {
        orig(self);
        if (HasPreloaded) return;
        self.StartCoroutine(Preload(self));
        self.StartCoroutine(PreloadBar());
    }

    private static void SetupCanvas()
    {
        _canvasObj = new GameObject("[Architect] Preload Status");
        _canvasObj.SetActive(false);
        Object.DontDestroyOnLoad(_canvasObj);

        _canvasObj.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = _canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        _canvasObj.AddComponent<GraphicRaycaster>();

        UIUtils.MakeImage("Preload BG", _canvasObj, Vector2.zero,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(3000, 3000))
            .sprite = ResourceUtils.LoadSpriteResource("preloader_bg");
        
        UIUtils.MakeImage("Preload Frame", _canvasObj, Vector2.zero,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(1030, 100))
            .sprite = ResourceUtils.LoadSpriteResource("preloader_frame");
        
        var bar = UIUtils.MakeImage("Preload Bar", _canvasObj, Vector2.zero,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 70));
        bar.sprite = ResourceUtils.LoadSpriteResource("preloader_bar");

        _preloadBar = bar.GetComponent<RectTransform>();
    }

    private static IEnumerator PreloadBar()
    {
        while (_preloadBarPoint < 995)
        {
            _secondsSinceLastSet += Time.deltaTime;
            _preloadBarPoint = Mathf.Lerp(_preloadBarPoint, 1000f * _finishedPreloads / ToPreload.Count, 
                _secondsSinceLastSet / 10);
            _preloadBar.sizeDelta = new Vector2(_preloadBarPoint, 70);
            yield return null;
        }

        yield return GameManager.instance.ReturnToMainMenu(false);
        Object.Destroy(_canvasObj);

        HasPreloaded = true;
        AudioListener.pause = false;
    }

    private static IEnumerator Preload(GameManager manager)
    {
        _canvasObj.SetActive(true);
        
        // Prevents errors where objects would try to find Hornet and she isn't present
        var hornetPrefab = GameManager.instance.LoadHeroPrefab();
        yield return hornetPrefab;
        var hornet = Object.Instantiate(hornetPrefab.Result);
        Object.DontDestroyOnLoad(hornet);
        
        // Prevents assets from being unloaded
        var keepObjects = new Hook(typeof(AssetBundle).GetMethod(nameof(AssetBundle.UnloadAsync)),
            (Func<AssetBundle, bool, AssetBundleUnloadOperation> orig, AssetBundle self, bool _) => orig(self, false));
        // Prevents enemies from dying during preload
        var stopDeath = new Hook(typeof(HealthManager).GetMethod("TakeDamage", 
            BindingFlags.NonPublic | BindingFlags.Instance),
            (Action<HealthManager, HitInstance> _, HealthManager _, HitInstance _) => {});
        
        foreach (var pair in ToPreload)
        {
            var process = Addressables.LoadSceneAsync("Scenes/" + pair.Key, 
                LoadSceneMode.Additive);
            while (_activePreloads >= 3) yield return null;
            _activePreloads++;
            manager.StartCoroutine(PreloadScene(process, pair.Value));
        }
        
        while (_activePreloads > 0) yield return null;
        
        EditorUI.CompleteSetup();
        
        keepObjects.Dispose();
        stopDeath.Dispose();
        
        Resources.UnloadUnusedAssets();
        
        Object.Destroy(hornet);
        GameManager.instance.UnloadHeroPrefab();
    }
    
    private static IEnumerator PreloadScene(AsyncOperationHandle<SceneInstance> process, 
        List<(string, IPreload)> objects)
    {
        yield return process;
        
        var rootObjects = process.Result.Scene.GetRootGameObjects();
        foreach (var obj in rootObjects) obj.SetActive(false);

        foreach (var preload in objects)
        {
            try
            {
                var foundObject = ObjectUtils.GetGameObjectFromArray(rootObjects, preload.Item1);
            
                preload.Item2.BeforePreload(foundObject);
                
                var obj = Object.Instantiate(foundObject);
                obj.SetActive(false);

                Object.DontDestroyOnLoad(obj);

                preload.Item2.AfterPreload(obj);
            }
            catch (NullReferenceException)
            {
                ArchitectPlugin.Logger.LogError($"Could not find object {preload.Item1} for preload");
            }
            catch (ArgumentException)
            {
                ArchitectPlugin.Logger.LogError($"Invalid path {preload.Item1} for preload");
            }
        }
        
        yield return Addressables.UnloadSceneAsync(process);
        _finishedPreloads++;
        _activePreloads--;
        _secondsSinceLastSet = 0;
    }

    public static void RegisterPreload(IPreload obj)
    {
        if (!ToPreload.ContainsKey(obj.Scene)) ToPreload[obj.Scene] = [];
        ToPreload[obj.Scene].Add((obj.Path, obj));
    }
}