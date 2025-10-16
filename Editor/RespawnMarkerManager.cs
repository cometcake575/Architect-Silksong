using System;
using System.Reflection;
using Architect.Storage;
using Architect.Utils;
using MonoMod.RuntimeDetour;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Architect.Editor;

public static class RespawnMarkerManager
{
    private static GameObject _marker;
    private static GameObject _icon;
    
    public static void Init()
    {
        if (!Settings.ShowRespawnPoint) return;
        
        _marker = new GameObject("[Architect] Respawn Marker")
        {
            transform = { position = new Vector3(0, 0, 0.005f) }
        };
        
        _marker.SetActive(false);
        Object.DontDestroyOnLoad(_marker);
        
        _icon = new GameObject("Icon")
        {
            transform =
            {
                parent = _marker.transform,
                localPosition = Vector3.zero
            }
        };

        _marker.AddComponent<SpriteRenderer>().sprite = ResourceUtils.LoadSpriteResource("respawn_text", ppu: 64);
        _icon.AddComponent<SpriteRenderer>().sprite = ResourceUtils.LoadSpriteResource("respawn_marker", ppu: 64);

        _ = new Hook(typeof(HeroController).GetMethod(nameof(HeroController.Awake),
                BindingFlags.NonPublic | BindingFlags.Instance),
            (Action<HeroController> orig, HeroController self) =>
            {
                orig(self);
                self.gameObject.AddComponent<RespawnPreview>();
            });
    }

    public class RespawnPreview : MonoBehaviour
    {
        private PlayerData _pd;
        private Vector3 _lastPos;
        private bool _lastLeft;
        
        private void Start()
        {
            _marker.SetActive(true);
            _pd = HeroController.instance.playerData;
        }

        private void Update()
        {
            var facingLeft = _pd.hazardRespawnFacing switch
            {
                HazardRespawnMarker.FacingDirection.None => 
                    _pd.hazardRespawnLocation.x - HeroController.instance.transform.position.x > 0.1f,
                HazardRespawnMarker.FacingDirection.Left => true,
                _ => false
            };
            if (facingLeft == _lastLeft && _pd.hazardRespawnLocation == _lastPos) return;

            _lastLeft = facingLeft;
            _lastPos = _pd.hazardRespawnLocation;

            if (!HeroController.instance.TryFindGroundPoint(out var point, _lastPos, false)) return;
            
            _marker.transform.SetPositionX(point.x);
            _marker.transform.SetPositionY(point.y - 0.15f);
            _icon.transform.SetScaleX(facingLeft ? 1 : -1);
        }
        
        private void OnDisable()
        {
            _marker?.SetActive(false);
        }
    }
}