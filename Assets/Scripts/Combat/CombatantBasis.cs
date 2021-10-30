using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatantBasis : MonoBehaviour
{
    public enum Action { Attack, Block, Special };
    public enum Status { None, Burn, Wet, Earthbound, Gust, Holy, Fallen, Molten, Vaporise, Lightning, Hellfire, Ice, HolyWater, Blight, Corruption};

    public Action nextAction;
    public Status statusCondition;
    public Card.Element nextActionPrimaryElem;
    public Card.Element nextActionSecondaryElem;
    public string combatantName;
    public int totalHitPoints;
    public int currentHitPoints;
    public int attack;
    public int speed;
    public float defenseMultiplier = 1f;
    public Card.Element resistance = Card.Element.None;
    public int temporaryHitPoints = 0;
    public int negativeHitPointShield = 0;

    //card onPlay variables
    public int attackCardBonus = 0;
    public int shieldReturnDmg = 0;
    public bool untargettable = false;
    public bool canCounterAttack = false;

    public TMPro.TextMeshPro text;

    public GameObject appliedCard = null;
    public GameObject heldItem = null;

    public GameObject target = null;
    public bool isSlain = false;
    public bool isEnemy = false;

    private Action previousAction;

    public void ExecuteAction()
    {
        if (previousAction == Action.Block)
        {
            defenseMultiplier = 1f;
            attackCardBonus = 0;
            canCounterAttack = false;
            nextActionPrimaryElem = Card.Element.None;
            nextActionSecondaryElem = Card.Element.None;
        }

        if (nextAction == Action.Attack)
            Attack();
        else if (nextAction == Action.Block)
            Block();
        else if (nextAction == Action.Special)
            Special();

        previousAction = nextAction;

        //clearing card stats on attack
        if (previousAction == Action.Attack)
        {
            attackCardBonus = 0;
            nextActionPrimaryElem = Card.Element.None;
            nextActionSecondaryElem = Card.Element.None;
        }
    }

    public virtual void TakeDamage(int damageAmount, Card.Element damageType1, Card.Element damageType2, GameObject attacker)
    {
        // Check for elemental combo
        float elementalComboMultiplier = 1f;

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

        if (currentHitPoints <= negativeHitPointShield)
        {
            Debug.Log(combatantName + " Slain");
            isSlain = true;
            this.GetComponent<SpriteRenderer>().enabled = false;
            text.enabled = false;
        }
        else //check counterattack if survived hit
        {
            if (canCounterAttack)
            {
                attacker.GetComponent<CombatantBasis>().TakeDamage(attackCardBonus, damageType1, damageType2, gameObject);
                print("Counter attack");
            }
        }
    }

    public virtual void SelectAction()
    {
        int rand = Random.Range(0, 3);
        if(rand == 0)
        {
            if(previousAction == Action.Attack)
            {
                rand = Random.Range(0, 2);
                if(rand == 0)
                    nextAction = Action.Block;
                else
                    nextAction = Action.Special;
                return;
            }
            nextAction = Action.Attack;
        } else if(rand == 1)
        {
            if (previousAction == Action.Block)
            {
                rand = Random.Range(0, 2);
                if (rand == 0)
                    nextAction = Action.Attack;
                else
                    nextAction = Action.Special;
                return;
            }
            nextAction = Action.Block;
        } else if(rand == 2)
        {
            if (previousAction == Action.Special)
            {
                rand = Random.Range(0, 2);
                if (rand == 0)
                    nextAction = Action.Attack;
                else
                    nextAction = Action.Block;
                return;
            }
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
        if(nextAction == Action.Attack)
        {
            text.text = "Attack " + target.GetComponent<CombatantBasis>().combatantName;
        } else if(nextAction == Action.Special)
        {
            text.text = "Special " + target.GetComponent<CombatantBasis>().combatantName;
        }
        Debug.Log("Target Selected");
    }

    public virtual void Attack()
    {
        CombatantBasis cb = target.GetComponent<CombatantBasis>();

        int damageTotal = attack + attackCardBonus; // Get modifier from card here

        cb.TakeDamage(damageTotal, nextActionPrimaryElem, nextActionSecondaryElem, gameObject);

        Debug.Log("Attack");
    }

    public virtual void Block()
    {
        // Increase defense multipler to 2X
        defenseMultiplier = 2f;

        //counterattack from attack card
        if (attackCardBonus > 0)
        {
            canCounterAttack = true;
        }

        Debug.Log("Block");
    }

    public virtual void Special()
    {
        Debug.Log("Special");
    }
}
