using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeCombat : CombatantBasis
{
    //public override void Attack()
    //{
    //    // Apply damage to target's enemy script
    //    CombatantBasis cb = target.GetComponent<CombatantBasis>();

    //    int damageTotal = attack - 0; // Get modifier from card here (apply negative)

    //    cb.TakeDamage(damageTotal, damageType);
    //    Debug.Log(combatantName + " Attack");
    //}

    //public override void Block()
    //{
    //    negativeHitPointShield += 0; // Get temporary hit points from card here

    //    // Increase defense multipler to 2X
    //    defenseMultiplier = 2f;
    //    Debug.Log(combatantName + " Block");
    //}

    public override void Special()
    {
        // Do special random effect
        Debug.Log(combatantName + " Special");
    }
}
