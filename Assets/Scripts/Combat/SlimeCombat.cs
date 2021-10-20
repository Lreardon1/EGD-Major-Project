using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeCombat : CombatantBasis
{
    public override void Attack()
    {
        // Apply damage to target's enemy script
        Debug.Log("Slime Attack");
    }

    public override void Block()
    {
        // Increase defense multipler to 2X
        Debug.Log("Slime Block");
    }

    public override void Special()
    {
        // Do special random effect
        Debug.Log("Slime Special");
    }
}
