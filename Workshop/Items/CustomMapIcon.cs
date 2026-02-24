using System.Collections.Generic;
using Architect.Utils;
using BepInEx;
using GlobalEnums;
using TMProOld;
using UnityEngine;

namespace Architect.Workshop.Items;

public class CustomMapIcon : SpriteItem
{
    public static readonly List<CustomMapIcon> Icons = [];
    
    public string Scene = string.Empty;
    public Vector2 Pos = Vector2.zero;
    public int Mode;
    public string Text = string.Empty;
    public float FontSize = 6.2f;
    public Vector2 Offset = Vector2.zero;
    public Color Colour = Color.white;
    public string ReqVar;

    private GameObject _iconObj;
    private SpriteRenderer _renderer;
    
    public override void Register()
    {
        Icons.Add(this);
        Setup();
        base.Register();
    }

    public override void Unregister()
    {
        Icons.Remove(this);
        if (_iconObj) Object.Destroy(_iconObj);
    }

    public void Setup()
    {
        if (!SceneUtils.CustomScenes.TryGetValue(Scene, out var scene) || !scene.Map) return;
        _iconObj = Object.Instantiate(SceneGroup.MapIconPrefab, scene.Map.transform);
        _iconObj.transform.localPosition = Pos;
        _iconObj.transform.GetChild(0).localPosition = Offset;
        
        _iconObj.GetComponentInChildren<MeshRenderer>().sortingOrder = 100;
        
        _renderer = _iconObj.GetComponentInChildren<SpriteRenderer>();
        _renderer.sortingOrder = 100;
        
        _iconObj.GetComponentInChildren<SetTextMeshProGameText>().text = (LocalStr)Text;
        var tmp = _iconObj.GetComponentInChildren<TextMeshPro>();
        tmp.fontSize = FontSize;
        tmp.color = Colour;

        if (Mode != 0)
        {
            var mdh1 = _iconObj.transform.GetChild(0).gameObject.AddComponent<MapDisplayHandler>();
            var mdh2 = _iconObj.transform.GetChild(1).gameObject.AddComponent<MapDisplayHandler>();
            mdh1.isQm = mdh2.isQm = Mode == 1;
            mdh1.reqVar = mdh2.reqVar = ReqVar;
        }

        _renderer.sprite = Sprite;
        
        _iconObj.SetActive(true);
    }

    public class MapDisplayHandler : MonoBehaviour
    {
        public bool isQm;
        public string reqVar;
        
        private GameMap _gameMap;
        private Renderer _renderer;

        private void Reset() => _gameMap = GetComponentInParent<GameMap>(true);

        private void Awake()
        {
            _gameMap = GetComponentInParent<GameMap>();
            _gameMap.UpdateQuickMapDisplay += Refresh;
            _renderer = GetComponent<Renderer>();
        }

        private void Start()
        {
            if (DisplayOnWorldMapOnly.updateState == DisplayOnWorldMapOnly.UpdateState.Never) return;
            Refresh(DisplayOnWorldMapOnly.updateState == DisplayOnWorldMapOnly.UpdateState.QuickMap, MapZone.NONE);
        }

        private void OnDestroy()
        {
            _gameMap.UpdateQuickMapDisplay -= Refresh;
        }

        private void Refresh(bool isQuickMap, MapZone _)
        {
            DisplayOnWorldMapOnly.updateState = isQuickMap ? DisplayOnWorldMapOnly.UpdateState.QuickMap : DisplayOnWorldMapOnly.UpdateState.Normal;
            if (!_renderer) return;
            _renderer.enabled = isQuickMap == isQm && ShouldBeVisible();
        }

        private bool ShouldBeVisible()
        {
            if (reqVar.IsNullOrWhiteSpace()) return true;
            return ArchitectData.Instance.BoolVariables.TryGetValue(reqVar, out var val) && val;
        }
    }

    protected override void OnReadySprite()
    {
        if (_renderer) _renderer.sprite = Sprite;
    }
}