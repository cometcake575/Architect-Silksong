using System;
using UnityEngine;

namespace Architect.Content.Preloads;

public class BasicPreload(
    string scene, 
    string path,
    Action<GameObject> callback,
    bool hads = false) : IPreload
{
    public string Scene { get; } = scene;
    public string Path { get; } = path;

    public void AfterPreload(GameObject preload) => callback(preload);
    
    public bool IsHideAndDontSave => hads;
}