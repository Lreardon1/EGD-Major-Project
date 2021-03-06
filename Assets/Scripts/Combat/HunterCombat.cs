using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HunterCombat : CombatantBasis
{
    public bool specialAttack = false;

    public override void ExecuteAction()
    {
        base.ExecuteAction();

        if (previousAction == Action.Special)
        {
            attackCardBonus = 0;
            nextActionPrimaryElems.Clear();
            nextActionPrimaryElems.Add(Card.Element.None);
            nextActionSecondaryElems.Clear();
            nextActionSecondaryElems.Add(Card.Element.None);
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
        int lowestHP = int.MaxValue;
        GameObject lowestHPTarget = targets[0];

        foreach(GameObject enemy in targets)
        {
            CombatantBasis cb = enemy.GetComponent<CombatantBasis>();
            if(!cb.untargettable && cb.currentHitPoints < lowestHP)
            {
                lowestHP = cb.currentHitPoints;
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

    public override void Attack()
    {
        CombatantBasis cb = target.GetComponent<CombatantBasis>();

        float damageTotal = 0; 

        if(specialAttack) // Special attack does 1/3 regular attack damage to all enemies
        {
            damageTotal = ((attack + attackCardBonus)/3f) * attackMultiplier; // Get modifier from card here
        } else
        {
            damageTotal = (attack + attackCardBonus) * attackMultiplier; // Get modifier from card here
        }

        cb.TakeDamage(damageTotal, nextActionPrimaryElems[nextActionPrimaryElems.Count - 1], nextActionSecondaryElems[nextActionSecondaryElems.Count - 1], gameObject);

        Debug.Log("Hunter Attack");
    }

    public override void Special()
    {
        CombatManager cm = FindObjectOfType<CombatManager>();
        specialAttack = true;
        audioSource.PlayOneShot((AudioClip)Resources.Load("Sound/SFX/Bow_Shoot", typeof(AudioClip)), 0.7f);
        foreach (GameObject enemy in cm.activeEnemies)
        {
            if (enemy.GetComponent<CombatantBasis>().isSlain)
                continue;
            if (enemy.GetComponent<CombatantBasis>().untargettable)
                continue;
            target = enemy;
            Attack();
        }

        specialAttack = false;
        Debug.Log(combatantName + " Special Attack");
    }
}