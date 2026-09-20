using System;
using System.Collections.Generic;
using System.Linq;
using Architect.Behaviour.Utility;
using Architect.Content.Preloads;
using Architect.Placements;
using Architect.Storage;
using Architect.Utils;
using BepInEx;
using JetBrains.Annotations;
using MonoMod.RuntimeDetour;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Architect.Workshop.Items;

public class CustomMenuStyle : WorkshopItem
{
    private static readonly Sprite Icon = ResourceUtils.LoadSpriteResource("custom_title");
    private static readonly List<CustomMenuStyle> Styles = [];
    
    private static readonly Dictionary<MenuStyles.MenuStyle, CustomMenuStyle> StyleLookup = [];
    private static readonly Dictionary<string, CustomMenuStyle> IdLookup = [];
    
    private static MenuStyles _ms;
    private GameObject _parent;

    public string Name = string.Empty;
    public Color AmbientColor = Color.white;
    public float AmbientIntensity = 1;
    public float BluePlaneVibrancy = 1;
    public bool HideTitle;
    public bool AutoActivate;

    public string RequiredBool = string.Empty;

    public static GameObject Title;
    public override string LoadScene => $"{Id}_Title";
    
    public static void Init()
    {
        PreloadManager.RegisterPreload(new BasicPreload("Menu_Title", "LogoTitle",
            o =>
            {
                Title = Object.Instantiate(o);
                Title.name = "Title Reference";
                Title.SetActive(false);
                Object.DontDestroyOnLoad(Title);

                var camLock = new GameObject("Cam Lock")
                {
                    transform =
                    {
                        parent = Title.transform,
                        position = new Vector3(14.6f, 8.3f, -38.1f)
                    }
                };
                camLock.AddComponent<CameraBorder>().type = 4;
            }));
        
        typeof(MenuStyles).Hook(nameof(MenuStyles.Awake),
            (Action<MenuStyles> orig, MenuStyles self) =>
            {
                _ms = self;
        
                foreach (var style in Styles.ToArray())
                {
                    style.Unregister();
                    style.Register();
                }
                
                orig(self);
            });
        
        typeof(MenuStyles).Hook(nameof(MenuStyles.LoadRecentMenuStyle),
            (Action<MenuStyles, bool> orig, MenuStyles self, bool fade) =>
            {
                if (!GlobalArchitectData.Instance.MenuStyle.IsNullOrWhiteSpace())
                {
                    if (PreloadManager.HasPreloaded) SetStyle();
                    return;
                }
                orig(self, fade);
            });
        
        typeof(MenuStyles).Hook(nameof(MenuStyles.SetInSubMenu),
            (Action<MenuStyles, bool> orig, MenuStyles self, bool value) =>
            {
                orig(self, value);
                if (StyleLookup.TryGetValue(self.Styles[self.CurrentStyle], out var custom)) 
                    custom._lsc.Fade(value ? 0.0f : 1f, self.ForegroundFadeTime);
            });
        
        typeof(MenuStyles).Hook(nameof(MenuStyles.SetStyle),
            (Action<MenuStyles, int, bool, bool> orig, MenuStyles self, int index, bool fade, bool save) =>
            {
                orig(self, index, fade, save);
                
                var isCustom = StyleLookup.TryGetValue(self.Styles[index], out var custom);
                
                if (PreloadManager.HasPreloaded) GlobalArchitectData.Instance.MenuStyle = isCustom ? custom.Id : string.Empty;
                
                var title = UIManager.instance.gameTitle.gameObject;
                title.GetOrAddComponent<LogoTitleBlocker>().Style = custom;
                title.SetActive(!isCustom || !custom.HideTitle);
            });

        _ = new Hook(typeof(MenuStyles.MenuStyle).GetProperty(nameof(MenuStyles.MenuStyle.IsAvailable))!.GetGetMethod(),
            (Func<MenuStyles.MenuStyle, bool> orig, MenuStyles.MenuStyle self) =>
            {
                if (!StyleLookup.TryGetValue(self, out var custom)) return orig(self);
                return custom.RequiredBool.IsNullOrWhiteSpace() || 
                       GlobalArchitectData.Instance.BoolVariables.GetValueOrDefault(custom.RequiredBool);
            });
    }

    public class LogoTitleBlocker : MonoBehaviour
    {
        [CanBeNull] public CustomMenuStyle Style;

        private void Update()
        {
            if (Style is { HideTitle: true }) gameObject.SetActive(false);
        }
    }

    private MenuStyles.MenuStyle _style;
    private LoadSceneContents _lsc;
    private CustomScene _customScene;
    
    public override void Register()
    {
        Styles.Add(this);

        if (_ms)
        {

            _parent = new GameObject(Id)
            {
                transform =
                {
                    parent = _ms.transform,
                    localPosition = new Vector3(-5f, -7.8454f, 3.5469f)
                }
            };
            _parent.SetActive(false);

            _lsc = _parent.AddComponent<LoadSceneContents>();
            _lsc.id = Id;

            _style = new MenuStyles.MenuStyle
            {
                DisplayName = $"ArchitectMod_{Name}",
                StyleObject = _parent,
                CameraColorCorrection = new MenuStyles.MenuStyle.CameraCurves(),
                AmbientColor = AmbientColor,
                BlurPlaneVibranceOffset = BluePlaneVibrancy,
                AmbientIntensity = AmbientIntensity
            };


            var styles = _ms.Styles.ToList();
            styles.Add(_style);
            _ms.Styles = styles.ToArray();
            StyleLookup[_style] = this;
        }

        _customScene = new CustomScene
        {
            Id = $"{Id}_Title",
            Group = "Titles",
            TilemapWidth = 0,
            TilemapHeight = 0
        };
        _customScene.Register();

        IdLookup[Id] = this;
        
        if (AutoActivate && GlobalArchitectData.Instance.AutoActivatedTitleScreens.Add(Id + (ExternalSource ?? "")))
        {
            GlobalArchitectData.Instance.MenuStyle = Id;
            if (_ms) SetStyle();
        }
    }

    public override void Unregister()
    {
        Styles.Remove(this);
        
        if (_parent) Object.Destroy(_parent);
        
        var styles = _ms.Styles.ToList();
        styles.Remove(_style);
        _ms.Styles = styles.ToArray();
        
        if (_style != null) StyleLookup.Remove(_style);
        IdLookup.Remove(Id);

        _customScene?.Unregister();
    }

    public override Sprite GetIcon()
    {
        return Icon;
    }

    public class LoadSceneContents : MonoBehaviour
    {
        public string id;
        private Scene _scene;
        private IEnumerable<MenuForegroundMarker.MenuFader> _faders;
        
        public void Fade(float target, float time)
        {
            foreach (var fader in _faders) fader.FadeTo(target, time);
        }

        public void OnEnable()
        {
            if (!_ms.started) return;
            _scene = SceneManager.CreateScene($"{id}_title");

            var ld = StorageManager.LoadScene($"{id}_title");
            foreach (var placement in ld.Placements)
            {
                var obj = placement.SpawnObject();
                obj.WipeBehaviour();

                if (obj)
                {
                    SceneManager.MoveGameObjectToScene(obj, _scene);
                    PlacementManager.Objects[placement.GetId()] = obj;
                    PlacementManager.OnPlace?.Invoke(placement.GetPlacementType().GetId(), placement.GetId(), obj);
                }
            }

            foreach (var block in ld.ScriptBlocks) block.Setup(false);
            foreach (var block in ld.ScriptBlocks) block.LateSetup();

            foreach (var marker in _scene.GetRootGameObjects()
                         .SelectMany(o => o.GetComponentsInChildren<MenuForegroundMarker>())) marker.Setup();
            _faders = _scene.GetRootGameObjects()
                .SelectMany(o => o.GetComponentsInChildren<MenuForegroundMarker.MenuFader>());
            
            Fade(_ms.isInSubMenu ? 0 : 1, 0);
        }

        private void OnDisable()
        {
            if (_scene.IsValid()) SceneManager.UnloadSceneAsync(_scene);
        }
    }

    public static void SetStyle()
    {
        if (!IdLookup.TryGetValue(GlobalArchitectData.Instance.MenuStyle, out var style)) return;

        for (var i = 0; i < _ms.Styles.Length; i++)
        {
            if (_ms.Styles[i] == style._style)
            {
                _ms.SetStyle(i, false);
                break;
            }
        }
    }
}