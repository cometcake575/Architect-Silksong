using System;
using Architect.Behaviour.Utility;
using Architect.Storage;
using Architect.Utils;
using GlobalEnums;
using TMProOld;
using UnityEngine;

namespace Architect.Behaviour.Custom;

public class CustomText : PreviewableBehaviour
{
    public string defaultText = "Sample Text";
    public int verticalAlignment;
    public int horizontalAlignment;
    public Vector2 textAreaSize = new(20, 10);
    public Vector2 offset;
    public Color textColour = Color.white;
    public bool onHud;
    
    private GameObject _textObj;
    private TextMeshPro _tmp;

    private bool _preview;

    private static GameObject DialogueBox
    {
        get
        {
            if (field) return field;
            return field = GameCameras.instance.hudCamera.transform
                .Find("In-game").Find("DialogueManager").Find("DialogueBox").Find("Text").gameObject;
        }
    }

    private void Start()
    {
        if (isAPreview) return;
        Setup();
    }
    
    private void Setup()
    {
        if (_textObj) Destroy(_textObj);
        
        _textObj = Instantiate(DialogueBox, transform);

        _tmp = _textObj.GetComponent<TextMeshPro>();
        _tmp.text = defaultText;
        _tmp.color = textColour;
        _tmp.maxVisibleCharacters = 99999;
        
        _tmp.alignment = (verticalAlignment, horizontalAlignment) switch
        {
            (0, 0) => TextAlignmentOptions.TopLeft,
            (0, 1) => TextAlignmentOptions.Top,
            (0, 2) => TextAlignmentOptions.TopRight,
            (1, 0) => TextAlignmentOptions.Left,
            (1, 1) => TextAlignmentOptions.Center,
            (1, 2) => TextAlignmentOptions.Right,
            (2, 0) => TextAlignmentOptions.BottomLeft,
            (2, 1) => TextAlignmentOptions.Bottom,
            (2, 2) => TextAlignmentOptions.BottomRight,
            _ => throw new ArgumentOutOfRangeException()
        };
        _tmp.rectTransform.sizeDelta = textAreaSize;

        if (onHud)
        {
            _textObj.transform.position = offset;
            return;
        }
        
        _textObj.transform.localPosition = Vector3.zero;
        _textObj.RemoveComponent<MeshSortingOrder>();
        _textObj.layer = (int)PhysLayers.DEFAULT;
        _tmp.renderer.sortingLayerName = "Default";
        _tmp.renderer.sortingOrder = 0;
    }

    private void Update()
    {
        if (!isAPreview) return;

        var preview = Settings.Preview.IsPressed;
        if (preview == _preview) return;
        
        _preview = preview;
        if (preview) Setup();
        else if (_textObj) Destroy(_textObj);
    }

    public void SetText(string text)
    {
        _tmp.text = defaultText = text;
    }
}