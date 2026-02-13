using System.Collections;
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
    public bool directSet;

    public float r;
    public float g;
    public float b;
    public float a;

    public bool startApplied;
    public bool particles;
    
    public GameObject target;

    private Color _color;
    private bool _setup;

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

    public IEnumerator DoApply(float fadeTime, Color color, bool forceAlpha)
    {
        var lk = target.GetOrAddComponent<MiscFixers.ColorLock>();
        if (lk.permanent) yield break;
        lk.enabled = false;

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
        
        if (particles) foreach (var renderer in target.GetComponentsInChildren<ParticleSystem>(true))
        {
            _current++;
            StartCoroutine(FadeRoutine(fadeTime, renderer, color, useAlphaByDefault || forceAlpha));
        }

        yield return new WaitUntil(() => _current == 0);
        lk.enabled = true;
    }

    private IEnumerator FadeRoutine(float fadeTime, SpriteRenderer sr, Color color, bool useAlpha)
    {
        var time = 0f;
        
        var start = sr.color;
        var end = directSet ? color : start * color;
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
        var end = directSet ? color : start * color;
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
        var end = directSet ? color : start * color;
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