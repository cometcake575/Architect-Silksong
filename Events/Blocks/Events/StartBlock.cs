using System.Collections.Generic;
using UnityEngine;

namespace Architect.Events.Blocks.Events;

public class StartBlock : ScriptBlock
{
    protected override IEnumerable<string> Outputs => ["OnStart"];
    protected override Color Color => Color.green;
    protected override string Name => "On Room Load";

    protected override void SetupReference()
    {
        new GameObject("[Architect] Start Block").AddComponent<StartEvent>().Block = this;
    }

    public class StartEvent : MonoBehaviour
    {
        public StartBlock Block;
        private bool _done;
        
        private void Update()
        {
            if (_done) return;
            _done = true;
            Block.Event("OnStart");
        }
    }
}