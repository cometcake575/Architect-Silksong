using Architect.Utils;
using UnityEngine;

namespace Architect.Behaviour.Custom;

public class WhiteSpikes : SoundMaker
{
    private const float BOT_Y = -5.5f;
    private const float TOP_Y = 0;
    
    private const float UP_TIME = 0.35f;
    private const float DOWN_TIME = 0.35f;

    private float _target;

    private float _bouncePoint;
    
    public float shiftDelay;
    public bool up;
    public float speed;

    private static readonly Sprite Top = ResourceUtils.LoadSpriteResource("Spikes.spikes_top", ppu:64);
    private static readonly Sprite Moving = ResourceUtils.LoadSpriteResource("Spikes.spikes_moving", ppu:64);

    private static AudioClip _upSound;
    private static AudioClip _downSound;

    private SpriteRenderer _renderer;
    private Collider2D _damageCollider;

    public static void Init()
    {
        ResourceUtils.LoadClipResource("Spikes.spikes_up", clip => _upSound = clip);
        ResourceUtils.LoadClipResource("Spikes.spikes_down", clip => _downSound = clip);
    }

    private void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _damageCollider = GetComponentInChildren<Collider2D>();

        _renderer.sprite = shiftDelay > 0 ? Moving : Top;

        if (!up)
        {
            transform.SetLocalPositionY(BOT_Y);
            _target = TOP_Y;
        }
        else _target = BOT_Y;
    }

    private void Update()
    {
        if (_bouncePoint > 0)
        {
            transform.SetLocalPositionY(Mathf.Sin(Mathf.Deg2Rad * _bouncePoint) / 5);
            _bouncePoint -= Time.deltaTime * 2500 * speed;
            if (_bouncePoint <= 0) _renderer.sprite = Top;
            return;
        }
        
        if (shiftDelay > 0)
        {
            shiftDelay -= Time.deltaTime * speed;
            if (shiftDelay <= 0)
            {
                if (up)
                {
                    _bouncePoint = 180;
                    PlaySound(_downSound, 5);
                    _target = BOT_Y;
                    return;
                }

                _damageCollider.enabled = true;
                _renderer.sprite = Moving;
                PlaySound(_upSound, 5);
                _target = TOP_Y;
            }
            else return;
        }
        
        transform.SetLocalPositionY(transform.localPosition.y + (up ? -speed : speed) * Time.deltaTime * 35);

        if (_target < 0)
        {
            if (transform.localPosition.y <= BOT_Y)
            {
                transform.SetLocalPositionY(BOT_Y);
                
                shiftDelay = DOWN_TIME;
                up = false;

                _damageCollider.enabled = false;
            }
        }
        else
        {
            if (transform.localPosition.y >= TOP_Y)
            {
                transform.SetLocalPositionY(TOP_Y);
                
                _bouncePoint = 180;
                
                shiftDelay = UP_TIME;
                up = true;
            }
        }
    }
}