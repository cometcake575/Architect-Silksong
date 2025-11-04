using System;
using System.Collections;
using Architect.Editor;
using Architect.Utils;
using MonoMod.RuntimeDetour;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class AnimPlayer : MonoBehaviour
{
    private static AnimPlayer _active;
    
    public string clipName;

    public bool takeCtrl;
    private bool _tookCtrl;
    
    public bool overrideAnimTime;
    public float animTime;

    private float _animTimeRemaining;

    public static void Init()
    {
        _ = new Hook(typeof(HeroAnimationController).GetProperty(nameof(HeroAnimationController.controlEnabled))
            !.GetGetMethod(), (Func<HeroAnimationController, bool> orig, HeroAnimationController self) => 
            !_active && orig(self));
        
        typeof(tk2dSpriteAnimator).Hook(nameof(tk2dSpriteAnimator.Play),
            (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, float, float> orig, 
                tk2dSpriteAnimator self, tk2dSpriteAnimationClip clip, float clipStartTime, float overrideFps) =>
            {
                if (_active && self.gameObject == HeroController.instance.gameObject) return;
                orig(self, clip, clipStartTime, overrideFps);
            }, typeof(tk2dSpriteAnimationClip), typeof(float), typeof(float));
    }

    public void Play()
    {
        if (_active) return;
        StartCoroutine(DoPlay());
    }

    private IEnumerator DoPlay()
    {
        var hero = HeroController.instance;
        
        var clip = hero.animCtrl.GetClip(clipName);
        if (clip == null) yield break;
        
        _animTimeRemaining = overrideAnimTime ? animTime : clip.Duration;
        hero.animCtrl.PlayClipForced(clipName);

        _active = this;

        if (takeCtrl)
        {
            yield return hero.FreeControl();
            if (_active != this) yield break;
            _tookCtrl = true;
            EditManager.IgnoreControlRelinquished = true;
            hero.RelinquishControl();
        }
    }

    private void Update()
    {
        if (_animTimeRemaining <= 0) return;

        _animTimeRemaining -= Time.deltaTime;
        if (_animTimeRemaining <= 0) Stop();
    }

    public void Stop()
    {
        if (_active == this)
        {
            _active = null;
            _animTimeRemaining = 0;
            
            gameObject.BroadcastEvent("OnFinish");

            if (_tookCtrl)
            {
                _tookCtrl = false;
                EditManager.IgnoreControlRelinquished = false;
                HeroController.instance.RegainControl();
            }
        }
    }

    public void OnDisable() => Stop();
}