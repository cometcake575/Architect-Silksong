using Architect.Placements;
using Architect.Utils;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class ObjectShaker : PreviewableBehaviour
{
    public string path = string.Empty;

    public bool startActive;
    
    private JitterSelf _js;
    public Vector3 amountMin;
    public Vector3 amountMax;
    public float frequency;
    
    private void Start()
    {
        if (isAPreview) return;
        
        if (!PlacementManager.TryGetValue(path, out var o)) o = ObjectUtils.FindGameObject(path);
        if (!o) return;

        _js = o.AddComponent<JitterSelf>();
        _js.startInactive = !startActive;
        
        _js.config.AmountMin = amountMin;
        _js.config.AmountMax = amountMax;
        _js.config.Frequency = frequency;
    }
    
    public void StartJitter()
    {
        if (_js) _js.StartJitter();
    }
    
    public void StopJitter()
    {
        if (_js) _js.StopJitter();
    }
}