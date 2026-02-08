using Architect.Events.Blocks.Outputs;
using Architect.Storage;
using BepInEx;
using TeamCherry.Localization;
using UnityEngine;

namespace Architect.Workshop.Items;

public class CustomItem : SpriteItem
{
    private CustomCollectable _item;

    public string ItemName = string.Empty;
    public string ItemDesc = string.Empty;
    public string UseDesc = string.Empty;
    public string UseType = "Break";
    public bool CanUse;
    public bool Consume;
    public string UseEvent;
    public int MaxAmount = int.MaxValue;
    
    public override (string, string)[] FilesToDownload => [
        (IconUrl, "png"),
        (WavURL1, "wav"),
        (WavURL2, "wav")
    ];

    public override void Register()
    {
        _item = ScriptableObject.CreateInstance<CustomCollectable>();
        
        _item.consumeEvent = UseEvent;
        _item.consume = Consume;
        
        _item.name = Id;
        _item.displayName = new LocalisedString("ArchitectMod", ItemName);
        _item.description = new LocalisedString("ArchitectMod", ItemDesc);
        _item.useResponseTextOverride = new LocalisedString("ArchitectMod", UseType);
        _item.useResponses =
        [
            new CollectableItem.UseResponse
            {
                UseType = CanUse ? CollectableItem.UseTypes.Rosaries : CollectableItem.UseTypes.None,
                Description = new LocalisedString("ArchitectMod", UseDesc)
            }
        ];
        
        _item.customMaxAmount = MaxAmount;
        _item.setExtraPlayerDataBools = [];
        _item.setExtraPlayerDataInts = [];
        
        CollectableItemManager.Instance.masterList.Add(_item);
        WorkshopManager.CustomItems.Add(this);
        
        base.Register();
        RefreshAudio1();
        RefreshAudio2();
        
        CollectableItemManager.IncrementVersion();
    }

    public override void Unregister()
    {
        WorkshopManager.CustomItems.Remove(this);
        CollectableItemManager.Instance.masterList.Remove(_item);
        CollectableItemManager.IncrementVersion();
    }

    protected override void OnReadySprite()
    {
        _item.icon = Sprite;
    }

    protected void OnReadyAudio1()
    {
        _item.useSounds = new AudioEventRandom
        {
            Volume = Volume1,
            PitchMax = MaxPitch1,
            PitchMin = MinPitch1,
            Clips = [Clip1]
        };
    }

    protected void OnReadyAudio2()
    {
        _item.instantUseSounds = new AudioEventRandom
        {
            Volume = Volume2,
            PitchMax = MaxPitch2,
            PitchMin = MinPitch2,
            Clips = [Clip2]
        };
    }

    public AudioClip Clip1;
    
    public string WavURL1;
    public float Volume1 = 1;
    public float MaxPitch1 = 1.2f;
    public float MinPitch1 = 0.8f;

    public void RefreshAudio1()
    {
        if (WavURL1.IsNullOrWhiteSpace()) return;
        CustomAssetManager.DoLoadSound(WavURL1, clip =>
        {
            Clip1 = clip;
            OnReadyAudio1();
        });
    }

    public AudioClip Clip2;
    
    public string WavURL2;
    public float Volume2 = 1;
    public float MaxPitch2 = 1.2f;
    public float MinPitch2 = 0.8f;

    public void RefreshAudio2()
    {
        if (WavURL2.IsNullOrWhiteSpace()) return;
        CustomAssetManager.DoLoadSound(WavURL2, clip =>
        {
            Clip2 = clip;
            OnReadyAudio2();
        });
    }

    public class CustomCollectable : CollectableItemBasic
    {
        public string consumeEvent;
        public bool consume;
        
        public override void ConsumeItemResponse()
        {
            if (consumeEvent.IsNullOrWhiteSpace()) return;
            BroadcastBlock.DoBroadcast(consumeEvent);
        }

        public override bool TakeItemOnConsume => consume;
    }
}