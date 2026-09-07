using System.Collections;
using Architect.Behaviour.Fixers;
using Architect.Placements;
using Architect.Utils;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class MenuForegroundMarker : MonoBehaviour
{
    public string id = string.Empty;

    public void Setup()
    {
        if (!PlacementManager.TryGetValue(id, out var obj)) return;

        foreach (var o in obj.GetComponentsInChildren<Transform>(true))
        {
            o.gameObject.AddComponent<MenuFader>();
        }
    }

    public class MenuFader : MonoBehaviour
    {
        private float _baseAlpha;

        private MeshRenderer _mr;
        private SpriteRenderer _sr;
        private tk2dSprite _tk;

        private bool _started;

        private void Setup()
        {
            _started = true;
            gameObject.RemoveComponent<MiscFixers.ColorLock>();
            
            _sr = GetComponent<SpriteRenderer>();
            if (_sr)
            {
                _baseAlpha = _sr.color.a;
                return;
            }
            
            _tk = GetComponent<tk2dSprite>();
            if (_tk)
            {
                _baseAlpha = _tk.color.a;
                return;
            }
            
            _mr = GetComponent<MeshRenderer>();
            if (_mr)
            {
                _baseAlpha = _mr.material.color.a;
            }
        }
        
        public void FadeTo(float amount, float time)
        {
            if (!_started) Setup();
            
            if (_sr) StartCoroutine(DoFadeTo(_sr, _baseAlpha * amount, time));
            if (_mr) StartCoroutine(DoFadeTo(_mr, _baseAlpha * amount, time));
            if (_tk) StartCoroutine(DoFadeTo(_tk, _baseAlpha * amount, time));
        }

        private static IEnumerator DoFadeTo(SpriteRenderer sr, float target, float time)
        {
            var start = sr.color.a;
            var elapsed = 0f;
            while (elapsed < time)
            {
                sr.color = sr.color.Where(a: Mathf.Lerp(start, target, Mathf.Clamp01(elapsed / time)));
                yield return null;
                elapsed += Time.deltaTime;
            }

            sr.color = sr.color.Where(a: target);
        }

        private static IEnumerator DoFadeTo(MeshRenderer mr, float target, float time)
        {
            var start = mr.material.color.a;
            var elapsed = 0f;
            while (elapsed < time)
            {
                mr.material.color = mr.material.color.Where(a: Mathf.Lerp(start, target, Mathf.Clamp01(elapsed / time)));
                yield return null;
                elapsed += Time.deltaTime;
            }

            mr.material.color = mr.material.color.Where(a: target);
        }

        private static IEnumerator DoFadeTo(tk2dSprite tk, float target, float time)
        {
            var start = tk.color.a;
            var elapsed = 0f;
            while (elapsed < time)
            {
                tk.color = tk.color.Where(a: Mathf.Lerp(start, target, Mathf.Clamp01(elapsed / time)));
                yield return null;
                elapsed += Time.deltaTime;
            }

            tk.color = tk.color.Where(a: target);
        }
    }
}