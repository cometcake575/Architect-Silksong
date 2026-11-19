using System.Collections.Generic;
using Architect.Config.Types;

namespace Architect.Config;

public static class ConfigurationManager
{
    public static readonly Dictionary<string, ConfigType> ConfigTypes = [];
    
    public static ConfigType RegisterConfigType(ConfigType type)
    {
        if (ConfigTypes.ContainsKey(type.Id)) 
            ArchitectPlugin.Logger.LogWarning($"{type.Id} is already a registered config type!");
        ConfigTypes[type.Id] = type;
        return type;
    }
    
    public static ConfigValue DeserializeConfigValue(string configType, string serializedValue)
    {
        return ConfigTypes.TryGetValue(configType, out var type) ? type.Deserialize(serializedValue) : null;
    }

    public enum PreviewContext
    {
        Cursor,
        Placement
    }
}