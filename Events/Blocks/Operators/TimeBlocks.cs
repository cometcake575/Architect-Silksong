using System;
using System.Collections.Generic;
using UnityEngine;

namespace Architect.Events.Blocks.Operators;

public abstract class TimeBlockType : ScriptBlock
{
    public int Mode = 0;
    
    protected DateTime GetNow() => Mode == 0 ? DateTime.Now : DateTime.UtcNow;
}

public class TimeBlock : TimeBlockType
{
    protected override Color Color => Color.yellow;
    protected override string Name => "Time Block";

    protected override IEnumerable<(string, string)> OutputVars =>
    [
        ("Days", "Number"),
        ("Hours", "Number"),
        ("Minutes", "Number"),
        ("Seconds", "Number")
    ];

    protected override object GetValue(string id)
    {
        var diff = GetNow() - DateTime.UnixEpoch;
        return id switch
        {
            "Days" => diff.TotalDays,
            "Hours" => diff.TotalHours,
            "Minutes" => diff.TotalMinutes,
            _ => diff.TotalSeconds
        };
    }
}

public class DayBlock : TimeBlockType
{
    protected override Color Color => Color.yellow;
    protected override string Name => "Day Block";

    protected override IEnumerable<(string, string)> OutputVars =>
    [
        ("OfYear", "Number"),
        ("OfWeek", "Number"),
        ("Hour", "Number"),
        ("Minute", "Number"),
        ("Second", "Number")
    ];

    protected override object GetValue(string id)
    {
        var now = GetNow();
        return id switch
        {
            "OfYear" => now.DayOfYear,
            "OfWeek" => (int)now.DayOfWeek,
            "Hour" => (int)now.TimeOfDay.TotalHours,
            "Minute" => (int)now.TimeOfDay.TotalMinutes % 60,
            _ => (int)now.TimeOfDay.TotalSeconds % 60
        };
    }
}