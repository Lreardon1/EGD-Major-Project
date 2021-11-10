using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriestCombat : CombatantBasis
{
    public int specialHealAmount = 10;

    public override void SelectTarget(List<GameObject> targets) //TODO:: STILL NEEDS TO HANDLE RETARGETTING IF RANDOMLY CHOOSING UNTARGETTABLE COMBATANT
    {
        if (nextAction == Action.Block)
        {
            target = null;
            return;
        }

        int highestHP = 0;
        GameObject highestHPTarget = targets[0];

        foreach (GameObject enemy in targets)
        {
            CombatantBasis cb = enemy.GetComponent<CombatantBasis>();
            if (!cb.untargettable && cb.currentHitPoints > highestHP)
            {
                highestHP = cb.currentHitPoints;
                target = enemy;
            }
        }

        if (nextAction == Action.Attack)
        {
            text.text = "Attack " + target.GetComponent<CombatantBasis>().combatantName;
        }
        else if (nextAction == Action.Special)
        {
            text.text = "Special " + target.GetComponent<CombatantBasis>().combatantName;
        }
        Debug.Log("Target Selected");
        lr.positionCount = 2;
        lr.SetPosition(0, targetLineStart.position);
        lr.SetPosition(1, target.transform.position);
        lr.enabled = true;
    }

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
