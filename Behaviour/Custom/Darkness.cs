using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Architect.Behaviour.Custom;

public class Darkness : MonoBehaviour
{
    private static readonly List<Darkness> DarknessObjects = [];

    public int amount = 1;

    private void OnEnable()
    {
        DarknessObjects.Add(this);
        Refresh();
    }

    private void OnDisable()
    {
        DarknessObjects.Remove(this);
        Refresh();
    }

    private void Update()
    {
        if (DarknessObjects.Count > 0) Refresh();
    }

    private static void Refresh()
    {
        DarknessRegion.SetDarknessLevel(Math.Clamp(DarknessObjects.Sum(o => o.amount), 0, 2));
    }
}