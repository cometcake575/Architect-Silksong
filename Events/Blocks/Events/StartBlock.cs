using System;
using System.Collections.Generic;
using UnityEngine;

namespace Architect.Events.Blocks.Events;

public class StartBlock : ScriptBlock
{
    public override IEnumerable<string> Inputs => [];
    public override IEnumerable<string> Outputs => ["OnStart"];
    protected override int InputCount => 0;
    protected override int OutputCount => 1;
    protected override Color Color => Color.green;
    protected override string Name => "Start";
    protected override string Type => "start";

    protected override bool SetupReference()
    {
        new GameObject("[Architect] Start Block").AddComponent<StartEvent>().Block = this;
        return true;
    }

    protected override void Trigger(string trigger) { }

    public class StartEvent : MonoBehaviour
    {
        public StartBlock Block;
        
        private void Start() => Block.Event("OnStart");
    }
}