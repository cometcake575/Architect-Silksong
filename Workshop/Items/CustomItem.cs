using System.Linq;
using Architect.Events.Blocks.Outputs;
using Architect.Storage;
using Architect.Utils;
using BepInEx;
using UnityEngine;

namespace Architect.Workshop.Items;

public class CustomItem : SpriteItem
{
    private ICustomItem _item;

    public CustomItemType ItemType = CustomItemType.Normal;
    
    public LocalStr ItemName = string.Empty;
    public LocalStr ItemDesc = string.Empty;
    public LocalStr UseDesc = string.Empty;
    
    public int MaxAmount = int.MaxValue;

    public bool Hidden;
    
    // Usable
    public LocalStr UseType = "Break";
    public bool Consume;
    public string UseEvent;
    
    public override (string, string)[] FilesToDownload => [
        (IconUrl, "png"),
        (WavURL1, "wav"),
        (WavURL2, "wav")
    ];
    
    // Courier
    public int CourierEffects;
    public Color BarColour = Color.white;
    public int Time = 0;
    public int RewardCost = 0;
    
    public override void Register()
    {
        if (ItemType == CustomItemType.Courier) _item = ScriptableObject.CreateInstance<CustomCourierItem>();
        else _item = ScriptableObject.CreateInstance<CustomCollectable>();
        
        _item.Register(this);
        
        WorkshopManager.CustomItems.Add(Id, this);
        
        base.Register();
        RefreshAudio1();
        RefreshAudio2();
        
        CollectableItemManager.IncrementVersion();
    }

    public override void Unregister()
    {
        WorkshopManager.CustomItems.Remove(Id);
        _item.Unregister();
        CollectableItemManager.IncrementVersion();
    }

    protected override void OnReadySprite()
    {
        _item.SetSprite(Sprite);
    }

    public string WavURL1;
    public float Volume1 = 1;
    public float MaxPitch1 = 1.2f;
    public float MinPitch1 = 0.8f;

    public void RefreshAudio1()
    {
        if (WavURL1.IsNullOrWhiteSpace()) return;
        CustomAssetManager.DoLoadSound(WavURL1, clip => _item.SetAudio1(clip));
    }

    public string WavURL2;
    public float Volume2 = 1;
    public float MaxPitch2 = 1.2f;
    public float MinPitch2 = 0.8f;

    public void RefreshAudio2()
    {
        if (WavURL2.IsNullOrWhiteSpace()) return;
        CustomAssetManager.DoLoadSound(WavURL2, clip => _item.SetAudio2(clip));
    }

    public interface ICustomItem
    {
        public void Register(CustomItem item);
        
        public void Unregister();

        public void SetSprite(Sprite sprite) {}
        
        public void SetAudio1(AudioClip clip) {}
        
        public void SetAudio2(AudioClip clip) {}
    }

    public enum CustomItemType
    {
        Normal,
        Usable,
        Memento,
        Courier
    }

    private static CollectableItemMementoList _mementoList;

    public static CollectableItemMementoList MementoList
    {
        get
        {
            if (!_mementoList) _mementoList = Resources.FindObjectsOfTypeAll<CollectableItemMementoList>()
                .FirstOrDefault();
            return _mementoList;
        }
    }

    public class CustomCollectable : CollectableItemMemento, ICustomItem
    {
        public string consumeEvent;
        public bool consume;

        private CustomItem _item;
        
        public override void ConsumeItemResponse()
        {
            if (consumeEvent.IsNullOrWhiteSpace()) return;
            BroadcastBlock.DoBroadcast(consumeEvent);
        }

        public override bool TakeItemOnConsume => consume;

        public void Register(CustomItem item)
        {
            _item = item;
            
            name = item.Id;
            displayName = item.ItemName;
            description = item.ItemDesc;
            useResponseTextOverride = item.UseType;
            useResponses =
            [
                new UseResponse
                {
                    UseType = item.ItemType == CustomItemType.Usable ? UseTypes.Rosaries : UseTypes.None,
                    Description = item.UseDesc
                }
            ];
        
            customMaxAmount = item.MaxAmount;
            setExtraPlayerDataBools = [];
            setExtraPlayerDataInts = [];

            isHidden = item.Hidden;
        
            consumeEvent = item.UseEvent;
            consume = item.Consume;
            
            CollectableItemManager.Instance.masterList.Add(this);
            if (MementoList && _item.ItemType == CustomItemType.Memento) MementoList.Add(this);
        }

        public void SetSprite(Sprite sprite)
        {
            icon = sprite;
        }

        public void SetAudio1(AudioClip clip)
        {
            useSounds = new AudioEventRandom
            {
                Volume = _item.Volume1,
                PitchMax = _item.MaxPitch1,
                PitchMin = _item.MinPitch1,
                Clips = [clip]
            };
        }

        public void SetAudio2(AudioClip clip)
        {
            instantUseSounds = new AudioEventRandom
            {
                Volume = _item.Volume2,
                PitchMax = _item.MaxPitch2,
                PitchMin = _item.MinPitch2,
                Clips = [clip]
            };
        }
        
        public override bool IsVisibleInCollection()
        {
            if (_item.ItemType == CustomItemType.Memento) return base.IsVisibleInCollection();
            return CollectedAmount > 0;
        }
        
        public override string GetDescription(ReadSource readSource)
        {
            return description;
        }
        
        public void Unregister()
        {
            CollectableItemManager.Instance.masterList.Remove(this);
            if (MementoList) MementoList.Remove(this);
            Destroy(this);
        }
    }

    public class CustomCourierItem : DeliveryQuestItem, ICustomItem
    {
        public string consumeEvent;
        public bool consume;
        public int reward;

        public override void ConsumeItemResponse()
        {
            if (consumeEvent.IsNullOrWhiteSpace()) return;
            BroadcastBlock.DoBroadcast(consumeEvent);
        }

        public override bool TakeItemOnConsume => consume;

        public void Register(CustomItem item)
        {
            name = item.Id;
            displayName = item.ItemName;
            description = item.ItemDesc;
            useResponseTextOverride = item.UseType;
            useResponses =
            [
                new UseResponse
                {
                    UseType = UseTypes.None,
                    Description = item.UseDesc
                }
            ];
            
            customMaxAmount = item.MaxAmount;

            isHidden = item.Hidden;
        
            consumeEvent = item.UseEvent;
            consume = item.Consume;
            
            var effectSource = item.CourierEffects switch
            {
                0 => null,
                1 => MiscUtils.GetSavedItem("Courier Supplies"),
                2 => MiscUtils.GetSavedItem("Courier Supplies Slave"),
                _ => MiscUtils.GetSavedItem("Courier Supplies Gourmand")
            } as DeliveryQuestItem;
            if (effectSource)
            {
                breakHeroEffect = effectSource.breakHeroEffect;
                breakUIEffect = effectSource.breakUIEffect;
                heroLoopEffect = effectSource.heroLoopEffect;
                hitHeroEffect = effectSource.hitHeroEffect;
                hitUIEffect = effectSource.hitUIEffect;
                uiLoopEffect = effectSource.uiLoopEffect;
            }

            barColour = item.BarColour;
            totalTimer = item.Time;
            reward = item.RewardCost;
            
            CollectableItemManager.Instance.masterList.Add(this);
        }

        public void SetSprite(Sprite sprite)
        {
            icon = sprite;
        }
        
        public override bool IsVisibleInCollection()
        {
            return CollectedAmount > 0;
        }
        
        public override string GetDescription(ReadSource readSource)
        {
            return description;
        }
        
        public void Unregister()
        {
            CollectableItemManager.Instance.masterList.Remove(this);
            Destroy(this);
        }
    }
}