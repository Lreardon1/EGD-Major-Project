using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Modifier
{
    public enum ModifierEnum { Attack, Defense, StatusInfliction, Priority };

    public ModifierEnum name;
    public Sprite icon;
    public int type;
    public int intVal;
    public Sprite spriteVal;

    public Modifier(ModifierEnum n, Sprite i, int t, int iVal, Sprite sVal)
    {
        name = n;
        icon = i;
        type = t;
        intVal = iVal;
        spriteVal = sVal;
    }
}
