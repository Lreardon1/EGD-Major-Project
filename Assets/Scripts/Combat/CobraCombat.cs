using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CobraCombat : CombatantBasis
{
    public override void Special()
    {
        if(nextActionPrimaryElems[0] != Card.Element.None)
        {
            nextActionPrimaryElems.Add(Card.Element.Dark);
        }
        Attack();

        Debug.Log(combatantName + " Special");
    }
}
