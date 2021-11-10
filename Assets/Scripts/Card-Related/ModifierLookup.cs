using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ModifierLookup
{
    private static string path = "CardAssets/";
    public static Dictionary<Modifier.ModifierEnum, Modifier> modifierLookupTable;
    public static Dictionary<Sprite, string> spriteConversionTable;

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
    }
}
