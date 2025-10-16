using System;
using System.Collections.Generic;
using Architect.Behaviour.Custom;
using Architect.Content.Custom;
using Architect.Utils;
using UnityEngine;

namespace Architect.Behaviour.Abilities;

public class AbilityCrystal : SoundMaker
{
    public int count;
    public string type;
    public bool singleUse;
    public float regenTime;

    private SpriteRenderer _renderer;
    private FloatAnim _float;
    
    private Sprite _activeSprite;
    private Sprite _inactiveSprite;

    private float _remainingTime;
    
    private static readonly Sprite SingleUsed = 
        ResourceUtils.LoadSpriteResource("Crystals.used_s", FilterMode.Point, ppu:15);
    private static readonly Sprite DoubleUsed = 
        ResourceUtils.LoadSpriteResource("Crystals.used_m", FilterMode.Point, ppu:15);
    private static readonly Sprite TripleUsed = 
        ResourceUtils.LoadSpriteResource("Crystals.used_l", FilterMode.Point, ppu:15);

    private static AudioClip _use;
    private static AudioClip _regen;

    public static void Init()
    {
        ResourceUtils.LoadClipResource("Crystals.use", clip => _use = clip);
        ResourceUtils.LoadClipResource("Crystals.regen", clip => _regen = clip);
    }

    private void Start()
    {
        _renderer = GetComponentInChildren<SpriteRenderer>();
        _float = GetComponentInChildren<FloatAnim>();
        
        _activeSprite = _renderer.sprite;
        _inactiveSprite = count switch
        {
            1 => SingleUsed,
            2 => DoubleUsed,
            _ => TripleUsed
        };
        
        if (singleUse) _renderer.color = new Color(1, 0.5f, 0.5f, 1);
    }

    private void Update()
    {
        if (_remainingTime > 0)
        {
            _remainingTime -= Time.deltaTime;
            if (_remainingTime <= 0)
            {
                _renderer.sprite = _activeSprite;
                gameObject.BroadcastEvent("OnRegen");
                _float.active = true;
                PlaySound(_regen);
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (_remainingTime > 0) return;
        if (!other.gameObject.GetComponent<HeroController>()) return;

        AbilityObjects.ActiveCrystals[type] = Math.Max(count, AbilityObjects.ActiveCrystals.GetValueOrDefault(type, 0));
        
        if (singleUse) gameObject.SetActive(false);
        else if (regenTime > 0)
        {
            _renderer.sprite = _inactiveSprite;
            _remainingTime = regenTime;
            _float.active = false;
        }
        
        gameObject.BroadcastEvent("OnCollect");
        
        PlaySound(_use);
        
        AbilityObjects.RefreshCrystalUI();
    }
}