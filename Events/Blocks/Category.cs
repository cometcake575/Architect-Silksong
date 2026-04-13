using System;
using System.Collections.Generic;
using Architect.Events.Blocks.Config.Types;
using UnityEngine;

namespace Architect.Events.Blocks;

public class Category
{
    public string Name;
    public Color Colour;

    public static readonly List<Category> Categories = [];
    
    public static readonly Category All = new("All", Color.white);
    public static readonly Category Events = new("Events", Color.green);
    public static readonly Category World = new("Player / World", new Color(0.6f, 0.5f, 0.9f));
    public static readonly Category Logic = new("Logic", new Color(0.9f, 0.7f, 0.3f));
    public static readonly Category Data = new("Data", new Color(0.9f, 0.5f, 0.2f));
    public static readonly Category Time = new("Time", Color.yellow);
    public static readonly Category Visual = new("Visual / UI", new Color(0.9f, 0.2f, 0.2f));
    public static readonly Category Functions = new("Functions", new Color(0.2f, 0.8f, 0.8f));
    
    public readonly List<(Func<ScriptBlock>, string)> Blocks = [];

    public Category(string name, Color colour)
    {
        Name = name;
        Colour = colour;

        Categories.Add(this);
    }

    public void RegisterBlock<T>(string id, List<ConfigType> configGroup = null, Action init = null) where T : ScriptBlock, new()
    {
        RegisterBlock<T>(id, id, configGroup, init);
    }

    public void RegisterBlock<T>(string id, string name, List<ConfigType> configGroup = null, Action init = null) where T : ScriptBlock, new()
    {
        init?.Invoke();
        var func = () => new T
        {
            Type = id, 
            Config = configGroup,
            Position = ScriptManager.BlockSpawnPos,
            Color = Colour
        };
        Blocks.Add((func, name));
        ScriptManager.BlockTypes[id] = func;
    }

    public void RegisterHiddenBlock<T>(string name, List<ConfigType> configGroup = null) where T : ScriptBlock, new()
    {
        ScriptManager.BlockTypes[name] = () => new T
        {
            Type = name, 
            Config = configGroup,
            Position = ScriptManager.BlockSpawnPos,
            Color = Colour
        };
    }
}