using System;
using System.Collections;
using System.Reflection;
using Architect.Behaviour.Utility;
using Architect.Utils;
using GlobalEnums;
using MonoMod.RuntimeDetour;
using UnityEngine;

namespace Architect.Behaviour.Custom;

public class Binoculars : MonoBehaviour
{
    public static bool BinocularsActive;
    
    public float speed = 20;

    public float maxZoom = 2.5f;
    public float minZoom = 0.25f;
    public float startZoom = 1;
    public Vector3 startOffset;

    private bool _active;

    private Vector3 _targetPos;
    private float _zoom;

    private Vector3 _playerPos;

    private void Update()
    {
        if (!_active) return;

        var actions = InputHandler.Instance.inputActions;
        if (actions.Jump.WasPressed)
        {
            BinocularsActive = false;
            _active = false;
            HeroController.instance.damageMode = DamageMode.FULL_DAMAGE;
            HeroController.instance.vignette.gameObject.SetActive(true);
            HeroController.instance.RegainControl();
            StartCoroutine(ReturnZoom());
            gameObject.BroadcastEvent("OnStop");
            return;
        }

        float vertical = 0;
        if (actions.Up.IsPressed) vertical++;
        if (actions.Down.IsPressed) vertical--;

        float horizontal = 0;
        if (actions.Right.IsPressed) horizontal++;
        if (actions.Left.IsPressed) horizontal--;

        var zf = GameCameras.instance.tk2dCam.ZoomFactor;

        _targetPos = CameraBorder.KeepWithinBounds(_targetPos + 
                      new Vector3(horizontal * Time.deltaTime * speed / zf, vertical * Time.deltaTime * speed / zf, 0));
        
        var cameraTransform = GameCameras.instance.cameraController.transform;
        cameraTransform.position = Vector3.Lerp(cameraTransform.position, _targetPos, 9 * Time.deltaTime);

        _zoom = Mathf.Clamp(_zoom + Input.mouseScrollDelta.y / 20, minZoom, maxZoom);

        HeroController.instance.transform.position = _playerPos;

        GameCameras.instance.tk2dCam.ZoomFactor = Mathf.Lerp(zf, _zoom, 15 * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<NailSlash>()) StartUsing();
    }

    public static void Init()
    {
        typeof(CameraController).Hook("LateUpdate",
            (Action<CameraController> orig, CameraController self) =>
            {
                if (BinocularsActive) return;
                orig(self);
            });
        
        typeof(HeroController).Hook("Move",
            (Action<HeroController, float, bool> orig, HeroController self, float moveDir, bool useInp) =>
            {
                if (BinocularsActive) return;
                orig(self, moveDir, useInp);
            });
    }

    public void StartUsing()
    {
        _zoom = startZoom;
        BinocularsActive = true;
        _active = true;
        HeroController.instance.damageMode = DamageMode.NO_DAMAGE;
        HeroController.instance.vignette.gameObject.SetActive(false);
        HeroController.instance.RelinquishControl();

        _playerPos = HeroController.instance.transform.position;
        _targetPos = GameCameras.instance.cameraController.transform.position + startOffset;

        gameObject.BroadcastEvent("OnStart");
    }

    private static IEnumerator ReturnZoom()
    {
        while (Math.Abs(GameCameras.instance.tk2dCam.ZoomFactor - 1) > 0.001f)
        {
            if (BinocularsActive) yield break;
            GameCameras.instance.tk2dCam.ZoomFactor =
                Mathf.Lerp(GameCameras.instance.tk2dCam.ZoomFactor, 1, 10 * Time.deltaTime);
            yield return null;
        }

        GameCameras.instance.tk2dCam.ZoomFactor = 1;
    }
}