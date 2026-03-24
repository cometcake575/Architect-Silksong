using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Architect.Content.Preloads;
using Architect.Objects.Categories;
using Architect.Objects.Groups;
using Architect.Objects.Placeable;
using Architect.Placements;
using Architect.Utils;
using BepInEx;
using UnityEngine;
using UnityEngine.Animations;
using UnityStandardAssets.ImageEffects;
using Object = UnityEngine.Object;

namespace Architect.Content.Custom;

public static class CameraObjects
{
    private static readonly FieldInfo RgbChannelTex = typeof(ColorCorrectionCurves).GetField("rgbChannelTex",
        BindingFlags.NonPublic | BindingFlags.Instance);

    public static void Init()
    {
        CustomCameraTarget.InitTarget();
        PreloadManager.RegisterPreload(new BasicPreload("coremanagers_assets__gamecameras",
            "Assets/Prefabs/Camera/_GameCameras.prefab",
            o =>
            {
                var tk2dCam = o.transform.GetChild(1).GetChild(0).gameObject;

                tk2dCam.SetActive(false);
                var cam = Object.Instantiate(tk2dCam);
                Object.DontDestroyOnLoad(cam);

                cam.RemoveComponent<CameraController>();
                cam.RemoveComponent<AudioListener>();
                cam.RemoveComponent<FastNoise>();
                cam.RemoveComponent<NewCameraNoise>();
                cam.RemoveComponent<ForceCameraAspect>();
                cam.RemoveComponent<CameraRenderScaled>();
                cam.RemoveComponent<CameraRenderHooks>();
                cam.RemoveComponent<CameraShakeManager>();

                cam.AddComponent<CustomCamera>();

                cam.transform.SetPositionZ(0);
                
                tk2dCam.SetActive(true);

                Object.Destroy(cam.transform.GetChild(4).gameObject);

                Categories.Utility.AddStart(new CustomObject("Camera", "camera_object", cam,
                        description: "An extra camera that can be rendered using a Camera View.",
                        postSpawnAction: c => { c.transform.SetPositionZ(-38.1f); },
                        sprite: ResourceUtils.LoadSpriteResource("camera"))
                    .WithConfigGroup(ConfigGroup.Camera));
            }, notSceneBundle: true));


        var cam = new GameObject("[Architect] Camera View");
        cam.SetActive(false);
        Object.DontDestroyOnLoad(cam);

        var back = new GameObject("Backing")
        {
            transform =
            {
                parent = cam.transform,
                localPosition = Vector3.zero,
                localScale = new Vector2(0.5f, 0.5f)
            }
        };
        var sr = back.AddComponent<SpriteRenderer>();
        sr.color = Color.black;
        sr.sprite = UIUtils.Square;

        cam.AddComponent<CustomCameraTarget>();

        cam.AddComponent<MeshRenderer>();
        var mesh = new Mesh
        {
            vertices =
            [
                new Vector3(-2.5f, -2.5f, 0),
                new Vector3(2.5f, -2.5f, 0),
                new Vector3(-2.5f, 2.5f, 0),
                new Vector3(2.5f, 2.5f, 0)
            ],
            triangles = [0, 2, 1, 2, 3, 1],
            normals = [-Vector3.forward, -Vector3.forward, -Vector3.forward, -Vector3.forward],
            uv = [new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1)]
        };
        cam.AddComponent<MeshFilter>().mesh = mesh;

        Categories.Utility.AddStart(new CustomObject("Camera View", "camera_view", cam,
                description: "Renders the view of an extra camera.",
                sprite: ResourceUtils.LoadSpriteResource("camera_view", ppu: 40, filterMode: FilterMode.Point))
            .WithConfigGroup(ConfigGroup.CameraView)
            .DoIgnoreScale());

        typeof(ColorCorrectionCurves).Hook("OnDestroy",
            (Action<ColorCorrectionCurves> orig, ColorCorrectionCurves self) =>
            {
                if (self.GetComponent<CustomCamera>()) RgbChannelTex.SetValue(self, null);
                orig(self);
            });
    }

    public class CustomCamera : MonoBehaviour
    {
        public string id;
        public int resolution = 1024;

        private bool _setup;
        private RenderTexture _rt;

        private Camera _camera;
        private ColorCorrectionCurves _ccc;
        private ColorCorrectionCurves _accc;

        private void Start()
        {
            _camera = GetComponent<Camera>();

            _ccc = GetComponent<ColorCorrectionCurves>();
            _accc = GameCameras.instance.tk2dCam.GetComponent<ColorCorrectionCurves>();
        }

        private void Update()
        {
            _ccc.saturation = _accc.saturation;
            _ccc.blueChannel = _accc.blueChannel;
            _ccc.redChannel = _accc.redChannel;
            _ccc.greenChannel = _accc.greenChannel;
            RgbChannelTex.SetValue(_ccc, RgbChannelTex.GetValue(_accc));

            if (!_setup)
            {
                _setup = true;
                if (id.IsNullOrWhiteSpace() || !PlacementManager.Objects.TryGetValue(id, out var target))
                {
                    gameObject.SetActive(false);
                    return;
                }

                var mr = target.GetComponent<MeshRenderer>();
                if (!mr)
                {
                    gameObject.SetActive(false);
                    return;
                }

                var x = target.transform.GetScaleX();
                var y = target.transform.GetScaleY();

                int width;
                int height;

                if (x > y)
                {
                    width = resolution;
                    height = Math.Abs(Mathf.FloorToInt(y / x * resolution));
                }
                else
                {
                    height = resolution;
                    width = Math.Abs(Mathf.FloorToInt(x / y * resolution));
                }

                var rt = new RenderTexture(width, height, 64, RenderTextureFormat.ARGB64)
                {
                    name = "[Architect] Camera Target Texture"
                };
                mr.sharedMaterial = new Material(Shader.Find("Sprites/Default"))
                {
                    mainTexture = _camera.targetTexture = _rt = rt
                };
                _camera.enabled = true;
            }
        }

        private void OnDestroy()
        {
            Destroy(_rt);
        }
    }
    
    public class CustomCameraTarget : MonoBehaviour
    {
        public bool onUI;
        
        public float xOffset;

        public float yOffset;

        public int anchorTo;

        private static readonly int UILayer = LayerMask.NameToLayer("UI");

        private PositionConstraint _constraint;
        private MeshRenderer _mr;

        private static readonly List<CustomCameraTarget> Pngs = [];

        public bool ignoreHudOut;

        private static bool _hudOut;

        public static void InitTarget()
        {
            typeof(GameCameras).Hook(nameof(GameCameras.HUDIn),
                (Action<GameCameras> orig, GameCameras self) =>
                {
                    _hudOut = false;
                    Pngs.RemoveAll(png => !png);
                    foreach (var png in Pngs.Where(png => png._constraint && png.ignoreHudOut))
                        png._constraint.constraintActive = true;
                    orig(self);
                });

            typeof(GameCameras).Hook(nameof(GameCameras.HUDOut),
                (Action<GameCameras> orig, GameCameras self) =>
                {
                    _hudOut = true;
                    Pngs.RemoveAll(png => !png);
                    foreach (var png in Pngs.Where(png => png._constraint && png.ignoreHudOut))
                        png._constraint.constraintActive = false;
                    orig(self);
                });
        }
        
        public void Start()
        {
            if (!onUI) return;
            
            gameObject.layer = UILayer;
            gameObject.transform.GetChild(0).gameObject.layer = UILayer;
            var anchor = GameCameras.instance.hudCamera.transform
                .Find("In-game").Find("Anchor TL").Find("Hud Canvas Offset").Find("Hud Canvas");

            _constraint = gameObject.AddComponent<PositionConstraint>();
            Pngs.Add(this);
            _constraint.constraintActive = true;

            var source = new ConstraintSource
            {
                sourceTransform = anchor,
                weight = 1
            };

            _constraint.AddSource(source);

            _mr = GetComponent<MeshRenderer>();
            _mr.sortingLayerName = "Over";

            UpdatePos();
            if (_hudOut && _constraint)
            {
                _constraint.constraintActive = false;
                transform.position = new Vector3(-10.3535f, 7.533f, 38.1f) + _constraint.translationOffset;
            }
        }

        private void OnEnable()
        {
            if (!onUI) return;
            Pngs.AddIfNotPresent(this);
            if (_hudOut && _constraint) _constraint.constraintActive = false;
        }

        private void UpdatePos()
        {
            if (!onUI) return;
            var offset = new Vector2(xOffset + 10.3535f, yOffset - 6.81f);

            switch (anchorTo)
            {
                case 1:
                    offset.x += 0.94f * PlayerData.instance.maxHealth;
                    break;
                case 2:
                    if (!ToolItemManager.Instance) return;
                    if (ToolItemManager.Instance.boundAttackTools == null) return;

                    var hc = GameCameras.instance.hudCamera.transform
                        .Find("In-game").Find("Anchor TL").Find("Hud Canvas Offset").Find("Hud Canvas");
                    var spool = hc.Find("Tool Icons");
                    offset.x += 1.02f *
                                ToolItemManager.Instance.boundAttackTools.Count(o => o)
                                + spool.transform.GetPositionX();
                    break;
            }

            _constraint.translationOffset = offset;
        }

        private void LateUpdate()
        {
            if (!onUI) return;
            if (anchorTo != 0) UpdatePos();
        }
    }
}