using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Architect.Events.Blocks.Outputs;

public class JournalEntryBlock : ScriptBlock
{
    protected override IEnumerable<string> Inputs => ["Increment", "Complete"];
    protected override IEnumerable<(string, string)> OutputVars => [
        ("Seen", "Number"),
        ("Completed", "Boolean")
    ];

    private static readonly Color DefaultColor = new(0.6f, 0.2f, 0.9f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Journal Entry Control";

    protected override void Reset()
    {
        EntryName = "";
    }

    public string EntryName;
    
    protected override void Trigger(string trigger)
    {
        var entry = EnemyJournalManager.Instance.recordList.list.FirstOrDefault(o => o.name == EntryName);
        if (!entry) return;
        if (trigger == "Increment") entry.Get();
        else entry.Get(entry.killsRequired);
    }

    protected override object GetValue(string id)
    {
        var entry = EnemyJournalManager.Instance.recordList.list.FirstOrDefault(o => o.name == EntryName);
        if (!entry) return null;
        if (id == "Seen") return entry.KillCount;
        return entry.KillCount == entry.killsRequired;
    }
}
