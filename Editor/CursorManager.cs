using System.Linq;
using Architect.Config;
using Architect.Objects.Placeable;
using Architect.Utils;
using UnityEngine;
using UnityEngine.Video;
using Object = UnityEngine.Object;

namespace Architect.Editor;

public static class CursorManager
{
    private static GameObject _cursorObject;
    private static SpriteRenderer _renderer;

    public static bool NeedsRefresh = true;
    public static bool ObjectChanged = true;

    public static Vector3 Offset;

    private static bool _needsFullRefresh = true;
    private static float _canFullRefreshTime;
    private static float _prevScale = 1;
    
    public static void Init()
    {
        _cursorObject = new GameObject("[Architect] Cursor");

        _renderer = _cursorObject.AddComponent<SpriteRenderer>();
        
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
        
        if (!_cursorObject.activeSelf || NeedsRefresh || (_canFullRefreshTime < Time.time && _needsFullRefresh))
        {
            _cursorObject.SetActive(true);
            
            NeedsRefresh = false;
            _needsFullRefresh = false;
            Refresh(placeable);
        }

        _cursorObject.transform.position = EditManager.GetWorldPos(Input.mousePosition, true) + Offset;
    }

    public static void Refresh(PlaceableObject obj)
    {
        var rot = EditManager.CurrentRotation + obj.Rotation;

        var oldZ = Offset.z;
        Offset = PreviewUtils.FixPreview(
            _renderer,
            obj,
            EditManager.CurrentlyFlipped,
            rot,
            EditManager.CurrentScale);

        var play = _cursorObject.GetComponent<VideoPlayer>();
        if (ObjectChanged)
        {
            if (play) play.url = "";
            _cursorObject.transform.localScale = obj.LossyScale * EditManager.CurrentScale;
            _renderer.sprite = obj.Sprite;
        } else _cursorObject.transform.localScale = _cursorObject.transform.localScale / _prevScale * EditManager.CurrentScale;
        
        _prevScale = EditManager.CurrentScale;
        
        _cursorObject.transform.SetRotation2D(rot + obj.ChildRotation + obj.Tk2dRotation);
        
        if (_canFullRefreshTime > Time.time && !ObjectChanged)
        {
            Offset.z = oldZ;
            _needsFullRefresh = true;
            return;
        }
        _cursorObject.transform.localScale = obj.LossyScale * EditManager.CurrentScale;
        ObjectChanged = false;
        
        _canFullRefreshTime = Time.time + 2;
        
        Offset.z += obj.ZPosition + 0.001f;
        _renderer.sortingOrder = 0;

        if (play) play.url = "";
        
        foreach (var config in EditManager.Config.Values.OrderBy(configVal => configVal.GetPriority()))
        {
            config.SetupPreview(_cursorObject, ConfigurationManager.PreviewContext.Cursor);
        }
    }
}