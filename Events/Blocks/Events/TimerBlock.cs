using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Architect.Events.Blocks.Events;

public class TimerBlock : ScriptBlock
{
    protected override IEnumerable<string> Inputs => [];
    protected override IEnumerable<string> Outputs => ["OnCall"];
    protected override int InputCount => 0;
    protected override int OutputCount => 1;
    protected override Color Color => Color.green;
    protected override string Name => "Timer";

    public float StartDelay = 1;
    public float RepeatDelay = 1;
    public float RandDelay;
    public int MaxCalls = -1;
    
    protected override void SetupReference()
    {
        var te = new GameObject("[Architect] Timer Block").AddComponent<TimerEvent>();
        te.Block = this;
    }

    public class TimerEvent : MonoBehaviour
    {
        public TimerBlock Block;
        
        private int _calls;
        private float _time;
        private float _cRepeatDelay;

        private void Update()
        {
            if (Block.StartDelay > 0)
            {
                Block.StartDelay -= Time.deltaTime;
                if (Block.StartDelay > 0) return;
                _time -= Block.StartDelay;
            }
            else
            {
                _time += Time.deltaTime;
                if (_time < _cRepeatDelay) return;
                _time -= _cRepeatDelay;
            }

            _calls++;
            _cRepeatDelay = Block.RepeatDelay + Random.value * Block.RandDelay;
            Block.Event("OnCall");
            if (Block.MaxCalls != -1 && _calls >= Block.MaxCalls)
            {
                _calls = 0;
                gameObject.SetActive(false);
            }
        }
    }
}