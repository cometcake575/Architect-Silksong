using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Architect.Behaviour.Custom;
using MonoMod.RuntimeDetour;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class CameraBorder : MonoBehaviour
{
    private static readonly List<CameraBorder> Borders = [];

    public int type;
    public int activeType;

    private void OnEnable()
    {
        Borders.Add(this);
    }

    private void OnDisable()
    {
        Borders.Remove(this);
    }

    public static void Init()
    {
        _ = new Hook(typeof(CameraController).GetMethod("LateUpdate",
                BindingFlags.NonPublic | BindingFlags.Instance),
            (Action<CameraController> orig, CameraController self) =>
            {
                orig(self);
                self.transform.position = KeepWithinBounds(self.transform.position);
            });
    }

    public static Vector3 KeepWithinBounds(Vector3 position)
    {
        foreach (var bord in Borders
                     .Where(bord => bord.activeType == 0 || 
                                    bord.activeType == 1 != Binoculars.BinocularsActive))
        {
            switch (bord.type)
            {
                case 0:
                {
                    if (position.x < bord.transform.position.x) position.x = bord.transform.position.x;
                    break;
                }
                case 1:
                {
                    if (position.x > bord.transform.position.x) position.x = bord.transform.position.x;
                    break;
                }
                case 2:
                {
                    if (position.y > bord.transform.position.y) position.y = bord.transform.position.y;
                    break;
                }
                case 3:
                {
                    if (position.y < bord.transform.position.y) position.y = bord.transform.position.y;
                    break;
                }
            }
        }

        return position;
    }
}