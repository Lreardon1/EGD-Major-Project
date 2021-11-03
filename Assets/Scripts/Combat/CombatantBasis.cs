using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatantBasis : MonoBehaviour
{
    public enum Action { None, Attack, Block, Special };
    public enum Status { None, Burn, Wet, Earthbound, Gust, Holy, Fallen, Molten, Vaporise, Lightning, Hellfire, Ice, HolyWater, Blight, Corruption};

    public Action nextAction;
    public Status statusCondition;
    public StatusScript statusScript;
    public Card.Element nextActionPrimaryElem;
    public Card.Element nextActionSecondaryElem;
    public string combatantName;
    public int totalHitPoints;
    public int currentHitPoints;
    public int attack;
    public int speed;
    //handling buffs
    public List<Buff> attachedBuffs = new List<Buff>();
    public float attackMultiplier = 1f;
    public float defenseMultiplier = 1f;
    public float speedMultiplier = 1f;
    public Card.Element resistance = Card.Element.None;

    public int temporaryHitPoints = 0;
    public int negativeHitPointShield = 0;

    //card onPlay variables
    public int attackCardBonus = 0;
    public int shieldReturnDmg = 0;
    public bool untargettable = false;
    public bool canCounterAttack = false;
    public bool hasPriority = false;

    public TMPro.TextMeshPro text;

    // TODO : solution because no function is called to inform combatbasis of changed applied card
    public GameObject appliedCard
    {
        get
        {
            return appliedCardReal;
        }
        set
        {
            if (value == appliedCardReal) return;

            if (value != null)
            {
                if (value.GetComponent<Card>() == null) return;
                Card c = value.GetComponent<Card>();
                MakePopup(
                    GetColorStringOfElement(c.element) + "Played " + c.name + " on " + name + "</color>",
                    (Texture2D)value.GetComponent<Image>().mainTexture, 
                    value.GetComponent<Image>().color);
            }

            appliedCardReal = value;
        }
    }
    private GameObject appliedCardReal = null;

    public GameObject heldItem = null;

    public GameObject target = null;
    public bool isSlain = false;
    public bool isEnemy = false;

    public GameObject uiCollider;

    public Action previousAction = Action.None;

    public GameObject combatPopupPrefab;

    private void MakePopup(string text, Texture2D image, Color col)
    {
        GameObject popup = Instantiate(combatPopupPrefab, transform.position + transform.up * 0.4f, transform.rotation);
        popup.GetComponent<CombatPopup>().Init(text, image, col);
    }

    private string GetColorStringOfElement(Card.Element ele)
    {
        switch (ele)
        {
            //black, blue, green, orange, purple, red, white, and yellow
            default:
            case Card.Element.None:
                return "<color=\"white\">";
            case Card.Element.Fire:
                return "<color=\"red\">";
            case Card.Element.Water:
                return "<color=\"blue\">";
            case Card.Element.Air:
                return "<color=\"yellow\">";
            case Card.Element.Earth:
                return "<color=\"orange\">";
            case Card.Element.Light:
                return "<color=\"white\">";
            case Card.Element.Dark:
                return "<color=\"purple\">";
        }
    }

    public void ExecuteAction()
    {
        if(statusCondition == Status.Burn)
        {
            TakeStatusDamage(StatusScript.burnDamage, Status.Burn);
            if(CheckIsSlain())
            {
                return;
            }
        }

        if (previousAction == Action.Block)
        {
            defenseMultiplier -= 1f;
            attackCardBonus = 0;
            canCounterAttack = false;
            nextActionPrimaryElem = Card.Element.None;
            nextActionSecondaryElem = Card.Element.None;
        }
        // visuals
        MakePopup("Using " + nextAction + " Action", null, Color.white);

        if (nextAction == Action.Attack)
            Attack();
        else if (nextAction == Action.Block)
            Block();
        else if (nextAction == Action.Special)
            Special();

        previousAction = nextAction;
        hasPriority = false;
        untargettable = false;

        //buffs tick down after an action
        for(int i = 0; i < attachedBuffs.Count; i++)
        {
            Buff tempBuff = attachedBuffs[i];
            attachedBuffs[i].TickDuration();
            if (tempBuff == null)
                i--;
        }

        //clearing card stats on attack
        if (previousAction == Action.Attack)
        {
            attackCardBonus = 0;
            nextActionPrimaryElem = Card.Element.None;
            nextActionSecondaryElem = Card.Element.None;
        }
    }

    public void Heal(int healingAmount)
    {
        currentHitPoints = Mathf.Clamp(currentHitPoints + healingAmount, 0, totalHitPoints);

        // visuals
        MakePopup("<color=\"green\"> Healed for " + healingAmount + "</color>", null, Color.white);

    }

    public void TakeStatusDamage(float damageAmount, Status status)
    {
        currentHitPoints -= (int)(damageAmount);

        // visuals
        MakePopup("<color=\"red\"> Took " + damageAmount + " status damange for " + status + "</color>", null, Color.white);
        CheckIsSlain();
    }

    public virtual void TakeDamage(float damageAmount, Card.Element damageType1, Card.Element damageType2, GameObject attacker)
    {
        // Check for elemental combo
        float elementalComboMultiplier = 1f;

        statusScript.OnTakeDamageStatusHandler(statusCondition, attacker, (int)damageAmount);

        int shieldValue = temporaryHitPoints;

        currentHitPoints -= (int)((damageAmount * elementalComboMultiplier) / defenseMultiplier);
        Debug.Log(combatantName + " took " + damageAmount + " of " + damageType1 + " type and " + damageType2);

        // visuals, TODO : make a string construction system to color elements differently?
        MakePopup("<color=\"red\"> Took " + damageAmount + "</color>", null, Color.white);

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
        if(newStatus != Status.None)
        {
            statusScript.ApplyNewStatus(newStatus, attacker);
        }


        if (!CheckIsSlain()) //check counterattack if survived hit
        {
            if (canCounterAttack)
            {
                attacker.GetComponent<CombatantBasis>().TakeDamage(attackCardBonus * attackMultiplier, damageType1, damageType2, gameObject);
                print("Counter attack");
            }
        }
    }

    public bool CheckIsSlain()
    {
        if (currentHitPoints <= negativeHitPointShield)
        {
            Debug.Log(combatantName + " Slain");
            isSlain = true;
            this.GetComponent<SpriteRenderer>().enabled = false;
            text.enabled = false;
            return true;
        }
        return false;
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

    public virtual void SelectTarget(List<GameObject> targets) //TODO:: STILL NEEDS TO HANDLE RETARGETTING IF RANDOMLY CHOOSING UNTARGETTABLE COMBATANT
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

        float damageTotal = (attack + attackCardBonus) * attackMultiplier; // Get modifier from card here

        cb.TakeDamage(damageTotal, nextActionPrimaryElem, nextActionSecondaryElem, gameObject);

        Debug.Log("Attack");
    }

    public virtual void Block()
    {
        // Increase defense multipler to 2X
        defenseMultiplier += 1f;

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
