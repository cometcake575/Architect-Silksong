using Architect.Utils;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class Timer : MonoBehaviour
{
    public float startDelay = 1;
    public float repeatDelay = 1;
    public int maxCalls = -1;
    private int _calls;
    private float _time;

    private void Update()
    {
        if (startDelay > 0)
        {
            startDelay -= Time.deltaTime;
            if (startDelay > 0) return;
            _time -= startDelay;
        }
        else
        {
            _time += Time.deltaTime;
            if (_time < repeatDelay) return;
            _time -= repeatDelay;
        }

        _calls++;
        gameObject.BroadcastEvent("OnCall");
        if (maxCalls != -1 && _calls >= maxCalls)
        {
            _calls = 0;
            gameObject.SetActive(false);
        }
    }
}