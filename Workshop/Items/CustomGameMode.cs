using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Architect.Utils;
using BepInEx;
using MonoMod.RuntimeDetour;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Architect.Workshop.Items;

public class CustomGameMode : SpriteItem
{
    public string RequiredBool = string.Empty;
    public string SetBool = string.Empty;

    private static string _willSetBool = string.Empty;
    private static bool _willCustomEntry;
    private static bool _wasCustomEntry;
    private static bool _willSkipCinematic;
    private static string _customEntryScene;
    private static string _customEntrySpawn;
    
    public string Title = string.Empty;
    public string Desc = string.Empty;

    public bool SkipCinematic;
    public bool CustomEntry;
    public string CustomEntryScene = string.Empty;
    public string CustomEntrySpawn = string.Empty;

    public bool IsSteelSoul;

    // Vanilla is 1, Steel Soul is 2
    public float Priority = 3;

    public int Replace = 0;

    private GameObject _go;
    private Image _img;

    private static readonly List<CustomGameMode> GameModes = [];

    public static void Init()
    {
        typeof(UIManager).Hook(nameof(UIManager.Awake),
            (Action<UIManager> orig, UIManager self) =>
            {
                orig(self);
                foreach (var gm in GameModes.ToArray())
                {
                    gm.Unregister();
                    gm.Register();
                }
            });
        
        typeof(StartGameEventTrigger).Hook(nameof(StartGameEventTrigger.OnSubmit),
            (Action<StartGameEventTrigger, BaseEventData> orig, StartGameEventTrigger self, BaseEventData eventData) =>
            {
                if (self.hasTriggered)
                {
                    orig(self, eventData);
                    return;
                }

                if (self is CustomStartGameEventTrigger cSelf)
                {
                    _willSetBool = cSelf.setBool;
                    if (cSelf.customEntry)
                    {
                        _willCustomEntry = true;
                        _customEntryScene = cSelf.customEntryScene;
                        _customEntrySpawn = cSelf.customEntrySpawn;
                    }

                    _willSkipCinematic = cSelf.skipCinematic;
                }
                else _willSetBool = string.Empty;
                
                orig(self, eventData);
            });

        OpeningSequence seq = null;
        typeof(OpeningSequence).Hook(nameof(OpeningSequence.OnEnable),
            (Action<OpeningSequence> orig, OpeningSequence self) =>
            {
                orig(self);
                seq = self;
                if (_willCustomEntry)
                {
                    _willCustomEntry = false;
                    _wasCustomEntry = true;
                    self.loadSave = true;
                    PlayerData.instance.respawnScene = _customEntryScene;
                    PlayerData.instance.respawnMarkerName = _customEntrySpawn;
                }
            });

        _ = new Hook(typeof(ChainSequence).GetProperty(nameof(ChainSequence.IsPlaying))!.GetGetMethod(),
            (Func<ChainSequence, bool> orig, ChainSequence self) =>
            {
                if (_willSkipCinematic && seq.isLevelReady)
                {
                    _willSkipCinematic = false;
                    return false;
                }
                return orig(self);
            });
        
        typeof(HeroController).Hook(nameof(HeroController.SceneInit),
            (Action<HeroController> orig, HeroController self) =>
            {
                if (!_willSetBool.IsNullOrWhiteSpace())
                {
                    ArchitectData.Instance.BoolVariables[_willSetBool] = true;
                    _willSetBool = string.Empty;
                }

                if (_wasCustomEntry)
                {
                    _wasCustomEntry = false;
                    PlayerData.instance.disablePause = false;
                    PlayerData.instance.bindCutscenePlayed = true;
                    EventRegister.SendEvent("SHOW HP");
                }
                
                orig(self);
            });
    }
    
    public override void Register()
    {
        GameModes.Add(this);
        
        if (!UIManager.instance) return;

        var content = UIManager.instance.playModeMenuScreen.transform.Find("Content");
        var btn = content.Find("NormalButton").gameObject;

        _go = Object.Instantiate(btn, content);
        _go.name = $"{Id}Button";
        
        _img = _go.transform.Find("Image").GetComponent<Image>();
        _img.sprite = null;

        _go.transform.Find("Text").GetComponent<AutoLocalizeTextUI>().text = (LocalStr)Title;
        _go.transform.Find("DescriptionText").GetComponent<AutoLocalizeTextUI>().text = (LocalStr)Desc;
        
        var customBtn = _go.AddComponent<CustomGameButton>();
        customBtn.priority = Priority;

        List<CustomGameButton> children = [];
        foreach (Transform child in content)
        {
            var cgb = child.GetComponent<CustomGameButton>();
            if (!cgb)
            {
                cgb = child.gameObject.AddComponent<CustomGameButton>();
                cgb.priority = child.name.StartsWith("Normal") ? 1 : 2;
            }
            
            children.Add(cgb);
        }

        foreach (var child in children.OrderBy(c => c.priority)) 
            child.transform.SetAsLastSibling();

        customBtn.forceInactive = Replace switch
        {
            1 => btn,
            2 => content.Find("SteelButton").gameObject,
            _ => null
        };
        customBtn.Update();
        
        _go.RemoveComponent<StartGameEventTrigger>();

        var startTrigger = _go.AddComponent<CustomStartGameEventTrigger>();
        if (IsSteelSoul) startTrigger.permaDeath = true;
        startTrigger.requiredBool = RequiredBool;
        startTrigger.setBool = SetBool;

        if (CustomEntry)
        {
            startTrigger.customEntry = true;
            startTrigger.customEntryScene = CustomEntryScene;
            startTrigger.customEntrySpawn = CustomEntrySpawn;
        }

        startTrigger.skipCinematic = SkipCinematic;

        base.Register();
    }

    protected override void OnReadySprite()
    {
        if (_img) _img.sprite = Sprite;
    }

    public override void Unregister()
    {
        GameModes.Remove(this);
        if (_go) Object.Destroy(_go);
    }

    private class CustomGameButton : MonoBehaviour
    {
        public float priority;
        public GameObject forceInactive;

        public void Update()
        {
            if (forceInactive) forceInactive.SetActive(false);
        }

        private void OnDisable()
        {
            if (!forceInactive) return;
            forceInactive.SetActive(true);
        }
    }

    private class CustomStartGameEventTrigger : StartGameEventTrigger
    {
        public string requiredBool;
        public string setBool;

        public bool customEntry;
        public bool skipCinematic;
        public string customEntryScene;
        public string customEntrySpawn;
        
        public override bool IsFulfilled()
        {
            return (requiredBool.IsNullOrWhiteSpace() ||
                    GlobalArchitectData.Instance.BoolVariables.TryGetValue(requiredBool, out var b) && b) &&
                   (!permaDeath ||
                    GameManager.instance.GetStatusRecordInt("RecPermadeathMode") == 1);
        }
    }
}