using UnityEngine;

namespace Architect.Behaviour.Utility;

public class MusicController : MonoBehaviour
{
    private void Update()
    {
        GameManager.instance.noMusicSnapshot.TransitionTo(0);
    }
}