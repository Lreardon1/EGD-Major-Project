using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatantBasis : MonoBehaviour
{
    public enum Action { Attack, Block, Special };
    public enum DamageType { None, Fire, Water, Earth, Air, Light, Dark};
    public enum Status { None, Burn, Wet, Earthbound, Gust, Holy, Fallen, Molten, Vaporise, Lightning, Hellfire, Ice, HolyWater, Blight, Corruption};

    public Action nextAction;
    public Status statusCondition;
    public string combatantName;
    public int totalHitPoints;
    public int currentHitPoints;
    public int attack;
    public int speed;
    public float defenseMultiplier = 1f;
    public DamageType resistance = DamageType.None;
    public int temporaryHitPoints = 0;
    public int negativeHitPointShield = 0;

    public TMPro.TextMeshPro text;

    public GameObject appliedCard = null;
    public GameObject heldItem = null;

    public GameObject target = null;
    public bool isSlain = false;

    private Action previousAction;

    public void ExecuteAction()
    {
        defenseMultiplier = 1f;

        if (nextAction == Action.Attack)
            Attack();
        else if (nextAction == Action.Block)
            Block();
        else if (nextAction == Action.Special)
            Special();

        previousAction = nextAction;
    }

    public virtual void TakeDamage(int damageAmount, string damageType)
    {
        // Check for elemental combo
        float elementalComboMultiplier = 1f;

        currentHitPoints -= (int)((damageAmount * elementalComboMultiplier) / defenseMultiplier);
        Debug.Log(combatantName + " took " + damageAmount + " of " + damageType + " type");

        if(currentHitPoints <= negativeHitPointShield)
        {
            Debug.Log(combatantName + " Slain");
            isSlain = true;
            this.GetComponent<SpriteRenderer>().enabled = false;
            text.enabled = false;
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

        int damageTotal = attack + 0; // Get modifier from card here

        string damageType = "none"; // Get damage type from card here

        cb.TakeDamage(damageTotal, damageType);
        Debug.Log("Attack");
    }

    public virtual void Block()
    {
        temporaryHitPoints += 0; // Get temporary hit points from card here

        // Increase defense multipler to 2X
        defenseMultiplier = 2f;

        Debug.Log("Block");
    }

    public virtual void Special()
    {
        Debug.Log("Special");
    }
}
