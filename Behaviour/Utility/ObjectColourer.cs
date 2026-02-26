using System;
using System.Collections;
using System.Linq;
using Architect.Behaviour.Fixers;
using Architect.Events.Blocks.Objects;
using Architect.Placements;
using Architect.Prefabs;
using Architect.Utils;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class ObjectColourer : MonoBehaviour
{
    public string targetId;

    public bool useAlphaByDefault;
    public int mode;

    public float r;
    public float g;
    public float b;
    public float a;

    public bool startApplied;
    public bool particles;
    
    public GameObject target;

    private Color _color;
    private bool _setup;

    public static void Init()
    {
        typeof(SpriteFlash).Hook(nameof(SpriteFlash.Awake),
            (Action<SpriteFlash> orig, SpriteFlash self) =>
            {
                self.children ??= [];
                self.parents ??= [];
                orig(self);
            });
        
        typeof(SpriteFlash).Hook(nameof(SpriteFlash.FlashRoutine), FlashRoutine);
    }

    private static IEnumerator FlashRoutine(
        Func<SpriteFlash, float,
            float,
            float,
            float,
            float,
            bool,
            int,
            SpriteFlash.RepeatingFlash, IEnumerator> orig,
        SpriteFlash self,
        float amount,
        float timeUp,
        float stayTime,
        float timeDown,
        float stayDownTime,
        bool repeating,
        int repeatTimes,
        SpriteFlash.RepeatingFlash repeatingFlash)
    {
        var sfo = self.GetComponent<SpriteFlashOld>();
        if (sfo && Mathf.Approximately(sfo.old.a, 1)) yield break;
        var o = orig(self, amount, timeUp, stayTime, timeDown, stayDownTime, repeating, repeatTimes, repeatingFlash);
        while (o.MoveNext()) yield return o.Current;
        if (sfo) self.SetParams(sfo.old.a, sfo.old);
    }

    private void Update()
    {
        if (!_setup)
        {
            _setup = true;
            _color = new Color(r, g, b, a);

            GameObject t;
            if (target) t = target;
            else if (!PlacementManager.Objects.TryGetValue(targetId, out t)) return;
            
            var prefab = t.GetComponent<Prefab>();
            if (prefab)
            {
                foreach (var spawn in prefab.spawns)
                {
                    var go = Instantiate(gameObject);
                    go.transform.position = transform.position - t.transform.position + spawn.transform.position;
                    go.GetComponent<ObjectColourer>().target = spawn;
                    
                    var obrs = GetComponents<ObjectBlock.ObjectBlockReference>();
                    foreach (var obr in obrs)
                    {
                        obr.Spawns.Add(go);
                        go.AddComponent<ObjectBlock.ObjectBlockReference>().Block = obr.Block;
                    }
                }
                return;
            }

            target = t;
            
            if (startApplied) Apply(0);
        }
    }

    public void Apply(float fadeTime, Color? color = null)
    {
        if (!target) return;
        StartCoroutine(DoApply(fadeTime, color ?? _color, color.HasValue));
    }

    public void StopFade()
    {
        StopAllCoroutines();
        _current = 0;
    }

    private int _current;

    private static readonly Shader FlashShader = 
        Resources.FindObjectsOfTypeAll<Shader>()
        .First(o => o.name == "Sprites/Default-ColorFlash");

    public IEnumerator DoApply(float fadeTime, Color color, bool forceAlpha)
    {
        var lk = target.GetOrAddComponent<MiscFixers.ColorLock>();
        if (lk.permanent) yield break;
        lk.enabled = false;

        if (mode == 2)
        {
            if (!forceAlpha && !useAlphaByDefault) color.a = 1;
            foreach (var rend in target.GetComponentsInChildren<Renderer>())
            {
                rend.material.shader = FlashShader;
                var sf = rend.gameObject.GetOrAddComponent<SpriteFlash>();
                StartCoroutine(FadeRoutine(fadeTime, sf, color));
            }
        }
        else
        {
            foreach (var sr in target.GetComponentsInChildren<SpriteRenderer>(true))
            {
                _current++;
                StartCoroutine(FadeRoutine(fadeTime, sr, color, useAlphaByDefault || forceAlpha));
            }

            foreach (var sr in target.GetComponentsInChildren<tk2dSprite>(true))
            {
                _current++;
                StartCoroutine(FadeRoutine(fadeTime, sr, color, useAlphaByDefault || forceAlpha));
            }

            if (particles)
                foreach (var renderer in target.GetComponentsInChildren<ParticleSystem>(true))
                {
                    _current++;
                    StartCoroutine(FadeRoutine(fadeTime, renderer, color, useAlphaByDefault || forceAlpha));
                }
        }

        yield return new WaitUntil(() => _current == 0);
        lk.enabled = true;
    }

    private class SpriteFlashOld : MonoBehaviour
    {
        public Color old = Color.clear;
    }
    
    private IEnumerator FadeRoutine(float fadeTime, SpriteFlash sr, Color color)
    {
        var time = 0f;

        var old = sr.gameObject.GetOrAddComponent<SpriteFlashOld>();
        var start = old.old;

        while (time < fadeTime)
        {
            if (!sr) yield break;
            var col = Color.Lerp(start, color, time / fadeTime);
            sr.SetParams(col.a, col);
            old.old = col;
            time += Time.deltaTime;
            yield return null;
        }

        sr.SetParams(color.a, color);
        old.old = color;
        _current--;
    }
    
    private IEnumerator FadeRoutine(float fadeTime, SpriteRenderer sr, Color color, bool useAlpha)
    {
        var time = 0f;
        
        var start = sr.color;
        var end = mode == 0 ? start * color : color;
        if (!useAlpha) end.a = start.a;
        
        while (time < fadeTime)
        {
            if (!sr) yield break;
            sr.color = Color.Lerp(start, end, time / fadeTime);
            time += Time.deltaTime;
            yield return null;
        }
        
        sr.color = end;
        _current--;
    }

    private IEnumerator FadeRoutine(float fadeTime, tk2dSprite sr, Color color, bool useAlpha)
    {
        var time = 0f;
        
        var start = sr.color;
        var end = mode == 0 ? start * color : color;
        if (!useAlpha) end.a = start.a;
        
        while (time < fadeTime)
        {
            if (!sr) yield break;
            sr.color = Color.Lerp(start, end, time / fadeTime);
            time += Time.deltaTime;
            yield return null;
        }
        
        sr.color = end;
        _current--;
    }

    private IEnumerator FadeRoutine(float fadeTime, ParticleSystem sr, Color color, bool useAlpha)
    {
        var time = 0f;
        
        var main = sr.main;
        
        var start = main.startColor.color;
        var end = mode == 0 ? start * color : color;
        if (!useAlpha) end.a = start.a;
        
        while (time < fadeTime)
        {
            if (!sr) yield break;
            main.startColor = Color.Lerp(start, end, time / fadeTime);
            time += Time.deltaTime;
            yield return null;
        }
        
        main.startColor = end;
        _current--;
    }
}