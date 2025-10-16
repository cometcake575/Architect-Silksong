using UnityEngine;

namespace Architect.Content.Preloads;

public interface IPreload
{
    public string Scene { get; }
    public string Path { get; }

    public void BeforePreload(GameObject original) {}
    public void AfterPreload(GameObject preload) {}
}