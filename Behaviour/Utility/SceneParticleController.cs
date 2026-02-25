using System.Collections;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class SceneParticleController : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(Disable());
    }

    private static IEnumerator Disable()
    {
        yield return new WaitForSeconds(0.2f);
        GameCameras.instance.sceneParticles.DisableParticles();
    }
}