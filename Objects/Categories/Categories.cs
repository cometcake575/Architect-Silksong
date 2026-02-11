using System.Collections.Generic;

namespace Architect.Objects.Categories;

public static class Categories
{
    public static readonly List<AbstractCategory> AllCategories = [];
    
    /** Miscellaneous objects like benches or items that give free silk */
    public static readonly Category Misc = RegisterCategory("Miscellaneous", 9);
    /** NPCs that can be talked to */
    public static readonly Category Npcs = RegisterCategory("Npcs", 7);
    /** Effects such as particles */
    public static readonly Category Effects = RegisterCategory("Effects", 8);
    /** Hazards taken from enemy attacks */
    public static readonly Category Attacks = RegisterCategory("Attacks", 6);
    /** Things that do damage */
    public static readonly Category Hazards = RegisterCategory("Hazards", 5);
    /** Usable objects like levers, doors etc. */
    public static readonly Category Interactable = RegisterCategory("Interactable", 4);
    /** Objects you can stand on */
    public static readonly Category Solids = RegisterCategory("Solids", 3);
    /** Objects useful for platforming */
    public static readonly Category Platforming = RegisterCategory("Platforming", 2);
    /** Enemies and bosses */
    public static readonly Category Enemies = RegisterCategory("Enemies", 1);
    /** Utility objects for things like trigger zones and object removers */
    public static readonly Category Utility = RegisterCategory("Utility", 10);
    /** Legacy objects that must be enabled in the configuration */
    public static readonly Category Legacy = new("Legacy", -1);
    /** Objects relating to giving or limiting abilities */
    public static readonly Category Abilities = RegisterCategory("Abilities", 9);
    /** Every object in a single category */
    public static readonly AbstractCategory All = RegisterCategory(AllCategory.Instance);
    
    public static void Init()
    {
        RegisterCategory(BlankCategory.Instance);
        RegisterCategory(SavedCategory.Instance);
        RegisterCategory(FavouritesCategory.Instance);
        
        RegisterCategory(BlankCategory.Instance);
        RegisterCategory(Legacy);
    }
    
    public static Category RegisterCategory(string name, int priority = 0)
    {
        var category = new Category(name, priority);
        RegisterCategory(category);
        return category;
    }
    
    public static AbstractCategory RegisterCategory(AbstractCategory category) {
        AllCategories.Add(category);
        return category;
    }
}