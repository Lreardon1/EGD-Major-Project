using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarriorCombat : CombatantBasis
{
    public override void TakeDamage(float damageAmount, Card.Element damageType1, Card.Element damageType2, GameObject attacker)
    {
        statusScript.OnTakeDamageStatusHandler(statusCondition, attacker, (int)damageAmount);

        int shieldValue = temporaryHitPoints;

        int totalDamageAmount = (int)((damageAmount) / defenseMultiplier);

        shieldValue -= (int)((damageAmount) / defenseMultiplier);
        currentHitPoints += Mathf.Clamp(shieldValue, -10000, 0);
        Debug.Log(combatantName + " took " + totalDamageAmount + " of " + damageType1 + " type and " + damageType2);

        // visuals, TODO : make a string construction system to color elements differently?
        MakePopup("<color=\"red\"> Took " + totalDamageAmount + "</color>", null, Color.white);

        //audio
        audioSource.PlayOneShot((AudioClip)Resources.Load("Sound/SFX/Hit_Flash", typeof(AudioClip)), 0.7f);


        //if damage shielded during attack
        if (shieldValue > 0 && shieldReturnDmg > 0)
        {
            //return damage
            attacker.GetComponent<CombatantBasis>().TakeDamage(shieldReturnDmg, Card.Element.None, Card.Element.None, gameObject);
            //lose return damage and resistance on shield loss
            if (temporaryHitPoints <= 0)
            {
                shieldReturnDmg = 0;
                shieldResistance = 0;
            }
        }

        Status newStatus = statusScript.GetStatusResult(damageType1, damageType2);
        if (newStatus != Status.None)
        {
            statusScript.ApplyNewStatus(newStatus, attacker);
        }

        healthBar.SetHealth(currentHitPoints);
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

    public override void SelectTarget() //TODO:: STILL NEEDS TO HANDLE RETARGETTING IF RANDOMLY CHOOSING UNTARGETTABLE COMBATANT
    {
        CombatManager cm = FindObjectOfType<CombatManager>();
        List<GameObject> targets = new List<GameObject>();
        if (isEnemy)
            targets = cm.activePartyMembers;
        else
            targets = cm.activeEnemies;

        if (nextAction == Action.Block)
        {
            target = null;
            return;
        }
        
        int highestMaxHP = 0;
        GameObject highestMaxHPTarget = targets[0];

        foreach (GameObject enemy in targets)
        {
            CombatantBasis cb = enemy.GetComponent<CombatantBasis>();
            if (!cb.untargettable && cb.currentHitPoints > highestMaxHP)
            {
                highestMaxHP = cb.currentHitPoints;
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
        
        Block();
        canCounterAttack = true;
        // Do special Defend party mechanic
        Debug.Log(combatantName + " Special Counter");
    }
}
