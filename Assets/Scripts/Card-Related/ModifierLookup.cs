using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ModifierLookup
{
    private static string path = "Testing/";
    public static Dictionary<Modifier.ModifierEnum, Modifier> modifierLookupTable;

    public static void LoadModifierTable() {
        //DEFINE ALL SEARCHABLE MODIFIERS HERE
        //formatting is string to search by, then matching sprite for CardRenderer followed by editable type int (0 for number, 1 for draggable) in a KeyValuePair
        modifierLookupTable = new Dictionary<Modifier.ModifierEnum, Modifier>()
        {
            { Modifier.ModifierEnum.Attack, new Modifier(Modifier.ModifierEnum.Attack, Resources.Load<Sprite>(path + "amogus"), 0, 0, null) },
            { Modifier.ModifierEnum.Defense, new Modifier(Modifier.ModifierEnum.Defense, Resources.Load<Sprite>(path + "amogus"), 0, 0, null) },
            { Modifier.ModifierEnum.StatusInfliction, new Modifier(Modifier.ModifierEnum.StatusInfliction, Resources.Load<Sprite>(path + "amogus"), 1, 0, null) },
            { Modifier.ModifierEnum.Priority, new Modifier(Modifier.ModifierEnum.Priority, Resources.Load<Sprite>(path + "amogus"), 1, 0, null) }
        };
    }
}
