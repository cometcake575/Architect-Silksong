using System;
using System.Collections.Generic;
using Architect.Utils;
using UnityEngine;

namespace Architect.Events.Blocks.Operators;

public class BoolVarBlock : ScriptBlock
{
    protected override IEnumerable<string> Inputs => ["Set"];
    protected override IEnumerable<(string, string)> InputVars => [("New Value", "Boolean")];
    protected override IEnumerable<(string, string)> OutputVars => [("Value", "Boolean")];

    private static readonly Dictionary<string, PersistentBoolItem> Vars = [];
    private static readonly Dictionary<string, bool> TempVars = [];
    
    private static readonly Color DefaultColor = new(0.9f, 0.5f, 0.2f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Variable (Bool)";

    public string Id = "";
    public int PType;

    private PersistentBoolItem _pbi;

    public static void Init()
    {
        typeof(HeroController).Hook(nameof(HeroController.SceneInit),
            (Action<HeroController> orig, HeroController self) =>
            {
                orig(self);
                TempVars.Clear();
            });
    }

    protected override void SetupReference()
    {
        if (PType == 0) return;
        if (!Vars.TryGetValue(Id, out var item) || !item)
        {
            var o = new GameObject("[Architect] Variable Block");
            o.SetActive(false);
            _pbi = o.AddComponent<PersistentBoolItem>();
            _pbi.itemData = new PersistentBoolItem.PersistentBoolData
            {
                ID = Id,
                SceneName = "All",
                IsSemiPersistent = PType == 1
            };
            o.SetActive(true);
            Vars[Id] = _pbi;
        }
        else _pbi = item;
    }

    protected override void Trigger(string trigger)
    {
        var val = GetVariable<bool>("New Value");
        if (PType == 0)
        {
            TempVars[Id] = val;
            return;
        }
        _pbi.itemData.Value = val;
    }

    protected override object GetValue(string id)
    {
        if (PType == 0)
        {
            return TempVars.ContainsKey(Id) && TempVars[Id];
        }
        return _pbi.itemData.Value;
    }
}


public class NumVarBlock : ScriptBlock
{
    protected override IEnumerable<string> Inputs => ["Set"];
    protected override IEnumerable<(string, string)> InputVars => [("New Value", "Number")];
    protected override IEnumerable<(string, string)> OutputVars => [("Value", "Number")];

    private static readonly Dictionary<string, PersistentIntItem> Vars = [];
    private static readonly Dictionary<string, float> TempVars = [];
    
    private static readonly Color DefaultColor = new(0.9f, 0.5f, 0.2f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Variable (Number)";

    public string Id = "";
    public int PType;

    private PersistentIntItem _pii;

    public static void Init()
    {
        typeof(HeroController).Hook(nameof(HeroController.SceneInit),
            (Action<HeroController> orig, HeroController self) =>
            {
                orig(self);
                TempVars.Clear();
            });
    }

    protected override void SetupReference()
    {
        if (PType == 0) return;
        if (!Vars.TryGetValue(Id, out var item) || !item)
        {
            var o = new GameObject("[Architect] Variable Block");
            o.SetActive(false);
            _pii = o.AddComponent<PersistentIntItem>();
            _pii.itemData = new PersistentIntItem.PersistentIntData
            {
                ID = Id,
                SceneName = "All",
                IsSemiPersistent = PType == 1
            };
            o.SetActive(true);
            Vars[Id] = _pii;
        }
        else _pii = item;
    }

    protected override void Trigger(string trigger)
    {
        var val = GetVariable<float>("New Value");
        if (PType == 0)
        {
            TempVars[Id] = val;
            return;
        }
        _pii.itemData.Value = Mathf.RoundToInt(val * 1000);
    }

    protected override object GetValue(string id)
    {
        if (PType == 0)
        {
            if (TempVars.TryGetValue(Id, out var value)) return value;
            return 0;
        }
        return _pii.itemData.Value / 1000f;
    }
}
