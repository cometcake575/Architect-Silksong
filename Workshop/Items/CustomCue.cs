using System.Linq;
using Architect.Behaviour.Utility;
using Architect.Storage;
using Architect.Utils;
using BepInEx;
using UnityEngine;

namespace Architect.Workshop.Items;

public class CustomCue : WorkshopItem
{
    private static readonly Sprite Icon = ResourceUtils.LoadSpriteResource("audio_player");
    
    private MusicCue _mcue;
    private AtmosCue _acue;
    private readonly MusicCue.MusicChannelInfo[] _musicChannelInfos = [new(), new(), new(), new(), new(), new()];
    private readonly AtmosCue.AtmosChannelInfo[] _atmosChannelInfos = [new(), new(), new(), new(), new()];
    
    public readonly string[] WavUrls = ["", "", "", "", "", ""]; 
    public readonly MusicChannelSync[] SyncModes = [
        MusicChannelSync.Implicit,
        MusicChannelSync.Implicit,
        MusicChannelSync.Implicit,
        MusicChannelSync.Implicit,
        MusicChannelSync.Implicit,
        MusicChannelSync.Implicit];
    public bool IsAtmos;

    public override (string, string)[] FilesToDownload => 
        WavUrls.Where(w => !w.IsNullOrWhiteSpace()).Select(w => (w, "wav")).ToArray();
    
    public override void Register()
    {
        if (IsAtmos)
        {
            _acue = ScriptableObject.CreateInstance<AtmosCue>();

            _acue.alternatives = [];
            _acue.channelInfos = _atmosChannelInfos;
            _acue.name = Id;
        
            AudioPlayer.CustomAtmosCues.Add(Id, _acue);
        } else {
            _mcue = ScriptableObject.CreateInstance<MusicCue>();

            _mcue.alternatives = [];
            _mcue.channelInfos = _musicChannelInfos;
            _mcue.name = Id;
            _mcue.originalMusicEventName = string.Empty;

            for (var i = 0; i < 6; i++) _musicChannelInfos[i].sync = SyncModes[i];
        
            AudioPlayer.CustomMusicCues.Add(Id, _mcue);
        }

        RefreshSound();
    }

    private void RefreshSound()
    {
        for (var i = 0; i < (IsAtmos ? 5 : 6); i++)
        {
            if (WavUrls[i].IsNullOrWhiteSpace()) return;
            var i1 = i;
            CustomAssetManager.DoLoadSound(WavUrls[i], wav =>
            {
                wav.LoadAudioData();
                if (IsAtmos) _atmosChannelInfos[i1].clip = wav;
                else _musicChannelInfos[i1].clip = wav;
            });
        }
    }
    
    public override void Unregister()
    {
        if (_acue) Object.Destroy(_acue);
        if (_mcue) Object.Destroy(_mcue);
        AudioPlayer.CustomMusicCues.Remove(Id);
        AudioPlayer.CustomAtmosCues.Remove(Id);
    }

    public override Sprite GetIcon()
    {
        return Icon;
    }
}