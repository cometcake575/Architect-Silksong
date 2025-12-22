using Architect.Behaviour.Fixers;
using Architect.Storage;
using Architect.Utils;
using UnityEngine;
using UnityEngine.Video;

namespace Architect.Behaviour.Custom;

public interface IPlayable
{
    public void Play();
}

public interface IPausable
{
    public void Pause();
    public void Reset();
}

public class PngObject : MonoBehaviour, IPlayable, IPausable
{
    private SpriteRenderer _renderer;
    private Sprite[] _sprites;
    
    private float _remainingFrameTime;
    private int _frame;
    
    public string url;
    public bool point;
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
        _renderer.material = MiscFixers.SpriteMaterial;
        CustomAssetManager.DoLoadSprite(url, point, ppu, hcount, vcount, SaveSprites);
        _count = Mathf.Max(1, hcount * vcount - dummy);

        var anim = GetComponent<Animator>();
        if (anim) anim.enabled = false;
    }

    public void SaveSprites(Sprite[] newSprites)
    {
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

public class Mp4Object : MonoBehaviour, IPlayable, IPausable
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
    
    public float volume = 1;
    public float pitch = 1;
    public bool globalSound = true;
    public bool loop;
    
    protected void Start()
    {
        if (string.IsNullOrEmpty(url)) return;
        CustomAssetManager.DoLoadSound(gameObject, url);
    }

    public void Play()
    {
        if (!sound) return;
        PlaySound(sound, volume, pitch, globalSound, loop);
    }
}

public class PngPreview : MonoBehaviour
{
    public float ppu = 100;
    
    public bool point;

    public int vcount = 1;
    
    public int hcount = 1;
}
