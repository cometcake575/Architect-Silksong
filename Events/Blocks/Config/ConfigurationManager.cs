using System.Collections.Generic;
using Architect.Events.Blocks.Config.Types;

namespace Architect.Events.Blocks.Config;

public static class ConfigurationManager
{
    public static readonly Dictionary<string, ConfigType> ConfigTypes = [];
    
    public static ConfigType RegisterConfigType(ConfigType type)
    {
        ConfigTypes[type.Id] = type;
        return type;
    }
    
    public static ConfigValue DeserializeConfigValue(string configType, string serializedValue)
    {
        return ConfigTypes[configType].Deserialize(serializedValue);
    }
}