using GlobalEnums;
using TeamCherry;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Architect.Behaviour.Utility;

public class CustomGradeMarker : MonoBehaviour
{
    public MapZone mapZone = MapZone.NONE;
    public int act3;

    public bool setAmbientColour;
    public bool setAmbientIntensity;
    public bool setHeroLight;
    public bool setSaturation;

    private void Start()
    {
        if (mapZone == MapZone.NONE) return;
        
        var handle = Addressables.LoadAssetAsync<References>("ReferencesData");
        var references = handle.WaitForCompletion();
        var settings = references.sceneDefaultSettings.GetMapZoneSettingsRuntime(mapZone,
            act3 == 1 || (act3 == 2 && PlayerData.instance.blackThreadWorld)
                ? SceneManagerSettings.Conditions.BlackThread
                : SceneManagerSettings.Conditions.None);
        Addressables.Release(handle);

        if (settings == null) return;
        var gm = GetComponent<GradeMarker>();
        if (setSaturation) gm.saturation = settings.saturation;
        if (setHeroLight) gm.heroLightColor = settings.heroLightColor;
        if (setAmbientIntensity) gm.ambientIntensity = settings.defaultIntensity;
        if (setAmbientColour) gm.ambientColor = settings.defaultColor;
        gm.redChannel = new AnimationCurve(settings.redChannel.keys);
        gm.blueChannel = new AnimationCurve(settings.blueChannel.keys);
        gm.greenChannel = new AnimationCurve(settings.greenChannel.keys);
    }
}