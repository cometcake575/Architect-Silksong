using System;
using System.Collections.Generic;
using MonoMod.RuntimeDetour;
using UnityEngine;

namespace Architect.Events.Blocks.Outputs;

public class TimeSlowerBlock : ScriptBlock
{
    public static void Init()
    {
        _ = new Hook(typeof(GameManager).GetProperty("TimeSlowed")!.GetGetMethod(),
            (Func<GameManager, bool> orig, GameManager self) => 
                orig(self) && self.timeSlowedCount > _timeSlowedCount);
    }
    
    protected override IEnumerable<string> Inputs => ["Slow"];
    protected override IEnumerable<string> Outputs => ["End"];

    private static readonly Color DefaultColor = new(0.2f, 0.2f, 0.8f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Time Slowdown";
    
    public float ChangeTime;
    public float WaitTime;
    public float ReturnTime;
    public float TargetSpeed;
    public bool NoPause;

    private static int _timeSlowedCount;

    protected override void Trigger(string id)
    {
        if (!NoPause) _timeSlowedCount++;
        GameManager.instance.StartCoroutine(GameManager.instance.FreezeMoment(ChangeTime, WaitTime, ReturnTime, TargetSpeed, 
            () =>
            {
                if (!NoPause) _timeSlowedCount--;
                Event("End");
            }));
    }
}
