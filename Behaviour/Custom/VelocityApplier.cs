using UnityEngine;

namespace Architect.Behaviour.Custom;

public class VelocityApplier : MonoBehaviour
{
    public float x;
    public float y;

    private bool _started;

    private void Update()
    {
        if (_started) return;
        _started = true;
        var rb2d = GetComponent<Rigidbody2D>();
        rb2d.linearVelocityX = x;
        rb2d.linearVelocityY = y;
    }
}