using Architect.Events;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class KeyListener : MonoBehaviour
{
    public KeyCode key = KeyCode.None;

    private void Update()
    {
        if (Input.GetKeyDown(key)) EventManager.BroadcastEvent(gameObject, "KeyPressed");
        if (Input.GetKeyUp(key)) EventManager.BroadcastEvent(gameObject, "KeyReleased");
    }
}