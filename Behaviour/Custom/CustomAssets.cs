using System.Collections;
using System.Collections.Generic;
using Architect.Behaviour.Fixers;
using Architect.Storage;
using Architect.Utils;
using BepInEx;
using UnityEngine;
using UnityEngine.Video;

namespace Architect.Behaviour.Custom;

public interface IPlayable
{
    public void Play();
    public void Pause();
    public void Reset();
}

public class PngObject : MonoBehaviour, IPlayable
{
    private SpriteRenderer _renderer;
    private Sprite[] _sprites;
    
    private float _remainingFrameTime;
    private int _frame;
    
    public string url;
    public bool point;
    public bool glow = true;
    public float ppu = 100;
    private int _count = 1;
    public int vcount = 1;
    public int hcount = 1;
    public int dummy;
    public float frameTime = 1;
    public bool playing;

    private void Awake()
    {
        if (string.IsNullOrEmpty(url)) return;
        
        _renderer = GetComponent<SpriteRenderer>();
        if (!glow) _renderer.material = MiscFixers.SpriteMaterial;
        CustomAssetManager.DoLoadSprite(url, point, ppu, hcount, vcount, SaveSprites);
        _count = Mathf.Max(1, hcount * vcount - dummy);

        var anim = GetComponent<Animator>();
        if (anim) anim.enabled = false;
    }

    public void SaveSprites(Sprite[] newSprites)
    {
        if (!_renderer) return;
        _sprites = newSprites;
        _renderer.sprite = _sprites[0];

        if (frameTime == 0) _count = 1;
        else _remainingFrameTime = frameTime;
    }

    private void Update()
    {
        if (_count <= 1 || !playing) return;
        _remainingFrameTime -= Time.deltaTime;
        while (_remainingFrameTime < 0)
        {
            _remainingFrameTime += frameTime;
            _frame++;
            if (_frame >= _count)
            {
                gameObject.BroadcastEvent("OnFinish");
                _frame %= _count;
            }

            if (playing) _renderer.sprite = _sprites[_frame];
        }
    }

    public void Play()
    {
        playing = true;
        _renderer.sprite = _sprites[_frame];
    }

    public void Pause() => playing = false;

    public void Reset()
    {
        _frame = 0;
        _renderer.sprite = _sprites[0];
    }
}

public class Mp4Object : MonoBehaviour, IPlayable
{
    public string url;
    public bool playOnStart = true;
    private VideoPlayer _player;

    private bool _shouldPlay = true;
    private bool _playing = true;

    private bool _started;

    private void Awake()
    {
        if (string.IsNullOrEmpty(url)) return;
        _player = gameObject.GetComponent<VideoPlayer>();
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

    private float _volume = 1;
    private float _gmVol = GameManager.instance ? GameManager.instance.GetImplicitCinematicVolume() : 1;

    public float Volume
    {
        get => _volume;
        set
        {
            _gmVol = GameManager.instance.GetImplicitCinematicVolume();
            Source.volume = value * _gmVol;
            _volume = value;
        }
    }

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
        CustomAssetManager.DoLoadSound(gameObject, url);
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
            Source.Play();
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
        if (!Mathf.Approximately(_gmVol, GameManager.instance.GetImplicitCinematicVolume())) Volume = Volume;
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
