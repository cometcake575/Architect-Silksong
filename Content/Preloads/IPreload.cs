using UnityEngine;

namespace Architect.Content.Preloads;

public interface IPreload
{
    public string Scene { get; }
    public string Path { get; }

    public void OnPreload(GameObject preload) {}

    public bool IsNotSceneBundle { get; }
}