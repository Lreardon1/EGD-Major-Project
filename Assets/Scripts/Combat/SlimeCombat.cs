using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeCombat : CombatantBasis
{
    public int slimeHealAmount = 4;

    public override void Special()
    {
        Heal(slimeHealAmount);
        Debug.Log(combatantName + " Special");
    }
}
