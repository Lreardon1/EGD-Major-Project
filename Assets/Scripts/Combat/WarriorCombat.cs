using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarriorCombat : CombatantBasis
{
    public override void TakeDamage(float damageAmount, Card.Element damageType1, Card.Element damageType2, GameObject attacker)
    {
        // Check for elemental combo
        float elementalComboMultiplier = 1f;

        statusScript.OnTakeDamageStatusHandler(statusCondition, attacker, (int)damageAmount);

        int shieldValue = temporaryHitPoints;

        currentHitPoints -= (int)((damageAmount * elementalComboMultiplier) / defenseMultiplier);
        Debug.Log(combatantName + " took " + damageAmount + " of " + damageType1 + " type and " + damageType2);

        //if damage shielded during attack
        if (shieldValue > 0 && shieldReturnDmg > 0)
        {
            //return damage
            attacker.GetComponent<CombatantBasis>().TakeDamage(shieldReturnDmg, Card.Element.None, Card.Element.None, gameObject);
            //lose return damage on shield loss
            if (temporaryHitPoints <= 0)
            {
                shieldReturnDmg = 0;
            }
        }

        Status newStatus = statusScript.GetStatusResult(damageType1, damageType2);
        if (newStatus != Status.None)
        {
            statusScript.ApplyNewStatus(newStatus, attacker);
        }


        if (!CheckIsSlain()) //check counterattack if survived hit
        {
            if (canCounterAttack)
            {
                if(previousAction == Action.Special)
                {
                    attacker.GetComponent<CombatantBasis>().TakeDamage((damageAmount + attackCardBonus)  * attackMultiplier, damageType1, damageType2, gameObject);
                    print("Special Counter attack");
                } else
                {
                    attacker.GetComponent<CombatantBasis>().TakeDamage(attackCardBonus * attackMultiplier, damageType1, damageType2, gameObject);
                    print("Counter attack");
                }
            }
        }
    }

    public override void Special()
    {
        Block();
        canCounterAttack = true;
        // Do special Defend party mechanic
        Debug.Log(combatantName + " Special Counter");
    }
}
