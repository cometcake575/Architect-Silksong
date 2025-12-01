using UnityEngine;
using Camera = GlobalSettings.Camera;

namespace Architect.Behaviour.Utility;

public class CameraShaker : MonoBehaviour
{
    public int shakeType;

    public void Shake()
    {
        Camera.MainCameraShakeManager.DoShake(shakeType switch
        {
            0 => Camera.TinyShake,
            1 => Camera.SmallShake,
            2 => Camera.AverageShake,
            _ => Camera.BigShake
        }, this, false);
    }
}