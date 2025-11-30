using UnityEngine;

namespace Architect.Behaviour.Custom;

public class SoundMaker : MonoBehaviour
{
    private AudioSource _source;
    
    public bool muted;
    
    public virtual void Awake()
    {
        if (muted) return;
        _source = gameObject.AddComponent<AudioSource>();
        _source.maxDistance = 1;
    }

    public void PlaySound(AudioClip clip, float volume = 1, float pitch = 1, bool global = false, bool loop = false)
    {
        if (muted) return;

        _source.spatialBlend = global ? 0 : 1;

        _source.pitch = pitch;
        _source.clip = clip;
        _source.volume = volume * GameManager.instance.GetImplicitCinematicVolume() * 5;
        _source.loop = loop;
        
        _source.Play();
    }
}