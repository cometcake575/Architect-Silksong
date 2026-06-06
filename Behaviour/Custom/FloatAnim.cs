using Architect.Behaviour.Utility;
using UnityEngine;

namespace Architect.Behaviour.Custom;

public class FloatAnim : PreviewableBehaviour
{
    public bool active = true;
    
    private float _startY;
    
    private void Awake()
    {
        if (isAPreview)
        {
            enabled = false;
            return;
        }
        
        _startY = transform.localPosition.y;
    }

    private void Update()
    {
        transform.SetLocalPositionY(_startY + (active ? Mathf.Sin(Time.time * 4) / 15 : 0));
    }
}