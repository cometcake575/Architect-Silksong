using System;
using Architect.Prefabs;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class SceneBorderRemover : MonoBehaviour
{
    private static int _count;

    public static void Init()
    {
        _ = new Hook(typeof(CameraController).GetProperty("AllowExitingSceneBounds")!.GetGetMethod(),
            (Func<CameraController, bool> orig, CameraController self) => 
                orig(self) || _count > 0 || PrefabManager.InPrefabScene);
        
        _ = new ILHook(typeof(CameraTarget).GetMethod(nameof(CameraTarget.Update)), il =>
        {
            var cursor = new ILCursor(il);

            // Find the usage of 0.0f two instructions before 9999f (when the first lower boundary variable is set)
            if (!cursor.TryGotoNext(
                    MoveType.Before, 
                    instr => instr.MatchLdcR4(0.0f),
                    _ => true,
                    instr => instr.MatchLdcR4(9999f)
                )) return;

            // Remove the instruction
            cursor.Remove();
            // Adds a new instruction of -9999f as the new lower bound
            cursor.Emit(OpCodes.Ldc_R4, -9999f);

            // Find the next usage of 0.0f (when the second lower boundary variable is set)
            if (!cursor.TryGotoNext(
                    MoveType.Before,
                    instr => instr.MatchLdcR4(0.0f)
                )) return;

            // Remove the instruction
            cursor.Remove();
            // Adds a new instruction of -9999f as the new lower bound
            cursor.Emit(OpCodes.Ldc_R4, -9999f);
        });
    }
    
    private void OnEnable() => _count++;

    private void OnDisable() => _count--;
}