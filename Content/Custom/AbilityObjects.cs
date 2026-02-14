using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Architect.Behaviour.Abilities;
using Architect.Behaviour.Custom;
using Architect.Editor;
using Architect.Objects.Categories;
using Architect.Objects.Groups;
using Architect.Objects.Placeable;
using Architect.Utils;
using GlobalEnums;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using PrepatcherPlugin;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Architect.Content.Custom;

public static class AbilityObjects
{
    private static GameObject _canvasObj;
    
    // Currently active ability objects
    internal static readonly Dictionary<string, List<Binding>> ActiveBindings = [];
    internal static readonly Dictionary<string, List<Binding>> ActiveVisibleBindings = [];
    internal static readonly Dictionary<string, int> ActiveCrystals = [];
    
    // Registered ability objects
    internal static readonly List<(string, Sprite)> Bindings = [];
    internal static readonly List<(string, Sprite)> CrestBindings = [];
    internal static readonly List<(string, int, Sprite)> Crystals = [];
    
    // UI Icons
    internal static readonly List<Image> BindingIcons = [];
    internal static readonly List<Image> CrystalIcons = [];
    
    public static void Init()
    {
        AbilityCrystal.Init();
        MakeCrystals("Dash", "dash");
        MakeCrystals("Clawline", "harpoon");
        MakeCrystals("Double Jump", "wings");
        SetupCrystalHooks();
        
        Binding.Init();
        Categories.Abilities.Add(MakeAbilityBinding("Dash", "dash"));
        Categories.Abilities.Add(MakeAbilityBinding("Clawline", "harpoon"));
        Categories.Abilities.Add(MakeAbilityBinding("Wall Jump", "wall_jump"));
        Categories.Abilities.Add(MakeAbilityBinding("Silk Soar", "super_jump"));
        Categories.Abilities.Add(MakeAbilityBinding("Drift", "drift"));
        Categories.Abilities.Add(MakeAbilityBinding("Double Jump", "double_jump"));
        Categories.Abilities.Add(MakeAbilityBinding("Silk Heart", "silk_heart"));
        Categories.Abilities.Add(MakeAbilityBinding("Frost", "frost"));
        Categories.Abilities.Add(MakeAbilityBinding("Needle", "needle", "Locks the needle damage to 5"));
        Categories.Abilities.Add(MakeAbilityBinding("Attack", "attack"));
        Categories.Abilities.Add(MakeAbilityBinding("Jump", "jump"));
        // Categories.Abilities.Add(MakeAbilityBinding("Tool", "tools"));
        SetupBindingHooks();
        
        CrestBinding.InitCrestBindings();
        Categories.Abilities.Add(MakeCrestBinding("Hunter", "hunter_crest", "Hunter"));
        Categories.Abilities.Add(MakeCrestBinding("Hunter v2", "hunter_2_crest", "Hunter_v2"));
        Categories.Abilities.Add(MakeCrestBinding("Hunter v3", "hunter_3_crest", "Hunter_v3"));
        Categories.Abilities.Add(MakeCrestBinding("Reaper", "reaper_crest", "Reaper"));
        Categories.Abilities.Add(MakeCrestBinding("Wanderer", "wanderer_crest", "Wanderer"));
        Categories.Abilities.Add(MakeCrestBinding("Beast", "beast_crest", "Warrior"));
        Categories.Abilities.Add(MakeCrestBinding("Witch", "witch_crest", "Witch"));
        Categories.Abilities.Add(MakeCrestBinding("Architect", "architect_crest", "Toolmaster"));
        Categories.Abilities.Add(MakeCrestBinding("Shaman", "shaman_crest", "Spell"));
        Categories.Abilities.Add(MakeCrestBinding("Cursed", "cursed_crest", "Cursed"));
        Categories.Abilities.Add(MakeCrestBinding("Cloakless", "cloakless_crest", "Cloakless"));
        
        SetupVisuals();
    }

    public static void Update()
    {
        if (!GameManager.SilentInstance) return;
        _canvasObj.SetActive(!GameManager.SilentInstance.isPaused && GameManager.SilentInstance.IsGameplayScene()
                                                                  && !EditManager.IsEditing);
    }

    #region Make Crystals
    private static void MakeCrystals(string name, string id)
    {
        MakeCrystal($"{name} Crystal", id, $"Crystals.{id}_s", 1);
        MakeCrystal($"Double {name} Crystal", id, $"Crystals.{id}_m", 2);
        MakeCrystal($"Triple {name} Crystal", id, $"Crystals.{id}_l", 3);
    }

    private static void MakeCrystal(string name, string id, string spriteTexture, int count)
    {
        var crystalObj = new GameObject(name);
        
        crystalObj.SetActive(false);
        Object.DontDestroyOnLoad(crystalObj);

        var col = crystalObj.AddComponent<CircleCollider2D>();
        col.radius = 0.48f;
        col.isTrigger = true;
        col.offset = new Vector2(0, -0.1f * (3-count));

        var sprite = ResourceUtils.LoadSpriteResource(spriteTexture, FilterMode.Point, ppu:15);

        crystalObj.transform.position = new Vector3(0, 0, 0.005f);
        var child = new GameObject("Sprite");
        child.transform.SetParent(crystalObj.transform, false);
        
        child.AddComponent<SpriteRenderer>().sprite = sprite;
        child.AddComponent<FloatAnim>();
        
        var crystal = crystalObj.AddComponent<AbilityCrystal>();
        crystal.type = id;
        crystal.count = count;
        
        Crystals.Add((id, count, sprite));
        
        Categories.Abilities.Add(new CustomObject(name, $"{id}_{count}", crystalObj,
            description: "Lets the player use an ability a limited number of times without needing to unlock it.\n\n" +
                         "Can also be used to refresh abilities midair or without silk.\n\n" +
                         "Ability crystals override any active bindings.")
            .WithConfigGroup(ConfigGroup.AbilityCrystal)
            .WithBroadcasterGroup(BroadcasterGroup.AbilityCrystal)
            .WithReceiverGroup(ReceiverGroup.AbilityCrystal));
    }
    #endregion

    #region Make Bindings

    private static PlaceableObject MakeAbilityBinding(string name, string id, string overrideDesc = null)
    {
        var obj = new GameObject($"{name} Binding");
        Object.DontDestroyOnLoad(obj);
        obj.SetActive(false);
        
        obj.transform.position = new Vector3(0, 0, 0.005f);
        
        var binding = obj.AddComponent<Binding>();
        binding.bindingType = id;
        
        var enabledSprite = ResourceUtils.LoadSpriteResource($"Bindings.{id}_enabled");
        binding.enabledSprite = enabledSprite;
        binding.disabledSprite = ResourceUtils.LoadSpriteResource($"Bindings.{id}_disabled");
        
        obj.AddComponent<SpriteRenderer>().sprite = enabledSprite;
        Bindings.Add((id, enabledSprite));
        
        var collider = obj.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.65f;

        return new CustomObject($"{name} Binding", $"{id}_binding", obj,
                description: (overrideDesc ?? "Temporarily disables a skill. Touching the binding will toggle it.\n\n") +
                             "Bindings are active by default, set 'Binding Active' to false\n" +
                             "for a binding the player must touch to enable.\n\n" +
                             "Set 'Reversible' to true to allow the player to toggle the binding multiple times.")
            .WithBroadcasterGroup(BroadcasterGroup.Bindings)
            .WithConfigGroup(ConfigGroup.Bindings);
    }

    private static PlaceableObject MakeCrestBinding(string name, string id, string crestId)
    {
        var obj = new GameObject($"{name} Crest Binding");
        Object.DontDestroyOnLoad(obj);
        obj.SetActive(false);
        
        obj.transform.position = new Vector3(0, 0, 0.005f);
        
        var binding = obj.AddComponent<CrestBinding>();
        binding.bindingType = crestId;
        
        var enabledSprite = ResourceUtils.LoadSpriteResource($"Bindings.{id}_enabled");
        binding.enabledSprite = enabledSprite;
        binding.disabledSprite = ResourceUtils.LoadSpriteResource($"Bindings.{id}_disabled");
        
        obj.AddComponent<SpriteRenderer>().sprite = enabledSprite;
        CrestBindings.Add((crestId, enabledSprite));
        
        var collider = obj.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.65f;

        return new CustomObject($"{name} Binding", $"{id}_binding", obj,
                description: "Locks the player to this crest when the binding is active.\n" +
                             "If multiple crest bindings are placed, the crest with the most bindings will be the active crest.\n\n" +
                             "Touching the binding will toggle it. 'Binding Active' determines whether it starts on.\n" +
                             "Enable 'Reversible' to allow the binding to be toggled on and off multiple times.")
            .WithBroadcasterGroup(BroadcasterGroup.Bindings)
            .WithConfigGroup(ConfigGroup.Bindings);
    }
    #endregion

    #region Binding Checks
    private static bool BindingCheck(bool orig, string type)
    {
        if (!orig) return false;

        if (!ActiveBindings.TryGetValue(type, out var list) || list.Count == 0) return true;

        list.RemoveAll(binding => !binding);

        return list.Count(binding => binding.active && binding.gameObject.activeInHierarchy) == 0;
    }
    
    private static bool VisibleBindingCheck(string type)
    {
        if (!ActiveVisibleBindings.TryGetValue(type, out var list) || list.Count == 0) return true;

        list.RemoveAll(binding => !binding);

        return list.Count(binding => binding.active && binding.gameObject.activeInHierarchy) == 0;
    }

    public static (string, Sprite)? GetActiveCrestBinding()
    {
        var max = 0;
        
        (string, Sprite)? activeBinding = null;
        
        foreach (var bind in CrestBindings)
        {
            if (!ActiveBindings.TryGetValue(bind.Item1, out var active)) continue;
            active.RemoveAll(binding => !binding);
            
            var count = active.Count(binding => binding.active && binding.gameObject.activeInHierarchy);

            if (count > max)
            {
                max = count;
                activeBinding = bind;
            }
        }

        return activeBinding;
    }
    #endregion

    #region UI
    private static void SetupVisuals()
    {
        _canvasObj = new GameObject("[Architect] Ability Canvas");
        _canvasObj.SetActive(false);
        Object.DontDestroyOnLoad(_canvasObj);

        _canvasObj.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = _canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        
        _canvasObj.AddComponent<GraphicRaycaster>();

        for (var i = 0; i <= Bindings.Count; i++)
        {
            BindingIcons.Add(UIUtils.MakeImage($"Binding Icon {i}", _canvasObj, new Vector2(26 * i + 22, 22),
                Vector2.zero, Vector2.zero, new Vector2(45, 45)));
        }

        for (var i = 0; i < Crystals.Count/3; i++)
        {
            var img = UIUtils.MakeImage($"Crystal Icon {i}", _canvasObj, new Vector2(18 * i + 22, -22),
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(45, 45));
            img.preserveAspect = true;
            CrystalIcons.Add(img);
        }
        
        RefreshBindingUI();
        RefreshCrystalUI();
    }
    
    public static void RefreshBindingUI()
    {
        var i = 0;
        foreach (var bind in Bindings
                     .Where(bind => !VisibleBindingCheck(bind.Item1)))
        {
            BindingIcons[i].sprite = bind.Item2;
            i++;
        }

        var crestBinding = GetActiveCrestBinding();
        if (crestBinding.HasValue)
        {
            if (!VisibleBindingCheck(crestBinding.Value.Item1))
            {
                BindingIcons[i].sprite = crestBinding.Value.Item2;
                i++;
            }
        }

        for (; i <= Bindings.Count; i++) BindingIcons[i].sprite = ArchitectPlugin.BlankSprite;
    }

    public static void RefreshCrystalUI()
    {
        var i = 0;
        foreach (var bind in Crystals
                     .Where(bind => ActiveCrystals.GetValueOrDefault(bind.Item1, 0) == bind.Item2))
        {
            CrystalIcons[i].sprite = bind.Item3;
            i++;
        }

        for (var k = i; k < Crystals.Count/3; k++) CrystalIcons[k].sprite = ArchitectPlugin.BlankSprite;
    }
    #endregion

    #region Hooks
    private static void SetupBindingHooks()
    {
        PlayerDataVariableEvents.OnGetBool += (pd, name, current) => 
            name == nameof(pd.hasDash) ? BindingCheck(current, "dash") : current;
        typeof(HeroController).Hook(nameof(HeroController.CanDash),
            (Func<HeroController, bool> orig, HeroController self) => BindingCheck(orig(self), "dash")
                                                                      || (ActiveCrystals.GetValueOrDefault("dash", 0) > 0
                                                                          && InputHandler.Instance.inputActions
                                                                              .Dash.WasPressed));
        
        typeof(HeroController).Hook(nameof(HeroController.CanHarpoonDash),
            (Func<HeroController, bool> orig, HeroController self) => BindingCheck(orig(self), "harpoon")
                                                                      || ActiveCrystals.GetValueOrDefault("harpoon", 0) > 0);
        
        typeof(HeroController).Hook(nameof(HeroController.IsFacingNearSlideableWall),
            (Func<HeroController, bool> orig, HeroController self) => BindingCheck(orig(self), "wall_jump"));
        
        typeof(HeroController).Hook(nameof(HeroController.CanJump),
            (Func<HeroController, bool> orig, HeroController self) => BindingCheck(orig(self), "jump"));
        
        typeof(HeroController).Hook(nameof(HeroController.CanPlayNeedolin),
            (Func<HeroController, bool> orig, HeroController self) => BindingCheck(orig(self), "needolin"));
        
        typeof(HeroController).Hook(nameof(HeroController.CanSuperJump),
            (Func<HeroController, bool> orig, HeroController self) => BindingCheck(orig(self), "super_jump"));
        
        typeof(HeroController).Hook(nameof(HeroController.CanAttack),
            (Func<HeroController, bool> orig, HeroController self) => BindingCheck(orig(self), "attack"));
        
        typeof(HeroController).Hook(nameof(HeroController.CanDownAttack),
            (Func<HeroController, bool> orig, HeroController self) => BindingCheck(orig(self), "attack"));
        
        typeof(HeroController).Hook(nameof(HeroController.CanNailCharge),
            (Func<HeroController, bool> orig, HeroController self) => BindingCheck(orig(self), "attack"));
        
        typeof(HeroController).Hook(nameof(HeroController.Attack),
            (Action<HeroController, AttackDirection> orig, HeroController self, AttackDirection dir) =>
            {
                if (!BindingCheck(true, "attack")) return;
                orig(self, dir);
            });

        _ = new Hook(typeof(PlayerData).GetProperty(nameof(PlayerData.nailDamage))!.GetGetMethod(),
            (Func<PlayerData, int> orig, PlayerData self) => 
                BindingCheck(true, "needle") ? orig(self) : 5);
        
        _ = new Hook(typeof(PlayerData).GetProperty(nameof(PlayerData.CurrentSilkRegenMax))!.GetGetMethod(),
            (Func<PlayerData, int> orig, PlayerData self) => 
                BindingCheck(true, "silk_heart") ? orig(self) : 0);
        
        typeof(HeroController).Hook("CanWallJump",
            (Func<HeroController, bool, bool> orig, HeroController self, bool checkControlState)
                => BindingCheck(orig(self, checkControlState), "wall_jump"));
        
        typeof(HeroController).Hook(nameof(HeroController.CanDoubleJump),
            (Func<HeroController, bool, bool> orig, HeroController self, bool checkControlState)
                => BindingCheck(orig(self, checkControlState), "double_jump")
                   || (ActiveCrystals.GetValueOrDefault("wings", 0) > 0 && 
                       InputHandler.Instance.inputActions.Jump.WasPressed));
        
        typeof(HeroController).Hook(nameof(HeroController.SetStartWithBrolly),
            (Action<HeroController> orig, HeroController self) =>
            {
                if (!BindingCheck(true, "drift")) return;
                orig(self);
            });
        
        typeof(HeroController).Hook(nameof(HeroController.SetStartWithDoubleJump),
            (Action<HeroController> orig, HeroController self) =>
            {
                if (!BindingCheck(true, "double_jump")
                       || (ActiveCrystals.GetValueOrDefault("wings", 0) > 0 &&
                           InputHandler.Instance.inputActions.Jump.WasPressed)) return;
                orig(self);
            });
        
        typeof(HeroController).Hook("CanFloat",
            (Func<HeroController, bool, bool> orig, HeroController self, bool checkControlState)
                => BindingCheck(orig(self, checkControlState), "drift"));

        _ = new ILHook(typeof(HeroController).GetMethod("TickFrostEffect",
            BindingFlags.NonPublic | BindingFlags.Instance), il =>
        {
            var cursor = new ILCursor(il);
            if (!cursor.TryGotoNext(
                    MoveType.After,
                    instr => instr.MatchStloc(1)
                )) return;

            // New check
            cursor.Emit(OpCodes.Call, typeof(AbilityObjects).GetMethod(nameof(ShouldDisableFreezing)));
            cursor.Emit(OpCodes.Stloc_1);
        });
        
        /*
        ToolItemManager.SetExtraEquippedTool();
        ToolItemManager.SetEquippedTools(PlayerData.instance.CurrentCrestID, []);
        typeof(ToolItemManager).Hook(nameof(ToolItemManager.GetCurrentEquippedTools),
            (Func<List<ToolItem>> orig) =>
            {
                if (!BindingCheck(true, "tools")) return [];
                return orig();
            });*/
    }

    public static bool ShouldDisableFreezing()
    {
        return BindingCheck(PlayerData.instance.hasDoubleJump, "frost");
    }
    
    private static void SetupCrystalHooks()
    {
        HookUtils.OnHeroAwake += self =>
        {
            self.OnDoubleJumped += () =>
            {
                if (ActiveCrystals.ContainsKey("wings"))
                {
                    ActiveCrystals["wings"] -= 1;
                    RefreshCrystalUI();
                }
            };
            self.OnHazardRespawn += () =>
            {
                ActiveCrystals.Clear();
                RefreshCrystalUI();
            };
        };

        typeof(HeroController).Hook("HeroDashPressed",
            (Action<HeroController> orig, HeroController self) =>
            {
                orig(self);
                if (InputHandler.Instance.inputActions.Dash.WasPressed && ActiveCrystals.ContainsKey("dash"))
                {
                    ActiveCrystals["dash"] -= 1;
                    RefreshCrystalUI();
                }
            });

        HookUtils.OnFsmAwake += fsm =>
        {
            switch (fsm.FsmName)
            {
                case "Harpoon Dash":
                {
                    var silk = fsm.FsmVariables.FindFsmInt("Current Silk");
                    fsm.GetState("Can Do?").AddAction(() =>
                    {
                        if (ActiveCrystals.GetValueOrDefault("harpoon", 0) > 0)
                        {
                            ActiveCrystals["harpoon"] -= 1;
                            silk.Value = 1;
                            RefreshCrystalUI();
                        }
                    }, 2);
                    break;
                }
                case "Sprint":
                    fsm.GetState("Dash Stab Dir").AddAction(() =>
                    {
                        if (!BindingCheck(true, "attack")) fsm.SendEvent("DO SPRINT SKID");
                    }, 0);
                    break;
            }
        };
    }
    #endregion
}