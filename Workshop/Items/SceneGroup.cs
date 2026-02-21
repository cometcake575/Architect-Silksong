using System;
using System.Collections.Generic;
using System.Linq;
using Architect.Storage;
using Architect.Utils;
using BepInEx;
using GlobalEnums;
using MonoMod.RuntimeDetour;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Architect.Workshop.Items;

public class SceneGroup : SpriteItem
{
    public string GroupName = string.Empty;
    public bool DisableAct3Bg;

    public Vector2 MapPos;
    public Vector2 LabelPos;
    public Vector2 ZoomPos;

    public string Variable = string.Empty;

    public float Radius = 0.2f;

    public bool HasMapZone;
    public Color MapColour = Color.white;
    
    public string MapUrl = string.Empty;
    public bool MPoint;
    public float MPpu = 100;
    
    public readonly string[] OverwriteEnter = ["", "", "", ""];
    public readonly string[] OverwriteExit = ["", "", "", ""];

    public Sprite MapSprite;
    
    public override (string, string)[] FilesToDownload => [
        (IconUrl, "png"),
        (MapUrl, "png")
    ];

    public SaveSlotBackgrounds.AreaBackground Background;

    private static InventoryWideMap _wideMap;

    public static void Init()
    {
        typeof(InventoryItemWideMapZone)
            .Hook(nameof(InventoryItemWideMapZone.GetNextSelectable),
                (Func<InventoryItemWideMapZone, InventoryItemManager.SelectionDirection, InventoryItemSelectable> orig,
                    InventoryItemWideMapZone self, InventoryItemManager.SelectionDirection dir) =>
                {
                    var splitName = self.name.Split("_");
                    switch (splitName.Length)
                    {
                        case > 4:
                        {
                            var match = SceneUtils.SceneGroups.Values
                                .FirstOrDefault(cm => cm.OverwriteEnter[(int)dir].Contains(splitName[4]));
                            if (match != null) return match.MapZone;
                            break;
                        }
                        case > 2:
                        {
                            var match = SceneUtils.SceneGroups.Values
                                .FirstOrDefault(cm => cm.OverwriteEnter[(int)dir].Contains(splitName[2]));
                            if (match != null) return match.MapZone;
                            break;
                        }
                    }
                    
                    var cmz = self.GetComponent<CustomMapZone>();
                    if (cmz)
                    {
                        foreach (Transform child in _wideMap.transform)
                        {
                            var splitChild = child.name.Split("_");
                            switch (splitChild.Length)
                            {
                                case > 4:
                                    if (cmz.SceneGroup.OverwriteExit[(int)dir].Contains(splitChild[4])) 
                                        return child.GetComponent<InventoryItemWideMapZone>();
                                    break;
                                case > 2:
                                    if (cmz.SceneGroup.OverwriteExit[(int)dir].Contains(splitChild[2])) 
                                        return child.GetComponent<InventoryItemWideMapZone>();
                                    break;
                            }
                        }

                        return self.GetSelectableFromAutoNavGroup<InventoryItemSelectable>(dir)
                            as InventoryItemWideMapZone;
                    }

                    return orig(self, dir);
                }, typeof(InventoryItemManager.SelectionDirection));

        typeof(InventoryWideMap).Hook(nameof(InventoryWideMap.UpdatePositions),
            (Action<InventoryWideMap> orig, InventoryWideMap self) =>
            {
                if (self != _wideMap)
                {
                    ArchitectPlugin.Logger.LogInfo("Setting up WideMap");
                    _wideMap = self;
                    foreach (var group in SceneUtils.SceneGroups.Values)
                    {
                        group.RegisterMap();
                    }
                }

                orig(self);
            });

        Vector2 zoomPos = default;
        typeof(InventoryItemWideMapZone).Hook(nameof(InventoryItemWideMapZone.Submit),
            (Func<InventoryItemWideMapZone, bool> orig, InventoryItemWideMapZone self) =>
            {
                var cmz = self.GetComponent<CustomMapZone>();
                zoomPos = cmz ? cmz.SceneGroup.ZoomPos : default;

                return orig(self);
            });
        
        typeof(InventoryMapManager).Hook(nameof(InventoryMapManager.ZoomIn),
            (Action<InventoryMapManager, MapZone, bool> orig, InventoryMapManager self, MapZone zone, bool animate) =>
            {
                if (zone == GlobalEnums.MapZone.NONE
                    && zoomPos == default
                    && SceneUtils.CustomScenes.TryGetValue(GameManager.instance.sceneName, out var scene)
                    && SceneUtils.SceneGroups.TryGetValue(scene.Group, out var group)) zoomPos = group.ZoomPos;
                orig(self, zone, animate);
            });

        typeof(GameMap).Hook(nameof(GameMap.GetZoomPosition),
            (Func<GameMap, MapZone, Vector2> orig, GameMap self, MapZone zone) =>
                zoomPos != default ? -zoomPos : orig(self, zone));

        typeof(GameMap).Hook(nameof(GameMap.GetClosestWideMapZone),
            (Func<GameMap, IEnumerable<InventoryItemWideMapZone>, InventoryItemWideMapZone> orig,
                GameMap self,
                IEnumerable<InventoryItemWideMapZone> wideMapPieces) =>
            {
                var result = orig(self, wideMapPieces);
                var lowest = float.MaxValue;
                var zoneInfo = self.mapZoneInfo[(int)result.zoomToZone];
                
                var a1 = -self.transform.localPosition;
                
                foreach (var parent in zoneInfo.Parents)
                {
                    if (parent.Parent)
                    {
                        for (var index2 = 0; index2 < parent.Parent.transform.childCount; ++index2)
                        {
                            var child = parent.Parent.transform.GetChild(index2);
                            var localScenePos = GameMap.GetLocalScenePos(child);
                            var num2 = Vector2.Distance(a1, localScenePos);
                            if (num2 <= lowest) lowest = num2;
                        }
                    }
                }

                foreach (var custom in SceneUtils.SceneGroups.Values)
                {
                    if (!custom.HasMap()) continue;
                    var dist = Vector2.Distance(a1, custom.ZoomPos) - custom.Radius;
                    if (dist < lowest)
                    {
                        lowest = dist;
                        result = custom.MapZone;
                    }
                }

                return result;
            });
        
        typeof(GameMap).Hook(nameof(GameMap.EnableUnlockedAreas),
            (Action<GameMap, MapZone?> orig, GameMap self, MapZone? setCurrent) =>
            {
                if (setCurrent.HasValue)
                {
                    if (setCurrent.Value == GlobalEnums.MapZone.NONE)
                    {
                        if (SceneUtils.CustomScenes.TryGetValue(GameManager.instance.sceneName, out var scene)
                            && SceneUtils.SceneGroups.TryGetValue(scene.Group, out var group)
                            && group.HasMap()) group.FocusMapObject.SetActive(true);
                    }
                }
                else
                {
                    foreach (var group in SceneUtils.SceneGroups.Values.Where(group => group.HasMap()))
                    {
                        group.FocusMapObject.SetActive(true);
                    }
                }
                orig(self, setCurrent);
            });

        _ = new Hook(typeof(InventoryItemWideMapZone).GetProperty("IsUnlocked")!.GetGetMethod(),
            (Func<InventoryItemWideMapZone, bool> orig, InventoryItemWideMapZone self) =>
            {
                var cmz = self.GetComponent<CustomMapZone>();
                return cmz
                    ? cmz.SceneGroup.HasMap()
                    : orig(self);
            });
    }

    public bool HasMap()
    {
        if (!HasMapZone) return false;
        return Variable.IsNullOrWhiteSpace() || ArchitectData.Instance.BoolVariables.GetValueOrDefault(Variable, false);
    }

    public InventoryItemWideMapZone MapZone;
    private GameObject _mapObject;

    public GameObject FocusMapObject;

    private static readonly int UILayer = LayerMask.NameToLayer("UI");
    
    public void RegisterMap()
    {
        if (!HasMapZone) return;
        
        _mapObject = Object.Instantiate(
            _wideMap.transform.Find("Wide_map__0007_Bonetown").gameObject,
            _wideMap.transform,
            true);
        _mapObject.name = $"Wide_map_{Id}";
        
        MapZone = _mapObject.GetComponent<InventoryItemWideMapZone>();
        MapZone.Selectables = [null, null, null, null];
        MapZone.zoomToZone = GlobalEnums.MapZone.NONE;

        var cmz = _mapObject.AddComponent<CustomMapZone>();
        cmz.SceneGroup = this;
        cmz.OnEnable();
        
        var an = _mapObject.transform.Find("Area Name");
        an.GetComponent<SetTextMeshProGameText>().text = (LocalStr)GroupName;
        an.SetPositionX(an.GetPositionX() + LabelPos.x);
        an.SetPositionY(an.GetPositionY() + LabelPos.y);
        
        _mapObject.transform.localPosition = MapPos;
        
        _mapObject.GetComponent<SpriteRenderer>().sprite = MapSprite;

        var gm = GameManager.instance.gameMap;
        FocusMapObject = new GameObject(Id)
        {
            layer = UILayer,
            transform =
            {
                parent = gm.transform,
                localPosition = ZoomPos
            }
        };
    }
    
    public override void Register()
    {
        Background = new SaveSlotBackgrounds.AreaBackground
        {
            BackgroundImage = ArchitectPlugin.BlankSprite,
            NameOverride = (LocalStr)GroupName,
            Act3OverlayOptOut = DisableAct3Bg
        };
        
        SceneUtils.SceneGroups.Add(Id, this);
        
        base.Register();
        RefreshMapSprite();
        
        if (_wideMap) RegisterMap();
    }

    public void RefreshMapSprite()
    {
        if (MapUrl.IsNullOrWhiteSpace()) return;
        CustomAssetManager.DoLoadSprite(MapUrl, MPoint, MPpu, 1, 1, sprites =>
        {
            if (sprites.IsNullOrEmpty()) return;
            MapSprite = sprites[0];
            if (_mapObject) _mapObject.GetComponent<SpriteRenderer>().sprite = MapSprite;
        });
    }
    
    protected override void OnReadySprite()
    {
        Background.BackgroundImage = Sprite;
    }

    public override void Unregister()
    {
        SceneUtils.SceneGroups.Remove(Id);
        if (_mapObject)
        {
            if (MapZone.didAwake)
            {
                MapZone.pane.OnPaneStart -= MapZone.EvaluateUnlocked;
            }

            _wideMap.selectables = null;

            Object.Destroy(_mapObject);
        }
        if (FocusMapObject) Object.Destroy(FocusMapObject);
    }

    public class CustomMapZone : MonoBehaviour
    {
        public SceneGroup SceneGroup;

        public void OnEnable()
        {
            if (SceneGroup == null) return;
            SceneGroup.MapZone.initialColor = SceneGroup.MapColour;
            SceneGroup.MapZone.initialLabelColor = Color.white;
        }
    }
}