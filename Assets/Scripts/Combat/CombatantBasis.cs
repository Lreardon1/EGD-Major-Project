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
    public List<Card.Element> nextActionPrimaryElems = new List<Card.Element>();
    public List<Card.Element> nextActionSecondaryElems = new List<Card.Element>();
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
    public float resistance = 0.0f;

    public bool specialHasTarget = false;

    public int temporaryHitPoints = 0;
    public int negativeHitPointShield = 0;

    //card onPlay variables
    public int attackCardBonus = 0;
    public int shieldReturnDmg = 0;
    public float shieldResistance = 0.0f;
    public bool untargettable = false;
    public bool canCounterAttack = false;
    public bool hasPriority = false;

    //storage for temp removed effects to allow for undoing cards
    public Status unappliedStatus;
    public List<Card.Element> unappliedPrimElem;
    public List<Card.Element> unappliedSecElem;

    public bool silenced = false;

    public TMPro.TextMeshPro text;
    public HealthbarController healthBar;
    public LineRenderer lr;
    public Transform targetLineStart;

    public PauseManager pauseManager;

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
                //MakePopup(
                //    GetColorStringOfElement(c.element) + "Played " + c.name + " on " + name + "</color>",
                //    (Texture2D)value.GetComponent<Image>().mainTexture, 
                //    value.GetComponent<Image>().color);
            }

            appliedCardReal = value;
        }
    }
    private GameObject appliedCardReal = null;

    public GameObject heldItem = null;

    public GameObject target = null;
    public GameObject oldTarget = null;
    public bool isSlain = false;
    public bool isEnemy = false;
    public bool isChanneling = false;
    public bool bonusAttack = false;

    public GameObject uiCollider;

    public Action previousAction = Action.None;

    public GameObject combatPopupPrefab;

    public void Start()
    {
        healthBar.SetMaxHealth(totalHitPoints, currentHitPoints);

        nextActionPrimaryElems.Add(Card.Element.None);
        nextActionSecondaryElems.Add(Card.Element.None);
    }

    public void MakePopup(string text, Texture2D image, Color col)
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

    public virtual void ExecuteAction()
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
            nextActionPrimaryElems.Clear();
            nextActionPrimaryElems.Add(Card.Element.None);
            nextActionSecondaryElems.Clear();
            nextActionSecondaryElems.Add(Card.Element.None);
        }
        // visuals
        if (!isChanneling) { //skips action if channeling
            MakePopup("Using " + nextAction + " Action", null, Color.white);

            if (nextAction == Action.Attack)
                Attack();
            else if (nextAction == Action.Block)
                Block();
            else if (nextAction == Action.Special)
                Special();

            previousAction = nextAction;
        }
        else
        {
            MakePopup("Channeling for next turn's Attack", null, Color.white);
            previousAction = Action.None;
        }

        //special case for handling bonus attack card
        if (bonusAttack)
        {
            nextAction = Action.Attack;
            if (target == null) //TODO:: ALSO NEEDS TO RETARGET IF PREVIOUS ATTACK KILLED TARGET
            {
                SelectTarget();
            }
            Attack();
            bonusAttack = false;
        }

        hasPriority = false;
        untargettable = false;
        oldTarget = null;

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
            nextActionPrimaryElems.Clear();
            nextActionPrimaryElems.Add(Card.Element.None);
            nextActionSecondaryElems.Clear();
            nextActionSecondaryElems.Add(Card.Element.None);
        }
        lr.enabled = false;

        pauseManager.RefreshPartyView();
    }

    public void Heal(int healingAmount)
    {
        currentHitPoints = Mathf.Clamp(currentHitPoints + healingAmount, 0, totalHitPoints);

        // visuals
        MakePopup("<color=\"green\"> Healed for " + healingAmount + "</color>", null, Color.white);
        healthBar.SetHealth(currentHitPoints);
    }

    public void TakeStatusDamage(float damageAmount, Status status)
    {
        currentHitPoints -= (int)(damageAmount);

        // visuals
        MakePopup("<color=\"red\"> Took " + damageAmount + " status damange for " + status + "</color>", null, Color.white);
        healthBar.SetHealth(currentHitPoints);
        CheckIsSlain();
    }

    public virtual void TakeDamage(float damageAmount, Card.Element damageType1, Card.Element damageType2, GameObject attacker)
    {

        statusScript.OnTakeDamageStatusHandler(statusCondition, attacker, (int)damageAmount);

        int shieldValue = temporaryHitPoints;

        int totalDamageAmount = (int)((damageAmount) / defenseMultiplier);

        currentHitPoints -= (int)((damageAmount) / defenseMultiplier);
        Debug.Log(combatantName + " took " + totalDamageAmount + " of " + damageType1 + " type and " + damageType2);

        // visuals, TODO : make a string construction system to color elements differently?
        MakePopup("<color=\"red\"> Took " + totalDamageAmount + "</color>", null, Color.white);

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
        if(newStatus != Status.None)
        {
            statusScript.ApplyNewStatus(newStatus, attacker);
        }

        healthBar.SetHealth(currentHitPoints);
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
            healthBar.gameObject.SetActive(false);
            lr.gameObject.SetActive(false);
            text.enabled = false;
            return true;
        }
        return false;
    }

    public virtual void SelectAction()
    {
        //special case if channeling for attack
        if (isChanneling)
        {
            nextAction = Action.Attack;
            SelectTarget();
            isChanneling = false;
            return;
        }

        if (silenced)
        {
            int random = Random.Range(0, 2);
            if(random == 0)
            {
                if(previousAction == Action.Attack)
                {
                    nextAction = Action.Block;
                    text.text = "Block";
                    return;
                }
                nextAction = Action.Attack;
                SelectTarget();
            } else if(random == 1)
            {
                if (previousAction == Action.Block)
                {
                    nextAction = Action.Attack;
                    SelectTarget();
                    return;
                }
                nextAction = Action.Block;
                text.text = "Block";
            }
            return;
        }

        int rand = Random.Range(0, 3);
        if(rand == 0)
        {
            if(previousAction == Action.Attack)
            {
                rand = Random.Range(0, 2);
                if(rand == 0)
                {
                    nextAction = Action.Block;
                    text.text = "Block";
                }
                else
                {
                    nextAction = Action.Special;
                    text.text = "Special";
                    if (specialHasTarget)
                        SelectTarget();
                } 
                return;
            }
            nextAction = Action.Attack;
            SelectTarget();
        } else if(rand == 1)
        {
            if (previousAction == Action.Block)
            {
                rand = Random.Range(0, 2);
                if (rand == 0)
                {
                    nextAction = Action.Attack;
                    SelectTarget();
                }
                else
                {
                    nextAction = Action.Special;
                    text.text = "Special";
                    if (specialHasTarget)
                        SelectTarget();
                }
                return;
            }
            nextAction = Action.Block;
            text.text = "Block";
        } else if(rand == 2)
        {
            if (previousAction == Action.Special)
            {
                rand = Random.Range(0, 2);
                if (rand == 0)
                {
                    nextAction = Action.Attack;
                    SelectTarget();
                }
                else
                {
                    nextAction = Action.Block;
                    text.text = "Block";
                }
                return;
            }
            nextAction = Action.Special;
            text.text = "Special";
            if (specialHasTarget)
                SelectTarget();
        }

    }

    public virtual void SelectTarget() //TODO:: STILL NEEDS TO HANDLE RETARGETTING IF RANDOMLY CHOOSING UNTARGETTABLE COMBATANT
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
        lr.positionCount = 2;
        lr.SetPosition(0, targetLineStart.position);
        lr.SetPosition(1, target.transform.position);
        lr.enabled = true;
    }

    public virtual void Attack()
    {
        CombatantBasis cb = target.GetComponent<CombatantBasis>();

        float damageTotal = (attack + attackCardBonus) * attackMultiplier; // Get modifier from card here

        cb.TakeDamage(damageTotal, nextActionPrimaryElems[nextActionPrimaryElems.Count-1], nextActionSecondaryElems[nextActionSecondaryElems.Count-1], gameObject);

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

    //not entirely ideal solution and still has some extreme edge cases where it could behave unsavorably, if enough time will touch up later
    public void RemoveElements(Card.Element primary, Card.Element secondary)
    {
        if (primary != Card.Element.None)
        {
            for (int i = 0; i < nextActionPrimaryElems.Count; i++)
            {
                if (nextActionPrimaryElems[i] == primary)
                {
                    nextActionPrimaryElems.RemoveAt(i);
                    break;
                }
            }
        }

        if (secondary != Card.Element.None)
        {
            for (int i = 0; i < nextActionSecondaryElems.Count; i++)
            {
                if (nextActionSecondaryElems[i] == secondary)
                {
                    nextActionSecondaryElems.RemoveAt(i);
                    break;
                }
            }
        }
    }

    public void RemoveElementsForTurn()
    {
        unappliedPrimElem = nextActionPrimaryElems;
        unappliedSecElem = nextActionSecondaryElems;
        nextActionPrimaryElems = new List<Card.Element>();
        nextActionPrimaryElems.Add(Card.Element.None);
        nextActionSecondaryElems = new List<Card.Element>();
        nextActionSecondaryElems.Add(Card.Element.None);
    }

    public void ReturnElementsForTurn()
    {
        nextActionPrimaryElems = unappliedPrimElem;
        nextActionSecondaryElems = unappliedSecElem;
        unappliedPrimElem = null;
        unappliedSecElem = null;
    }

    public void RemoveStatus()
    {
        unappliedStatus = statusCondition;
        statusCondition = Status.None;
    }

    public void ReapplyStatus()
    {
        statusCondition = unappliedStatus;
        unappliedStatus = Status.None;
    }
}
