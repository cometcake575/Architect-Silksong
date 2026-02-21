using System;
using System.Collections.Generic;
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
    
    private static readonly Color DefaultColor = new(0.9f, 0.5f, 0.2f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Variable (Bool)";

    public string Id = "";
    public int PType;

    public static void Init()
    {
        typeof(HeroController).Hook(nameof(HeroController.SceneInit),
            (Action<HeroController> orig, HeroController self) =>
            {
                orig(self);
                TempVars.Clear();
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
            _ => false
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
    
    private static readonly Color DefaultColor = new(0.9f, 0.5f, 0.2f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Variable (Number)";

    public string Id = "";
    public int PType;
    
    public static void Init()
    {
        typeof(HeroController).Hook(nameof(HeroController.SceneInit),
            (Action<HeroController> orig, HeroController self) =>
            {
                orig(self);
                TempVars.Clear();
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
        }
    }

    protected override object GetValue(string id)
    {
        return PType switch
        {
            0 => TempVars.GetValueOrDefault(Id, 0),
            1 => SemiVars.GetValueOrDefault(Id, 0),
            2 => ArchitectData.Instance.FloatVariables.GetValueOrDefault(Id, 0),
            _ => 0
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
    
    private static readonly Color DefaultColor = new(0.9f, 0.5f, 0.2f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Variable (Text)";

    public string Id = "";
    public int PType;
    
    public static void Init()
    {
        typeof(HeroController).Hook(nameof(HeroController.SceneInit),
            (Action<HeroController> orig, HeroController self) =>
            {
                orig(self);
                TempVars.Clear();
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
        }
    }

    public static string GetVar(string id)
    {
        return TempVars.GetValueOrDefault(id,
            SemiVars.GetValueOrDefault(id, ArchitectData.Instance.StringVariables.GetValueOrDefault(id, "null")));
    }

    protected override object GetValue(string id)
    {
        return PType switch
        {
            0 => TempVars.GetValueOrDefault(Id, ""),
            1 => SemiVars.GetValueOrDefault(Id, ""),
            2 => ArchitectData.Instance.StringVariables.GetValueOrDefault(Id, ""),
            _ => ""
        };
    }
}
