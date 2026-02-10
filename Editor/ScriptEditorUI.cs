using System;
using System.Collections.Generic;
using Architect.Events.Blocks;
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
    public static GameObject Blocks => ScriptManager.IsLocal ? _localBlocks : _globalBlocks;
    public static GameObject Lines => ScriptManager.IsLocal ? _localLines : _globalLines;
    public static GameObject ScriptParent => ScriptManager.IsLocal ? LocalParent : GlobalParent;
    
    public static GameObject ToggleParent;
    
    public static GameObject GlobalParent;
    public static GameObject LocalParent;
    
    private static GameObject _localBlocks;
    private static GameObject _localLines;
    
    private static GameObject _globalBlocks;
    private static GameObject _globalLines;
    
    private static Transform _blockTransformSource;
    
    private static Image _bgImg;
    private static Text _bgTxt;

    private static Text _localBtnText;
    private static Text _globalBtnText;

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

        var scripts = CreateBlankParent("Scripts", scriptUI, 0);
        
        GlobalParent = CreateBlankParent("Global Script", scripts, 0);
        LocalParent = CreateBlankParent("Local Script", scripts, 0);
        
        _localBlocks = CreateBlankParent("Local Blocks", LocalParent, 20);
        _localLines = CreateBlankParent("Local Lines", LocalParent, 20);
        
        _globalBlocks = CreateBlankParent("Global Blocks", GlobalParent, 20);
        _globalLines = CreateBlankParent("Global Lines", GlobalParent, 20);
        
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

                (ScriptManager.IsLocal ? PlacementManager.GetLevelData() : PlacementManager.GetGlobalData())
                    .ScriptBlocks.Add(block);
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

        ToggleParent = CreateBlankParent("Mode Toggles", scriptUI, 0);
        _localBtnText = SetupSwitchButton(ToggleParent, true, "Local Script", new Vector3(-200, 20));
        _localBtnText.color = Color.yellow;
        _globalBtnText = SetupSwitchButton(ToggleParent, false, "Global Script", new Vector3(200, 20));
        
        typeof(HeroController).Hook(nameof(HeroController.SceneInit),
            (Action<HeroController> orig, HeroController self) =>
            {
                orig(self);
                GlobalParent.transform.localPosition = Vector3.zero;
                LocalParent.transform.localPosition = Vector3.zero;
            });

        ScriptManager.IsLocal = true;
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
    
    private static Text SetupSwitchButton(GameObject parent, bool local, string name, Vector3 pos)
    {
        var size = new Vector2(750, 40);
        var (btn, label) = UIUtils.MakeTextButton(name + " Button", name, parent, pos, 
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), size:size);
        
        btn.onClick.AddListener(() =>
        {
            _localBtnText.color = local ? Color.yellow : Color.white;
            _globalBtnText.color = local ? Color.white : Color.yellow;
            ScriptManager.IsLocal = local;
            Deletable.DeleteButton.SetActive(false);
        });
        label.textComponent.fontSize = 8;
        return label.textComponent;
    }

    private static GameObject CreateBlankParent(string name, GameObject parent, int yOffset)
    {
        var obj = new GameObject(name)
        {
            transform = { parent = parent.transform }
        };
        var rt = obj.AddComponent<RectTransform>();
        rt.anchorMax = Vector2.one;
        rt.anchorMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.offsetMin = Vector2.zero;
        rt.anchoredPosition = new Vector2(0, yOffset);

        return obj;
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

    public class BackgroundDrag : MonoBehaviour, IDragHandler, IBeginDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        private Vector2 _offset;
        
        private Image _selectionImage;
        private RectTransform _selectionRect;
        private RectTransform _uiParentRect;
        private Vector2 _selectStartLocal;
        private Vector2 _selectStartScreen;
        private bool _isSelecting;

        private void Awake()
        {
            _uiParentRect = transform.parent as RectTransform;

            var selGo = new GameObject("Selection Rectangle");
            selGo.transform.SetParent(_uiParentRect, false);

            _selectionRect = selGo.AddComponent<RectTransform>();

            _selectionRect.anchorMin = Vector2.zero;
            _selectionRect.anchorMax = Vector2.zero;
            _selectionRect.pivot = Vector2.zero;
            _selectionRect.anchoredPosition = Vector2.zero;
            _selectionRect.sizeDelta = Vector2.zero;
            _selectionRect.localScale = Vector3.one;

            _selectionImage = selGo.AddComponent<Image>();
            _selectionImage.sprite = UIUtils.Square;
            _selectionImage.color = new Color(0f, 0.5f, 1f, 0.12f);
            _selectionImage.raycastTarget = false;

            var outline = selGo.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0.5f, 1f, 0.8f);
            outline.effectDistance = new Vector2(1.5f, 1.5f);
            outline.useGraphicAlpha = true;

            selGo.transform.SetAsLastSibling();

            selGo.SetActive(false);
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                ScriptManager.ClearSelection();
                return;
            }

            if (eventData.button != PointerEventData.InputButton.Right) return;
            if (_uiParentRect == null) return;

            _selectStartScreen = eventData.position;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _uiParentRect, eventData.position, eventData.pressEventCamera, out _selectStartLocal))
            {
                _isSelecting = false;
                return;
            }

            _isSelecting = true;

            var parentRect = _uiParentRect.rect;
            var pivotOffset = new Vector2(parentRect.width * _uiParentRect.pivot.x, parentRect.height * _uiParentRect.pivot.y);
            var bottomLeftStart = _selectStartLocal + pivotOffset;

            _selectionRect.anchoredPosition = bottomLeftStart;
            _selectionRect.sizeDelta = Vector2.zero;
            _selectionImage.gameObject.SetActive(true);
            // keep rectangle above everything while dragging
            _selectionRect.transform.SetAsLastSibling();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                _offset = (Vector2)ScriptParent.transform.position - eventData.position;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right && _isSelecting && _uiParentRect != null)
            {
                if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        _uiParentRect, eventData.position, eventData.pressEventCamera, out var currentLocal))
                    return;

                var parentRect = _uiParentRect.rect;
                var pivotOffset = new Vector2(parentRect.width * _uiParentRect.pivot.x, parentRect.height * _uiParentRect.pivot.y);
                var startBL = _selectStartLocal + pivotOffset;
                var currBL = currentLocal + pivotOffset;

                var min = Vector2.Min(startBL, currBL);
                var max = Vector2.Max(startBL, currBL);
                var size = max - min;

                _selectionRect.anchoredPosition = min;
                _selectionRect.sizeDelta = size;
                _selectionRect.transform.SetAsLastSibling();
                return;
            }

            if (eventData.button == PointerEventData.InputButton.Left && !_isSelecting)
            {
                ScriptParent.transform.position = eventData.position + _offset;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Right) return;
            if (!_isSelecting) return;

            var endScreen = eventData.position;
            var sMin = Vector2.Min(_selectStartScreen, endScreen);
            var sMax = Vector2.Max(_selectStartScreen, endScreen);
            var selectionRect = new Rect(sMin, sMax - sMin);

            if (selectionRect.width < 4f && selectionRect.height < 4f)
            {
                ScriptManager.ClearSelection();
            }
            else
            {
                var selectedIds = new List<string>();
                foreach (var pair in ScriptManager.Blocks)
                {
                    var block = pair.Value;
                    if (block?.BlockObject == null) continue;
                    var rt = block.BlockObject.GetComponent<RectTransform>();
                    if (!rt) continue;

                    var bCorners = new Vector3[4];
                    rt.GetWorldCorners(bCorners);
                    var bMin = new Vector2(float.MaxValue, float.MaxValue);
                    var bMax = new Vector2(float.MinValue, float.MinValue);
                    for (var i = 0; i < 4; i++)
                    {
                        var bsp = RectTransformUtility.WorldToScreenPoint(null, bCorners[i]);
                        bMin = Vector2.Min(bMin, bsp);
                        bMax = Vector2.Max(bMax, bsp);
                    }

                    var blockRect = new Rect(bMin, bMax - bMin);

                    if (selectionRect.Overlaps(blockRect, true))
                    {
                        selectedIds.Add(block.BlockId);
                    }
                }

                ScriptManager.SetSelection(selectedIds);
            }

            _isSelecting = false;
            _selectionImage.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (Input.mouseScrollDelta.y == 0) return;
            
            Deletable.DeleteButton.SetActive(false);
            var par = ScriptParent.transform.parent;
            _blockTransformSource.transform.position = Input.mousePosition;

            _blockTransformSource.localScale = ScriptParent.transform.localScale;
            
            ScriptParent.transform.SetParent(_blockTransformSource, true);
            _blockTransformSource.localScale = new Vector2(
                Mathf.Clamp(_blockTransformSource.localScale.x + Input.mouseScrollDelta.y / 40, 0.1f, 2), 
                Mathf.Clamp(_blockTransformSource.localScale.y + Input.mouseScrollDelta.y / 40, 0.1f, 2));
            ScriptParent.transform.SetParent(par, true);
        }
    }
}