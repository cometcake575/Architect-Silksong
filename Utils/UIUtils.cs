using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;

namespace Architect.Utils;

public static class UIUtils
{
    public static readonly Sprite Square = ResourceUtils.LoadSpriteResource("square");

    private static readonly Color LightGrey = new(0.7f, 0.7f, 0.7f);
    
    private static readonly Sprite ButtonSprite = ResourceUtils.LoadSpriteResource(
        "button_outline",
        new Vector2(0.5f, 0.5f),
        border: new Vector4(6, 5, 6, 5)
    );
    private static readonly Sprite ButtonSpriteBlank = ResourceUtils.LoadSpriteResource(
        "button_no_outline",
        new Vector2(0.5f, 0.5f),
        border: new Vector4(6, 5, 6, 5)
    );
    
    private static readonly Vector2 Centre = new(0.5f, 0.5f);

    public static (Button, Image, Label) MakeButtonWithImage(string name,
        GameObject parent,
        Vector2 pos,
        Vector2 anchorMin,
        Vector2 anchorMax,
        int size,
        int imageSize,
        bool doOutline = true)
    {
        var gameObject = new GameObject(name);

        var trans = gameObject.AddComponent<RectTransform>();
        var btn = gameObject.AddComponent<Button>();
        var outline = gameObject.AddComponent<Image>();
        
        var c = btn.colors;
        c.pressedColor = LightGrey;
        c.disabledColor = Color.grey;
        c.fadeDuration = 0.1f;
        btn.colors = c;

        btn.targetGraphic = outline;

        outline.sprite = doOutline ? ButtonSprite : ButtonSpriteBlank;
        outline.type = Image.Type.Sliced;
        
        trans.SetParent(parent.transform, false);
        
        trans.anchorMin = anchorMin;
        trans.anchorMax = anchorMax;

        trans.sizeDelta = new Vector2(size, size);
        gameObject.transform.localScale /= 3;

        trans.anchoredPosition = pos;

        var imgObj = new GameObject("Image");
        imgObj.transform.SetParent(trans, false);
        
        imgObj.AddComponent<RectTransform>().sizeDelta = new Vector2(imageSize, imageSize);
        var img = imgObj.AddComponent<Image>();
        img.preserveAspect = true;
        
        var label = MakeLabel(name + " Label", gameObject, Vector2.zero, Centre, Centre);
        label.textComponent.alignment = TextAnchor.MiddleCenter;

        return (btn, img, label);
    }

    public static (Button, Label) MakeTextButton(string name, string text, GameObject parent, Vector2 pos,
        Vector2 anchorMin, Vector2 anchorMax, bool hasOutline = true, Vector2 size = default)
    {
        var gameObject = new GameObject(name);

        var trans = gameObject.AddComponent<RectTransform>();
        var btn = gameObject.AddComponent<Button>();

        Label label;

        if (hasOutline)
        {
            label = MakeLabel(name + " Label", parent, pos, anchorMin, anchorMax);
            var outline = gameObject.AddComponent<Image>();
            btn.targetGraphic = outline;

            outline.sprite = ButtonSprite;
            outline.type = Image.Type.Sliced;
            
            var c = btn.colors;
            c.pressedColor = LightGrey;
            c.fadeDuration = 0.1f;
            btn.colors = c;
            label.textComponent.fontSize = 8;
        }
        else
        {
            label = MakeLabel(name + " Label", gameObject, Vector2.zero, Centre, Centre);
            btn.targetGraphic = label.textComponent;
            label.transform.SetParent(trans);
            label.textComponent.fontSize = 44;
            label.transform.localScale /= 3;
        }
        
        label.textComponent.alignment = TextAnchor.MiddleCenter;
        label.textComponent.text = text;

        trans.SetParent(parent.transform, false);
        
        trans.anchorMin = anchorMin;
        trans.anchorMax = anchorMax;

        gameObject.transform.localScale /= 3;

        trans.anchoredPosition = pos;

        if (size == Vector2.zero) gameObject.AddComponent<LabelledButton>().label = label;
        else trans.sizeDelta = size;

        return (btn, label);
    }

    public static (InputField, Label) MakeTextbox(string name, GameObject parent, Vector2 pos,
        Vector2 anchorMin, Vector2 anchorMax, float width, float height, int fontSize = 20)
    {
        var gameObject = new GameObject(name);

        var trans = gameObject.AddComponent<RectTransform>();
        var field = gameObject.AddComponent<InputField>();

        trans.sizeDelta = new Vector2(width, height);

        var label = MakeLabel(name + " Label", parent, pos, anchorMin, anchorMax, width - 15);
        var outline = gameObject.AddComponent<Image>();
        field.targetGraphic = outline;

        outline.sprite = ButtonSprite;
        outline.type = Image.Type.Sliced;

        var c = field.colors;
        c.selectedColor = LightGrey;
        c.pressedColor = LightGrey;
        c.fadeDuration = 0.1f;
        field.colors = c;

        field.textComponent = label.textComponent;
        field.textComponent.supportRichText = false;
        
        label.textComponent.fontSize = fontSize;

        label.transform.localScale /= 3;
        label.textComponent.alignment = TextAnchor.MiddleLeft;

        trans.SetParent(parent.transform, false);

        trans.anchorMin = anchorMin;
        trans.anchorMax = anchorMax;

        gameObject.transform.localScale /= 3;

        trans.anchoredPosition = pos;
        
        return (field, label);
    }

    public class LabelledButton : MonoBehaviour
    {
        public Label label;

        private void Start()
        {
            var trans = GetComponent<RectTransform>();
            trans.sizeDelta = new Vector2(label.textComponent.preferredWidth * 3 + 10,
                label.textComponent.preferredHeight * 3);
        }
    }

    public static Label MakeLabel(string name, GameObject parent, Vector2 pos, Vector2 anchorMin, Vector2 anchorMax, 
        float maxWidth = 80, float maxHeight = 80, HorizontalWrapMode wrapMode = HorizontalWrapMode.Overflow)
    {
        var gameObject = new GameObject(name);

        var trans = gameObject.AddComponent<RectTransform>();
        var label = gameObject.AddComponent<Label>();
        label.textComponent = gameObject.AddComponent<Text>();
        label.textComponent.supportRichText = false;
        label.textComponent.horizontalOverflow = wrapMode;
        
        trans.SetParent(parent.transform, false);
        trans.SetAsFirstSibling();

        trans.anchorMin = anchorMin;
        trans.anchorMax = anchorMax;

        trans.sizeDelta = new Vector2(maxWidth, maxHeight);

        trans.anchoredPosition = pos;
        
        return label;
    }

    public class Label : MonoBehaviour
    {
        public string font = "Perpetua";

        public Text textComponent;

        private void Start()
        {
            textComponent.font = GetFont(font);
        }
    }

    private static readonly Dictionary<string, Font> Fonts = [];

    public static Font GetFont(string fontName)
    {
        if (Fonts.TryGetValue(fontName, out var font1)) return font1;
        
        foreach (var font in Resources.FindObjectsOfTypeAll<Font>())
        {
            if (font.name == fontName)
            {
                Fonts[fontName] = font;
                
                return font;
            }
        }

        return Fonts.GetValueOrDefault(fontName);
    }

    public static Image MakeImage(
        string name, 
        GameObject parent, 
        Vector2 pos,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 size
    )
    {
        var imgObj = new GameObject(name);
        imgObj.transform.SetParent(parent.transform, false);

        var trans = imgObj.AddComponent<RectTransform>();

        trans.anchoredPosition = pos;
        trans.anchorMin = anchorMin;
        trans.anchorMax = anchorMax;
        
        trans.sizeDelta = size;
        var img = imgObj.AddComponent<Image>();
        img.preserveAspect = false;

        imgObj.transform.localScale *= 0.5f;
        
        return img;
    }

    public static void RemoveOffset(this GameObject obj)
    {
        var rt = obj.GetOrAddComponent<RectTransform>();
        rt.anchorMax = Vector2.one;
        rt.anchorMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.offsetMin = Vector2.zero;
    }
}