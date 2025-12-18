using System;
using Architect.Content.Preloads;
using UnityEngine;

namespace Architect.Objects.Placeable;

public class PreloadObject : PlaceableObject, IPreload
{
    public string Scene { get; }
    public string Path { get; }
    public bool IsHideAndDontSave { get; }
    
    private readonly Action<GameObject> _preloadAction;

    public PreloadObject(
        string name, 
        string id, 
        (string, string) path,
        string description = null,
        Action<GameObject> postSpawnAction = null, 
        Action<GameObject> preloadAction = null,
        bool preview = false,
        bool hideAndDontSave = false,
        Sprite sprite = null,
        Sprite uiSprite = null)
        : base(name, id, description, postSpawnAction, preview, sprite, uiSprite)
    {
        Scene = path.Item1;
        Path = path.Item2;
        IsHideAndDontSave = hideAndDontSave;

        _preloadAction = preloadAction;
        
        PreloadManager.RegisterPreload(this);
    }

    public void BeforePreload(GameObject original) => _preloadAction?.Invoke(original);

    public virtual void AfterPreload(GameObject preload) => FinishSetup(preload);
}