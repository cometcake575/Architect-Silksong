using System.Collections.Generic;
using UnityEngine;

namespace Architect.Events.Blocks.Events;

public class KeyBlock : ScriptBlock
{
    protected override IEnumerable<string> Inputs => [];
    protected override IEnumerable<string> Outputs => ["OnPress", "OnRelease"];
    protected override Color Color => Color.green;
    protected override string Name => "Key Listener";

    public KeyCode Key = KeyCode.None;
    
    protected override void SetupReference()
    {
        var te = new GameObject("[Architect] Key Listener Block").AddComponent<KeyEvent>();
        te.Block = this;
    }

    public class KeyEvent : MonoBehaviour
    {
        public KeyBlock Block;
        
        private void Update()
        {
            if (Input.GetKeyDown(Block.Key)) Block.Event("OnPress");
            if (Input.GetKeyUp(Block.Key)) Block.Event("OnRelease");
        }
    }
}