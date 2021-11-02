using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriestCombat : CombatantBasis
{
    public int specialHealAmount = 10;

    public override void Special()
    {
        CombatManager cm = FindObjectOfType<CombatManager>();
        foreach(GameObject activeMember in cm.activePartyMembers)
        {
            activeMember.GetComponent<CombatantBasis>().Heal(specialHealAmount);
        }
        Debug.Log(combatantName + " Special Heal");
    }
}
