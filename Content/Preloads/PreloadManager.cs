using System.Collections;
using System.Collections.Generic;
using Architect.Editor;
using Architect.Utils;
using Silksong.AssetHelper.ManagedAssets;
using Silksong.AssetHelper.Plugin;
using UnityEngine;
using UnityEngine.UI;

namespace Architect.Content.Preloads;

public static class PreloadManager
{
    public static bool HasPreloaded;

    private static GameObject _canvasObj;

    private static int _count;

    private static readonly Dictionary<string, List<(string, IPreload)>> ToPreload = [];
    private static readonly List<(IPreload, ManagedAsset<GameObject>)> Preloaded = [];
    
    public static void Init()
    {
        AudioListener.pause = true;
        
        SetupCanvas();
        
        RegisterPreloads();
        AssetRequestAPI.InvokeAfterBundleCreation(FinishPreloading);
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
    }
    
    private static void RegisterPreloads()
    {
        foreach (var (source, pair) in ToPreload)
        {
            foreach (var (path, preload) in pair)
            {
                var asset = preload.IsNotSceneBundle ?
                    ManagedAsset<GameObject>.FromNonSceneAsset(path, source) : 
                    ManagedAsset<GameObject>.FromSceneAsset(source, path);
                
                Preloaded.Add((preload, asset));
            }
        }
    }

    private static void FinishPreloading()
    {
        if (HasPreloaded) return;
        _canvasObj.SetActive(true);
        ArchitectPlugin.Instance.StartCoroutine(Preload());
    }

    private static IEnumerator Preload()
    {
        yield return new WaitForSeconds(1);
        foreach (var (preload, asset) in Preloaded)
        {
            ArchitectPlugin.Instance.StartCoroutine(Prepare(preload, asset));
        }

        while (_count < Preloaded.Count) yield return null;
        
        EditorUI.CompleteSetup();
        HasPreloaded = true;
        Object.Destroy(_canvasObj);
        AudioListener.pause = false;
    }

    private static IEnumerator Prepare(IPreload preload, ManagedAsset<GameObject> asset)
    {
        asset.Load();
        yield return asset.Handle;

        if (asset.Handle.OperationException != null) yield break;

        var foundObject = asset.Handle.Result;
        preload.BeforePreload(foundObject);
        preload.AfterPreload(foundObject);
        _count++;
    }
    
    public static void RegisterPreload(IPreload obj)
    {
        if (!ToPreload.ContainsKey(obj.Scene)) ToPreload[obj.Scene] = [];
        ToPreload[obj.Scene].Add((obj.Path, obj));
    }
}