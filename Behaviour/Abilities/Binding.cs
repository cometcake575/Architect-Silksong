using Architect.Behaviour.Custom;
using Architect.Content.Custom;
using Architect.Events;
using Architect.Utils;
using UnityEngine;

namespace Architect.Behaviour.Abilities;

public class Binding : SoundMaker
{
    public string bindingType;
    public Sprite disabledSprite;
    public Sprite enabledSprite;
    public bool active;
    public bool reversible;
    public bool uiVisible = true;
    
    private SpriteRenderer _renderer;
    private bool _used;

    private static AudioClip _use;

    public static void Init()
    {
        ResourceUtils.LoadClipResource("Bindings.use", clip => _use = clip);
    }

    private void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
        
        if (!AbilityObjects.ActiveBindings.TryGetValue(bindingType, out var binders))
        {
            binders = [];
            AbilityObjects.ActiveBindings[bindingType] = binders;
        }
        binders.Add(this);

        if (uiVisible)
        {
            if (!AbilityObjects.ActiveVisibleBindings.TryGetValue(bindingType, out var b2))
            {
                b2 = [];
                AbilityObjects.ActiveVisibleBindings[bindingType] = b2;
            }

            b2.Add(this);
        }

        _renderer.sprite = active ? enabledSprite : disabledSprite;
        
        OnToggle();
    }

    private void OnEnable()
    {
        OnToggle();
    }

    private void OnDisable()
    {
        OnToggle();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_used) return;
        if (!other.gameObject.GetComponent<HeroController>()) return;

        active = !active;

        if (active)
        {
            _renderer.sprite = enabledSprite;
            EventManager.BroadcastEvent(gameObject, "OnBind");
        }
        else
        {
            _renderer.sprite = disabledSprite;
            EventManager.BroadcastEvent(gameObject, "OnUnbind");
        }
        
        PlaySound(_use, 2);
        OnToggle();

        if (reversible) return;
        _used = true;
        _renderer.enabled = false;
    }

    public virtual void OnToggle()
    {
        AbilityObjects.RefreshBindingUI();
    }
}