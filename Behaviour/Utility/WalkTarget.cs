using System.Collections;
using Architect.Editor;
using Architect.Utils;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class WalkTarget : MonoBehaviour
{
    public float speed;
    public string anim;
    
    private bool _walking;
    
    public void StartWalk() => StartCoroutine(Walk());
    
    public void StopWalk() => _walking = false;

    public IEnumerator Walk()
    {
        if (_walking) yield break;
        
        var hero = HeroController.instance;
        var body = hero.rb2d;
        var heroTrans = hero.transform;

        yield return hero.FreeControl();
        
        if (!this) yield break;

        hero.RelinquishControl();
        hero.StopAnimationControl();
        var animator = hero.GetComponent<tk2dSpriteAnimator>();
        animator.Play(anim);

        EditManager.IgnoreControlRelinquished = true;
        _walking = true;

        if (heroTrans.position.x > transform.position.x)
        {
            hero.FaceLeft();
            while (heroTrans.position.x > transform.position.x)
            {
                if (!this || !EditManager.IgnoreControlRelinquished) yield break;
                if (!_walking) break;
                body.linearVelocityX = -speed;
                yield return null;
            }
        }
        else
        {
            hero.FaceRight();
            while (heroTrans.position.x < transform.position.x)
            {
                if (!this || !EditManager.IgnoreControlRelinquished) yield break;
                if (!_walking) break;
                body.linearVelocityX = speed;
                yield return null;
            }
        }

        _walking = false;
        EditManager.IgnoreControlRelinquished = false;
        
        hero.RegainControl();
        hero.StartAnimationControl();
        
        gameObject.BroadcastEvent("OnFinish");

        if (anim == "Sprint") hero.sprintFSM.SendEvent("SKID END");
    }
}
