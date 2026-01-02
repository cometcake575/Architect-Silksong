using Architect.Utils;
using UnityEngine;

namespace Architect.Behaviour.Custom;

public class SoundMaker : MonoBehaviour
{
    protected AudioSource Source;
    
    public bool muted;
    
    public virtual void Awake()
    {
        if (muted) return;
        Source = gameObject.GetOrAddComponent<AudioSource>();
        Source.minDistance = 10;
    }

    public void PlaySound(AudioClip clip, float volume = 1, float pitch = 1, bool global = false, bool loop = false)
    {
        if (muted) return;

        Source.spatialBlend = global ? 0 : 1;
        
        Source.pitch = pitch;
        Source.clip = clip;
        Source.volume = volume * GameManager.instance.GetImplicitCinematicVolume() * 5;
        Source.loop = loop;
        
        Source.Play();
    }
}