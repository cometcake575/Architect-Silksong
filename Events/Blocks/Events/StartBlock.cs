using System.Collections.Generic;
using UnityEngine;

namespace Architect.Events.Blocks.Events;

public class StartBlock : ScriptBlock
{
    protected override IEnumerable<string> Outputs => ["PreStart", "OnStart"];
    protected override string Name => "On Room Load";

    public override GameObject SetupReference()
    {
        var obj = new GameObject("[Architect] Start Block");
        obj.AddComponent<StartEvent>().Block = this;
        return obj;
    }

    public class StartEvent : MonoBehaviour
    {
        public StartBlock Block;
        private bool _done;

        private void Start() => Block.Event("PreStart");
        
        private void Update()
        {
            if (_done) return;
            _done = true;
            Block.Event("OnStart");
        }
    }
}