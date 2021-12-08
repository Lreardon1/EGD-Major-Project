using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CobraCombat : CombatantBasis
{
    public override void Special()
    {
        MakePopup("Using Special Dark Strike", null, Color.white);
        
        nextActionPrimaryElems.Add(Card.Element.Dark);
        
        Attack();

        Debug.Log(combatantName + " Special");
    }
}
