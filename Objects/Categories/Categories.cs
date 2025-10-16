using System.Collections.Generic;

namespace Architect.Objects.Categories;

public static class Categories
{
    public static readonly List<AbstractCategory> AllCategories = [];
    
    /** Miscellaneous objects like benches or items that give free silk */
    public static readonly Category Misc = RegisterCategory("Miscellaneous", 6);
    /** Things that do damage (except for enemies) */
    public static readonly Category Hazards = RegisterCategory("Hazards", 5);
    /** Usable objects like levers, doors etc. */
    public static readonly Category Interactable = RegisterCategory("Interactable", 4);
    /** Objects you can stand on */
    public static readonly Category Solids = RegisterCategory("Solids", 3);
    /** Objects useful for platforming */
    public static readonly Category Platforming = RegisterCategory("Platforming", 2);
    /** Enemies and bosses */
    public static readonly Category Enemies = RegisterCategory("Enemies", 1);
    public static readonly Category Utility = RegisterCategory("Utility", 9);
    /** Utility objects for things like trigger zones and object removers */
    /** Collectible objects */
    public static readonly Category Collectibles;// = RegisterCategory("Collectibles", 8);
    /** Objects relating to giving or limiting abilities */
    public static readonly Category Abilities = RegisterCategory("Abilities", 7);
    /** Every object in a single category */
    public static readonly AbstractCategory All = RegisterCategory(AllCategory.Instance);
    
    public static void Init()
    {
        RegisterCategory(BlankCategory.Instance);
        RegisterCategory(PrefabsCategory.Instance);
        RegisterCategory(FavouritesCategory.Instance);
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