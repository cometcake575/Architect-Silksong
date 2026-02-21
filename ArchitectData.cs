using System.Collections.Generic;

namespace Architect;

public class ArchitectData
{
    public static ArchitectData Instance => 
        ArchitectPlugin.Instance.SaveData ?? (ArchitectPlugin.Instance.SaveData = new ArchitectData());

    public Dictionary<string, string> StringVariables = [];
    public Dictionary<string, float> FloatVariables = [];
    public Dictionary<string, bool> BoolVariables = [];
}