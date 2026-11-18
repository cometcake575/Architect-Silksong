using System.Collections.Generic;
using UnityEngine;

namespace Architect.Events.Blocks.Events;

public class MultiplayerOutBlock : ScriptBlock
{
    protected override IEnumerable<string> Outputs => ["OnReceive"];

    protected override string Name => "Multiplayer Receive";
    
    public string EventName;

    public override GameObject SetupReference()
    {
        var obj = new GameObject("[Architect] Multiplayer Out Block");
        var te = obj.AddComponent<MpEvent>();
        te.Block = this;
        return obj;
    }

    public class MpEvent : MonoBehaviour
    {
        public MultiplayerOutBlock Block;

        public static readonly List<MpEvent> Events = [];  

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
