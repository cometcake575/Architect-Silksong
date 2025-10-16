using System.Linq;
using Architect.Config;
using Architect.Objects.Placeable;
using Architect.Utils;
using UnityEngine;
using UnityEngine.Video;

namespace Architect.Editor;

public static class CursorManager
{
    private static GameObject _cursorObject;
    private static SpriteRenderer _renderer;
    private static VideoPlayer _player;

    public static bool NeedsRefresh = true;

    public static Vector3 Offset;
    
    public static void Init()
    {
        _cursorObject = new GameObject("[Architect] Cursor");

        _renderer = _cursorObject.AddComponent<SpriteRenderer>();
        
        _player = _cursorObject.AddComponent<VideoPlayer>();
        
        _cursorObject.SetActive(false);

        _renderer.color = new Color(1, 0.2f, 0.2f, 0.5f);
        
        Object.DontDestroyOnLoad(_cursorObject);
    }

    public static void Update()
    {
        if (!EditManager.IsEditing ||
            !HeroController.instance || 
            GameManager.instance.isPaused || 
            EditManager.CurrentObject is not PlaceableObject placeable)
        {
            _cursorObject.SetActive(false);
            return;
        }
        
        if (NeedsRefresh)
        {
            NeedsRefresh = false;
            Refresh(placeable);
        }
        
        _cursorObject.SetActive(true);

        _cursorObject.transform.position = EditManager.GetWorldPos(Input.mousePosition, true) + Offset;
    }

    public static void Refresh(PlaceableObject obj)
    {
        var rot = EditManager.CurrentRotation + obj.Rotation;
        
        Offset = PreviewUtils.FixPreview(
            _renderer,
            obj,
            EditManager.CurrentlyFlipped,
            rot,
            EditManager.CurrentScale);
        Offset.z += obj.ZPosition + 0.001f;
        
        _renderer.sprite = obj.Sprite;
        _cursorObject.transform.localScale = obj.LossyScale * EditManager.CurrentScale;
        _cursorObject.transform.SetRotation2D(rot + obj.ChildRotation + obj.Tk2dRotation);

        _renderer.sortingOrder = 0;
        
        _player.url = "";
        
        foreach (var config in EditManager.Config.Values.OrderBy(configVal => configVal.GetPriority()))
        {
            config.SetupPreview(_cursorObject, ConfigurationManager.PreviewContext.Cursor);
        }
    }
}