using System.Collections.Generic;
using UnityEngine;

namespace Architect.Events.Blocks.Events;

public class ReceiveBlock : LocalBlock
{
    protected override IEnumerable<string> Outputs => ["OnReceive"];

    protected override string Name => "Receive";
    
    public string ActualEventName = string.Empty;
    public string EventName = string.Empty;

    public override void SetupReference()
    {
        var te = new GameObject("[Architect] Receive Block").AddComponent<RcEvent>();
        te.Block = this;
    }

    public class RcEvent : MonoBehaviour
    {
        public ReceiveBlock Block;

        public static readonly List<RcEvent> Events = [];  

        private void OnEnable()
        {
            Events.Add(this);
        }
        
        private void OnDisable()
        {
            Events.Remove(this);
        }
    }
}