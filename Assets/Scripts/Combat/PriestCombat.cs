using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriestCombat : CombatantBasis
{
    public override void Attack()
    {
        // Apply damage to target's enemy script
        Debug.Log("Mage Attack");
    }

    public override void Block()
    {
        // Increase defense multipler to 2X
        Debug.Log("Mage Block");
    }

    public override void Special()
    {
        // Do special random effect
        Debug.Log("Mage Special");
    }
}
