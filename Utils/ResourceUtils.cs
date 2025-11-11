using System;
using System.Collections;
using System.IO;
using System.Reflection;
using Architect.Storage;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking;

namespace Architect.Utils;

public static class ResourceUtils
{
    internal static Sprite LoadSpriteResource(
        string spritePath,
        FilterMode filterMode = FilterMode.Bilinear,
        Vector4 border = default,
        float ppu = 100)
    {
        return LoadSpriteResource(spritePath, new Vector2(0.5f, 0.5f), filterMode, border, ppu);
    }

    internal static Sprite LoadSpriteResource(
        string spritePath,
        Vector2 pivot, 
        FilterMode filterMode = FilterMode.Bilinear,
        Vector4 border = default,
        float ppu = 100)
    {
        var path = $"Architect.Resources.{spritePath}.png";

        var asm = Assembly.GetExecutingAssembly();

        using var s = asm.GetManifestResourceStream(path);
        if (s == null) return null;
        var buffer = new byte[s.Length];
        _ = s.Read(buffer, 0, buffer.Length);
        var tex = new Texture2D(2, 2);
        tex.LoadImage(buffer, true);
        tex.wrapMode = TextureWrapMode.Clamp;

        var sprite = Sprite.Create(
            tex, 
            new Rect(0, 0, tex.width, tex.height),
            pivot, 
            ppu, 
            0U, 
            SpriteMeshType.Tight,
            border);
        
        sprite.texture.filterMode = filterMode;
        
        return sprite;
    }

    [CanBeNull]
    internal static Sprite[] LoadSprites(string spritePath, bool point, float ppu, int count)
    {
        if (!File.Exists(spritePath)) return null;

        var tex = new Texture2D(2, 2);
        tex.LoadImage(File.ReadAllBytes(spritePath), true);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = point ? FilterMode.Point : FilterMode.Bilinear;

        var sprites = new Sprite[count];
        var height = tex.height / (float)count;
        for (var i = 0; i < count; i++)
        {
            sprites[count - i - 1] = Sprite.Create(tex, new Rect(0, height * i, tex.width, height),
                new Vector2(0.5f, 0.5f), ppu);
        }

        return sprites;
    }

    internal static IEnumerator LoadClip(string clipPath, Action<AudioClip> callback)
    {
        var www = UnityWebRequestMultimedia.GetAudioClip(new Uri(clipPath), GetAudioType(clipPath));
        yield return www.SendWebRequest();
        
        if (www.result == UnityWebRequest.Result.Success)
        {
            callback.Invoke(DownloadHandlerAudioClip.GetContent(www));
        } else ArchitectPlugin.Logger.LogInfo(www.error);
    }

    internal static void LoadClipResource(string clipPath, Action<AudioClip> callback)
    {
        var path = StorageManager.DataPath + $"ModAssets/{clipPath}.wav";
        
        if (!File.Exists(path))
        {
            var asm = Assembly.GetExecutingAssembly();

            using var s = asm.GetManifestResourceStream($"Architect.Resources.{clipPath}.wav");

            if (s == null) return;

            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            s.CopyTo(fs);
        }

        ArchitectPlugin.Instance.StartCoroutine(LoadClip(path, callback));
    }
    
    private static AudioType GetAudioType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLower();
        return ext switch
        {
            ".wav" => AudioType.WAV,
            ".mp3" => AudioType.MPEG,
            ".ogg" => AudioType.OGGVORBIS,
            ".aif" or ".aiff" => AudioType.AIFF,
            _ => AudioType.UNKNOWN
        };
    }
}