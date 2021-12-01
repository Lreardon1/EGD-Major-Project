using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ModifierLookup
{
    private static string path = "CardAssets/";
    public static Dictionary<Modifier.ModifierEnum, Modifier> modifierLookupTable;
    public static Dictionary<Sprite, string> spriteConversionTable;
    public static Dictionary<Sprite, string> titleLookup;
    public static Dictionary<Sprite, string> descLookup;
    public static Dictionary<string, Sprite> stringToSpriteConversionTable;

    public static void LoadModifierTable() {
        //DEFINE ALL SEARCHABLE MODIFIERS HERE
        //formatting is string to search by, then matching sprite for CardRenderer followed by editable type int (0 for number, 1 for draggable) in a KeyValuePair
        modifierLookupTable = new Dictionary<Modifier.ModifierEnum, Modifier>()
        {
            { Modifier.ModifierEnum.NumModifier, new Modifier(Modifier.ModifierEnum.NumModifier, Resources.Load<Sprite>(path + "NumEditor"), null) },
            { Modifier.ModifierEnum.SecondaryElement, new Modifier(Modifier.ModifierEnum.SecondaryElement, Resources.Load<Sprite>(path + "ElementIcon"), null) },
            { Modifier.ModifierEnum.Utility, new Modifier(Modifier.ModifierEnum.Utility, Resources.Load<Sprite>(path + "Utility"), null) }
        };

        spriteConversionTable = new Dictionary<Sprite, string>()
        {
            { Resources.Load<Sprite>(path + "fire"), "fire" },
            { Resources.Load<Sprite>(path + "water"), "water" },
            { Resources.Load<Sprite>(path + "earth"), "earth" },
            { Resources.Load<Sprite>(path + "air"), "air" },
            { Resources.Load<Sprite>(path + "light"), "light" },
            { Resources.Load<Sprite>(path + "dark"), "dark" },
            { Resources.Load<Sprite>(path + "plus2"), "plus2" },
            { Resources.Load<Sprite>(path + "plus4"), "plus4" },
            { Resources.Load<Sprite>(path + "plus6"), "plus6" },
            { Resources.Load<Sprite>(path + "plus8"), "plus8" },
            { Resources.Load<Sprite>(path + "mana2"), "mana2" },
            { Resources.Load<Sprite>(path + "mana3"), "mana3" },
            { Resources.Load<Sprite>(path + "mana4"), "mana4" },
            { Resources.Load<Sprite>(path + "prio"), "prio" },
            { Resources.Load<Sprite>(path + "adj"), "adj" },
            { Resources.Load<Sprite>(path + "all"), "all" }
        };

        stringToSpriteConversionTable = new Dictionary<string, Sprite>()
        {
            { "fire", Resources.Load<Sprite>(path + "fire") },
            { "water", Resources.Load<Sprite>(path + "water") },
            { "earth", Resources.Load<Sprite>(path + "earth") },
            { "air", Resources.Load<Sprite>(path + "air") },
            { "light", Resources.Load<Sprite>(path + "light") },
            { "dark", Resources.Load<Sprite>(path + "dark") },
            { "plus2", Resources.Load<Sprite>(path + "plus2") },
            { "plus4", Resources.Load<Sprite>(path + "plus4") },
            { "plus6", Resources.Load<Sprite>(path + "plus6") },
            { "plus8", Resources.Load<Sprite>(path + "plus8") },
            { "mana2", Resources.Load<Sprite>(path + "mana2") },
            { "mana3", Resources.Load<Sprite>(path + "mana3") },
            { "mana4", Resources.Load<Sprite>(path + "mana4") },
            { "prio", Resources.Load<Sprite>(path + "prio") },
            { "adj", Resources.Load<Sprite>(path + "adj") },
            { "all", Resources.Load<Sprite>(path + "all") }
        };

        titleLookup = new Dictionary<Sprite, string>()
        {
            { Resources.Load<Sprite>(path + "fire"), "Fire Element" },
            { Resources.Load<Sprite>(path + "water"), "Water Element" },
            { Resources.Load<Sprite>(path + "earth"), "Earth Element" },
            { Resources.Load<Sprite>(path + "air"), "Air Element" },
            { Resources.Load<Sprite>(path + "light"), "Light Element" },
            { Resources.Load<Sprite>(path + "dark"), "Dark Element" },
            { Resources.Load<Sprite>(path + "plus2"), "+2" },
            { Resources.Load<Sprite>(path + "plus4"), "+4" },
            { Resources.Load<Sprite>(path + "plus6"), "+6" },
            { Resources.Load<Sprite>(path + "plus8"), "+8" },
            { Resources.Load<Sprite>(path + "mana2"), "Reduce Mana Cost" },
            { Resources.Load<Sprite>(path + "mana3"), "Reduce Mana Cost" },
            { Resources.Load<Sprite>(path + "mana4"), "Reduce Mana Cost" },
            { Resources.Load<Sprite>(path + "prio"), "Give Priority" },
            { Resources.Load<Sprite>(path + "adj"), "Target Adjacent Allies" },
            { Resources.Load<Sprite>(path + "all"), "Target All Allies" }
        };

        descLookup = new Dictionary<Sprite, string>()
        {
            { Resources.Load<Sprite>(path + "fire"), "fire" },
            { Resources.Load<Sprite>(path + "water"), "water" },
            { Resources.Load<Sprite>(path + "earth"), "earth" },
            { Resources.Load<Sprite>(path + "air"), "air" },
            { Resources.Load<Sprite>(path + "light"), "light" },
            { Resources.Load<Sprite>(path + "dark"), "dark" },
            { Resources.Load<Sprite>(path + "plus2"), "plus2" },
            { Resources.Load<Sprite>(path + "plus4"), "plus4" },
            { Resources.Load<Sprite>(path + "plus6"), "plus6" },
            { Resources.Load<Sprite>(path + "plus8"), "plus8" },
            { Resources.Load<Sprite>(path + "mana2"), "mana2" },
            { Resources.Load<Sprite>(path + "mana3"), "mana3" },
            { Resources.Load<Sprite>(path + "mana4"), "mana4" },
            { Resources.Load<Sprite>(path + "prio"), "prio" },
            { Resources.Load<Sprite>(path + "adj"), "adj" },
            { Resources.Load<Sprite>(path + "all"), "all" }
        };
    }
}
