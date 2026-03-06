using System;
using System.Collections;
using System.Collections.Generic;
using Architect.Utils;
using BepInEx;
using Newtonsoft.Json;
using Silksong.AssetHelper.ManagedAssets;
using UnityEngine;
using UnityEngine.Audio;

namespace Architect.Behaviour.Utility;

public class AudioPlayer : MonoBehaviour
{
    public bool isAtmos;
    public bool playOnStart;
    public bool lockMusic = true;
    public string cueId;

    private static readonly Dictionary<string, ManagedAsset<MusicCue>> MusicCues = [];
    private static readonly Dictionary<string, ManagedAsset<AtmosCue>> AtmosCues = [];

    private static readonly List<AudioPlayer> Players = [];
    private static bool _isUnlocked;

    public static void Init()
    {
        var dict = JsonConvert
            .DeserializeObject<Dictionary<string, List<string>>>(ResourceUtils.LoadTextResource("audioassets.json"));

        foreach (var (bundle, assets) in dict)
        {
            foreach (var asset in assets)
            {
                var id = asset.Split("/")[^1].Split(".")[0];
                if (asset.Contains("Music"))
                    MusicCues[id] = ManagedAsset<MusicCue>.FromNonSceneAsset(asset, bundle);
                else AtmosCues[id] = ManagedAsset<AtmosCue>.FromNonSceneAsset(asset, bundle);
            }
        }
        
        typeof(AudioManager).Hook(nameof(AudioManager.ApplyAtmosCue),
            (Action<AudioManager, AtmosCue, float, bool> orig, AudioManager self, AtmosCue atmosCue,
                float transitionTime, bool markWaitForAtmos) =>
            {
                Players.RemoveAll(i => !i);
                if (Players.Count > 0 && !_isUnlocked) return;
                orig(self, atmosCue, transitionTime, markWaitForAtmos);
            }, typeof(AtmosCue), typeof(float), typeof(bool));
        
        typeof(AudioManager).Hook(nameof(AudioManager.ApplyMusicCue),
            (Action<AudioManager, MusicCue, float, float, bool> orig, AudioManager self, MusicCue musicCue,
                float delayTime, float transitionTime, bool markWaitForAtmos) =>
            {
                Players.RemoveAll(i => !i);
                if (Players.Count > 0 && !_isUnlocked) return;
                orig(self, musicCue, delayTime, transitionTime, markWaitForAtmos);
            }, typeof(MusicCue), typeof(float), typeof(float), typeof(bool));
        
        typeof(AudioManager).Hook(nameof(AudioManager.ApplyMusicSnapshot),
            (Action<AudioManager, AudioMixerSnapshot, float, float, bool> orig, AudioManager self,
                AudioMixerSnapshot snapshot, float delayTime, float transitionTime, bool blockMusicMarker) =>
            {
                Players.RemoveAll(i => !i);
                if (Players.Count > 0 && !_isUnlocked) return;
                orig(self, snapshot, delayTime, transitionTime, blockMusicMarker);
            }, typeof(AudioMixerSnapshot), typeof(float), typeof(float), typeof(bool));
    }

    private void OnEnable()
    {
        if (lockMusic) Players.Add(this);
    }

    private void OnDisable()
    {
        if (lockMusic) Players.Remove(this);
    }

    private void Start()
    {
        if (playOnStart) Play();
    }

    public void Play()
    {
        if (cueId.IsNullOrWhiteSpace()) return;
        StartCoroutine(DoPlay());
    }

    public IEnumerator DoPlay()
    {
        if (isAtmos)
        {
            if (!AtmosCues.TryGetValue(cueId, out var cue)) yield break;
            yield return cue.Load();
            _isUnlocked = true;
            AudioManager.Instance.ApplyAtmosCue(cue.Handle.Result, 0);
            GameManager.instance.sm.atmosCue = cue.Handle.Result;
            GameManager.instance.sm.atmosSnapshot = cue.Handle.Result.snapshot;
        }
        else
        {
            if (!MusicCues.TryGetValue(cueId, out var cue)) yield break;
            yield return cue.Load();
            _isUnlocked = true;
            AudioManager.Instance.ApplyMusicCue(cue.Handle.Result, 0, 0, true);
            GameManager.instance.sm.musicCue = cue.Handle.Result;
            GameManager.instance.sm.musicSnapshot = cue.Handle.Result.snapshot;
        }

        _isUnlocked = false;
    }
}