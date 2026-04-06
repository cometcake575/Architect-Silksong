using System;
using System.Collections;
using Architect.Content.Preloads;
using Architect.Storage;
using Silksong.AssetHelper.ManagedAssets;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Architect.Objects.Placeable;

public class PreloadObject : PlaceableObject, IPreload
{
    public string Scene { get; }
    public string Path { get; }
    
    public bool IsNotSceneBundle { get; }

    public bool ShouldLoad;
    public bool ShouldAlwaysLoad => ShouldLoad || Settings.LoadAllAssets.Value;

    public bool Loaded;

    private readonly Action<GameObject> _preloadAction;

    private ManagedAsset<GameObject> _asset;

    public PreloadObject(
        string name, 
        string id, 
        (string, string) path,
        string description = null,
        Action<GameObject> postSpawnAction = null, 
        Action<GameObject> preloadAction = null,
        bool preview = false,
        bool notSceneBundle = false,
        Sprite sprite = null,
        Sprite uiSprite = null)
        : base(name, id, description, postSpawnAction, preview, sprite, uiSprite)
    {
        Scene = path.Item1;
        Path = path.Item2;
        IsNotSceneBundle = notSceneBundle;

        _preloadAction = preloadAction;
        
        PreloadManager.RegisterPreload(this);
    }
    
    public void SetAsset(ManagedAsset<GameObject> asset)
    {
        _asset = asset;
    }

    public override IEnumerator EnsureLoaded()
    {
        if (Loaded) yield break;

        PreloadManager.IsLoading = true;
        yield return _asset.Load();
        if (_asset.Handle.OperationException != null || Loaded)
        {
            PreloadManager.IsLoading = false;
            yield break;
        }

        Loaded = true;
        OnPreload(_asset.Handle.Result);
        PreloadManager.IsLoading = false;
    }

    public void OnPreload(GameObject preload)
    {
        if (IsNotSceneBundle && preload.GetComponent<HealthManager>())
        {
            var active = preload.activeSelf;
            preload.SetActive(false);
            var p = preload;
            preload = Object.Instantiate(preload);
            Object.DontDestroyOnLoad(preload);
            if (active) p.SetActive(true);
        }
        
        _preloadAction?.Invoke(preload);
        FinishSetup(preload);
        Loaded = true;
    }
}