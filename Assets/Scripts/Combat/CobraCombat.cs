using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CobraCombat : CombatantBasis
{
    public override void Special()
    {
        if(nextActionPrimaryElem != Card.Element.None)
        {
            nextActionPrimaryElem = Card.Element.Dark;
        }
        Attack();

        Debug.Log(combatantName + " Special");
    }
}
