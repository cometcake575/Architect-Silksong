using System.Collections.Generic;
using UnityEngine;

namespace Architect.Events.Blocks.Events;

public class ReceiveBlock : LocalBlock
{
    protected override IEnumerable<string> Outputs => ["OnReceive"];

    protected override string Name => "Receive";
    
    public string ActualEventName = string.Empty;
    public string EventName = string.Empty;

    public override GameObject SetupReference()
    {
        var obj = new GameObject("[Architect] Receive Block");
        var te = obj.AddComponent<RcEvent>();
        te.Block = this;
        return obj;
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