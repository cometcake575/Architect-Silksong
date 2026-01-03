using System.Collections;
using System.Reflection;
using Architect.Utils;
using UnityEngine;

namespace Architect.Behaviour.Custom;

public class SilkSphere : SoundMaker, IHitResponder
{
    private SpriteRenderer _renderer;
    private Color _targetColor;
    
    private static readonly Color Dim = new(0.7f, 0.7f, 0.7f, 1);
    private static AudioClip _use;

    public static void Init()
    {
        ResourceUtils.LoadClipResource("silk_sphere", clip => _use = clip);
    }
    
    private void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _renderer.color = Dim;
        _targetColor = Dim;
    }

    private void Update()
    {
        _renderer.color = Color.Lerp(_renderer.color, _targetColor, Time.deltaTime * 10);
    }

    private IEnumerator FlashWhite()
    {
        _targetColor = Color.white;
        yield return new WaitForSeconds(0.08f);
        _targetColor = Dim;
    }

    public IHitResponder.HitResponse Hit(HitInstance damageInstance)
    {
        if (!damageInstance.IsNailDamage || !damageInstance.IsHeroDamage) return IHitResponder.Response.None;
        
        HeroController.instance.AddSilk(1, true);
        StartCoroutine(FlashWhite());
        PlaySound(_use, 10, pitch:Random.Range(1, 1.2f));

        return IHitResponder.Response.GenericHit;
    }
}