using System.Collections;
using Architect.Content.Preloads;
using Architect.Objects.Categories;
using Architect.Objects.Placeable;
using Architect.Utils;
using BepInEx;
using Silksong.AssetHelper.ManagedAssets;
using Silksong.AssetHelper.Plugin;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Architect.Workshop.Items;

public class CustomPreloadObject : WorkshopItem
{
    public override bool RegisterEarly => true;

    private static bool _loading;

    public string Name = string.Empty;
    public string Scene = string.Empty;
    public string Path = string.Empty;

    private PreloadObject _preloadObject;

    public override void Register()
    {
        if (Scene.IsNullOrWhiteSpace() || Path.IsNullOrWhiteSpace() || Name.IsNullOrWhiteSpace()) return;
        
        if (_preloadObject != null) return;
        
        _preloadObject = new PreloadObject(Name, $"custom_{Id}", (Scene, Path));
        Categories.Misc.Add(_preloadObject);

        if (!AssetRequestAPI.RequestApiAvailable)
        {
            ArchitectPlugin.Instance.StartCoroutine(EnsureLoaded());
        }
    }

    private IEnumerator EnsureLoaded()
    {
        yield return new WaitUntil(() => _loading == false);
        
        _loading = true;
        var asset = ManagedAsset<GameObject>.FromSceneAsset(Scene, Path);
        _preloadObject.SetAsset(asset);

        asset.Load();
        yield return asset.Handle;
        if (asset.Handle.Status == AsyncOperationStatus.Failed) yield return RuntimeRepack();
        
        _loading = false;
    }

    public override void Unregister()
    {
        if (_preloadObject == null) return;
        
        PreloadManager.UnregisterPreload(_preloadObject);
        PlaceableObject.RegisteredObjects.Remove(Id);
        Categories.Misc.Remove(_preloadObject);

        _preloadObject = null;
    }
    
    private static readonly Sprite Icon = ResourceUtils.LoadSpriteResource("object_extractor", FilterMode.Point);

    public override Sprite GetIcon() => Icon;

    private IEnumerator RuntimeRepack()
    {
        AssetRequestAPI.RequestApiAvailable = true;
        var asset = ManagedAsset<GameObject>.FromSceneAsset(Scene, Path);
        AssetRequestAPI.RequestApiAvailable = false;
        
        yield return StartupOverrideManager.WrapStartManagerStart(null!, null!);
        
        _preloadObject.SetAsset(asset);
    }
}