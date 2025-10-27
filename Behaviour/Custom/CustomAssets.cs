using Architect.Storage;
using UnityEngine;
using UnityEngine.Video;

namespace Architect.Behaviour.Custom;

public class PngObject : MonoBehaviour
{
    public string url;
    public bool point;
    public float ppu = 100;

    private void Awake()
    {
        if (string.IsNullOrEmpty(url)) return;
        CustomAssetManager.DoLoadSprite(gameObject, url, point, ppu);
    }
}

public interface IPlayable
{
    public void Play();
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
}

public class WavObject : SoundMaker, IPlayable
{
    public string url;
    public AudioClip sound;
    
    public float volume = 1;
    public float pitch = 1;
    public bool globalSound = true;
    
    protected void Start()
    {
        if (string.IsNullOrEmpty(url)) return;
        CustomAssetManager.DoLoadSound(gameObject, url);
    }

    public void Play()
    {
        if (!sound) return;
        PlaySound(sound, volume, pitch, globalSound);
    }
}

public class PngPreview : MonoBehaviour
{
    public float ppu = 100;
    
    public bool point;
}
