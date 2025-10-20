using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Architect.Objects.Placeable;
using Architect.Utils;
using HutongGames.PlayMaker.Actions;
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
    }

    private delegate string ToStringOrig(ref LocalisedString self, bool allowBlankText);
    
    public delegate void GetRespawnInfo(GameManager self, out string a, out string b);
    
    // Fixes an exception on the bench when a copy is instantiated
    public static void FixBench(GameObject bench)
    {
        Object.DestroyImmediate(bench.transform.GetChild(2).gameObject);
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

    public class Kratt : MonoBehaviour;
    
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

    public class BellBaby : MonoBehaviour;

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

    public static void FixShakra(GameObject obj)
    {
        obj.AddComponent<Shakra>();
    }

    public static void FixSherma(GameObject obj)
    {
        obj.RemoveComponent<DeactivateIfPlayerdataTrue>();
        obj.AddComponent<Sherma>();
    }

    public static void FixShermaCaretaker(GameObject obj)
    {
        obj.AddComponent<ShermaCaretaker>();
    }
    
    public class Npc : MonoBehaviour
    {
        public string text = "Sample Text";
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
    
    public class ShermaCaretaker : Npc
    {
        private void Start()
        {
            var fsm = gameObject.LocateMyFSM("Control");
            fsm.GetState("Start Asleep?").AddAction(() => fsm.SendEvent("FINSIHED"), 0);
            fsm.GetState("Delivery?").AddAction(() => fsm.SendEvent("FINSIHED"), 0);
            fsm.GetState("Convo Check").AddAction(() => fsm.SendEvent("REPEAT"), 0);
            var dialogue = (RunDialogue) fsm.GetState("Repeat").actions[2];
            dialogue.Sheet = "ArchitectMod";
            dialogue.Key = text;
        }
    }
}