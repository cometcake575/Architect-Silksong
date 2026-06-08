using System.Linq;
using Architect.Behaviour.Fixers;
using Architect.Config;
using Architect.Objects.Placeable;
using Architect.Utils;
using UnityEngine;

namespace Architect.Editor;

public static class CursorManager
{
    private static GameObject _cursorObject;

    public static bool NeedsRefresh = true;
    public static bool ObjectChanged = true;

    public static Vector3 Offset;

    public static void Update()
    {
        if (!EditManager.IsEditing ||
            !HeroController.instance || 
            GameManager.instance.isPaused || 
            EditManager.CurrentObject is not PlaceableObject placeable)
        {
            if (_cursorObject) _cursorObject.SetActive(false);
            NeedsRefresh = true;
            return;
        }
        
        if (!_cursorObject || NeedsRefresh)
        {
            NeedsRefresh = false;
            Refresh(placeable);
        }

        _cursorObject.transform.position = (EditManager.GetWorldPos(Input.mousePosition, true) + Offset)
            .Where(z: EditManager.CurrentZ + 0.001f);
    }

    public static void Refresh(PlaceableObject type)
    {
        if (ObjectChanged && _cursorObject)
        {
            Object.Destroy(_cursorObject);
            _cursorObject = null;
        }
        ObjectChanged = false;

        if (_cursorObject)
        {
            _cursorObject.SetActive(true);
        } else {
            var wasPrefabActive = type.Prefab.activeSelf;
            type.Prefab.SetActive(false);
            var obj = Object.Instantiate(type.Prefab);
            if (wasPrefabActive) type.Prefab.SetActive(true);

            obj.name = "[Architect] Cursor";
            Object.DontDestroyOnLoad(obj);

            obj.AddComponent<MiscFixers.PreviewState>();
            
            type.PostSpawnAction?.Invoke(obj);
            
            _cursorObject = obj;

            Offset = Vector3.zero;
            if (type.SpritePreview)
            {
                _cursorObject = PreviewUtils.MakeSpritePreview(
                    _cursorObject, 
                    type, 
                    EditManager.CurrentlyFlipped, EditManager.CurrentRotation, EditManager.CurrentScale, 
                    out Offset);
            }
        
            var preview = _cursorObject.AddComponent<PreviewUtils.Preview>();
            preview.Setup(type);

            preview.Settings = new PreviewUtils.PreviewSettings(1, 0.2f, 0.2f, 0.5f);
        
            _cursorObject.SetActive(true);
            
            foreach (var rb2d in _cursorObject.GetComponentsInChildren<Rigidbody2D>(true))
            {
                rb2d.constraints = RigidbodyConstraints2D.FreezeAll;
            }
        }

        var rot = EditManager.CurrentRotation + type.Prefab.transform.GetRotation2D();
        _cursorObject.transform.SetRotation2D(rot);

        _cursorObject.transform.localScale = (type.IgnoreScale ? Vector3.one : type.Prefab.transform.localScale)
                                             * EditManager.CurrentScale;
        
        _cursorObject.transform.SetScaleX((EditManager.CurrentlyFlipped ? -1 : 1) 
                                          * (type.IgnoreScale ? 1 : type.Prefab.transform.GetScaleX()) 
                                          * EditManager.CurrentScale);
        
        foreach (var configVal in EditManager.Config.Values
                     .OrderBy(configVal => configVal.GetPriority())) 
            configVal.SetupPreview(_cursorObject, ConfigurationManager.PreviewContext.Cursor);
    }
}