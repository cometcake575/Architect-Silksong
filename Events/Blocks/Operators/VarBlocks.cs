using System;
using System.Collections.Generic;
using Architect.Storage;
using Architect.Utils;
using UnityEngine;

namespace Architect.Events.Blocks.Operators;

public class BoolVarBlock : LocalBlock
{
    protected override IEnumerable<string> Inputs => ["Set"];
    protected override IEnumerable<(string, string)> InputVars => [("New Value", "Boolean")];
    protected override IEnumerable<(string, string)> OutputVars => [("Value", "Boolean")];

    private static readonly Dictionary<string, bool> TempVars = [];
    public static readonly Dictionary<string, bool> SemiVars = [];
    
    protected override string Name => "Variable (Bool)";

    public string Id = "";
    public int PType;

    public bool Default;

    public static void Init()
    {
        typeof(HeroController).Hook(nameof(HeroController.SceneInit),
            (Action<HeroController> orig, HeroController self) =>
            {
                orig(self);
                TempVars.Clear();
                if (Settings.TestMode.Value)
                {
                    SemiVars.Clear();
                    ArchitectData.Instance.BoolVariables.Clear();
                }
            });
    }

    protected override void Trigger(string trigger)
    {
        var val = GetVariable<bool>("New Value");
        switch (PType)
        {
            case 0:
                TempVars[Id] = val;
                break;
            case 1:
                SemiVars[Id] = val;
                break;
            case 2:
                ArchitectData.Instance.BoolVariables[Id] = val;
                break;
            case 3:
                GlobalArchitectData.Instance.BoolVariables[Id] = val;
                break;
        }
    }

    protected override object GetValue(string id)
    {
        return PType switch
        {
            0 => TempVars.ContainsKey(Id) && TempVars[Id],
            1 => SemiVars.ContainsKey(Id) && SemiVars[Id],
            2 => ArchitectData.Instance.BoolVariables.ContainsKey(Id) &&
                 ArchitectData.Instance.BoolVariables[Id],
            3 => GlobalArchitectData.Instance.BoolVariables.ContainsKey(Id) &&
                 GlobalArchitectData.Instance.BoolVariables[Id],
            _ => Default
        };
    }
}

public class NumVarBlock : LocalBlock
{
    protected override IEnumerable<string> Inputs => ["Set"];
    protected override IEnumerable<(string, string)> InputVars => [("New Value", "Number")];
    protected override IEnumerable<(string, string)> OutputVars => [("Value", "Number")];

    private static readonly Dictionary<string, float> TempVars = [];
    public static readonly Dictionary<string, float> SemiVars = [];
    
    protected override string Name => "Variable (Number)";

    public string Id = "";
    public int PType;

    public float Default;
    
    public static void Init()
    {
        typeof(HeroController).Hook(nameof(HeroController.SceneInit),
            (Action<HeroController> orig, HeroController self) =>
            {
                orig(self);
                TempVars.Clear();
                if (Settings.TestMode.Value)
                {
                    SemiVars.Clear();
                    ArchitectData.Instance.FloatVariables.Clear();
                }
            });
    }

    protected override void Trigger(string trigger)
    {
        var val = GetVariable<float>("New Value");
        switch (PType)
        {
            case 0:
                TempVars[Id] = val;
                break;
            case 1:
                SemiVars[Id] = val;
                break;
            case 2:
                ArchitectData.Instance.FloatVariables[Id] = val;
                break;
            case 3:
                GlobalArchitectData.Instance.FloatVariables[Id] = val;
                break;
        }
    }

    protected override object GetValue(string id)
    {
        return PType switch
        {
            0 => TempVars.GetValueOrDefault(Id, Default),
            1 => SemiVars.GetValueOrDefault(Id, Default),
            2 => ArchitectData.Instance.FloatVariables.GetValueOrDefault(Id, Default),
            3 => GlobalArchitectData.Instance.FloatVariables.GetValueOrDefault(Id, Default),
            _ => Default
        };
    }
}

public class StringVarBlock : LocalBlock
{
    protected override IEnumerable<string> Inputs => ["Set"];
    protected override IEnumerable<(string, string)> InputVars => [("New Value", "Text")];
    protected override IEnumerable<(string, string)> OutputVars => [("Value", "Text")];

    private static readonly Dictionary<string, string> TempVars = [];
    public static readonly Dictionary<string, string> SemiVars = [];
    
    
    
    protected override string Name => "Variable (Text)";

    public string Id = "";
    public int PType;

    public string Default = string.Empty;
    
    public static void Init()
    {
        typeof(HeroController).Hook(nameof(HeroController.SceneInit),
            (Action<HeroController> orig, HeroController self) =>
            {
                orig(self);
                TempVars.Clear();
                if (Settings.TestMode.Value)
                {
                    SemiVars.Clear();
                    ArchitectData.Instance.StringVariables.Clear();
                }
            });
    }

    protected override void Trigger(string trigger)
    {
        var val = GetVariable<string>("New Value");
        switch (PType)
        {
            case 0:
                TempVars[Id] = val;
                break;
            case 1:
                SemiVars[Id] = val;
                break;
            case 2:
                ArchitectData.Instance.StringVariables[Id] = val;
                break;
            case 3:
                GlobalArchitectData.Instance.StringVariables[Id] = val;
                break;
        }
    }

    public static string GetVar(string id)
    {
        return TempVars.GetValueOrDefault(id,
            SemiVars.GetValueOrDefault(id, 
                ArchitectData.Instance.StringVariables.GetValueOrDefault(id, 
                    GlobalArchitectData.Instance.Keybinds.GetValueOrDefault(id, KeyCode.None)
                        .ToString())));
    }

    protected override object GetValue(string id)
    {
        return PType switch
        {
            0 => TempVars.GetValueOrDefault(Id, Default),
            1 => SemiVars.GetValueOrDefault(Id, Default),
            2 => ArchitectData.Instance.StringVariables.GetValueOrDefault(Id, Default),
            3 => GlobalArchitectData.Instance.StringVariables.GetValueOrDefault(Id, Default),
            _ => Default
        };
    }
}
