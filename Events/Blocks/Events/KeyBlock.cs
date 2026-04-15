using System.Collections.Generic;
using Architect.Utils;
using Architect.Workshop.Items;
using InControl;
using UnityEngine;

namespace Architect.Events.Blocks.Events;

public class KeyBlock : ToggleableBlock
{
    protected override IEnumerable<string> Outputs => ["OnPress", "OnRelease"];
    protected override IEnumerable<string> Inputs => ["Disable", "Enable"];
    protected override IEnumerable<(string, string)> OutputVars => [("Held", "Boolean")];
    protected override string Name => "Key Listener";

    public KeyCode Key = KeyCode.None;
    public PlayerAction PlayerAction = null;
    public CustomKeybind Keybind = null;

    public override void SetupReference()
    {
        var te = new GameObject("[Architect] Key Listener Block").AddComponent<KeyEvent>();
        te.Block = this;
    }
    
    protected override void Trigger(string trigger)
    {
        //InputHandler.Instance.buttons
        if (trigger == "Prompt")
        {
            /*ControlReminder.PushSingle(new CustomConfig
            {
                Text = (LocalStr)"Test",
                DisappearOnButtonPress = false,
                Button = 
            });
            ControlReminder.ShowPushed();*/
        }
        else base.Trigger(trigger);
    }

    public class CustomConfig : ControlReminder.SingleConfig
    {
         
    }

    public override object GetValue(string id)
    {
        if (Keybind != null) Key = GlobalArchitectData.Instance.Keybinds.GetValueOrDefault(Keybind.Id, Keybind.Default);
        return Input.GetKeyDown(Key) || (PlayerAction?.IsPressed ?? false);
    }

    public class KeyEvent : MonoBehaviour
    {
        public KeyBlock Block;
        
        private void Update()
        {
            if (Block.Keybind != null)
                Block.Key = GlobalArchitectData.Instance.Keybinds.GetValueOrDefault(Block.Keybind.Id, Block.Keybind.Default);
            if (Input.GetKeyDown(Block.Key) || (Block.PlayerAction?.WasPressed ?? false)) Block.Event("OnPress");
            if (Input.GetKeyUp(Block.Key) || (Block.PlayerAction?.WasReleased ?? false)) Block.Event("OnRelease");
        }
    }
}