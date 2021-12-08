using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolemHandCombatController : CombatantBasis
{
    public int groundHitDamage = 4;

    public override void Special()
    {
        MakePopup("Using Special Earthern Shake", null, Color.white);

        CombatManager cm = FindObjectOfType<CombatManager>();
        List<GameObject> adjacentTargets = cm.GetAdjacentCombatants(target);
        
        nextActionPrimaryElems.Add(Card.Element.Earth);

        Attack();
        foreach (GameObject member in adjacentTargets)
        {
            if (member.GetComponent<CombatantBasis>().isSlain)
                continue;
            if (member.GetComponent<CombatantBasis>().untargettable)
                continue;
            target = member;
            Attack();
        }
    }
}
