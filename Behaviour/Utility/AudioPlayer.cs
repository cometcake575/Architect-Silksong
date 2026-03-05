using System;
using System.Collections;
using System.Collections.Generic;
using Architect.Utils;
using BepInEx;
using Newtonsoft.Json;
using Silksong.AssetHelper.ManagedAssets;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class AudioPlayer : MonoBehaviour
{
    public bool isAtmos;
    public bool playOnStart;
    public string cueId;

    private static readonly Dictionary<string, ManagedAsset<MusicCue>> MusicCues = [];
    private static readonly Dictionary<string, ManagedAsset<AtmosCue>> AtmosCues = [];

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
            AudioManager.Instance.ApplyAtmosCue(cue.Handle.Result, 0);
        }
        else
        {
            if (!MusicCues.TryGetValue(cueId, out var cue)) yield break;
            yield return cue.Load();
            AudioManager.Instance.ApplyMusicCue(cue.Handle.Result, 0, 0, true);
        }
    }
}