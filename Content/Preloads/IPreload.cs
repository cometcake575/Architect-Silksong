using Silksong.AssetHelper.ManagedAssets;
using UnityEngine;

namespace Architect.Content.Preloads;

public interface IPreload
{
    public bool Loaded { get; }
    
    public string Scene { get; }
    public string Path { get; }

    public void OnPreload(GameObject preload) {}

    public void SetAsset(ManagedAsset<GameObject> asset);

    public void MarkLoaded();

    public bool IsNotSceneBundle { get; }
    
    public bool ShouldAlwaysLoad { get; }
}