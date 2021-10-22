using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarriorCombat : CombatantBasis
{
    public override void Attack()
    {
        // Apply damage to target's enemy script
        CombatantBasis cb = target.GetComponent<CombatantBasis>();

        int damageTotal = attack + 0; // Get modifier from card here

        string damageType = "none"; // Get damage type from card here

        cb.TakeDamage(damageTotal, damageType);
        Debug.Log(combatantName + " Attack");
    }

    public override void Block()
    {
        temporaryHitPoints += 0; // Get temporary hit points from card here

        // Increase defense multipler to 2X
        defenseMultiplier = 2f;
        Debug.Log(combatantName + " Block");
    }

    public override void Special()
    {
        // Do special Defend party mechanic
        Debug.Log(combatantName + " Special");
    }
}
