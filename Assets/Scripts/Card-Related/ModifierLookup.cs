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
            //{ Resources.Load<Sprite>(path + "amogus"), "fire" },
            /*{ Resources.Load<Sprite>(path + "amogus"), "water" },
            { Resources.Load<Sprite>(path + "amogus"), "earth" },
            { Resources.Load<Sprite>(path + "amogus"), "air" },
            { Resources.Load<Sprite>(path + "amogus"), "light" },
            { Resources.Load<Sprite>(path + "amogus"), "dark" },
            { Resources.Load<Sprite>(path + "amogus"), "plus2" },
            { Resources.Load<Sprite>(path + "amogus"), "mana2" },
            { Resources.Load<Sprite>(path + "amogus"), "prio" },
            { Resources.Load<Sprite>(path + "amogus"), "adj" },
            { Resources.Load<Sprite>(path + "amogus"), "all" }*/
        };
    }
}
