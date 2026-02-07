using System;
using System.Collections;
using System.Collections.Generic;
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
            QWHookEnabled = QuickWarpHook.Init();
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
    }

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
        if (GameManager.instance.IsFirstLevelForPlayer)
        {
            gs = HeroController.instance.rb2d.gravityScale;
            HeroController.instance.rb2d.gravityScale = 0;
        }

        yield return SceneManager.UnloadSceneAsync(current);

        yield return new WaitForSeconds(0.2f);
        GameManager.instance.SetPausedState(false);

        if (GameManager.instance.IsFirstLevelForPlayer)
        {
            Object.Destroy(tm);
            
            var (tm2, _) = CreateTileMap(info);
            var map = tm2.GetComponent<tk2dTileMap>();
            Tilemap = map;
            GameManager.instance.tilemap = map;
            map.ForceBuild();
            
            HeroController.instance.rb2d.gravityScale = gs;
        }

        yield return new WaitForSeconds(0.8f);
        GameManager.instance.SetPausedState(false);
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

        Tilemap.Build();

        tm.AddComponent<TilemapLateLoader>();
        
        return (tm, Tilemap.renderData);
    }
    
    public static List<(int, int)> TilemapChanges;
    public static List<(int, int)> ExtTilemapChanges;

    public class TilemapLateLoader : MonoBehaviour
    {
        public void Load()
        {
            if (!ExtTilemapChanges.IsNullOrEmpty())
            {
                foreach (var (x, y) in ExtTilemapChanges)
                {
                    if (Tilemap.GetTile(x, y, 0) == -1) Tilemap.SetTile(x, y, 0, 0);
                    else Tilemap.ClearTile(x, y, 0);
                }
            }

            if (!TilemapChanges.IsNullOrEmpty())
            {
                foreach (var (x, y) in TilemapChanges)
                {
                    if (Tilemap.GetTile(x, y, 0) == -1) Tilemap.SetTile(x, y, 0, 0);
                    else Tilemap.ClearTile(x, y, 0);
                }
            }
            
            Tilemap.Build();

            enabled = false;
        }

        private void Update()
        {
            Load();
        }
    }
}