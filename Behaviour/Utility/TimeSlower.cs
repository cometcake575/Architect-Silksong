using System;
using Architect.Utils;
using MonoMod.RuntimeDetour;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class TimeSlower : MonoBehaviour
{
    public float changeTime;
    public float waitTime;
    public float returnTime;
    public float targetSpeed;
    public bool noPause;

    private static int _timeSlowedCount;

    public static void Init()
    {
        _ = new Hook(typeof(GameManager).GetProperty("TimeSlowed")!.GetGetMethod(),
            (Func<GameManager, bool> orig, GameManager self) => 
                orig(self) && self.timeSlowedCount > _timeSlowedCount);
    }
    
    public void SlowTime()
    {
        if (!noPause) _timeSlowedCount++;
        GameManager.instance.StartCoroutine(GameManager.instance.FreezeMoment(changeTime, waitTime, returnTime, targetSpeed, 
            () =>
            {
                if (!noPause) _timeSlowedCount--;
                if (this) gameObject.BroadcastEvent("OnFinish");
            }));
    }
}