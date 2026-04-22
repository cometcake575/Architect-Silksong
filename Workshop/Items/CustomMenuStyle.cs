using System;
using System.Collections;
using System.Collections.Generic;
using Architect.Placements;
using Architect.Storage;
using Architect.Utils;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Architect.Workshop.Items;

public class CustomMenuStyle : WorkshopItem
{
    private static readonly List<CustomMenuStyle> Styles = [];
    
    private static GameObject _ms;
    private GameObject _parent;
    
    public static void Init()
    {
        typeof(MenuStyles).Hook(nameof(MenuStyles.Start),
            (Action<MenuStyles> orig, MenuStyles self) =>
            {
                orig(self);

                _ms = self.gameObject;
        
                ArchitectPlugin.Instance.StartCoroutine(LoadHero(self.gameObject));
                
                foreach (var style in Styles.ToArray())
                {
                    style.Unregister();
                    style.Register();
                }
            });
        
        typeof(GameManager).Hook(nameof(GameManager.LoadHeroPrefab),
            (Func<GameManager, AsyncOperationHandle<GameObject>> orig, GameManager self) =>
            {
                if (HeroController._instance)
                {
                    HeroController._instance.gameObject.RemoveComponent<FakePlayer>();
                }
                return orig(self);
            });
    }

    private static IEnumerator LoadHero(GameObject obj)
    {
        var hero = GameManager.instance.LoadHeroPrefab();
        yield return hero;
        
        var player = Object.Instantiate(hero.Result, obj.transform);
        player.AddComponent<FakePlayer>();
    }
    
    public class FakePlayer : MonoBehaviour
    {
        private HeroController _hc;
        
        private void Start()
        {
            _hc = GetComponent<HeroController>();
        }

        private void Update()
        {
            _hc.ResetHardLandingTimer();
            _hc.RelinquishControl();
            transform.SetPosition2D(9999999, 9999999);
        }
    }
    
    public override void Register()
    {
        Styles.Add(this);

        if (!_ms) return;

        _parent = new GameObject(Id)
        {
            transform =
            {
                parent = _ms.transform,
                localPosition = new Vector3(-5f, -7.8454f, 3.5469f)
            }
        };
        _parent.SetActive(false);
        
        var lsc = _parent.AddComponent<LoadSceneContents>();
        lsc.id = Id;
    }

    public class LoadSceneContents : MonoBehaviour
    {
        public string id;
        private Scene _scene;
        
        public void OnEnable()
        {
            _scene = SceneManager.CreateScene($"{id}_Title");

            var ld = StorageManager.LoadScene($"{id}_Title");
            foreach (var placement in ld.Placements)
            {
                var obj = placement.SpawnObject();
                
                if (obj)
                {
                    SceneManager.MoveGameObjectToScene(obj, _scene);
                    PlacementManager.Objects[placement.GetId()] = obj;
                    PlacementManager.OnPlace?.Invoke(placement.GetPlacementType().GetId(), placement.GetId(), obj);
                }
            }
            
            foreach (var block in ld.ScriptBlocks) block.Setup(false);
            foreach (var block in ld.ScriptBlocks) block.LateSetup();
        }

        private void OnDisable()
        {
            SceneManager.UnloadSceneAsync(_scene);
        }
    }

    public override void Unregister()
    {
        Styles.Remove(this);
        
        if (_parent) Object.Destroy(_parent);
    }

    public override Sprite GetIcon()
    {
        return ArchitectPlugin.BlankSprite;
    }
}