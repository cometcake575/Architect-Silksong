using System.Collections.Generic;
using UnityEngine;

namespace Architect.Events.Blocks.Events;

public class StartBlock : ScriptBlock
{
    protected override IEnumerable<string> Inputs => [];
    protected override IEnumerable<string> Outputs => ["OnStart"];
    protected override int InputCount => 0;
    protected override int OutputCount => 1;
    protected override Color Color => Color.green;
    protected override string Name => "On Room Load";

    protected override void SetupReference()
    {
        new GameObject("[Architect] Start Block").AddComponent<StartEvent>().Block = this;
    }

    public class StartEvent : MonoBehaviour
    {
        public StartBlock Block;
        
        private void Start() => Block.Event("OnStart");
    }
}