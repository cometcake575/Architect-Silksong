using Architect.Storage;
using Architect.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Architect.Editor;

public static class ScriptEditorUI
{
    public static GameObject BlocksParent;
    private static Transform _blockTransformSource;
    
    private static Image _bgImg;
    private static Text _bgTxt;
    
    public static void Init(GameObject scriptUI)
    {
        _bgImg = UIUtils.MakeImage(
            "Background",
            scriptUI,
            new Vector2(0, 2510),
            new Vector2(0.5f, 0),
            new Vector2(0.5f, 0),
            new Vector2(10000, 10000));
        _bgImg.color = Settings.EditorBackgroundColour.Value;
        _bgImg.sprite = ResourceUtils.LoadSpriteResource("square");
        _bgImg.gameObject.AddComponent<BackgroundDrag>();

        var label = UIUtils.MakeLabel(
            "Architect Text",
            scriptUI,
            Vector2.zero,
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f));
        label.font = "TrajanPro-Bold";
        _bgTxt = label.textComponent;
        _bgTxt.text = "architect";
        _bgTxt.alignment = TextAnchor.MiddleCenter;
        _bgTxt.fontSize = 50;
        label.transform.SetAsLastSibling();

        _blockTransformSource = new GameObject("Transform Source")
        {
            transform = { parent = scriptUI.transform }
        }.transform;
        
        BlocksParent = new GameObject("Blocks Parent")
        {
            transform = { parent = scriptUI.transform }
        };
        var bp = BlocksParent.AddComponent<RectTransform>();
        bp.anchorMax = Vector2.one;
        bp.anchorMin = Vector2.zero;
        bp.offsetMax = Vector2.zero;
        bp.offsetMin = Vector2.zero;
        bp.anchoredPosition = new Vector2(0, 20);
    }

    public static void UpdateColour()
    {
        var col = Settings.EditorBackgroundColour.Value;
        
        _bgImg.color = col;
        _bgTxt.color = col + new Color(
            col.r >= 0.9f ? -0.1f : 0.1f,
            col.g >= 0.9f ? -0.1f : 0.1f,
            col.b >= 0.9f ? -0.1f : 0.1f);
    }

    public class BackgroundDrag : MonoBehaviour, IDragHandler, IBeginDragHandler
    {
        private Vector2 _offset;
        
        public void OnDrag(PointerEventData eventData)
        {
            BlocksParent.transform.position = eventData.position + _offset;

            BlocksParent.transform.localPosition = new Vector3(
                Mathf.Clamp(BlocksParent.transform.localPosition.x, -500, 500),
                Mathf.Clamp(BlocksParent.transform.localPosition.y, -500, 500));
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            _offset = (Vector2)BlocksParent.transform.position - eventData.position;
        }

        private void Update()
        {
            if (Input.mouseScrollDelta.y == 0) return;
            
            var par = BlocksParent.transform.parent;
            _blockTransformSource.transform.position = Input.mousePosition;
            
            BlocksParent.transform.SetParent(_blockTransformSource, true);
            _blockTransformSource.localScale = new Vector2(
                Mathf.Clamp(_blockTransformSource.localScale.x + Input.mouseScrollDelta.y / 40, 0.25f, 2), 
                Mathf.Clamp(_blockTransformSource.localScale.y + Input.mouseScrollDelta.y / 40, 0.25f, 2));
            BlocksParent.transform.SetParent(par, true);

            BlocksParent.transform.localPosition = new Vector3(
                Mathf.Clamp(BlocksParent.transform.localPosition.x, -400, 400),
                Mathf.Clamp(BlocksParent.transform.localPosition.y, -400, 400));
        }
    }
}