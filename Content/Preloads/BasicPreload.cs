using System;
using UnityEngine;

namespace Architect.Content.Preloads;

public class BasicPreload(
    string scene, 
    string path,
    Action<GameObject> callback,
    bool notSceneBundle = false) : IPreload
{
    public string Scene { get; } = scene;
    public string Path { get; } = path;

    public void OnPreload(GameObject preload) => callback(preload);
    
    public bool IsNotSceneBundle => notSceneBundle;
}