using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Architect.Utils;
using Architect.Workshop.Items;
using UnityEngine;

namespace Architect.Events.Blocks.Outputs;

public class QuestBlock : ScriptBlock
{
    protected override IEnumerable<string> Inputs => ["Clear", "Offer", "Update", "Accept", "Complete", "SilentAccept", "SilentComplete"];
    protected override IEnumerable<string> Outputs => ["OnAccept", "OnDecline", "OnAcceptDismiss", "OnCompleteDismiss"];
    protected override IEnumerable<(string, string)> OutputVars => [
        ("Accepted", "Boolean"),
        Space,
        ("Completed", "Boolean"),
        Space,
        ("CanComplete", "Boolean")
    ];

    
    
    protected override string Name => "Custom Quest Control";

    public override void Reset()
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
            case "Offer":
                yield return HeroController.instance.FreeControl();
                HeroController.instance.RelinquishControl();

                var targets = quest.targets;
                var set = targets.Any(target => target.Counter is CustomItem.CustomCourierItem { reward: 0 });
                if (set) quest.targets = [];
                
                QuestYesNoBox.Open(() =>
                {
                    ArchitectPlugin.Instance.StartCoroutine(RegainControl());
                    PrepareCourierItems(quest);
                    Event("OnAccept");
                }, () =>
                {
                    ArchitectPlugin.Instance.StartCoroutine(RegainControl());
                    Event("OnDecline");
                }, true, quest, true);

                if (set) quest.targets = targets;
                break;
            case "Update":
                QuestManager.ShowQuestUpdatedStandalone(quest);
                break;
            case "Clear":
                quest.Completion = new QuestCompletionData.Completion();
                break;
            case "Accept":
                yield return HeroController.instance.FreeControl();
                HeroController.instance.RelinquishControl();
                GameCameras.instance.HUDOut();
                
                completion.IsAccepted = true;
                completion.IsCompleted = false;
                quest.Completion = completion;
                PrepareCourierItems(quest);
                
                quest.BeginQuest(() =>
                {
                    Event("OnAcceptDismiss");
                    ArchitectPlugin.Instance.StartCoroutine(RegainControl());
                    GameCameras.instance.HUDIn();
                });
                break;
            case "Complete":
                yield return HeroController.instance.FreeControl();
                
                GameCameras.instance.HUDOut();
                
                completion.IsCompleted = true;
                quest.Completion = completion;
                
                HeroController.instance.RelinquishControl();
                quest.ShowQuestCompleted(() =>
                {
                    Event("OnCompleteDismiss");
                    ArchitectPlugin.Instance.StartCoroutine(RegainControl());
                    GameCameras.instance.HUDIn();
                });
                break;
            case "SilentAccept":
                completion.IsAccepted = true;
                quest.Completion = completion;
                PrepareCourierItems(quest);
                break;
            case "SilentComplete":
                quest.SilentlyComplete();
                break;
        }
    }

    public static void PrepareCourierItems(FullQuestBase quest)
    {
        foreach (var target in quest.targets)
        {
            if (target.Counter is CustomItem.CustomCourierItem cci)
            {
                cci.Take(999, false);
                cci.Get(target.Count);
                quest.rewardCount = cci.reward;
            }
        }
    }

    private IEnumerator RegainControl()
    {
        yield return new WaitForSeconds(0.2f);
        HeroController.instance.RegainControl();
    }

    public override object GetValue(string id)
    {
        var quest = QuestManager.instance.masterList.GetByName(QuestName) as FullQuestBase;
        if (!quest) return false;
        return id switch
        {
            "Accepted" => quest.IsAccepted,
            "Completed" => quest.IsCompleted,
            _ => quest.CanComplete
        };
    }
}
