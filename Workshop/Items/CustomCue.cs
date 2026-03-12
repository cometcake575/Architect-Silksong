using Architect.Behaviour.Utility;
using Architect.Editor;
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
    private MusicCue.MusicChannelInfo _mci;
    private AtmosCue.AtmosChannelInfo _aci;
    
    public string WavUrl = string.Empty;
    public bool IsAtmos;
    
    public override (string, string)[] FilesToDownload => [(WavUrl, "wav")];
    
    public override void Register()
    {
        if (IsAtmos)
        {
            _acue = ScriptableObject.CreateInstance<AtmosCue>();

            _aci = new AtmosCue.AtmosChannelInfo();

            _acue.alternatives = [];
            _acue.channelInfos =
            [
                _aci,
                new AtmosCue.AtmosChannelInfo(),
                new AtmosCue.AtmosChannelInfo(),
                new AtmosCue.AtmosChannelInfo(),
                new AtmosCue.AtmosChannelInfo()
            ];
            _acue.name = Id;
        
            AudioPlayer.CustomAtmosCues.Add(Id, _acue);
        } else {
            _mcue = ScriptableObject.CreateInstance<MusicCue>();

            _mci = new MusicCue.MusicChannelInfo();

            _mcue.alternatives = [];
            _mcue.channelInfos =
            [
                _mci,
                new MusicCue.MusicChannelInfo(),
                new MusicCue.MusicChannelInfo(),
                new MusicCue.MusicChannelInfo(),
                new MusicCue.MusicChannelInfo(),
                new MusicCue.MusicChannelInfo()
            ];
            _mcue.name = Id;
            _mcue.originalMusicEventName = string.Empty;
        
            AudioPlayer.CustomMusicCues.Add(Id, _mcue);
        }

        RefreshSound();
    }

    private void RefreshSound()
    {
        if (WavUrl.IsNullOrWhiteSpace()) return;
        CustomAssetManager.DoLoadSound(WavUrl, wav =>
        {
            wav.LoadAudioData();
            if (IsAtmos) _aci.clip = wav;
            else _mci.clip = wav;
        });
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