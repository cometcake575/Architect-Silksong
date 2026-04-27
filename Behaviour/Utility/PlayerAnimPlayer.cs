using System;
using System.Collections;
using Architect.Editor;
using Architect.Events.Blocks;
using Architect.Utils;
using MonoMod.RuntimeDetour;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public interface IAnimPlayer
{
    public void Play();
    public void Stop();

    public string GetClip();
}

public class AnimPlayer : MonoBehaviour, IAnimPlayer
{
    public tk2dSpriteAnimator animator;

    public bool isLocked;
    public tk2dSpriteAnimationClip clip;
    
    public bool overrideAnimTime;
    public float animTime;

    private float _animTimeRemaining;
    
    public string GetClip() => animator ? animator.currentClip?.name ?? "" : "";
    
    public static void Init()
    {
        typeof(tk2dSpriteAnimator).Hook(nameof(tk2dSpriteAnimator.Play),
            (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, float, float> orig, 
                tk2dSpriteAnimator self, tk2dSpriteAnimationClip clip, float clipStartTime, float overrideFps) =>
            {
                var player = self.GetComponent<AnimPlayer>();
                if (player && player.isLocked) return;
                orig(self, clip, clipStartTime, overrideFps);
            }, typeof(tk2dSpriteAnimationClip), typeof(float), typeof(float));
    }
    
    public void Play()
    {
        _animTimeRemaining = overrideAnimTime ? animTime : clip.Duration;
        animator.Play(clip);
        isLocked = true;
    }

    public void Stop()
    {
        isLocked = false;
    }

    private void Update()
    {
        if (_animTimeRemaining <= 0) return;

        _animTimeRemaining -= Time.deltaTime;
        if (_animTimeRemaining <= 0) Stop();
    }
}

public class PlayerAnimPlayer : MonoBehaviour, IAnimPlayer
{
    public ScriptBlock Block;
    
    private static PlayerAnimPlayer _active;
    
    public string clipName;

    public bool takeCtrl;
    public bool clearXVel = true;
    public bool clearYVel;
    private bool _tookCtrl;
    
    public bool overrideAnimTime;
    public float animTime;

    private float _animTimeRemaining;
    
    public string GetClip() => HeroController.instance.animCtrl.animator.currentClip?.name ?? "";

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
            yield return hero.FreeControl(hc => hc.rb2d.linearVelocity == Vector2.zero);
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
        if (clearXVel) HeroController.instance.rb2d.linearVelocityX = 0;
        if (clearYVel) HeroController.instance.rb2d.linearVelocityY = 0;
        if (_animTimeRemaining <= 0) Stop();
    }

    public void Stop()
    {
        if (_active == this)
        {
            _active = null;
            _animTimeRemaining = 0;

            if (_tookCtrl)
            {
                _tookCtrl = false;
                EditManager.IgnoreControlRelinquished = false;
                HeroController.instance.RegainControl();
            }

            if (Block != null) Block.Event("Stop");
            else gameObject.BroadcastEvent("OnFinish");
        }
    }

    public void OnDisable() => Stop();
}