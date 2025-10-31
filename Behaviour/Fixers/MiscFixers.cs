using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Architect.Behaviour.Custom;
using Architect.Objects.Placeable;
using Architect.Utils;
using GlobalSettings;
using HutongGames.PlayMaker.Actions;
using MonoMod.RuntimeDetour;
using TeamCherry.Localization;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Architect.Behaviour.Fixers;

public static class MiscFixers
{
    public static void Init()
    {
        // Custom bench fix - if the bench was determined to be invalid, override it and use the saved data anyway
        // Fallback to first hazard respawn point

        #region Bench fixes

        typeof(GameManager).Hook("GetRespawnInfo",
            (GetRespawnInfo orig, GameManager self, out string scene, out string marker) =>
            {
                var savedRespawnScene = self.playerData.respawnScene;
                var savedRespawnMarker = self.playerData.respawnMarkerName;

                if (!string.IsNullOrEmpty(self.playerData.tempRespawnMarker))
                {
                    self.playerData.tempRespawnMarker = "Dummy";
                }

                orig(self, out scene, out marker);

                if (string.IsNullOrEmpty(self.playerData.tempRespawnMarker))
                {
                    scene = savedRespawnScene;
                    marker = savedRespawnMarker;
                }
            });

        typeof(GameManager).Hook("FindEntryPoint",
            (Func<GameManager, string, Scene, Vector2?> orig, GameManager self, string name, Scene scene) =>
            {
                var point = orig(self, name, scene);
                if (!point.HasValue)
                {
                    var hrm = SceneManager.GetActiveScene().GetRootGameObjects()
                        .SelectMany(obj => obj.GetComponentsInChildren<HazardRespawnMarker>(true))
                        .First();
                    return hrm.transform.position;
                }

                return point;
            });

        typeof(HeroController).Hook(nameof(HeroController.LocateSpawnPoint),
            (Func<HeroController, Transform> orig, HeroController self) =>
            {
                var point = orig(self);
                if (!point)
                {
                    var hrm = SceneManager.GetActiveScene().GetRootGameObjects()
                        .SelectMany(obj => obj.GetComponentsInChildren<HazardRespawnMarker>(true))
                        .First();
                    return hrm.transform;
                }

                return point;
            });

        #endregion

        typeof(LocalisedString).Hook(nameof(LocalisedString.ToString),
            (ToStringOrig orig, ref LocalisedString self, bool allowBlankText) =>
                self.Sheet == "ArchitectMod" ? self.Key : orig(ref self, allowBlankText), typeof(bool));

        typeof(DialogueBox).Hook(nameof(DialogueBox.ParseTextForDialogueLines),
            (Func<string, List<DialogueBox.DialogueLine>> orig, string text) =>
            {
                var lines = orig(Regex.Replace(text.Replace("<br>", "\n"),
                    @"<(?!hpage|page\b)(.*?)>", match =>
                    {
                        var originalTag = match.Groups[1].Value;
                        return $"%1open{originalTag}%2close";
                    }));

                return lines.Select(line => new DialogueBox.DialogueLine
                {
                    IsPlayer = line.IsPlayer,
                    Text = line.Text.Replace("%1open", "<").Replace("%2close", ">"),
                    Event = line.Event
                }).ToList();
            });

        typeof(CustomSceneManager).Hook(nameof(CustomSceneManager.UpdateAppearanceRegion),
            (Action<CustomSceneManager, bool> orig, CustomSceneManager self, bool forceImmediate) =>
            {
                try
                {
                    orig(self, forceImmediate);
                } catch (ArgumentNullException) { } catch (NullReferenceException) { }
            });

        typeof(SimpleCounter).Hook(nameof(SimpleCounter.Start),
            (Action<SimpleCounter> orig, SimpleCounter self) =>
            {
                if (self.GetComponent<CustomFleaCounter>()) return;
                orig(self);
            });

        HookUtils.OnFsmAwake += fsm =>
        {
            if (fsm.FsmName != "Area Title Control") return;

            var header = fsm.FsmVariables.FindFsmString("Title Sup");
            var footer = fsm.FsmVariables.FindFsmString("Title Sub");
            var body = fsm.FsmVariables.FindFsmString("Title Main");
            
            fsm.GetState("Init all").AddAction(() =>
            {
                if (!_overrideAreaText) return;

                header.value = _areaHeader;
                footer.value = _areaFooter;
                body.value = _areaBody;

                _overrideAreaText = false;
            });
        };

        _ = new Hook(typeof(tk2dSprite).GetProperty("color")!.GetSetMethod(),
            (Action<tk2dSprite, Color> orig, tk2dSprite self, Color color) =>
            {
                var cl = self.GetComponentInParent<ColorLock>();
                if (cl && cl.enabled) return;
                orig(self, color);
            });

        _ = new Hook(typeof(SpriteRenderer).GetProperty("color")!.GetSetMethod(),
            (Action<SpriteRenderer, Color> orig, SpriteRenderer self, Color color) =>
            {
                var cl = self.GetComponentInParent<ColorLock>();
                if (cl && cl.enabled) return;
                orig(self, color);
            });
    }
    
    public class ColorLock : MonoBehaviour;

    private delegate string ToStringOrig(ref LocalisedString self, bool allowBlankText);
    
    public delegate void GetRespawnInfo(GameManager self, out string a, out string b);
    
    public static void FixBench(GameObject bench)
    {
        Object.DestroyImmediate(bench.transform.GetChild(2).gameObject);
        bench.AddComponent<CustomBench>();
    }

    public class CustomBench : MonoBehaviour
    {
        private void Start()
        {
            var angle = transform.GetRotation2D();
            if (angle == 0) return;
            gameObject.AddComponent<RestBenchTilt>().tilt = angle;
        }
    }

    public static void MarkRing(GameObject obj)
    {
        obj.AddComponent<MapperRing>();
        obj.RemoveComponent<DeactivateIfPlayerdataTrue>();
    }

    public class MapperRing : MonoBehaviour
    {
        private Collider2D _col2d;
        private bool _ground;

        private void Start()
        {
            _col2d = GetComponent<Collider2D>();
        }

        private void OnCollisionEnter2D(Collision2D _)
        {
            if (CheckTouchingGround())
            {
                _ground = true;
                gameObject.BroadcastEvent("OnLand");
            }
        }

        private void OnCollisionExit2D(Collision2D _)
        {
            if (_ground && !CheckTouchingGround())
            {
                _ground = false;
                gameObject.BroadcastEvent("InAir");
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.GetComponentInChildren<NailSlash>()) gameObject.BroadcastEvent("OnHit");
        }

        public bool CheckTouchingGround()
        {
            var bounds1 = _col2d.bounds;
            double x1 = bounds1.min.x;
            bounds1 = _col2d.bounds;
            double y1 = bounds1.center.y;
            var vector21 = new Vector2((float)x1, (float)y1);
            Vector2 center = _col2d.bounds.center;
            var bounds2 = _col2d.bounds;
            double x2 = bounds2.max.x;
            bounds2 = _col2d.bounds;
            double y2 = bounds2.center.y;
            var vector22 = new Vector2((float)x2, (float)y2);
            bounds2 = _col2d.bounds;
            var distance = bounds2.extents.y + 0.16f;
            Debug.DrawRay(vector21, Vector2.down, Color.yellow);
            Debug.DrawRay(center, Vector2.down, Color.yellow);
            Debug.DrawRay(vector22, Vector2.down, Color.yellow);
            var raycastHit2D1 = Physics2D.Raycast(vector21, Vector2.down, distance, 256);
            var raycastHit2D2 = Physics2D.Raycast(center, Vector2.down, distance, 256);
            var raycastHit2D3 = Physics2D.Raycast(vector22, Vector2.down, distance, 256);
            return raycastHit2D1.collider || raycastHit2D2.collider || raycastHit2D3.collider;
        }
    }

    public static void FixKratt(GameObject obj)
    {
        var body = obj.GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        
        var fsm = obj.LocateMyFSM("Wounded Behaviour");
        fsm.GetState("Init").DisableAction(2);
        fsm.GetState("Land").DisableAction(2);
        fsm.GetState("Reactivate").DisableAction(1);
        fsm.GetState("Spoken?").AddAction(() => fsm.SendEvent("YES"), 0);
        
        fsm.GetState("Idle").AddAction(() =>
        {
            body.bodyType = RigidbodyType2D.Dynamic;
        });
        
        obj.GetComponent<EnemyHitEffectsRegular>().ReceivedHitEffect += (_, _) =>
        {
            obj.BroadcastEvent("OnHit");
        };
        
        obj.AddComponent<Kratt>();
    }

    public class TriggerActivator : MonoBehaviour
    {
        public int layer;
    }

    public class Kratt : TriggerActivator;
    
    public static void FixLamp(GameObject obj)
    {
        obj.transform.GetChild(0).GetChild(2).SetAsFirstSibling();
    }

    public static void FixRing(GameObject obj)
    {
        var fsm = obj.GetComponentInChildren<PlayMakerFSM>();

        fsm.fsmTemplate = null;
        fsm.GetState("Grab Start").AddAction(() => obj.BroadcastEvent("OnGrab"), 0);
        fsm.GetState("Cooldown").AddAction(() => obj.BroadcastEvent("OnRelease"), 0);
    }

    public static void FixPoleRing(GameObject obj)
    {
        obj.transform.GetChild(0).GetChild(0).Translate(0, -10.13f, 0);
        obj.transform.GetChild(1).GetChild(1).localPosition = Vector3.zero;
    }

    public static void FixBellSprite(GameObject obj)
    {
        obj.transform.GetChild(1).GetChild(0).GetChild(0).gameObject.AddComponent<PlaceableObject.SpriteSource>();
    }

    public static void FixBellBaby(GameObject obj)
    {
        obj.AddComponent<BellBaby>();
        
        var fsm = obj.LocateMyFSM("Control");
        
        var child = fsm.FsmVariables.FindFsmGameObject("Nearest Child");
        fsm.GetState("Idle").AddAction(() =>
        {
            if (!child.Value) child.Value = HeroController.instance.gameObject;
        }, 2);

        var mother = fsm.FsmVariables.FindFsmGameObject("Mother");
        fsm.GetState("Wait for Mother").AddAction(() =>
        {
            mother.Value = HeroController.instance.gameObject;
            fsm.SendEvent("MOTHER AWAKE");
        });
        
        fsm.GetState("Tink React").AddAction(() => obj.BroadcastEvent("OnHit"));
    }

    public class BellBaby : TriggerActivator;

    public static void FixMetronome(GameObject obj)
    {
        var plat = obj.GetComponent<MetronomePlat>();
        plat.ticker = obj.AddComponent<TimedTicker>();
        obj.AddComponent<MetronomeReactivator>();
    }

    public static void FixMemoryPlat(GameObject obj)
    {
        obj.transform.GetChild(2).GetChild(1).GetChild(2).GetChild(1).SetAsFirstSibling();
    }

    public static void SetMetronomeTime(GameObject obj, float delay)
    {
        obj.GetComponent<TimedTicker>().tickDelay = delay;
    }

    public static void SetMetronomeDelay(GameObject obj, float delay)
    {
        obj.GetComponent<TimedTicker>().timeElapsed = -delay;
    }
    
    private class MetronomeReactivator : MonoBehaviour
    {
        private void OnEnable()
        {
            var plat = gameObject.GetComponent<MetronomePlat>();
            if (!plat.didStart) return;
            plat.Start();
        }
    }

    public static void FixHoker(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Control");
        fsm.GetState("Alt Pos?").AddAction(() => fsm.SendEvent("FINISHED"), 0);
    }

    public static void FixUpdraft(GameObject obj)
    {
        obj.transform.SetScale2D(new Vector2(1, 1));
    }

    public static void DelayRotation(GameObject obj, float rot)
    {
        obj.AddComponent<RotateOnSpawn>().rotation = rot;
    }
    
    private class RotateOnSpawn : MonoBehaviour
    {
        public float rotation;

        private void Start()
        {
            transform.SetRotation2D(rotation);
        }
    }

    public static void FixGarmond(GameObject obj)
    {
        obj.RemoveComponent<ConstrainPosition>();
        
        var fsm = obj.LocateMyFSM("Control");
        fsm.fsmTemplate = null;

        fsm.GetState("Dormant").AddAction(() => fsm.SendEvent("QUICK TARGET"), 0);
        fsm.GetState("Black Thread?").AddAction(() => fsm.SendEvent("FINISHED"), 0);

        var endPoint = new GameObject("Battle End Point")
        {
            transform = { position = obj.transform.position }
        };
        fsm.GetState("Wait An Additional Frame")
            .AddAction(() => { fsm.FsmVariables.FindFsmGameObject("Battle End Point").Value = endPoint; }, 0);
        
        fsm.GetState("Idle").DisableAction(8);
        fsm.GetState("Chase Target").DisableAction(29);
        
        obj.AddComponent<Garmond>();
    }

    public static void FixGreenPrince(GameObject obj)
    {
        obj.AddComponent<GreenPrince>();
    }

    public static void FixShakra(GameObject obj)
    {
        obj.AddComponent<Shakra>();
    }

    private static readonly int EnemyLayer = LayerMask.NameToLayer("Enemies");
    public static void FixSecondSentinel(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Control");
        fsm.fsmTemplate = null;
        
        fsm.GetState("Ally Setup").DisableAction(2);
        
        fsm.GetState("Ally Wake").AddAction(() =>
        {
            // Finds target cursor
            var cursor = fsm.FsmVariables.FindFsmGameObject("Target Cursor").value;
            var cursorFsm = cursor.LocateMyFSM("Control");
            var target = cursorFsm.FsmVariables.FindFsmGameObject("Current Target");

            // Modifies the cursor's logic to target any enemy
            var getTarget = cursorFsm.GetState("Get Target");
            getTarget.DisableAction(0);
            getTarget.AddAction(() =>
            {
                var enemy = Object
                    .FindObjectsByType<Collider2D>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                    .Where(h => h.gameObject != obj && h.gameObject.layer == EnemyLayer)
                    .OrderBy(h => (h.transform.position - obj.transform.position).sqrMagnitude)
                    .FirstOrDefault();
                if (enemy && (enemy.transform.position - obj.transform.position).sqrMagnitude < 512) 
                    target.value = enemy.gameObject;
            }, 1);
        }, 5);
    }

    public static void FixSherma(GameObject obj)
    {
        obj.RemoveComponent<DeactivateIfPlayerdataTrue>();
        obj.AddComponent<Sherma>();
    }

    public static void PreFixArchitect(GameObject obj)
    {
        obj.transform.parent.DetachChildren();
        obj.transform.SetPositionZ(0.006f);
        
        obj.GetComponentInChildren<BoxCollider2D>().size = new Vector2(7, 2);
        obj.GetComponent<InteractableBase>().interactLabel = InteractableBase.PromptLabels.Speak;
    }

    public static void FixArchitect(GameObject obj)
    {
        obj.AddComponent<ArchitectNpc>();
    }

    public static void FixCaretaker(GameObject obj)
    {
        obj.AddComponent<Caretaker>();
    }

    public static void FixShermaCaretaker(GameObject obj)
    {
        obj.AddComponent<ShermaCaretaker>();
    }
    
    public class Npc : MonoBehaviour
    {
        public string text = "Sample Text";
    }
    
    public class Garmond : Npc
    {
        private void Start()
        {
            var fsm = gameObject.transform.Find("Victory NPC").gameObject.LocateMyFSM("Dialogue");
            fsm.GetState("Check").AddAction(() => fsm.SendEvent("REPEAT"), 0);
            var dialogue = (RunDialogue)fsm.GetState("Repeat").actions[1];
            dialogue.Sheet = "ArchitectMod";
            dialogue.Key = text;
        }
    }
    
    public class Shakra : Npc
    {
        private void Start()
        {
            var fsm = gameObject.LocateMyFSM("Dialogue");
            fsm.GetState("Meet?").AddAction(() => fsm.SendEvent("MEET"), 1);
                
            var state = fsm.GetState("Act 3 Meet");
            state.DisableAction(0);
            var dialogue = (RunDialogue) state.actions[1];
            dialogue.Sheet = "ArchitectMod";
            dialogue.Key = text;
        }
    }
    
    public class GreenPrince : Npc
    {
        private void Start()
        {
            var npc = GetComponent<BasicNPC>();
            var txt = npc.repeatText;
            txt.Sheet = "ArchitectMod";
            txt.Key = text;
            npc.talkText = [txt];
            npc.repeatText = txt;
        }
    }
    
    public class Sherma : Npc
    {
        private void Start()
        {
            var fsm = gameObject.LocateMyFSM("Conversation Control");
            fsm.GetState("Asleep").AddAction(() => fsm.SendEvent("WAKE"), 0);
            fsm.GetState("Choice").AddAction(() => fsm.SendEvent("REPEAT"), 0);
            var dialogue = (RunDialogue) fsm.GetState("Repeat").actions[0];
            dialogue.Sheet = "ArchitectMod";
            dialogue.Key = text;
        }
    }
    
    public class ArchitectNpc : Npc
    {
        private void Start()
        {
            var fsm = gameObject.LocateMyFSM("Behaviour");
            fsm.GetState("Start Pos").AddAction(() => fsm.SendEvent("BOTTOM"), 0);
            fsm.GetState("Met?").AddAction(() => fsm.SendEvent("FALSE"), 0);
            fsm.GetState("Is Act 3?").AddAction(() => fsm.SendEvent("FINISHED"), 0);
            
            var meet = fsm.GetState("Meet Dlg");
            meet.DisableAction(1);
            var dialogue = (RunDialogue) meet.actions[2];
            dialogue.Sheet = "ArchitectMod";
            dialogue.Key = text;
            
            fsm.GetState("Shop Up").AddAction(() => fsm.SendEvent("SHOP CLOSED"), 0);
            fsm.GetState("Will Leave?").AddAction(() => fsm.SendEvent("FINISHED"), 0);
        }
    }
    
    public class Caretaker : Npc
    {
        private void Start()
        {
            var fsm = gameObject.LocateMyFSM("Dialogue");
            fsm.GetState("Gone?").AddAction(() => fsm.SendEvent("FALSE"), 0);
            fsm.GetState("Delivery?").AddAction(() => fsm.SendEvent("FINISHED"), 0);
            fsm.GetState("Will Offer Snare?").AddAction(() => fsm.SendEvent("FINISHED"), 0);
            fsm.GetState("Lv1 Meet?").AddAction(() => fsm.SendEvent("SPEAK"), 0);
            var dialogue = (RunDialogue) fsm.GetState("Lv1 Meet").actions[1];
            dialogue.Sheet = "ArchitectMod";
            dialogue.Key = text;
        }
    }
    
    public class ShermaCaretaker : Npc
    {
        private void Start()
        {
            var fsm = gameObject.LocateMyFSM("Control");
            fsm.GetState("Start Asleep?").AddAction(() => fsm.SendEvent("FINISHED"), 0);
            fsm.GetState("Delivery?").AddAction(() => fsm.SendEvent("FINISHED"), 0);
            fsm.GetState("Convo Check").AddAction(() => fsm.SendEvent("REPEAT"), 0);
            var dialogue = (RunDialogue) fsm.GetState("Repeat").actions[2];
            dialogue.Sheet = "ArchitectMod";
            dialogue.Key = text;
        }
    }

    public static void FixFleaCounter(GameObject obj)
    {
        obj.transform.GetChild(2).gameObject.AddComponent<PngObject>();
        obj.AddComponent<CustomFleaCounter>();
    }

    private static readonly List<CustomFleaCounter> FleaCounters = [];
    
    private static bool _overrideAreaText;
    private static string _areaHeader;
    private static string _areaBody;
    private static string _areaFooter;
    
    public class CustomFleaCounter : MonoBehaviour
    {
        private static readonly Color Gold = UI.MaxItemsTextColor;
        private static readonly Color Silver = new(0.45f, 0.45f, 0.5f);
        private static readonly Color Bronze = new(0.4f, 0.2f, 0.05f);
        
        private SimpleCounter _counter;
        
        public string header; 
        public string footer; 
        
        public int currentCount;
        
        public int gold = int.MaxValue;
        public int silver = int.MaxValue;
        public int bronze = int.MaxValue;
        
        public bool high;

        private Color _color;
        private Color _prevColor;

        private string _currentEvent;
        
        private void Awake()
        {
            _counter = GetComponent<SimpleCounter>();
            _counter.counterText.text = currentCount.ToString();
            
            transform.SetScale2D(Vector2.one);
            
            RefreshGold(false);
        }

        public void Increment()
        {
            currentCount++;
            _counter.counterText.text = currentCount.ToString();
            
            RefreshGold(true);
        }

        public void Decrement()
        {
            currentCount--;
            _counter.counterText.text = currentCount.ToString();
            
            RefreshGold(true);
        }

        public void Announce()
        {
            _overrideAreaText = true;
            _areaHeader = header;
            _areaBody = currentCount.ToString();
            _areaFooter = footer;
            
            AreaTitle.Instance.gameObject.SetActive(true);
        }

        private void RefreshGold(bool effect)
        {
            if (high)
            {
                if (currentCount >= gold)
                {
                    _color = Gold;
                    _currentEvent = "OnGold";
                }
                else if (currentCount >= silver)
                {
                    _color = Silver;
                    _currentEvent = "OnSilver";
                }
                else if (currentCount >= bronze)
                {
                    _color = Bronze;
                    _currentEvent = "OnBronze";
                }
                else
                {
                    _color = Color.white;
                    _currentEvent = "OnWhite";
                }
            }
            else
            {
                if (currentCount < gold)
                {
                    _color = Gold;
                    _currentEvent = "OnGold";
                }
                else if (currentCount < silver)
                {
                    _color = Silver;
                    _currentEvent = "OnSilver";
                }
                else if (currentCount < bronze)
                {
                    _color = Bronze;
                    _currentEvent = "OnBronze";
                }
                else
                {
                    _color = Color.white;
                    _currentEvent = "OnWhite";
                }
            }

            var doEvent = false;
            if (_color != _prevColor && effect)
            {
                _counter.hitTargetEffect.SetActive(true);
                doEvent = true;
            }

            _counter.counterText.color = _color;
            _prevColor = _color;
            
            if (doEvent) gameObject.BroadcastEvent(_currentEvent);
        }

        private void OnEnable()
        {
            FleaCounters.Add(this);
        }
        
        private void OnDisable()
        {
            FleaCounters.Remove(this);
        }

        private void Update()
        {
            gameObject.transform.SetPosition2D(new Vector2(0, FleaCounters.IndexOf(this) * 1.5f));
        }
    }

    public static void FixConfetti(GameObject obj)
    {
        var system = obj.GetComponent<ParticleSystem>();
        
        var main = system.main;
        main.maxParticles = 100000;

        var emission = system.emission;
        emission.rateOverTime = 0;
    }

    public static void FixBilePlat(GameObject obj)
    {
        obj.transform.GetChild(1).SetAsFirstSibling();
    }
}