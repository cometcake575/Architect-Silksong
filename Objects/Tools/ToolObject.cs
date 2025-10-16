using System.Collections.Generic;
using Architect.Storage;
using Architect.Utils;
using UnityEngine;

namespace Architect.Objects.Tools;

public abstract class ToolObject : SelectableObject
{
    public static readonly Dictionary<Settings.Keybind, int> Keybinds = [];

    public readonly int Index;
    
    private readonly Sprite _sprite;
    
    protected ToolObject(string path, Settings.Keybind keybind, int index)
    {
        _sprite = ResourceUtils.LoadSpriteResource(path);
        Keybinds[keybind] = index;
        Index = index;
    }
    
    public override Sprite GetUISprite() => _sprite;
}