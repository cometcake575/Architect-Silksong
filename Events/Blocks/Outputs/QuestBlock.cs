using System.Collections;
using System.Collections.Generic;
using Architect.Utils;
using UnityEngine;

namespace Architect.Events.Blocks.Outputs;

public class QuestBlock : ScriptBlock
{
    protected override IEnumerable<string> Inputs => ["Accept", "Complete", "SilentComplete"];
    protected override IEnumerable<(string, string)> OutputVars => [
        ("Accepted", "Boolean"),
        Space,
        ("Completed", "Boolean")
    ];

    private static readonly Color DefaultColor = new(0.6f, 0.2f, 0.9f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Custom Quest Control";

    protected override void Reset()
    {
        QuestName = "";
    }

    public string QuestName;
    
    protected override void Trigger(string trigger)
    {
        ArchitectPlugin.Instance.StartCoroutine(TriggerRoutine(trigger));
    }

    private IEnumerator TriggerRoutine(string trigger)
    {
        var quest = QuestManager.instance.masterList.GetByName(QuestName) as FullQuestBase;
        if (!quest) yield break;

        var completion = quest.Completion;
        switch (trigger)
        {
            case "Accept":
                yield return HeroController.instance.FreeControl();
                HeroController.instance.RelinquishControl();
                
                completion.IsAccepted = true;
                completion.IsCompleted = false;
                quest.Completion = completion;
                
                quest.ShowQuestAccepted(() =>
                {
                    ArchitectPlugin.Instance.StartCoroutine(RegainControl());
                }, false);
                break;
            case "Complete":
                yield return HeroController.instance.FreeControl();
                
                completion.IsCompleted = true;
                quest.Completion = completion;
                
                HeroController.instance.RelinquishControl();
                quest.ShowQuestCompleted(() =>
                {
                    ArchitectPlugin.Instance.StartCoroutine(RegainControl());
                });
                break;
            case "SilentComplete":
                quest.SilentlyComplete();
                break;
        }
    }

    private IEnumerator RegainControl()
    {
        yield return new WaitForSeconds(0.2f);
        HeroController.instance.RegainControl();
    }

    protected override object GetValue(string id)
    {
        var quest = QuestManager.instance.masterList.GetByName(QuestName) as FullQuestBase;
        if (!quest) return false;
        return id == "Accepted" ? quest.IsAccepted : quest.IsCompleted;
    }
}
