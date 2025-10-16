using System.Collections;
using UnityEngine;

namespace Architect.Behaviour.Custom;

public class Fallthrough : MonoBehaviour
{
    public float fallthroughTime;
    private float _time;

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!collision.gameObject.GetComponent<HeroController>()) return;
        _time = 0;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.gameObject.GetComponent<HeroController>()) return;

        if (!InputHandler.Instance.inputActions.Down)
        {
            _time = 0;
            return;
        }

        _time += Time.deltaTime;
        if (_time < fallthroughTime) return;

        _time = 0;
        Physics2D.IgnoreCollision(collision.collider, collision.otherCollider);
        StartCoroutine(ReEnableCollision(collision.collider, collision.otherCollider));
    }

    private static IEnumerator ReEnableCollision(Collider2D self, Collider2D other)
    {
        while (self.Distance(other).isOverlapped) yield return null;
        Physics2D.IgnoreCollision(self, other, false);
    }
}