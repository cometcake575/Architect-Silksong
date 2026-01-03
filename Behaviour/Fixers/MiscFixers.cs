using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Architect.Behaviour.Custom;
using Architect.Content.Preloads;
using Architect.Editor;
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
    public static Material SpriteMaterial;
    
    public static void Init()
    {
        // Custom bench fix - if the bench was determined to be invalid, override it and use the saved data anyway
        // Fallback to first hazard respawn point
        
        PreloadManager.RegisterPreload(new BasicPreload(
            "Tut_02", "bone_plat_01", o =>
            {
                SpriteMaterial = o.GetComponent<SpriteRenderer>().material;
            }));

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
                        .FirstOrDefault();
                    if (!hrm) return null;
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
                        .FirstOrDefault();
                    return hrm ? hrm.transform : null;
                }

                return point;
            });

        #endregion
        
        typeof(MatchHeroFacing).Hook(nameof(MatchHeroFacing.DoMatch),
            (Action<MatchHeroFacing> orig, MatchHeroFacing self) =>
            {
                if (PreloadManager.HasPreloaded) orig(self);
            });

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

        HookUtils.OnFsmAwake += fsm =>
        {
            if (fsm.name == "door_act3_wakeUp") fsm.GetState("Init").DisableAction(1);
            if (EditManager.IsEditing && fsm.FsmName == "Detect Grab" && fsm.name == "Tendril") 
                fsm.gameObject.SetActive(false);
        };

        _ = new Hook(typeof(MaggotRegion).GetProperty(nameof(MaggotRegion.IsActive))!.GetGetMethod(),
            (Func<MaggotRegion, bool> orig, MaggotRegion self) =>
            {
                var water = self.GetComponent<Water>();
                return water ? water.maggot : orig(self);
            });

        _ = new Hook(typeof(AbyssWater).GetProperty(nameof(AbyssWater.IsActive),
                BindingFlags.NonPublic | BindingFlags.Instance)!.GetGetMethod(true),
            (Func<AbyssWater, bool> orig, AbyssWater self) =>
            {
                var water = self.GetComponent<Water>();
                return water ? water.abyss : orig(self);
            });
    }

    public class ColorLock : MonoBehaviour;

    private delegate string ToStringOrig(ref LocalisedString self, bool allowBlankText);
    
    public delegate void GetRespawnInfo(GameManager self, out string a, out string b);
    
    public static void FixTollBench(GameObject obj)
    {
        obj.transform.GetChild(3).GetChild(1).GetChild(2).GetChild(0)
            .gameObject.AddComponent<PlaceableObject.SpriteSource>();
        Object.DestroyImmediate(obj.transform.GetChild(4).GetChild(2).gameObject);
        obj.AddComponent<CustomBench>();
    }
    
    public static void FixBench(GameObject obj)
    {
        Object.DestroyImmediate(obj.transform.GetChild(2).gameObject);
        obj.AddComponent<CustomBench>();
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

    private static readonly int ParticleLayer = LayerMask.NameToLayer("Particle");

    public static void MarkRing(GameObject obj)
    {
        obj.AddComponent<MapperRing>();
        obj.RemoveComponent<DeactivateIfPlayerdataTrue>();
        obj.layer = ParticleLayer;
    }

    public class MapperRing : TriggerActivator
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
        
        obj.AddComponent<TriggerActivator>();
    }

    public class TriggerActivator : MonoBehaviour
    {
        public int layer;
    }
    
    public static void FixLamp(GameObject obj)
    {
        obj.transform.GetChild(0).GetChild(2).SetAsFirstSibling();
    }
    
    public static void FixBigLamp(GameObject obj)
    {
        obj.transform.SetPositionZ(0.05f);
        obj.transform.GetChild(0).GetChild(0).GetChild(2).SetAsFirstSibling();
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
        obj.AddComponent<TriggerActivator>();
        obj.layer = ParticleLayer;
        
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

    public static void FixMetronome(GameObject obj)
    {
        var plat = obj.GetComponent<MetronomePlat>();
        plat.ticker = obj.AddComponent<TimedTicker>();
        obj.AddComponent<MetronomeReactivator>();
    }

    public static void FixMemoryPlat(GameObject obj)
    {
        obj.transform.GetChild(2).GetChild(1).GetChild(2).GetChild(1).SetAsFirstSibling();
        Object.Destroy(obj.transform.GetChild(0).gameObject);
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
        var explode = fsm.GetState("Explode");
        explode.DisableAction(5);
        explode.AddAction(() =>
        {
            var ede = obj.GetComponent<EnemyDeathEffects>();
            ede.EmitSound();
            ede.EmitEffects(ede.EmitCorpse(
                90, 
                1, 
                AttackTypes.Generic, 
                NailElements.None, 
                null, 
                null, 
                out _));
            Object.Destroy(obj);
        });
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

    public static void FixGarmondBoss(GameObject obj)
    {
        FixGarmond(obj);

        var fsm = obj.LocateMyFSM("Control");
        fsm.GetState("Dormant").AddAction(() => fsm.SendEvent("CITADEL REMEET"), 0);
        fsm.GetState("Cit NPC").AddAction(() => fsm.SendEvent("BATTLE START"), 0);
        
        fsm.GetState("Auto Target?").AddAction(() =>
        {
            fsm.FsmVariables.FindFsmGameObject("Target").value = HeroController.instance.gameObject;
            fsm.SendEvent("FINISHED");
        }, 0);
        
        fsm.GetState("Keep Bouncing?").AddAction(() => fsm.SendEvent("FINISHED"), 0);
        
        fsm.GetState("Music").AddAction(() => fsm.SendEvent("FINISHED"), 0);

        fsm.GetState("Death Land").transitions = [];
        
        var roar = fsm.GetState("Enemy Roar");
        roar.DisableAction(2);
        roar.DisableAction(5);
        roar.DisableAction(6);
        roar.DisableAction(7);
        
        obj.transform.Find("Citadel Library NPC").gameObject.SetActive(false);
    }

    public static void FixGreenPrince(GameObject obj)
    {
        obj.AddComponent<BasicNpcFix>();
    }

    public static void FixSeth(GameObject obj)
    {
        obj.LocateMyFSM("Location").enabled = false;
        obj.AddComponent<Seth>();
    }

    public static void FixShakraBoss(GameObject obj)
    {
        //EnemyFixers.RemoveConstrainPosition(obj);
    }

    public static void FixShakra(GameObject obj)
    {
        obj.AddComponent<Shakra>();
    }

    private static readonly int EnemyLayer = LayerMask.NameToLayer("Enemies");

    public static void FixSecondSentinelAlly(GameObject obj)
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

    public static void PreFixLoam(GameObject obj)
    {
        obj.transform.parent.DetachChildren();
        obj.transform.SetPositionZ(0.006f);
    }

    public static void FixLoam(GameObject obj)
    {
        obj.AddComponent<Loam>();
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
        EnemyFixers.KeepActive(obj);
        obj.AddComponent<Caretaker>();
    }

    public static void FixFrightenedSherma(GameObject obj)
    {
        obj.AddComponent<FrightenedSherma>();
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
    
    public class Loam : Npc
    {
        private void Start()
        {
            var fsm = gameObject.LocateMyFSM("Behaviour");
            fsm.GetState("State?").AddAction(() => fsm.SendEvent("FINISHED"), 0);
            var dialogue = (RunDialogue)fsm.GetState("Repeat").actions[0];
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
    
    public class BasicNpcFix : Npc
    {
        private void Start()
        {
            var npc = GetComponent<BasicNPC>();
            var txt = npc.repeatText;
            txt.Sheet = "ArchitectMod";
            txt.Key = text;
            npc.talkText = [txt];
            npc.repeatText = txt;
            npc.returnText = txt;
        }
    }
    
    public class Seth : Npc
    {
        private void Start()
        {
            var fsm = gameObject.LocateMyFSM("Dialogue");
            fsm.GetState("Convo Check").AddAction(() => fsm.SendEvent("REPEAT"), 0);
            var dialogue = (RunDialogue)fsm.GetState("Repeat").actions[0];
            dialogue.Sheet = "ArchitectMod";
            dialogue.Key = text;
        }
    }
    
    public class Gilly : Npc
    {
        private void Start()
        {
            var fsm = gameObject.LocateMyFSM("Behaviour");
            fsm.GetState("Convo Check").AddAction(() => fsm.SendEvent("REPEAT"), 0);
            var dialogue = (RunDialogue)fsm.GetState("Repeat").actions[0];
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
            var dialogue = (RunDialogue)fsm.GetState("Repeat").actions[0];
            dialogue.Sheet = "ArchitectMod";
            dialogue.Key = text;
        }
    }
    
    public class Pilby : Npc
    {
        private void Start()
        {
            var fsm = gameObject.LocateMyFSM("Behaviour");
            fsm.GetState("Location Check").AddAction(() => fsm.SendEvent("PILGRIMS REST"), 0);
            fsm.GetState("Pilgrims Rest Choice").AddAction(() => fsm.SendEvent("FINISHED"), 0);

            var dialogue = (RunDialogue)fsm.GetState("Repeat PR").actions[0];
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
            fsm.GetState("Hail Hero?").AddAction(() => fsm.SendEvent("FINISHED"), 0);
            fsm.GetState("Gone?").AddAction(() => fsm.SendEvent("FALSE"), 0);
            fsm.GetState("Delivery?").AddAction(() => fsm.SendEvent("FINISHED"), 0);
            fsm.GetState("Will Offer Snare?").AddAction(() => fsm.SendEvent("FINISHED"), 0);
            fsm.GetState("Lv1 Meet?").AddAction(() => fsm.SendEvent("SPEAK"), 0);
            var dialogue = (RunDialogue) fsm.GetState("Lv1 Meet").actions[1];
            dialogue.Sheet = "ArchitectMod";
            dialogue.Key = text;
        }
    }
    
    public class Fleamaster : Npc
    {
        private void Start()
        {
            var fsm = gameObject.LocateMyFSM("Control");
            fsm.GetState("Outro Wave?").AddAction(() => fsm.SendEvent("CANCEL"), 0);
            fsm.GetState("Intro Type").AddAction(() => fsm.SendEvent("INTRO"), 0);
            fsm.GetState("Choice").AddAction(() => fsm.SendEvent("DECLINE"), 0);
            fsm.GetState("Decline").AddAction(() => fsm.SendEvent("CONVO_END"), 0);

            var dialogue = (RunDialogue)fsm.GetState("Intro").actions[0];
            dialogue.Sheet = "ArchitectMod";
            dialogue.Key = text;
        }
    }
    
    public class FrightenedSherma : Npc
    {
        private void Start()
        {
            var fsm = gameObject.LocateMyFSM("Conversation Control");
            fsm.fsm.globalTransitions = [];

            var meet = fsm.GetState("Meet 1");
            meet.DisableAction(0);
            var dialogue = (RunDialogue)meet.actions[4];
            dialogue.Sheet = "ArchitectMod";
            dialogue.Key = text;

            var turn = fsm.GetState("Turn Back");
            turn.DisableAction(0);
            turn.DisableAction(1);
            turn.DisableAction(2);
            turn.AddAction(() => fsm.SendEvent("FINISHED"), 4);
            
            fsm.GetState("Look Back").AddAction(() => fsm.SendEvent("FINISHED"), 0);
            fsm.GetState("Meet 2").AddAction(() => fsm.SendEvent("CONVO_END"), 0);
            
            var end = fsm.GetState("End Dialogue");
            ((EndDialogue)end.actions[1]).ReturnControl.value = true;
            ((ActivateInteractible)end.actions[0]).Activate.value = true;
            end.AddAction(() => fsm.SendEvent("FINISHED"), 2);
            end.AddAction(new Wait { time = 0.1f }, 0);
            
            fsm.GetState("Start Battle").AddAction(() =>
            {
                fsm.SetState("Idle");
            }, 0);
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
            var dialogue = (RunDialogue)fsm.GetState("Repeat").actions[2];
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

        public void Reset()
        {
            currentCount = 0;
            _counter.counterText.text = currentCount.ToString();
            
            RefreshGold(true);
        }

        public void Decrement()
        {
            currentCount--;
            _counter.counterText.text = currentCount.ToString();
            
            RefreshGold(true);
        }

        public void Announce() => TitleUtils.DisplayTitle(header, currentCount.ToString(), footer, 0);

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
                if (currentCount <= gold)
                {
                    _color = Gold;
                    _currentEvent = "OnGold";
                }
                else if (currentCount <= silver)
                {
                    _color = Silver;
                    _currentEvent = "OnSilver";
                }
                else if (currentCount <= bronze)
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

    public static void FixFleamaster(GameObject obj)
    {
        obj.AddComponent<Fleamaster>();
    }

    public static void FixChoirClapper(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Control");
        fsm.GetState("Init").DisableAction(0);
        fsm.GetState("Rest").AddAction(() =>
        {
            fsm.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
            fsm.SetState("Roar End");
        }, 2);

        var bt = fsm.FsmVariables.FindFsmBool("Black Threaded");
        fsm.GetState("Walk").AddAction(() =>
        {
            if (bt.value) return;
            var bts = obj.GetComponent<BlackThreadState>();
            if (bts && bts.CheckIsBlackThreaded()) bt.value = true;
        }, 0);

        fsm.FsmVariables.FindFsmFloat("Ground Y").value = obj.transform.position.y;
    }

    public static void FixCoral(GameObject obj)
    {
        obj.transform.parent = null;
        obj.SetActive(true);
        obj.GetComponent<ActivatingBase>().SetActive(true, true);
        obj.RemoveComponent<RandomlyFlipScale>();
        obj.RemoveComponent<RandomRotationDelay>();
    }

    public static void FixFlea(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Call Out");
        fsm.FsmVariables.FindFsmObject("Quest Target").Clear();
        fsm.GetState("Rescue 2").AddAction(() => obj.BroadcastEvent("OnSave"));
    }

    public static void FixMirror(GameObject obj)
    {
        obj.AddComponent<PngObject>();
        obj.transform.parent = null;
        obj.transform.SetScale2D(new Vector2(2, 2));
        obj.transform.SetPositionZ(-0.1f);
    }

    public static void FixPilby(GameObject obj)
    {
        obj.AddComponent<Pilby>();
    }

    public static void FixToll(GameObject obj)
    {
        obj.RemoveComponent<CurrencyCounterAppearRegion>();
        obj.AddComponent<Toll>();
    }

    public class Toll : MonoBehaviour
    {
        public string text = "Sample Text";

        private void Start()
        {
            var fsm = gameObject.LocateMyFSM("Behaviour (special)");

            var confirm = fsm.GetState("Confirm");
            confirm.DisableAction(0);
            
            var dialogue = confirm.actions[1];
            switch (dialogue)
            {
                case DialogueYesNoV2 ynv2:
                    ynv2.TranslationSheet = "ArchitectMod";
                    ynv2.TranslationKey = text;
                    break;
                case GetLocalisedString gls:
                {
                    gls.Enabled = false;
                    var str = fsm.FsmVariables.FindFsmString("Prompt Text Override");
                    confirm.AddAction(() => str.Value = text);
                    break;
                }
            }

            var give = fsm.GetState("Give Object");
            if (give == null) return;
            give.DisableAction(0);
            give.AddAction(() => gameObject.BroadcastEvent("OnPay"), 0);
        }
    }

    public static void FixShamanShell(GameObject obj)
    {
        obj.GetComponentInChildren<BoxCollider2D>().enabled = true;
        obj.transform.parent = null;
        obj.transform.SetRotation2D(0);
        obj.transform.SetPositionZ(0.006f);
    }

    public static void FixStatue(GameObject obj)
    {
        obj.transform.GetChild(1).gameObject.SetActive(false);

        var bh = obj.GetComponent<BreakableHolder>();
        bh.Broken.AddListener(() =>
        {
            obj.BroadcastEvent("OnBreak");
        });
    }

    public static void FixDecoration(GameObject obj)
    {
        obj.transform.SetScale2D(Vector2.one * 5);
        obj.transform.SetPositionZ(0.006f);
    }

    public static void FixWebDecoration(GameObject obj)
    {
        FixDecoration(obj);
        var fc = obj.AddComponent<FollowCamera>();
        fc.followX = true;
        fc.followY = true;
    }

    public static void FixSnow(GameObject obj)
    {
        FixDecoration(obj);
        var vars = obj.LocateMyFSM("FSM").FsmVariables;
        vars.FindFsmFloat("X Min").Value = -1000;
        vars.FindFsmFloat("Y Min").Value = -1000;
    }

    public static void FixGrindPlat(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("Control");
        fsm.FsmVariables.FindFsmBool("Drop Self").Value = true;
        fsm.FsmVariables.FindFsmFloat("Drop Y").Value = obj.transform.GetPositionY() - 5;
    }

    public static void FixBreakableWall(GameObject obj)
    {
        var fsm = obj.LocateMyFSM("breakable_wall_v2");
        fsm.fsmTemplate = null;
        var idle = fsm.GetState("Idle");
        var t2d = (Trigger2dEvent)idle.actions[2];
        var rd = (ReceivedDamage)idle.actions[4];
        rd.sendEventHeavy = rd.sendEvent;
        t2d.sendEvent = rd.sendEvent;

        var hit2 = fsm.GetState("Hit 2");
        var rd2 = (ReceivedDamage)hit2.actions[2];
        rd2.sendEventHeavy = rd2.sendEvent;
        
        obj.GetComponent<PersistentBoolItem>().OnSetSaveState += value =>
        {
            if (!value) return;
            obj.BroadcastEvent("OnBreak");
            obj.BroadcastEvent("LoadedBroken");
        };
        fsm.GetState("Break").AddAction(() =>
        {
            obj.BroadcastEvent("OnBreak");
            obj.BroadcastEvent("FirstBreak");
        });
    }

    public static void FocusFirstChild(GameObject obj) => obj.transform.GetChild(1).SetAsFirstSibling();

    public static void FixGilly(GameObject obj)
    {
        EnemyFixers.KeepActive(obj);
        obj.AddComponent<Gilly>();
    }

    public static void FixWater(GameObject obj)
    {
        obj.transform.parent = null;
        
        obj.transform.localScale = Vector3.one;
        var bc1 = obj.GetComponent<BoxCollider2D>();
        var bc2 = obj.transform.GetChild(0).GetComponent<BoxCollider2D>();
        var bc3 = obj.transform.GetChild(0).GetChild(0).GetComponent<BoxCollider2D>();
        bc1.size = Vector2.one * 5;
        bc2.size = Vector2.one * 5;
        bc3.size = Vector2.one * 5;
        
        bc1.offset = Vector2.zero;
        bc2.offset = Vector2.zero;
        bc3.offset = Vector2.zero;
        
        obj.AddComponent<Water>();
    }

    public class Water : MonoBehaviour
    {
        private SurfaceWaterRegion _swr;
        private MaggotRegion _mr;
        private BoxCollider2D _col;
        private Vector3 _lastPos;

        public bool maggot;
        public bool abyss;
        
        private void Start()
        {
            _swr = GetComponent<SurfaceWaterRegion>();
            _mr = GetComponent<MaggotRegion>();
            _col = GetComponent<BoxCollider2D>();
            _lastPos = transform.position;

            transform.GetChild(0).position = transform.position + new Vector3(0, 0.45f);
        }

        private void Update()
        {
            var sizeY = transform.GetScaleY() * _col.size.y;
            var offset = sizeY / 2 + 0.6f * Mathf.Min(transform.GetScaleY() * _col.size.y, 1);
            _swr.heroSurfaceY = transform.GetPositionY() + offset;
            _mr.heroMaggotsYPos = HeroController.instance.transform.GetPositionY() - transform.GetPositionY() - 1;
            var sm = _mr.spawnedHeroMaggots;
            if (sm)
            {
                sm.transform.rotation = transform.rotation;
                sm.transform.SetPositionY(transform.position.y + _mr.heroMaggotsYPos);
            }
            if (_swr.isHeroInside && _lastPos != transform.position)
            {
                HeroController.instance.transform.position += transform.position - _lastPos;
            }

            _lastPos = transform.position;
        }
    }

    public static void FixPreacher(GameObject obj)
    {
        obj.LocateMyFSM("State Control").enabled = false;

        obj.AddComponent<BasicNpcFix>();
    }
}