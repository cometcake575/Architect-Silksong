using Silksong.AssetHelper.ManagedAssets;
using UnityEngine;

namespace Architect.Content.Preloads;

public interface IPreload
{
    public string Scene { get; }
    public string Path { get; }

    public void OnPreload(GameObject preload) {}

    public void SetAsset(ManagedAsset<GameObject> asset);

    public bool IsNotSceneBundle { get; }
    
    public bool ShouldAlwaysLoad { get; }
}