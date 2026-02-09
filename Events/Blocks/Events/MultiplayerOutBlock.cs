using System.Collections.Generic;
using UnityEngine;

namespace Architect.Events.Blocks.Events;

public class MultiplayerOutBlock : ScriptBlock
{
    protected override IEnumerable<string> Outputs => ["OnReceive"];

    private static readonly Color DefaultColor = new(0.8f, 0.2f, 0.8f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Multiplayer Receive";
    
    public string EventName;

    public override void SetupReference()
    {
        var te = new GameObject("[Architect] Multiplayer Out Block").AddComponent<MpEvent>();
        te.Block = this;
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
