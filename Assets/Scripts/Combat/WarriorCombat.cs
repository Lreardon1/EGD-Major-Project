using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarriorCombat : CombatantBasis
{
    public override void Attack()
    {
        // Apply damage to target's enemy script
        Debug.Log("Warrior Attack");
    }

    public override void Block()
    {
        // Increase defense multipler to 2X
        Debug.Log("Warrior Block");
    }

    public override void Special()
    {
        // Do special Defend party mechanic
        Debug.Log("Warrior Special");
    }
}
