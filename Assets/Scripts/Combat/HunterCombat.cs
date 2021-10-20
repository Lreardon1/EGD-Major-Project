using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HunterCombat : CombatantBasis
{
    public override void Attack()
    {
        // Apply damage to target's enemy script
        Debug.Log("Ranger Attack");
    }

    public override void Block()
    {
        // Increase defense multipler to 2X
        Debug.Log("Ranger Block");
    }

    public override void Special()
    {
        // Do special multi attack
        Debug.Log("Ranger Special");
    }
}