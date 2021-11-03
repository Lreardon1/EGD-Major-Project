using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HunterCombat : CombatantBasis
{
    public bool specialAttack = false;

    public override void Attack()
    {
        CombatantBasis cb = target.GetComponent<CombatantBasis>();

        float damageTotal = 0; 

        if(specialAttack) // Special attack does 1/3 regular attack damage to all enemies
        {
            damageTotal = (attack/3f + attackCardBonus) * attackMultiplier; // Get modifier from card here
        } else
        {
            damageTotal = (attack + attackCardBonus) * attackMultiplier; // Get modifier from card here
        }

        cb.TakeDamage(damageTotal, nextActionPrimaryElem, nextActionSecondaryElem, gameObject);

        Debug.Log("Hunter Attack");
    }

    public override void Special()
    {
        CombatManager cm = FindObjectOfType<CombatManager>();
        specialAttack = true;
        foreach(GameObject enemy in cm.activeEnemies)
        {
            target = enemy;
            Attack();
        }

        specialAttack = false;
        Debug.Log(combatantName + " Special Attack");
    }
}