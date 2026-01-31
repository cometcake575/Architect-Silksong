using System.Collections.Generic;

namespace Architect.Objects.Groups;

public static class BroadcasterGroup
{
    public static readonly List<string> TriggerZone = ["ZoneEnter", "ZoneExit"];
    
    public static readonly List<string> Damageable = ["OnDamage"];
    
    public static readonly List<string> Exploding = ["OnExplode"];
    
    public static readonly List<string> Enemies = ["OnDeath", "FirstDeath", "LoadedDead", "OnDamage"];
    
    public static readonly List<string> Shardillard = GroupUtils.Merge(Enemies, ["OnBounce"]);
    
    public static readonly List<string> Judge = GroupUtils.Merge(Enemies, ["OnBlock"]);
    
    public static readonly List<string> Stilkin = GroupUtils.Merge(Enemies, ["OnAmbush"]);
    
    public static readonly List<string> Snitchfly = GroupUtils.Merge(Enemies, ["OnFlee"]);
    
    public static readonly List<string> Bosses = ["OnRoar", "OnDeath", "FirstDeath", "LoadedDead", "OnDamage"];
    
    public static readonly List<string> SkullTyrant = GroupUtils.Merge(Bosses, ["Stomp"]);
    
    public static readonly List<string> Karmelita = GroupUtils.Merge(Bosses, ["OnStun", "OnRecover"]);
    
    public static readonly List<string> Lugoli = GroupUtils.Merge(Bosses, ["OnStompLand", "OnButtLand", "OnLadleSlam"]);

    public static readonly List<string> BlackThreader = ["OnAttack"];
    
    public static readonly List<string> SlamEnemies = GroupUtils.Merge(Enemies, ["Slam"]);
    
    public static readonly List<string> SlamBosses = GroupUtils.Merge(Bosses, ["Slam"]);
    
    public static readonly List<string> SummonerBosses = GroupUtils.Merge(Bosses, ["TrySummon"]);

    public static readonly List<string> SavageBeastfly = GroupUtils.Merge(SummonerBosses, ["WallSlam", "FloorSlam"]);
    
    public static readonly List<string> Groal = GroupUtils.Merge(SummonerBosses, ["TrySpikeTrap"]);
    
    public static readonly List<string> Activatable = ["OnActivate"];
    
    public static readonly List<string> Finishable = ["OnFinish"];
    
    public static readonly List<string> Toll = ["OnPay"];
    
    public static readonly List<string> Breakable = ["OnBreak"];
    
    public static readonly List<string> BreakableWall = ["OnBreak", "FirstBreak", "LoadedBroken"];
    
    public static readonly List<string> ActiveDeactivatable = ["OnActivate", "OnDeactivate"];
    
    public static readonly List<string> Levers = ["OnPull", "FirstPull", "LoadedPulled"];
    
    public static readonly List<string> Buttons = ["OnPress", "FirstPress", "LoadedPressed"];
    
    public static readonly List<string> Bindings = ["OnBind", "OnUnbind"];
    
    public static readonly List<string> Binoculars = ["OnStart", "OnStop"];
    
    public static readonly List<string> Callable = ["OnCall"];
    
    public static readonly List<string> Benches = ["OnSit", "OnLeave", "OnSpawnAt"];
    
    public static readonly List<string> Item = ["BeforePickup"];
    
    public static readonly List<string> MapperRing = ["InAir", "OnHit", "OnLand", "OnCollide"];
    
    public static readonly List<string> Hittable = ["OnHit"];
    
    public static readonly List<string> KeyListener = ["KeyPressed", "KeyReleased"];
    
    public static readonly List<string> AbilityCrystal = ["OnCollect", "OnRegen"];
    
    public static readonly List<string> HarpoonRings = ["OnGrab", "OnRelease"];
    
    public static readonly List<string> Choice = ["Yes", "No"];
    
    public static readonly List<string> ObjectAnchor = ["OnReverse", "OnTrackEnd"];
    
    public static readonly List<string> TextDisplay = ["OnClose"];
    
    public static readonly List<string> Interaction = ["OnInteract"];
    
    public static readonly List<string> Fleas = ["OnSave"];
    
    public static readonly List<string> FleaCounter = ["OnGold", "OnSilver", "OnBronze", "OnWhite"];
    
    public static readonly List<string> PlayerHooks = [
        "FaceLeft",
        "FaceRight",
        "Jump",
        "WallJump",
        "DoubleJump",
        "Land",
        "HardLand",
        "Dash",
        "Attack",
        "OnHazardRespawn",
        "OnDamage",
        "OnDeath",
        "OnHeal",
        "NeedolinStart",
        "NeedolinStop"
    ];
}