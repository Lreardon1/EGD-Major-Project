using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatantBasis : MonoBehaviour
{
    public enum Action { Attack, Block, Special };

    public Action nextAction;
    public string combatantName;
    public int totalHitPoints;
    public int currentHitPoints;
    public int attack;
    public int speed;
    public float defenseMultiplier = 1f;
    public int temporaryHitPoints = 0;
    public int negativeHitPointShield = 0;

    public TMPro.TextMeshPro text;

    public GameObject appliedCard = null;
    public GameObject heldItem = null;

    public GameObject target = null;

    private Action previousAction;

    public void ExecuteAction()
    {
        if (nextAction == Action.Attack)
            Attack();
        else if (nextAction == Action.Block)
            Block();
        else if (nextAction == Action.Special)
            Special();
    }

    public virtual void TakeDamage(int damageAmount, string damageType)
    {
        Debug.Log(combatantName + " took " + damageAmount + " of " + damageType + " type");
    }

    public virtual void SelectAction()
    {
        int rand = Random.Range(0, 3);
        if(rand == 0)
        {
            nextAction = Action.Attack;
        } else if(rand == 1)
        {
            nextAction = Action.Block;
        } else if(rand == 3)
        {
            nextAction = Action.Special;
        }
    }

    public virtual void SelectTarget(List<GameObject> targets)
    {
        if(nextAction == Action.Block)
        {
            target = null;
            return;
        }
        int randint = Random.Range(0, targets.Count);
        target = targets[randint];
        Debug.Log("Target Selected");
    }

    public virtual void Attack()
    {
        Debug.Log("Attack");
    }

    public virtual void Block()
    {
        Debug.Log("Block");
    }

    public virtual void Special()
    {
        Debug.Log("Special");
    }
}
