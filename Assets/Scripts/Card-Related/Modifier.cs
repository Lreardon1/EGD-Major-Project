using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Modifier
{
    public enum ModifierEnum { None, NumModifier, SecondaryElement, Utility };

    public ModifierEnum name;
    public Sprite icon;
    public int intVal;
    public Sprite spriteVal;
    public string spriteParsing;

    public Modifier(ModifierEnum n, Sprite i, Sprite sVal)
    {
        name = n;
        icon = i;
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

    public void ActivateModifier(Card c)
    {
        if (spriteParsing == "fire")
        {
            c.secondaryElem = Card.Element.Fire;
        }
        else if (spriteParsing == "water")
        {
            c.secondaryElem = Card.Element.Water;
        }
        else if (spriteParsing == "earth")
        {
            c.secondaryElem = Card.Element.Earth;
        }
        else if (spriteParsing == "air")
        {
            c.secondaryElem = Card.Element.Air;
        }
        else if (spriteParsing == "light")
        {
            c.secondaryElem = Card.Element.Light;
        }
        else if (spriteParsing == "dark")
        {
            c.secondaryElem = Card.Element.Dark;
        }
        else if (spriteParsing == "plus2")
        {
            c.numMod = 2;
        }
        else if (spriteParsing == "plus4")
        {
            c.numMod = 4;
        }
        else if (spriteParsing == "plus6")
        {
            c.numMod = 6;
        }
        else if (spriteParsing == "plus8")
        {
            c.numMod = 8;
        }
        else if (spriteParsing == "mana2")
        {
            c.UpdateManaCost(c.manaCost - 2);
        }
        else if (spriteParsing == "mana3")
        {
            c.UpdateManaCost(c.manaCost - 3);
        }
        else if (spriteParsing == "mana4")
        {
            c.UpdateManaCost(c.manaCost - 4);
        }
        else if (spriteParsing == "prio")
        {
            c.givePrio = true;
        }
        else if (spriteParsing == "adj")
        {
            c.targetting = Card.AoE.Adjascent;
        }
        else if (spriteParsing == "all")
        {
            c.targetting = Card.AoE.All;
        }
    }

    public void DeactivateModifier(Card c)
    {
        if (name == ModifierEnum.SecondaryElement)
        {
            c.secondaryElem = Card.Element.None;
        }
        else if (name == ModifierEnum.NumModifier)
        {
            c.numMod = 0;
        }
        else //utility modifiers
        {
            if (spriteParsing == "mana2")
            {
                c.UpdateManaCost(c.manaCost + 2);
            }
            if (spriteParsing == "mana3")
            {
                c.UpdateManaCost(c.manaCost + 3);
            }
            if (spriteParsing == "mana4")
            {
                c.UpdateManaCost(c.manaCost + 4);
            }
            else if (spriteParsing == "prio")
            {
                c.givePrio = false;
            }
            else if (spriteParsing == "adj")
            {
                c.targetting = Card.AoE.Single;
            }
            else if (spriteParsing == "all")
            {
                c.targetting = Card.AoE.Single;
            }
        }
    }
}
