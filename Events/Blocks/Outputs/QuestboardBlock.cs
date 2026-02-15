using System;
using System.Collections;
using System.Collections.Generic;
using Architect.Content.Preloads;
using Architect.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Architect.Events.Blocks.Outputs;

public class QuestboardBlock : CollectionBlock<QuestboardBlock.QuestBlock>
{
    private static readonly Color DefaultColor = new(0.2f, 0.4f, 0.8f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Quest Board";

    private static GameObject _questBoard;
    
    public static void Init()
    {
        PreloadManager.RegisterPreload(new BasicPreload("Belltown", "Town States/Spinner Defeated/Quest Board Pivot/Quest_Board",
            o =>
            {
                o = Object.Instantiate(o);
                o.SetActive(false);
                Object.DontDestroyOnLoad(o);

                for (var i = 0; i < o.transform.childCount; i++)
                {
                    o.transform.GetChild(i).gameObject.SetActive(false);
                }
                
                _questBoard = o;
            }));
    }
    
    protected override IEnumerable<string> Inputs => ["Open"];

    protected override string ChildName => "Quest Item";
    protected override bool NeedsGap => true;

    private QuestBoardInteractable _qbi;

    public override void SetupReference()
    {
        var bo = Object.Instantiate(_questBoard);
        bo.transform.position = new Vector3(-9999, -9999);
        bo.SetActive(true);

        _finishedSetup = false;
        _qbi = bo.GetComponent<QuestBoardInteractable>();
        _qbi.questList = ScriptableObject.CreateInstance<QuestBoardList>();
    }

    private bool _finishedSetup;

    protected override void Trigger(string trigger)
    {
        if (!_finishedSetup)
        {
            var qib = _qbi.questBoard;
            qib.BoardClosed += _ => HeroController.instance.RegainControl();
            qib.QuestAccepted += () => HeroController.instance.RegainControl();
            _finishedSetup = true;
        }

        _qbi.name = Guid.NewGuid().ToString();
        ArchitectPlugin.Instance.StartCoroutine(Coroutine());
    }

    private IEnumerator Coroutine()
    {
        yield return HeroController.instance.FreeControl(_ => !GameManager.instance.isPaused);
        HeroController.instance.RelinquishControl();
        
        _qbi.questList.Clear();
        foreach (var child in Children.Children)
        {
            if (!child.GetVariable<bool>("Available", true) || !child.Quest) continue;
            _qbi.questList.Add(child.Quest);
        }
        
        _qbi.OpenBoard();
    }
    
    public class QuestBlock : ChildBlock
    {
        protected override Color Color => DefaultColor;

        public string QuestName = string.Empty;

        public BasicQuestBase Quest;

        public override void SetupReference()
        {
            Quest = QuestManager.instance.masterList.GetByName(QuestName) as FullQuestBase;
        }
        
        protected override IEnumerable<(string, string)> InputVars => [("Available", "Boolean")];
        protected override IEnumerable<string> Outputs => ["OnAccept"];
    }
}