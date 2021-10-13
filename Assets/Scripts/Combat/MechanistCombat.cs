using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechanistCombat : PartyMember
{
    public override void Attack()
    {
        // Apply damage to target's enemy script
        Debug.Log("Mechanist Attack");
    }

    public override void Block()
    {
        // Increase defense multipler to 2X
        Debug.Log("Mechanist Block");
    }

    public override void Special()
    {
        // Do special random effect
        Debug.Log("Mechanist Special");
    }
}
