using System.Collections.Generic;
using Architect.Storage;
using Architect.Utils;
using UnityEngine;

namespace Architect.Objects.Tools;

public abstract class ToolObject : SelectableObject
{
    public static readonly Dictionary<Settings.Keybind, int> Keybinds = [];
    public static readonly Dictionary<string, ToolObject> Tools = [];

    public readonly int Index;
    
    public readonly string Id;
    
    private readonly Sprite _sprite;
    
    protected ToolObject(string path, Settings.Keybind keybind, int index)
    {
        Id = path;
        Tools[Id] = this;
        
        _sprite = ResourceUtils.LoadSpriteResource(path);
        if (keybind != null) Keybinds[keybind] = index;
        Index = index;
        DisableTransformations = true;

    }

    public virtual bool Highlight => false;
    
    public override Sprite GetUISprite() => _sprite;
}