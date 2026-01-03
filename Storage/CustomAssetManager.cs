using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Architect.Behaviour.Custom;
using Architect.Config.Types;
using Architect.Editor;
using Architect.Utils;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Object = UnityEngine.Object;

namespace Architect.Storage;

public static class CustomAssetManager
{
    public static readonly Dictionary<string, Sprite[]> Sprites = new();
    public static readonly Dictionary<string, AudioClip> Sounds = new();

    public static readonly HashSet<string> LoadingSounds = [];
    public static readonly HashSet<string> LoadingSprites = [];

    public static async Task<bool> SaveFile(string url, string path)
    {
        try
        {
            var webClient = new WebClient();
            await webClient.DownloadFileTaskAsync(url, path);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static void WipeAssets()
    {
        foreach (var sp in Sprites.Values.SelectMany(spr => spr)) Object.Destroy(sp);
        foreach (var sp in Sounds.Values) Object.Destroy(sp);
        Sprites.Clear();
        Sounds.Clear();
    }

    public static void DoLoadVideo(VideoPlayer player, float? scale, string url)
    {
        ArchitectPlugin.Instance.StartCoroutine(LoadVideo(url, scale, player));
    }

    private static IEnumerator LoadVideo(string url, float? scale, [CanBeNull] VideoPlayer player = null)
    {
        var path = $"{GetPath(url)}.mov";
        if (!File.Exists(path))
        {
            var task = Task.Run(() => SaveFile(url, path));
            while (!task.IsCompleted) yield return null;
        }

        if (player)
        {
            player.url = path;
            while (player.width == 0) yield return null;

            var sc = scale.GetValueOrDefault(EditManager.CurrentScale);
            player.transform.SetScaleX(sc * player.width / 100);
            player.transform.SetScaleY(sc * player.height / 100);
        }
    }


    public static void DoLoadSprite(string url, bool point, float ppu, int hcount, int vcount, Action<Sprite[]> callback)
    {
        ArchitectPlugin.Instance.StartCoroutine(LoadSprite(url, point, ppu, Mathf.Max(1, hcount), Mathf.Max(1, vcount), callback));
    }

    private static IEnumerator LoadSprite(string url, bool point, float ppu, int hcount, int vcount, Action<Sprite[]> callback)
    {
        var id = $"{url}_{point}_{ppu}_{hcount}_{vcount}";
        if (LoadingSprites.Contains(id))
        {
            yield return new WaitUntil(() => !LoadingSprites.Contains(id));
            if (Sprites.TryGetValue(id, out var sprite)) callback(sprite);
            yield break;
        }
        if (!Sprites.ContainsKey(id))
        {
            LoadingSprites.Add(id);
            var path = $"{GetPath(url)}.png";
            var tmp = ResourceUtils.LoadSprites(path, point, ppu, hcount, vcount);
            if (tmp == null)
            {
                var task = Task.Run(() => SaveFile(url, path));
                while (!task.IsCompleted) yield return null;
                tmp = ResourceUtils.LoadSprites(path, point, ppu, hcount, vcount);
            }

            LoadingSprites.Remove(id);
            if (tmp == null) yield break;
            Sprites[id] = tmp;
        }

        callback(Sprites[id]);
    }

    public static void DoLoadSound(GameObject obj, string url)
    {
        ArchitectPlugin.Instance.StartCoroutine(LoadSound(url, obj));
    }

    private static IEnumerator LoadSound(string url, [CanBeNull] GameObject obj = null)
    {
        if (LoadingSounds.Contains(url))
        {
            yield return new WaitUntil(() => !LoadingSounds.Contains(url));
            if (obj && Sounds.TryGetValue(url, out var sound)) obj.GetComponent<WavObject>().sound = sound;
            yield break;
        }
        
        if (!Sounds.ContainsKey(url))
        {
            LoadingSounds.Add(url);
            var path = $"{GetPath(url)}.wav";
            if (!File.Exists(path))
            {
                var task = Task.Run(() => SaveFile(url, path));
                while (!task.IsCompleted) yield return null;
            }
            yield return ArchitectPlugin.Instance.StartCoroutine(ResourceUtils.LoadClip(path, clip =>
            {
                if (clip) Sounds[url] = clip;
                LoadingSounds.Remove(url);
            }));
        }

        if (obj) obj.GetComponent<WavObject>().sound = Sounds[url];
    }

    public static string GetPath(string url)
    {
        var pathUrl = Path.GetInvalidFileNameChars()
            .Aggregate(url, (current, c) => current.Replace(c, '_'));
        return $"{StorageManager.DataPath}Assets/{pathUrl}";
    }

    public static int DownloadingAssets;
    public static int Downloaded;
    public static int Failed;

    public static async Task TryDownloadAssets(StringConfigValue config, Text status, int downloadCount)
    {
        string fileType;
        if (config.GetTypeId().Equals("png_url")) fileType = ".png";
        else if (config.GetTypeId().Equals("wav_url")) fileType = ".wav";
        else if (config.GetTypeId().Equals("mp4_url")) fileType = ".mov";
        else return;

        var url = config.GetValue();
        DownloadingAssets += 1;
        var b = await SaveFile(url, GetPath(url) + fileType);
        DownloadingAssets -= 1;
        Downloaded += 1;
        status.text = "Downloading Assets...\n" +
                      $"{Downloaded}/{downloadCount}";

        if (!b) Failed++;
    }
}