using System;
using System.Collections.Generic;
using System.Linq;
using Architect.Utils;
using BepInEx;
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
    
    public string Title = string.Empty;
    public string Desc = string.Empty;

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
                
                if (self is CustomStartGameEventTrigger cSelf) _willSetBool = cSelf.setBool;
                else _willSetBool = string.Empty;
                
                orig(self, eventData);
            });
        
        typeof(HeroController).Hook(nameof(HeroController.SceneInit),
            (Action<HeroController> orig, HeroController self) =>
            {
                if (!_willSetBool.IsNullOrWhiteSpace())
                {
                    ArchitectData.Instance.BoolVariables[_willSetBool] = true;
                    _willSetBool = string.Empty;
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
        
        _go.RemoveComponent<StartGameEventTrigger>();

        var startTrigger = _go.AddComponent<CustomStartGameEventTrigger>();
        if (IsSteelSoul) startTrigger.permaDeath = true;
        startTrigger.requiredBool = RequiredBool;
        startTrigger.setBool = SetBool;

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

        private void Update()
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

        public override bool IsFulfilled()
        {
            return (requiredBool.IsNullOrWhiteSpace() ||
                    GlobalArchitectData.Instance.BoolVariables.TryGetValue(requiredBool, out var b) && b) &&
                   (!permaDeath ||
                    GameManager.instance.GetStatusRecordInt("RecPermadeathMode") == 1);
        }
    }
}