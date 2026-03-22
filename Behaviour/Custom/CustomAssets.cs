using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Architect.Behaviour.Fixers;
using Architect.Behaviour.Utility;
using Architect.Storage;
using Architect.Utils;
using BepInEx;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Video;

namespace Architect.Behaviour.Custom;

public interface IPlayable
{
    public void Play();
    public void Pause();
    public void Reset();
}

public class PngObject : PreviewableBehaviour, IPlayable
{
    private SpriteRenderer _renderer;
    protected Sprite[] Sprites;
    
    private float _remainingFrameTime;
    
    public int frame;
    public string url;
    public bool point;
    public bool ignoreGlow;
    public bool glow = true;
    public float ppu = 100;
    private int _count = 1;
    public int vcount = 1;
    public int hcount = 1;
    public int dummy;
    public float frameTime = 1;
    public bool playing;
    public bool loop = true;

    private void Awake()
    {
        if (string.IsNullOrEmpty(url)) return;
        
        _renderer = GetComponent<SpriteRenderer>();
        if (ignoreGlow) glow = true;
        if (!glow && _renderer) _renderer.material = MiscFixers.SpriteMaterial;
        CustomAssetManager.DoLoadSprite(url, point, ppu, hcount, vcount, SaveSprites);
        _count = Mathf.Max(1, hcount * vcount - dummy);

        var anim = GetComponent<Animator>();
        if (anim) anim.enabled = false;
    }

    public void SaveSprites(Sprite[] newSprites)
    {
        Sprites = newSprites;
        if (_renderer) _renderer.sprite = Sprites[0];

        _remainingFrameTime = frameTime;
    }

    private void Update()
    {
        if (frameTime <= 0 || _count <= 1 || !playing || !_renderer || Sprites == null) return;
        _remainingFrameTime -= Time.deltaTime;
        while (_remainingFrameTime < 0 && frameTime > 0)
        {
            _remainingFrameTime += frameTime;
            frame++;
            gameObject.BroadcastEvent("OnFrameChange");
            if (frame >= _count)
            {
                gameObject.BroadcastEvent("OnFinish");
                frame %= _count;
                if (!loop)
                {
                    playing = false;
                    return;
                }
            }

            _renderer.sprite = Sprites[frame];
        }
    }

    public void Play()
    {
        playing = true;
        if (!_renderer || Sprites == null) return;
        _renderer.sprite = Sprites[frame];
    }

    public void Pause() => playing = false;

    public void Reset()
    {
        frame = 0;
        if (_renderer) _renderer.sprite = Sprites[0];
    }

    public void SetFrame(int newFrame)
    {
        frame = newFrame;
            if (frame >= _count)
            {
                gameObject.BroadcastEvent("OnFinish");
                frame %= _count;
                if (!loop)
                {
                    playing = false;
                    return;
                }
            }

            _renderer.sprite = Sprites[frame];
    }
}

public class ParticleObject : PngObject
{
    private void Start()
    {
        foreach (var psr in GetComponentsInChildren<ParticleSystemRenderer>(true))
        {
            if (Sprites.IsNullOrEmpty() || !psr.material) return;
            psr.material.mainTexture = Sprites[0].texture;
        }
    }
}

public class UIPngObject : PngObject
{
    public float xOffset;
    
    public float yOffset;

    public int anchorTo;

    private static readonly int UILayer = LayerMask.NameToLayer("UI");

    private PositionConstraint _constraint;
    private SpriteRenderer _sr;
    private GameObject _par;

    private static readonly List<UIPngObject> Pngs = [];

    public bool ignoreHudOut;

    private static bool _hudOut;

    public static void Init()
    {
        typeof(GameCameras).Hook(nameof(GameCameras.HUDIn),
            (Action<GameCameras> orig, GameCameras self) =>
            {
                _hudOut = false;
                Pngs.RemoveAll(png => !png);
                foreach (var png in Pngs.Where(png => png._constraint && png.ignoreHudOut)) png._constraint.constraintActive = true;
                orig(self);
            });
        
        typeof(GameCameras).Hook(nameof(GameCameras.HUDOut),
            (Action<GameCameras> orig, GameCameras self) =>
            {
                _hudOut = true;
                Pngs.RemoveAll(png => !png);
                foreach (var png in Pngs.Where(png => png._constraint && png.ignoreHudOut)) png._constraint.constraintActive = false;
                orig(self);
            });
    }

    public void Start()
    {
        gameObject.layer = UILayer;
        var anchor = GameCameras.instance.hudCamera.transform
            .Find("In-game").Find("Anchor TL").Find("Hud Canvas Offset").Find("Hud Canvas");

        if (isAPreview && transform.parent)
        {
            _par = transform.parent.gameObject;
            transform.parent = null;
        }
        _constraint = gameObject.AddComponent<PositionConstraint>();
        Pngs.Add(this);
        _constraint.constraintActive = true;
        
        var source = new ConstraintSource
        {
            sourceTransform = anchor,
            weight = 1
        };
        
        _constraint.AddSource(source);

        _sr = GetComponent<SpriteRenderer>();
        _sr.sortingLayerName = "Over";
        
        UpdatePos();
        if (_hudOut && _constraint)
        {
            _constraint.constraintActive = false;
            transform.position = new Vector3(-10.3535f, 7.533f, 38.1f) + _constraint.translationOffset;
        }
    }

    private void OnEnable()
    {
        Pngs.AddIfNotPresent(this);
        if (_hudOut && _constraint) _constraint.constraintActive = false;
    }

    private bool _previewing;

    private void UpdatePos()
    {
        var offset = new Vector2(xOffset + 10.3535f, yOffset - 6.81f);
        
        switch (anchorTo)
        {
            case 1:
                offset.x += 0.94f * PlayerData.instance.maxHealth;
                break;
            case 2:
                if (!ToolItemManager.Instance) return;
                if (ToolItemManager.Instance.boundAttackTools == null) return;
                
                var hc = GameCameras.instance.hudCamera.transform
                    .Find("In-game").Find("Anchor TL").Find("Hud Canvas Offset").Find("Hud Canvas");
                var spool = hc.Find("Tool Icons");
                offset.x += 1.02f * 
                            ToolItemManager.Instance.boundAttackTools.Count(o => o) 
                            + spool.transform.GetPositionX();
                break;
        }

        _constraint.translationOffset = offset;
    }
    
    private void LateUpdate()
    {
        if (anchorTo != 0) UpdatePos();
        if (isAPreview)
        {
            if (!_par)
            {
                Destroy(gameObject);
                return;
            }
            if (_previewing != Settings.Preview.IsPressed)
            {
                _previewing = !_previewing;
                _sr.enabled = _previewing;
            }
        }
    }
}

public class Mp4Object : MonoBehaviour, IPlayable
{
    public string url;
    public bool playOnStart = true;
    public bool glow;
    private VideoPlayer _player;

    private bool _shouldPlay = true;
    private bool _playing = true;

    private bool _started;

    private void Awake()
    {
        if (string.IsNullOrEmpty(url)) return;
        if (!glow) GetComponent<SpriteRenderer>().material = MiscFixers.SpriteMaterial;
        _player = gameObject.GetComponent<VideoPlayer>();
        _player.loopPointReached += _ => gameObject.BroadcastEvent("OnFinish"); 
        CustomAssetManager.DoLoadVideo(_player, transform.GetScaleX(), url);
    }

    // This is used instead of playOnAwake so the first frame is displayed
    private void Update()
    {
        if (!_started)
        {
            _started = true;
            if (!playOnStart) Pause();
        }

        var paused = GameManager.instance.isPaused;
        if (!_shouldPlay) return;
        
        switch (paused)
        {
            case true when _playing:
                _playing = false;
                _player.Pause();
                break;
            case false when !_playing:
                _playing = true;
                _player.Play();
                break;
        }
    }

    public void Play()
    {
        _player.Play();
        _shouldPlay = true;
        _playing = true;
    }

    public void Pause()
    {
        _player.Pause();
        _shouldPlay = false;
        _playing = false;
    }

    public void Reset()
    {
        _player.time = 0;
    }
}

public class WavObject : SoundMaker, IPlayable
{
    public string url;
    public AudioClip sound;
    
    public bool isMusic;

    private float GmVol =>
        (isMusic ? GameManager.instance.gameSettings.musicVolume : GameManager.instance.gameSettings.masterVolume) / 10;

    private float _gmVol = 1;

    public float Volume
    {
        get;
        set
        {
            _gmVol = GmVol;
            Source.volume = value * _gmVol;
            field = value;
        }
    } = 1;

    public float pitch = 1;
    public bool globalSound = true;
    public bool loop;

    private bool _started;
    private bool _playing;
    public string syncId;

    private static readonly Dictionary<string, float> Syncs = []; 

    protected void Start()
    {
        if (string.IsNullOrEmpty(url)) return;
        CustomAssetManager.DoLoadSound(url, clip =>
        {
            sound = clip;
            sound.LoadAudioData();
        });
    }

    public void Play()
    {
        StartCoroutine(PlayWhenReady());
    }

    private IEnumerator PlayWhenReady()
    {
        yield return new WaitUntil(() => !this || sound);
        if (!this) yield break;
        _playing = true;
        if (_started && Source.time < sound.length)
        {
            Source.PlayScheduled(AudioSettings.dspTime);
            yield break;
        }
        _started = true;
        PlaySound(sound, Volume, pitch, globalSound, loop);
        
        if (!syncId.IsNullOrWhiteSpace() && Syncs.TryGetValue(syncId, out var v))
        {
            Source.time = v;
        }
    }

    public void Pause()
    {
        _playing = false;
        Source.Pause();
    }

    public void Reset()
    {
        _started = false;
        var play = _playing;
        Source.Stop();
        if (!syncId.IsNullOrWhiteSpace())
        {
            Syncs.Remove(syncId);
        }
        if (play) Play();
    }

    private void Update()
    {
        if (!Mathf.Approximately(_gmVol, GmVol)) Volume = Volume;
        if (!syncId.IsNullOrWhiteSpace() && _started)
        {
            Syncs[syncId] = Source.time;
        }
    }
}

public class PngPreview : MonoBehaviour
{
    public float ppu = 100;
    
    public bool point;

    public int vcount = 1;
    
    public int hcount = 1;
}
