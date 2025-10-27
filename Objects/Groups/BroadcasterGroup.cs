using System.Collections.Generic;

namespace Architect.Objects.Groups;

public static class BroadcasterGroup
{
    public static readonly List<string> TriggerZone = ["ZoneEnter", "ZoneExit"];
    
    public static readonly List<string> Enemies = ["OnDamage", "OnDeath"];
    
    public static readonly List<string> Activatable = ["OnActivate"];
    
    public static readonly List<string> ActiveDeactivatable = ["OnActivate", "OnDeactivate"];
    
    public static readonly List<string> Levers = ["OnPull", "FirstPull", "LoadedPulled"];
    
    public static readonly List<string> Buttons = ["OnPress", "FirstPress", "LoadedPressed"];
    
    public static readonly List<string> Bindings = ["OnBind", "OnUnbind"];
    
    public static readonly List<string> Binoculars = ["OnStart", "OnStop"];
    
    public static readonly List<string> Callable = ["OnCall"];
    
    public static readonly List<string> MapperRing = ["InAir", "OnHit", "OnLand"];
    
    public static readonly List<string> Hittable = ["OnHit"];
    
    public static readonly List<string> KeyListener = ["KeyPressed", "KeyReleased"];
    
    public static readonly List<string> AbilityCrystal = ["OnCollect", "OnRegen"];
    
    public static readonly List<string> HarpoonRings = ["OnGrab", "OnRelease"];
    
    public static readonly List<string> Choice = ["Yes", "No"];
    
    public static readonly List<string> ObjectAnchor = ["OnReverse"];
    
    public static readonly List<string> TextDisplay = ["OnClose"];
    
    public static readonly List<string> Interaction = ["OnInteract"];
    
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