using Architect.Content.Preloads;
using Architect.Utils;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

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
                cam.RemoveComponent<tk2dCamera>();
                cam.RemoveComponent<AudioListener>();
                cam.RemoveComponent<ForceCameraAspect>();
                cam.RemoveComponent<FastNoise>();
                cam.RemoveComponent<NewCameraNoise>();
                cam.RemoveComponent<CameraShakeManager>();
                cam.RemoveComponent<CameraRenderHooks>();
                cam.RemoveComponent<CameraRenderScaled>();
                
                tk2dCam.SetActive(true);

            }, notSceneBundle: true));
    }
}