using System;
using Silksong.AssetHelper.ManagedAssets;
using UnityEngine;

namespace Architect.Content.Preloads;

public class BasicPreload(
    string scene, 
    string path,
    Action<GameObject> callback,
    bool notSceneBundle = false) : IPreload
{
    public bool Loaded { get; private set; }

    public string Scene { get; } = scene;
    public string Path { get; } = path;

    public void OnPreload(GameObject preload) => callback(preload);
    
    public void SetAsset(ManagedAsset<GameObject> asset) { }
    public void MarkLoaded() => Loaded = true;

    public bool IsNotSceneBundle => notSceneBundle;
    public bool ShouldAlwaysLoad => true;
}