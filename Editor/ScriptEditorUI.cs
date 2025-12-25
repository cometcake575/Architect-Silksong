using System.Collections.Generic;
using Architect.Events.Blocks;
using Architect.Events.Blocks.Events;
using Architect.Events.Blocks.Operators;
using Architect.Placements;
using Architect.Storage;
using Architect.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

namespace Architect.Editor;

public static class ScriptEditorUI
{
    public static GameObject BlocksParent;
    public static GameObject LinesParent;
    private static Transform _blockTransformSource;
    
    private static Image _bgImg;
    private static Text _bgTxt;

    public static void Init(GameObject scriptUI)
    {
        ScriptManager.Init();
        
        _bgImg = UIUtils.MakeImage(
            "Background",
            scriptUI,
            new Vector2(0, 2510),
            new Vector2(0.5f, 0),
            new Vector2(0.5f, 0),
            new Vector2(10000, 10000));
        _bgImg.color = Settings.EditorBackgroundColour.Value;
        _bgImg.sprite = UIUtils.Square;
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
        
        LinesParent = new GameObject("Lines Parent")
        {
            transform = { parent = scriptUI.transform }
        };
        var lp = LinesParent.AddComponent<RectTransform>();
        lp.anchorMax = Vector2.one;
        lp.anchorMin = Vector2.zero;
        lp.offsetMax = Vector2.zero;
        lp.offsetMin = Vector2.zero;
        lp.anchoredPosition = new Vector2(0, 20);
        
        Deletable.Init(scriptUI);
        
        var addParent = new GameObject("Buttons Parent")
        {
            transform = { parent = scriptUI.transform }
        };
        var ap = addParent.AddComponent<RectTransform>();
        ap.anchorMax = new Vector2(0.5f, 1);
        ap.anchorMin = new Vector2(0.5f, 1);
        ap.offsetMax = Vector2.zero;
        ap.offsetMin = Vector2.zero;
        ap.anchoredPosition = new Vector2(0, -40);

        var (left, _) = UIUtils.MakeTextButton(
            "Go Left",
            "⬅",
            addParent,
            new Vector2(-310, 0),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            size: new Vector2(50, 50));
        var (right, _) = UIUtils.MakeTextButton(
            "Go Right",
            "➡",
            addParent,
            new Vector2(310, 0),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            size: new Vector2(50, 50));

        List<Text> labels = [];
        
        var index = 0;
        for (var i = 0; i < 8; i++)
        {
            var (btn, txt) = UIUtils.MakeTextButton(
                "Add Object",
                "[Placeholder]",
                addParent,
                new Vector2((i - 3.5f) * 75, 0),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                size: new Vector2(200, 50));
            var i1 = i;
            btn.onClick.AddListener(() =>
            {
                if (index + i1 >= ScriptManager.CurrentBlocks.Count) return;
                var block = ScriptManager.CurrentBlocks[index + i1].Item1();
                block.Setup(true, true);
                PlacementManager.GetLevelData().ScriptBlocks.Add(block);
            });
            labels.Add(txt.textComponent);
        }
        
        left.onClick.AddListener(() =>
        {
            index -= 8;
            if (index < 0) index += Mathf.CeilToInt(ScriptManager.CurrentBlocks.Count / 8f) * 8;

            DoRefresh();
        });
        right.onClick.AddListener(() =>
        {
            index += 8;
            if (index >= ScriptManager.CurrentBlocks.Count) 
                index -= Mathf.CeilToInt(ScriptManager.CurrentBlocks.Count / 8f) * 8;

            DoRefresh();
        });
        
        var (topBtn, topImg, _) = UIUtils.MakeButtonWithImage(
            "Top",
            scriptUI,
            new Vector2(-32, -62),
            new Vector2(1, 1),
            new Vector2(1, 1),
            128,
            64);
        topImg.sprite = ResourceUtils.LoadSpriteResource("Flowcharts.output", ppu: 15, filterMode: FilterMode.Point);
        topBtn.onClick.AddListener(() =>
        {
            ScriptManager.CurrentType = ScriptManager.BlockType.Output;
            index = 0;
            DoRefresh();
        });
        
        var (midBtn, midImg, _) = UIUtils.MakeButtonWithImage(
            "Middle",
            scriptUI,
            new Vector2(-32, -5),
            new Vector2(1, 0.5f),
            new Vector2(1, 0.5f),
            128,
            64);
        midImg.sprite = ResourceUtils.LoadSpriteResource("Flowcharts.middle", ppu: 15, filterMode: FilterMode.Point);
        midBtn.onClick.AddListener(() =>
        {
            ScriptManager.CurrentType = ScriptManager.BlockType.Process;
            index = 0;
            DoRefresh();
        });

        var (botBtn, botImg, _) = UIUtils.MakeButtonWithImage(
            "Bottom",
            scriptUI,
            new Vector2(-32, 52),
            new Vector2(1, 0),
            new Vector2(1, 0),
            128,
            64);
        botImg.sprite = ResourceUtils.LoadSpriteResource("Flowcharts.input", ppu: 15, filterMode: FilterMode.Point);
        botBtn.onClick.AddListener(() =>
        {
            ScriptManager.CurrentType = ScriptManager.BlockType.Input;
            index = 0;
            DoRefresh();
        });
        
        DoRefresh();
        return;

        void DoRefresh()
        {
            for (var i = 0; i < labels.Count; i++)
            {
                labels[i].text = ScriptManager.CurrentBlocks.Count <= index + i ? "" :
                    ScriptManager.CurrentBlocks[index + i].Item2;
            }
        }
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
            LinesParent.transform.position = BlocksParent.transform.position;

            var scale = BlocksParent.transform.localScale.x;
            BlocksParent.transform.localPosition = new Vector3(
                Mathf.Clamp(BlocksParent.transform.localPosition.x, -1250 * scale, 1250 * scale),
                Mathf.Clamp(BlocksParent.transform.localPosition.y, -1250 * scale, 1250 * scale));
            LinesParent.transform.localPosition = BlocksParent.transform.localPosition;
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            _offset = (Vector2)BlocksParent.transform.position - eventData.position;
        }

        private void Update()
        {
            if (Input.mouseScrollDelta.y == 0) return;
            
            Deletable.DeleteButton.SetActive(false);
            var par = BlocksParent.transform.parent;
            _blockTransformSource.transform.position = Input.mousePosition;
            
            BlocksParent.transform.SetParent(_blockTransformSource, true);
            LinesParent.transform.SetParent(_blockTransformSource, true);
            _blockTransformSource.localScale = new Vector2(
                Mathf.Clamp(_blockTransformSource.localScale.x + Input.mouseScrollDelta.y / 40, 0.25f, 2), 
                Mathf.Clamp(_blockTransformSource.localScale.y + Input.mouseScrollDelta.y / 40, 0.25f, 2));
            BlocksParent.transform.SetParent(par, true);
            LinesParent.transform.SetParent(par, true);

            var scale = BlocksParent.transform.localScale.x;
            BlocksParent.transform.localPosition = new Vector3(
                Mathf.Clamp(BlocksParent.transform.localPosition.x, -1250 * scale, 1250 * scale),
                Mathf.Clamp(BlocksParent.transform.localPosition.y, -1250 * scale, 1250 * scale));
            LinesParent.transform.localPosition = BlocksParent.transform.localPosition;
        }
    }
}