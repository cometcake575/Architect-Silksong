using System.Linq;
using Architect.Storage;
using Architect.Utils;
using BepInEx;
using UnityEngine;

namespace Architect.Workshop.Items;

public class CustomScene : SpriteItem
{
    public string Group = "None";
    
    public int TilemapWidth = 500;
    public int TilemapHeight = 500;

    public Vector2 MapPos;
    public string EIconUrl = string.Empty;
    public bool EPoint;
    public float EPpu = 100;
    
    public override (string, string)[] FilesToDownload => [
        (IconUrl, "png"),
        (EIconUrl, "png")
    ];

    public GameObject Map;
    public GameMapScene Gms;
    
    public override void Register()
    {
        SceneUtils.CustomScenes.Add(Id, this);
        
        if (Map) Object.Destroy(Map);
        TrySetupMap();
        if (SceneUtils.QWHookEnabled) QuickWarpHookLoader.RegisterScene(Group, Id);
    }

    public void TrySetupMap()
    {
        if (SceneUtils.SceneGroups.TryGetValue(Group, out var group) && group.FocusMapObject)
        {
            Map = Object.Instantiate(SceneGroup.MapSegmentPrefab, group.FocusMapObject.transform);
            Map.name = Id;
            Map.SetActive(true);
            Map.transform.localPosition = MapPos;

            Gms = Map.GetComponent<GameMapScene>();
            Gms.initialColor = group.MapColour;

            Gms.fullSprite = null;
            Gms.initialSprite = null;

            Gms.spriteRenderer = Gms.GetComponent<SpriteRenderer>();
            Gms.spriteRenderer.color = group.MapColour;

            RefreshMap();
        
            RefreshSprite();
            RefreshESprite();

            foreach (var icon in CustomMapIcon.Icons.Where(i => i.Scene == Id))
            {
                icon.Setup();
            }
        }
    }

    private void RefreshMap()
    {
        var pd = PlayerData.instance;
        Gms.hasBeenSet = false;
        if ((Gms.isMapped || pd.scenesVisited.Contains(Id)) && 
            SceneUtils.SceneGroups.TryGetValue(Group, out var group) && group.HasMapZone && 
            !CollectableItemManager.IsInHiddenMode())
        {
            if (pd.hasQuill) Gms.SetMapped();
        } else Gms.SetNotMapped();
    }

    public void RefreshESprite()
    {
        if (!Gms) return;
        if (EIconUrl.IsNullOrWhiteSpace())
        {
            Gms.initialState = GameMapScene.States.Hidden;
            RefreshMap();
            return;
        }
        Gms.initialState = GameMapScene.States.Rough;
        CustomAssetManager.DoLoadSprite(EIconUrl, EPoint, EPpu, 1, 1, sprites =>
        {
            if (sprites.IsNullOrEmpty()) return;
            if (Gms)
            {
                Gms.initialSprite = sprites[0];
                RefreshMap();
            }
        });
    }

    protected override void OnReadySprite()
    {
        if (Gms)
        {
            Gms.fullSprite = Sprite;
            RefreshMap();
        }
    }

    public override void Unregister()
    {
        SceneUtils.CustomScenes.Remove(Id);
        if (Map) Object.Destroy(Map);
        
        if (SceneUtils.QWHookEnabled) QuickWarpHookLoader.UnregisterScene(Group, Id);
    }
}