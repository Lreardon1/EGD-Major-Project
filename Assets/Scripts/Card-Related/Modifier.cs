using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Modifier
{
    public enum ModifierEnum { None, Attack, Defense, SecondaryElement, Priority };

    public ModifierEnum name;
    public Sprite icon;
    public int type;
    public int intVal;
    public Sprite spriteVal;
    public string spriteParsing;

    public Modifier(ModifierEnum n, Sprite i, int t, int iVal, Sprite sVal)
    {
        name = n;
        icon = i;
        type = t;
        intVal = iVal;
        spriteVal = sVal;

        parseSpriteVal();
    }

    public void setSpriteMod(Sprite s)
    {
        spriteVal = s;
        parseSpriteVal();
    }

    private void parseSpriteVal()
    {
        //if statements to set the sprite parsing string for easier parsing to determine card effect
        if (spriteVal == null)
        {
            spriteParsing = "";
        }
        else if (ModifierLookup.spriteConversionTable.ContainsKey(spriteVal))
        {
            spriteParsing = ModifierLookup.spriteConversionTable[spriteVal];
        }
        else
        {
            spriteParsing = "UNIDENTIFIED";
        }
    }
}
