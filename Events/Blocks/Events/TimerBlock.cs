using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Architect.Events.Blocks.Events;

public class TimerBlock : ToggleableBlock
{
    protected override IEnumerable<string> Inputs => ["Reset"];
    protected override IEnumerable<string> Outputs => ["OnCall"];
    protected override Color Color => Color.green;
    protected override string Name => "Timer";

    public float StartDelay = 1;
    public float RepeatDelay = 1;
    public float RandDelay;
    public int MaxCalls = -1;

    private TimerEvent _te;

    protected override void Trigger(string trigger)
    {
        if (!_te) return;
        _te.gameObject.SetActive(true);
        _te.Restart();
    }

    protected override void SetupReference()
    {
        _te = new GameObject("[Architect] Timer Block").AddComponent<TimerEvent>();
        _te.Block = this;
        _te.cStartDelay = StartDelay;
    }

    public class TimerEvent : MonoBehaviour
    {
        public TimerBlock Block;
        
        private int _calls;
        private float _time;
        private float _cRepeatDelay;
        
        public float cStartDelay;

        public void Restart()
        {
            _time = 0;
            _calls = 0;
            cStartDelay = Block.StartDelay;
        }

        private void Update()
        {
            if (!Block.Enabled) return;
            
            if (cStartDelay > 0)
            {
                cStartDelay -= Time.deltaTime;
                if (cStartDelay > 0) return;
                _time -= cStartDelay;
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