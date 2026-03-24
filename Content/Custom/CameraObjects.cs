using System;
using System.Collections.Generic;
using System.Reflection;
using Architect.Content.Preloads;
using Architect.Objects.Categories;
using Architect.Objects.Groups;
using Architect.Objects.Placeable;
using Architect.Placements;
using Architect.Utils;
using BepInEx;
using UnityEngine;
using UnityStandardAssets.ImageEffects;
using Object = UnityEngine.Object;

namespace Architect.Content.Custom;

public static class CameraObjects
{
    public static void Init()
    {
        PreloadManager.RegisterPreload(new BasicPreload("coremanagers_assets__gamecameras", "Assets/Prefabs/Camera/_GameCameras.prefab",
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
                
                tk2dCam.SetActive(true);
                
                Object.Destroy(cam.transform.GetChild(4).gameObject);
        
                Categories.Utility.AddStart(new CustomObject("Camera", "camera_object", cam,
                    description: "An extra camera that can be rendered using a Camera View.",
                    sprite: ResourceUtils.LoadSpriteResource("camera"))
                    .WithConfigGroup(ConfigGroup.Camera));
            }, notSceneBundle: true));

        var cam = new GameObject("Camera View");
        Object.DontDestroyOnLoad(cam);
        cam.SetActive(false);

        cam.AddComponent<MeshRenderer>();

        Categories.Utility.AddStart(new CustomObject("Camera View", "camera_view", cam,
            description: "Renders the view of an extra camera.",
            sprite: ResourceUtils.LoadSpriteResource("camera_view")));
    }

    public class CustomCamera : MonoBehaviour
    {
        public string id;

        private bool _setup;

        private Camera _camera;
        private ColorCorrectionCurves _ccc;
        private ColorCorrectionCurves _accc;

        private static readonly FieldInfo RgbChannelTex = typeof(ColorCorrectionCurves).GetField("rgbChannelTex",
            BindingFlags.NonPublic | BindingFlags.Instance);
        
        private void Start()
        {
            DontDestroyOnLoad(gameObject);
            
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
                ArchitectPlugin.Logger.LogInfo(0);
                if (id.IsNullOrWhiteSpace() || !PlacementManager.Objects.TryGetValue(id, out var target))
                {
                    gameObject.SetActive(false);
                    return;
                }
                ArchitectPlugin.Logger.LogInfo(1);

                var mr = target.GetComponent<MeshRenderer>();
                if (!mr)
                {
                    gameObject.SetActive(false);
                    return;
                }
                ArchitectPlugin.Logger.LogInfo(2);
                var rt = new RenderTexture();
                mr.sharedMaterial = new Material(Shader.Find("Sprites/Default"))
                {
                    mainTexture = _camera.targetTexture = rt
                };
                _camera.enabled = true;
                ArchitectPlugin.Logger.LogInfo(3);
            }
        }
    }
}