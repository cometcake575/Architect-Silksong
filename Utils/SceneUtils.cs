using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Architect.Behaviour.Utility;
using Architect.Content.Preloads;
using Architect.Placements;
using Architect.Storage;
using Architect.Workshop.Items;
using GlobalEnums;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using TeamCherry.Localization;
using TeamCherry.SharedUtils;
using tk2dRuntime.TileMap;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Architect.Utils;

public static class SceneUtils
{
    private static GameObject _manager;
    private static GameObject _sceneManager;
    private static GameObject _tilemap;
    private static GameObject _borderPrefab;

    public static tk2dTileMap Tilemap;

    public static readonly Dictionary<string, CustomScene> CustomScenes = [];
    public static readonly Dictionary<string, SceneGroup> SceneGroups = [];

    public static bool QWHookEnabled;

    public static void InitQWHook()
    {
        if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("io.github.hk-speedrunning.quickwarp"))
        {
            ArchitectPlugin.Logger.LogInfo("Enabling QuickWarp Hook");
            QuickWarpHookLoader.Init();
        }
    }
    
    public static void Init()
    {
        _borderPrefab = new GameObject("[Architect] Border Replacement");
        _borderPrefab.SetActive(false);
        Object.DontDestroyOnLoad(_borderPrefab);
        
        PreloadManager.RegisterPreload(new BasicPreload("Arborium_09", "_Managers", o =>
        {
            o = Object.Instantiate(o);
            o.SetActive(false);
            Object.DontDestroyOnLoad(o);
            o.name = "[Architect] Managers Preload";
            _manager = o;
        }));
        PreloadManager.RegisterPreload(new BasicPreload("Arborium_09", "_SceneManager", o =>
        {
            o = Object.Instantiate(o);
            o.SetActive(false);
            Object.DontDestroyOnLoad(o);
            o.name = "[Architect] Scene Manager Preload";
            _sceneManager = o;
        }));
        PreloadManager.RegisterPreload(new BasicPreload("Arborium_09", "TileMap", o =>
        {
            o = Object.Instantiate(o);
            o.SetActive(false);
            var tm = o.GetComponent<tk2dTileMap>();
            tm.enabled = true;
            for (var x = 0; x < tm.width; x++)
            {
                for (var y = 0; y < tm.height; y++)
                {
                    tm.ClearTile(x, y, 0);
                }
            }

            Object.DontDestroyOnLoad(o);
            o.name = "[Architect] Tilemap Preload";
            _tilemap = o;
        }));
        
        typeof(SceneLoad).Hook(nameof(SceneLoad.BeginRoutine), RedirectLoad);
        
        _ = new ILHook(typeof(SceneLoad)
                .GetMethod(nameof(SceneLoad.BeginRoutine), BindingFlags.NonPublic | BindingFlags.Instance)
                .GetStateMachineTarget(),
            il =>
            {
                var cursor = new ILCursor(il);
                
                // Goes to the point after the crash check has occured (about to throw an error)
                cursor.GotoNext(
                    MoveType.After,
                    instr => instr.MatchLdloc(2),
                    instr => instr.OpCode == OpCodes.Ldflda,
                    instr => instr.OpCode == OpCodes.Call,
                    instr => instr.OpCode == OpCodes.Brfalse_S
                );
                // Marks this point
                var source = cursor.MarkLabel();
                
                // Goes to the line after trying to load the scene
                cursor.GotoNext(
                    MoveType.After,
                    instr => instr.MatchLdloc(2),
                    instr => instr.OpCode == OpCodes.Ldc_I4_4,
                    instr => instr.OpCode == OpCodes.Call
                );
                // Marks this position
                var label = cursor.MarkLabel();
                
                // Returns to point of crash check, makes it skip scene load
                cursor.GotoLabel(source);
                cursor.Emit(OpCodes.Br, label);
            });
        
        typeof(GameManager).Hook(nameof(GameManager.LoadScene),
            (Action<GameManager, string> orig, GameManager self, string destScene) =>
            {
                if (CustomScenes.ContainsKey(destScene))
                {
                    StorageManager.SaveScene(self.sceneName, PlacementManager.GetLevelData());
                    StorageManager.SaveScene(StorageManager.GLOBAL, PlacementManager.GetGlobalData());
                    
                    PersistentAudioManager.OnLeaveScene();
                    PersistentAudioManager.QueueSceneEntry();
                    self.startedOnThisScene = false;
                    self.nextSceneName = destScene;
                    self.LastSceneLoad = new SceneLoad(self, new GameManager.SceneLoadInfo
                    {
                        SceneName = destScene
                    });
                    ArchitectPlugin.Instance.StartCoroutine(LoadScene(destScene));
                    return;
                }
                orig(self, destScene);
            });
        
        
        var blankBackground = new SaveSlotBackgrounds.AreaBackground
        {
            BackgroundImage = ArchitectPlugin.BlankSprite,
            NameOverride = new LocalisedString("ArchitectMod", "???")
        };
        typeof(SaveSlotBackgrounds).Hook(nameof(SaveSlotBackgrounds.GetBackground),
            (Func<SaveSlotBackgrounds, SaveStats, SaveSlotBackgrounds.AreaBackground> orig,
                SaveSlotBackgrounds self,
                SaveStats currentSaveStats) =>
            {
                var sceneName = currentSaveStats?.saveGameData?.playerData?.respawnScene ?? "";
                if (CustomScenes.TryGetValue(sceneName, out var scene))
                {
                    return SceneGroups.TryGetValue(scene.Group, out var group) 
                        ? group.Background ?? blankBackground : 
                        blankBackground;
                }
                return orig(self, currentSaveStats);
            }, typeof(SaveStats));
        
        typeof(InventoryMapManager).Hook(nameof(InventoryMapManager.GetStartSelectable),
            (Func<InventoryMapManager, InventoryItemSelectable> orig, InventoryMapManager self) =>
            {
                if (CustomScenes.TryGetValue(GameManager.instance.sceneName, out var scene)
                    && SceneGroups.TryGetValue(scene.Group, out var group)
                    && group.HasMap()) return group.MapZone;
                return orig(self);
            });
        
        typeof(GameMap).Hook(nameof(GameMap.TryOpenQuickMap),
            (TryOpenQuickMap orig, GameMap self, out string displayName) =>
            {
                displayName = string.Empty;
                
                if (CustomScenes.TryGetValue(GameManager.instance.sceneName, out var scene))
                {
                    if (!SceneGroups.TryGetValue(scene.Group, out var group) || !group.HasMap()) return false;
                    
                    self.DisableAllAreas();
                    self.EnableUnlockedAreas(MapZone.NONE);
                    displayName = group.GroupName;
                    self.transform.localScale = new Vector3(1.4725f, 1.4725f, 1f);
                    self.transform.SetPosition2D(group.ZoomPos * -1.4725f);
                    
                    if (scene.Map.activeInHierarchy)
                    {
                        var corpseScene = PlayerData.instance.HeroCorpseScene;
                        if (corpseScene != null && CustomScenes.TryGetValue(corpseScene, out var cs)
                            && SceneGroups.TryGetValue(cs.Group, out var cg) && cg == group)
                            self.shadeMarker.SetActive(true);
                        self.PositionCompassAndCorpse();
                    }

                    self.SetDisplayNextArea(true, MapZone.NONE);
                    self.SetupMapMarkers();
                    return true;
                }

                var o = orig(self, out displayName);
                return o;
            });
        
        typeof(GameMap).Hook(nameof(GameMap.GetSceneInfo),
            (GetSceneInfo orig,
                GameMap self,
                string sceneName,
                MapZone mapZone,
                out GameMapScene foundScene,
                out GameObject foundSceneObj,
                out Vector2 foundScenePos) =>
            {
                if (sceneName != null && CustomScenes.TryGetValue(sceneName, out var scene) && scene.Map)
                {
                    foundScene = scene.Gms;
                    foundSceneObj = scene.Map;
                    
                    var localPosition1 = foundSceneObj.transform.localPosition;
                    var localPosition2 = foundSceneObj.transform.parent.localPosition;
                    foundScenePos = (localPosition1 + localPosition2).Where(z: 0);

                    if (!foundSceneObj.activeInHierarchy) foundSceneObj = null;
                    return;
                }
                orig(self, sceneName, mapZone, out foundScene, out foundSceneObj, out foundScenePos);
            });

        _ = new Hook(typeof(PlayerData).GetProperty(nameof(PlayerData.HasAnyMap))!.GetGetMethod(),
            (Func<PlayerData, bool> orig, PlayerData self) => 
                orig(self) || SceneGroups.Values.Any(sg => sg.HasMap()));
        
        typeof(GameMap).Hook(nameof(GameMap.CalculateMapScrollBounds),
            (Action<GameMap> _, GameMap self) =>
            {
                self.CompleteVisibleLocalBoundsNow();
                if (self.updatePinAreaBounds)
                    self.CalculatePinAreaBounds();
                self.panMinX = float.MaxValue;
                self.panMaxX = float.MinValue;
                self.panMinY = float.MaxValue;
                self.panMaxY = float.MinValue;
                var instance = PlayerData.instance;
                var mapZone1 = self.displayingCompass ? self.GetCurrentMapZone() : MapZone.NONE;
                var flag1 = CollectableItemManager.IsInHiddenMode();
                var flag2 = self.IsLostInAbyssPostMap();
                var flag3 = flag1 | flag2;
                var bounds1 = new Bounds();
                var num = 0;
                for (var index = 0; index < self.mapZoneInfo.Length; ++index)
                {
                    var zoneInfo = self.mapZoneInfo[index];
                    var mapZone2 = (MapZone)index;
                    var flag4 = false;
                    foreach (var parent in zoneInfo.Parents)
                    {
                        if (parent.Parent)
                        {
                            if (mapZone2 != mapZone1 && !parent.Parent.gameObject.activeSelf)
                            {
                                if (!(parent.BoundsAddedByPinGroup == CaravanTroupeHunter.PinGroups.None | flag1 |
                                      flag2))
                                {
                                    var pdBool = CaravanTroupeHunter.PdBools[parent.BoundsAddedByPinGroup];
                                    if (!instance.GetVariable<bool>(pdBool) ||
                                        !self.HasRemainingPinFor(parent.BoundsAddedByPinGroup))
                                        continue;
                                }
                                else
                                    continue;
                            }

                            foreach (Transform transform in parent.Parent.transform)
                            {
                                if (transform.gameObject.activeSelf)
                                {
                                    var component = transform.GetComponent<SpriteRenderer>();
                                    if (component && component.sprite)
                                    {
                                        flag4 = true;
                                        break;
                                    }
                                }
                            }

                            if (flag4)
                                break;
                        }
                    }

                    if (flag4)
                    {
                        var mapZoomPositionNew = zoneInfo.GetWideMapZoomPositionNew();
                        if (mapZoomPositionNew.x < self.panMinX)
                            self.panMinX = mapZoomPositionNew.x;
                        if (mapZoomPositionNew.x > self.panMaxX)
                            self.panMaxX = mapZoomPositionNew.x;
                        if (mapZoomPositionNew.y < self.panMinY)
                            self.panMinY = mapZoomPositionNew.y;
                        if (mapZoomPositionNew.y > self.panMaxY)
                            self.panMaxY = mapZoomPositionNew.y;
                        var size = zoneInfo.VisibleLocalBounds.size;
                        var bounds2 = new Bounds(zoneInfo.VisibleLocalBounds.center, size);
                        if (num == 0)
                            bounds1 = bounds2;
                        else
                            bounds1.Encapsulate(bounds2);
                        ++num;
                    }
                }
                
                foreach (var group in SceneGroups.Values.Where(group => group.HasMap()).Distinct())
                {
                    foreach (var scene in CustomScenes.Values.Where(o => o.Group == group.Id))
                    {
                        var targetPos = group.ZoomPos + scene.MapPos;
                        if (targetPos.x < self.panMinX)
                            self.panMinX = targetPos.x;
                        if (targetPos.x > self.panMaxX)
                            self.panMaxX = targetPos.x;
                        if (targetPos.y < self.panMinY)
                            self.panMinY = targetPos.y;
                        if (targetPos.y > self.panMaxY)
                            self.panMaxY = targetPos.y;
                        
                        if (!scene.Gms || !scene.Gms.fullSprite) continue;
                        var bounds = GameMapScene.GetCroppedBounds(scene.Gms.fullSprite);
                        
                        var bounds2 = new Bounds(targetPos, bounds.extents * 1.15f);
                        if (num == 0) bounds1 = bounds2;
                        else bounds1.Encapsulate(bounds2);
                        ++num;
                    }
                }
                
                self.mapBounds = bounds1;
                var center = self.mapMarkerScrollArea.center;
                ArrayForEnumAttribute.EnsureArraySize(ref instance.placedMarkers,
                    typeof(MapMarkerMenu.MarkerTypes));
                if (!flag3)
                {
                    var bounds3 = new Bounds();
                    var flag5 = false;
                    if (self.markerParent && self.markerParent != self.transform)
                        self.markerToGameMapLocal =
                            self.transform.worldToLocalMatrix * self.markerParent.localToWorldMatrix;
                    for (var index1 = 0; index1 < self.spawnedMapMarkers.GetLength(0); ++index1)
                    {
                        var wrappedVector2List = instance.placedMarkers[index1];
                        if (wrappedVector2List == null)
                            instance.placedMarkers[index1] = wrappedVector2List = new WrappedVector2List();
                        foreach (var vector3 in wrappedVector2List.List
                                     .Select(t => self.markerToGameMapLocal.MultiplyPoint3x4(t)))
                        {
                            if (flag5)
                            {
                                bounds3.Encapsulate(vector3);
                            }
                            else
                            {
                                bounds3 = new Bounds(vector3, Vector3.zero);
                                flag5 = true;
                            }
                        }
                    }

                    if (flag5)
                        bounds1.Encapsulate(bounds3);
                }

                self.MapMarkerBounds = bounds1;
                self.NoPanBounds = bounds1;
                var bounds4 = self.ApplyScrollAreaOffset(bounds1);
                bounds4.center = -bounds4.center + center;
                self.MapMarkerBounds.center = -self.MapMarkerBounds.center + center;
                self.NoPanBounds.center = -self.NoPanBounds.center + center;
                if (num > 1)
                {
                    self.panMinX = bounds4.min.x;
                    self.panMinY = bounds4.min.y;
                    self.panMaxX = bounds4.max.x;
                    self.panMaxY = bounds4.max.y;
                }
                else
                {
                    self.panMinX += center.x;
                    self.panMaxX += center.x;
                    self.panMinY += center.y;
                    self.panMaxY += center.y;
                }

                self.ZoomedBounds = bounds4;
            });
    }

    private static readonly FieldInfo SceneMapStartScale = typeof(InventoryMapManager)
        .GetField(nameof(InventoryMapManager.SceneMapStartScale), BindingFlags.Public | BindingFlags.Static);

    public static void AdjustMapScale(InventoryWideMap wideMap)
    {
        var scale = 1f;
        
        foreach (var group in SceneGroups.Values.Where(g => g.HasMapZone && g.MapSprite))
        {
            var max = group.MapPos * 1.34f + (Vector2)group.MapSprite.bounds.max;
            var min = group.MapPos * 1.34f + (Vector2)group.MapSprite.bounds.min;
            
            scale = Mathf.Min(
                scale,
                6 / Mathf.Abs(max.y), 6 / Mathf.Abs(min.y),
                10 / Mathf.Abs(max.x), 10 / Mathf.Abs(min.x)
            );
        }

        if (wideMap) wideMap.transform.localScale = new Vector3(1.34f * scale, 1.34f * scale, 1);
        if (GameManager.instance.gameMap && GameManager.instance.gameMap.mapManager)
        {
            GameManager.instance.gameMap.mapManager.zoneMapInitialScale = new Vector3(1.34f * scale, 1.34f * scale, 1);
        }
        SceneMapStartScale.SetValue(null, new Vector3(0.39f * scale, 0.39f * scale, 1f));
    }
    
    public delegate bool TryOpenQuickMap(GameMap self, out string displayName);
    
    public delegate void GetSceneInfo(
        GameMap self,
        string sceneName,
        MapZone mapZone,
        out GameMapScene foundScene,
        out GameObject foundSceneObj,
        out Vector2 foundScenePos);

    private static IEnumerator RedirectLoad(
        Func<SceneLoad, IEnumerator> orig,
        SceneLoad self)
    {
        var o = orig(self);
        
        if (CustomScenes.ContainsKey(self.TargetSceneName))
        {
            self.ActivationComplete += Load;
        }
        
        while (o.MoveNext()) yield return o.Current;
        self.ActivationComplete -= Load;
        
        yield break;
        
        void Load()
        {
            ArchitectPlugin.Instance.StartCoroutine(LoadScene(self.TargetSceneName));
        }
    }

    private static bool _doneTilemapYet;

    public static IEnumerator LoadScene(string sceneName)
    {
        var current = GameManager.instance.sceneName;

        if (current == sceneName)
        {
            // Merges existing scene into temp to avoid name clash
            SceneManager.MergeScenes(SceneManager.GetActiveScene(), SceneManager.CreateScene("Temp"));
            current = "Temp";
        }

        var info = CustomScenes[sceneName];
        
        var scene = SceneManager.CreateScene(sceneName);
        var sm = CreateSceneManager();
        SceneManager.MoveGameObjectToScene(sm, scene);
        SceneManager.MoveGameObjectToScene(CreateManager(), scene);
        var (tm, rd) = CreateTileMap(info);
        SceneManager.MoveGameObjectToScene(tm, scene);
        SceneManager.MoveGameObjectToScene(rd, scene);
        SceneManager.SetActiveScene(scene);
        
        sm.AddComponent<CustomTransitionPoint>();
        var point = sm.AddComponent<TransitionPoint>();
        point.nonHazardGate = true;
        point.targetScene = "Belltown";
        point.entryPoint = "door1";

        var col = sm.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0, 0);
        col.offset = new Vector2(-9999, -9999);
        col.isTrigger = true;

        var gs = 0f;
        if (!_doneTilemapYet)
        {
            gs = HeroController.instance.rb2d.gravityScale;
            HeroController.instance.rb2d.gravityScale = 0;
        }

        yield return SceneManager.UnloadSceneAsync(current);

        yield return new WaitForSeconds(0.2f);
        UIManager.instance.AudioGoToGameplay(0);

        if (!_doneTilemapYet)
        {
            Object.Destroy(tm);
            
            var (tm2, _) = CreateTileMap(info);
            var map = tm2.GetComponent<tk2dTileMap>();
            Tilemap = map;
            GameManager.instance.tilemap = map;
            map.ForceBuild();
            
            HeroController.instance.rb2d.gravityScale = gs;

            _doneTilemapYet = true;
        }

        yield return new WaitForSeconds(0.8f);
        UIManager.instance.AudioGoToGameplay(0);
    }

    public static GameObject CreateSceneManager()
    {
        var sm = Object.Instantiate(_sceneManager);
        sm.name = "_SceneManager";
        var csm = sm.GetComponent<CustomSceneManager>();
        csm.borderPrefab = _borderPrefab;
        csm.overrideParticlesWith = MapZone.NONE;
        csm.noParticles = true;
        csm.mapZone = MapZone.NONE;
        csm.environmentType = EnvironmentTypes.NoEffect;
        csm.actorSnapshot = GameManager.instance.actorSnapshotUnpaused;
        csm.atmosSnapshot = GameManager.instance.noAtmosSnapshot;
        csm.enviroSnapshot = GameManager.instance.silentSnapshot;
        csm.musicSnapshot = GameManager.instance.noMusicSnapshot;
        csm.darknessLevel = 0;
        
        sm.AddComponent<HazardRespawnMarker>();
        sm.SetActive(true);
        return sm;
    }

    public static GameObject CreateManager()
    {
        var m = Object.Instantiate(_manager);
        m.name = "_Managers";
        m.SetActive(true);
        return m;
    }
    
    public static (GameObject, GameObject) CreateTileMap(CustomScene scene)
    {
        var tm = Object.Instantiate(_tilemap);
        tm.name = "TileMap";
        tm.SetActive(true);

        Tilemap = tm.GetComponent<tk2dTileMap>();
        Tilemap.width = scene.TilemapWidth;
        Tilemap.height = scene.TilemapHeight;

        Tilemap.layers[0].width = scene.TilemapWidth;
        Tilemap.layers[0].height = scene.TilemapHeight;

        GameManager.instance.tilemap = Tilemap;
        GameManager.instance.sceneWidth = scene.TilemapWidth;
        GameManager.instance.sceneHeight = scene.TilemapHeight;

        Tilemap.layers[0].numColumns = Mathf.CeilToInt(scene.TilemapWidth / 32f);
        Tilemap.layers[0].numRows = Mathf.CeilToInt(scene.TilemapHeight / 32f);

        var nsc = new List<SpriteChunk>();
        for (var row = 0; row < Tilemap.layers[0].numRows; row++)
        {
            for (var col = 0; col < Tilemap.layers[0].numColumns; col++)
            {
                nsc.Add(new SpriteChunk());
            }
        }
        Tilemap.layers[0].spriteChannel.chunks = nsc.ToArray();
        
        Tilemap.Build();

        tm.AddComponent<TilemapLateLoader>().scene = scene.Id;
        
        return (tm, Tilemap.renderData);
    }
    
    public static List<(int, int)> TilemapChanges;
    public static List<(int, int)> ExtTilemapChanges;
    public static string TilemapScene;

    public class TilemapLateLoader : MonoBehaviour
    {
        public string scene;
        
        public void Load()
        {
            if (!ExtTilemapChanges.IsNullOrEmpty())
            {
                foreach (var (x, y) in ExtTilemapChanges)
                {
                    try
                    {
                        if (Tilemap.GetTile(x, y, 0) == -1) Tilemap.SetTile(x, y, 0, 0);
                        else Tilemap.ClearTile(x, y, 0);
                    }
                    catch (Exception)
                    {
                        // Out of bounds
                    }
                }
            }

            if (!TilemapChanges.IsNullOrEmpty())
            {
                foreach (var (x, y) in TilemapChanges)
                {
                    try
                    {
                        if (Tilemap.GetTile(x, y, 0) == -1) Tilemap.SetTile(x, y, 0, 0);
                        else Tilemap.ClearTile(x, y, 0);
                    }
                    catch (Exception)
                    {
                        // Out of bounds
                    }
                }
            }
            
            Tilemap.Build();

            enabled = false;
        }

        private void Update()
        {
            if (scene == TilemapScene) Load();
        }
    }
}