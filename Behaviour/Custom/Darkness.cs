using System;
using System.Collections.Generic;
using System.Linq;
using Architect.Behaviour.Utility;
using UnityEngine;

namespace Architect.Behaviour.Custom;

public class Darkness : PreviewableBehaviour
{
    private static readonly List<Darkness> DarknessObjects = [];

    public int amount = 1;

    private void OnEnable()
    {
        if (isAPreview) return;
        DarknessObjects.Add(this);
        Refresh();
    }

    private void OnDisable()
    {
        if (isAPreview) return;
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