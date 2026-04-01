using System;
using System.Collections.Generic;
using System.Linq;
using Architect.Editor;
using Architect.Sharer;
using Architect.Storage;
using Architect.Utils;
using BepInEx;
using GlobalSettings;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Architect.Workshop.Items;

public class CustomCrest : SpriteItem
{
    private static readonly Dictionary<string, CustomCrest> Crests = [];

    public static void Init()
    {
        typeof(HeroController).Hook(nameof(HeroController.Start),
            (Action<HeroController> orig, HeroController self) =>
            {
                foreach (var crest in Crests.Values.ToArray())
                {
                    crest.Unregister();
                    crest.Register();
                }
                orig(self);
            });
        
        HookUtils.OnHeroAwake += controller =>
        {
            var fsm = controller.sprintFSM;
            fsm.fsmTemplate = null;
            
            fsm.GetState("Start Attack").AddAction(() =>
            {
                if (controller.CurrentConfigGroup is not CustomConfigGroup ccg) return;
                fsm.SendEvent(ccg.InheritsDashFrom.name switch
                {
                    "Reaper" => "REAPER",
                    "Wanderer" => "WANDERER",
                    "Warrior" => "WARRIOR",
                    "Spell" => "SHAMAN",
                    "Toolmaster" => "TOOLMASTER",
                    _ => string.Empty
                });
            }, 6);
        };
    }
    
    private ToolCrest _crest;
    public LocalStr Name = string.Empty;
    public LocalStr Desc = string.Empty;
    public LocalStr NamePrefix = string.Empty;
    public LocalStr EquipText = string.Empty;

    private readonly List<CrestSlot> _slots = [];
    
    public string HIconUrl = string.Empty;
    public bool HPoint;
    public float HPpu = 100;
    
    public string GIconUrl = string.Empty;
    public bool GPoint;
    public float GPpu = 100;
    
    public override (string, string)[] FilesToDownload => [
        (IconUrl, "png"),
        (HIconUrl, "png"),
        (GIconUrl, "png")
    ];

    public string Movement = "Hunter";
    
    public override void Register()
    {
        Crests[Id] = this;
        if (!HeroController.instance) return;
        
        _crest = ScriptableObject.CreateInstance<ToolCrest>();

        var (original, originalCg, originalCrest) = GetConfig(Movement);
        var hc = Object.Instantiate(original);

        var cg = new CustomConfigGroup
        {
            InheritsDashFrom = originalCrest,
            Config = hc,
            ActiveRoot = originalCg.ActiveRoot,
            NormalSlashObject = originalCg.NormalSlashObject,
            AlternateSlashObject = originalCg.AlternateSlashObject,
            WallSlashObject = originalCg.WallSlashObject,
            UpSlashObject = originalCg.UpSlashObject,
            AltUpSlashObject = originalCg.AltUpSlashObject,
            DownSlashObject = originalCg.DownSlashObject,
            AltDownSlashObject = originalCg.AltDownSlashObject,
            DashStab = originalCg.DashStab,
            DashStabAlt = originalCg.DashStabAlt,
            ChargeSlash = originalCg.ChargeSlash,
            TauntSlash = originalCg.TauntSlash
        };
        cg.Setup();
        var cfgs = HeroController.instance.configs.ToList();
        cfgs.Add(cg);
        HeroController.instance.configs = cfgs.ToArray();
        
        _crest.heroConfig = hc;
        
        _crest.name = Id;
        _crest.displayName = Name;
        _crest.description = Desc;
        _crest.getPromptDesc = Desc;
        _crest.itemNamePrefix = NamePrefix;
        _crest.equipText = EquipText;
        
        _crest.slots = _slots.Select(c =>
        {
            var up = _slots.FirstPosMin(s => s.Pos.y - c.Pos.y, Dist);
            var down = _slots.FirstPosMin(s => c.Pos.y - s.Pos.y, Dist);
            var left = _slots.FirstPosMin(s => c.Pos.x - s.Pos.x, Dist);
            var right = _slots.FirstPosMin(s => s.Pos.x - c.Pos.x, Dist);
            
            return new ToolCrest.SlotInfo
            {
                Position = c.Pos,
                Type = c.ToolType,
                AttackBinding = c.Binding,
                NavUpIndex = up,
                NavDownIndex = down,
                NavLeftIndex = left,
                NavRightIndex = right,
                NavUpFallbackIndex = up,
                NavDownFallbackIndex = down,
                NavLeftFallbackIndex = left,
                NavRightFallbackIndex = right,
                IsLocked = c.Lock
            };

            float Dist(CrestSlot s) => Vector2.Distance(s.Pos, c.Pos);
        }).ToArray();
        
        ToolItemManager.Instance.crestList.Add(_crest);
        WorkshopManager.CustomCrests.Add(Id, this);
        
        base.Register();
        RefreshHSprite();
        RefreshGSprite();

        if (PlayerData.instance.CurrentCrestID == Id)
        {
            ToolItemManager.AutoEquip(_crest, false, false);
        }
    }
    
    private void RefreshHSprite()
    {
        if (HIconUrl.IsNullOrWhiteSpace()) return;
        CustomAssetManager.DoLoadSprite(HIconUrl, HPoint, HPpu, 1, 1, sprites =>
        {
            if (sprites.IsNullOrEmpty()) return;
            _crest.crestSilhouette = sprites[0];
        });
    }

    private void RefreshGSprite()
    {
        if (GIconUrl.IsNullOrWhiteSpace()) return;
        CustomAssetManager.DoLoadSprite(GIconUrl, GPoint, GPpu, 1, 1, sprites =>
        {
            if (sprites.IsNullOrEmpty()) return;
            _crest.crestGlow = sprites[0];
        });
    }

    private static InventoryToolCrestList List
    {
        get
        {
            if (field) return field;
            return field = Resources.FindObjectsOfTypeAll<InventoryToolCrestList>()
                .FirstOrDefault(c => c.crests.Count > 0);
        }
    }

    public override void Unregister()
    {
        ToolItemManager.Instance.crestList.Remove(_crest);
        WorkshopManager.CustomCrests.Remove(Id);
        if (List)
        {
            var c = List.crests.FirstOrDefault(c => c.CrestData == _crest);
            if (!c) return;
            List.crests.Remove(c);
            foreach (var slot in c.activeSlots) slot.gameObject.SetActive(false);
            c.crestSprite.gameObject.SetActive(false);
            c.crestGlowSprite.gameObject.SetActive(false);
            c.crestSilhouette.gameObject.SetActive(false);
            Object.Destroy(c);
        }
    }

    protected override void OnReadySprite()
    {
        _crest.crestSprite = Sprite;
    }

    public class CrestSlot : WorkshopItem
    {
        public string CrestId = string.Empty;
        public ToolItemType ToolType = ToolItemType.Red;
        public AttackToolBinding Binding = AttackToolBinding.Neutral;
        public Vector2 Pos;
        public bool Lock;
        
        public override void Register()
        {
            WorkshopUI.RefreshIcon(this);
            if (!Crests.TryGetValue(CrestId, out var crest)) return;
            
            crest._slots.Add(this);
            
            crest.Unregister();
            crest.Register();
        }

        public override void Unregister()
        {
            if (!Crests.TryGetValue(CrestId, out var crest)) return;
            
            crest._slots.Remove(this);
            
            crest.Unregister();
            crest.Register();
        }

        public override Sprite GetIcon()
        {
            return List ? List.crests.First().templateSlots[(int)ToolType].Sprite : SharerManager.Placeholder;
        }
    }

    private static (HeroControllerConfig, HeroController.ConfigGroup, ToolCrest) GetConfig(string state)
    {
        var hunter = HeroController.instance.configs.First(c => c.Config.name == "Default");
        var (s, crest) = state switch
        {
            "Reaper" => (Gameplay.ReaperCrest.heroConfig, Gameplay.ReaperCrest),
            "Wanderer" => (Gameplay.WandererCrest.heroConfig, Gameplay.WandererCrest),
            "Beast" => (Gameplay.WarriorCrest.heroConfig, Gameplay.WarriorCrest),
            "Cloakless" => (Gameplay.CloaklessCrest.heroConfig, Gameplay.CloaklessCrest),
            "Architect" => (Gameplay.ToolmasterCrest.heroConfig, Gameplay.ToolmasterCrest),
            "Shaman" => (Gameplay.SpellCrest.heroConfig, Gameplay.SpellCrest),
            "Cursed" => (Gameplay.CursedCrest.heroConfig, Gameplay.CursedCrest),
            "Witch" => (Gameplay.WitchCrest.heroConfig, Gameplay.WitchCrest),
            _ => (hunter.Config, Gameplay.HunterCrest)
        };
        return (s, HeroController.instance.configs.FirstOrDefault(c => c.Config == s) ?? hunter, crest);
    }

    public class CustomConfigGroup : HeroController.ConfigGroup
    {
        public ToolCrest InheritsDashFrom;
    }
}