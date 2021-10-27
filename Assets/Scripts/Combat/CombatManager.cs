using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatManager : MonoBehaviour
{
    public enum CombatPhase {DrawPhase, PlayPhase, DiscardPhase, ActionPhase};

    public CombatPhase currentPhase = CombatPhase.PlayPhase;
    public CombatHandController chc;
    public List<Button> drawButtons = new List<Button>();
    public Text currentPhaseText;
    public Text manaText;

    public List<GameObject> partyMembers = new List<GameObject>();
    public List<GameObject> enemies = new List<GameObject>();

    private List<GameObject> activePartyMembers = new List<GameObject>();
    private List<GameObject> activeEnemies = new List<GameObject>();

    public List<GameObject> actionOrder = new List<GameObject>();

    public int maxMana = 30;
    public int currentMana = 20;
    

    public bool canDraw = false;
    public bool canPlay = true;
    private bool enoughMana = true;    

    
    // Start is called before the first frame update
    void Start()
    {
        foreach (GameObject member in partyMembers)
        {
            activePartyMembers.Add(member);
        }
        foreach (GameObject enemy in enemies)
        {
            activeEnemies.Add(enemy);
        }
        // Populate Partymembers and enemies
        ActivateDrawPhase();

    }

    public void ActivateDrawPhase()
    {
        currentPhase = CombatPhase.DrawPhase;
        UpdateDropZones();
        ToggleDrawButtons(true);
        foreach(GameObject member in partyMembers)
        {
            CombatantBasis memberScript = member.GetComponent<CombatantBasis>();
            memberScript.SelectAction();
            memberScript.SelectTarget(activeEnemies);

            if (memberScript.nextAction == CombatantBasis.Action.Attack)
            {
                memberScript.text.text = "Attack " + memberScript.target.GetComponent<CombatantBasis>().combatantName;
            }
            else if (memberScript.nextAction == CombatantBasis.Action.Block)
            {
                memberScript.text.text = "Block";
            }
            else if (memberScript.nextAction == CombatantBasis.Action.Special)
            {
                memberScript.text.text = "Special " + memberScript.target.GetComponent<CombatantBasis>().combatantName;
            }
        }

        foreach (GameObject enemy in enemies)
        {
            CombatantBasis enemyScript = enemy.GetComponent<CombatantBasis>();
            enemyScript.SelectAction(); 
            enemyScript.SelectTarget(activePartyMembers);
            if (enemyScript.nextAction ==  CombatantBasis.Action.Attack)
            {
                enemyScript.text.text = "Attack " + enemyScript.target.GetComponent<CombatantBasis>().combatantName;
            }
            else if (enemyScript.nextAction == CombatantBasis.Action.Block)
            {
                enemyScript.text.text = "Block";
            }
            else if (enemyScript.nextAction == CombatantBasis.Action.Special)
            {
                enemyScript.text.text = "Special " + enemyScript.target.GetComponent<CombatantBasis>().combatantName;
            }

        }

        CreateActionQueue();
        foreach(GameObject combatant in actionOrder)
        {
            Debug.Log(combatant.name);
        }

        ActivatePlayPhase();
        // Allies and enemies select actions to perform, Player selects number of cards to draw, transition to Play Phase
    }

    public void ActivatePlayPhase()
    {
        currentPhase = CombatPhase.PlayPhase;
        UpdateDropZones();

        ToggleDrawButtons(false);
        // Allow player to move cards to play on allies/enemies, update action order accordingly, ends when player clicks done or something, transition to Discard Phase
        ActivateDiscardPhase();
    }

    public void ActivateDiscardPhase()
    {
        currentPhase = CombatPhase.DiscardPhase;
        UpdateDropZones();
        // Player can drag cards to discard pile to discard them, ends when player clicks done or something, transition to Action Phase
        ActivateActionPhase();
    }

    public void ActivateActionPhase()
    {
        currentPhase = CombatPhase.ActionPhase;
        UpdateDropZones();

        currentPhaseText.text = "Action Phase";
        // Party members and enemies take turns attacking in action order, death prevents attacking, transition to Draw Phase
        StartCoroutine("StartActions");
        
    }

    public IEnumerator StartActions()
    {
        for(int i = 0; i < actionOrder.Count; i++)
        {
            CombatantBasis cb = actionOrder[i].GetComponent<CombatantBasis>();
            bool cardAlreadyPlayed = false;

            if (cb.appliedCard != null)
                cardAlreadyPlayed = true;

            if (enoughMana && !cardAlreadyPlayed)
            {
                Debug.Log("Play card on " + actionOrder[i].name);
                foreach (GameObject card in Deck.instance.viewOrder)
                {
                    DragDrop dd = card.GetComponent<DragDrop>();
                    List<GameObject> allZones = new List<GameObject>();
                    allZones.Add(actionOrder[i]);
                    dd.allowedDropZones.Clear();
                    dd.allowedDropZones = allZones;
                }

                bool done = false;
                while(!done)
                {
                    // skips when space is hit
                    if(Input.GetKeyDown(KeyCode.Space))
                    {
                        done = true;
                    }

                    // Need code to detect if card has been applied

                    if (cb.appliedCard != null)
                    {
                        Card cardScript = cb.appliedCard.GetComponent<Card>();
                        if(currentMana - cardScript.manaCost < 0)
                        {
                            Debug.Log("Not Enough Mana To Play This Card");

                            cb.appliedCard.transform.SetParent(chc.gameObject.transform);
                            cb.appliedCard.transform.localScale = new Vector3(1,1,1);
                            cb.appliedCard = null;
                            continue;
                        }

                        currentMana -= cardScript.manaCost;
                        manaText.text = "Mana: " + currentMana + "/" + maxMana;
                        // Activate Card Effect
                        done = true;

                    }
                    yield return null;
                }
            }

            cb.ExecuteAction();
            RemoveFallenCombatants();
            CheckWinCondition();
            UpdateTargets();
            CheckEnoughMana();
            // Check if any combatant was killed and update the action queue
        }

        currentPhase = CombatPhase.DrawPhase;
        ActivateDrawPhase();
        yield return null;
    }

    public void UpdateTargets()
    {
        foreach(GameObject partyMember in activePartyMembers)
        {
            CombatantBasis cb = partyMember.GetComponent<CombatantBasis>();
            if (cb.isSlain == false)
            {
                if(cb.target != null && cb.target.GetComponent<CombatantBasis>().isSlain)
                {
                    cb.SelectTarget(activeEnemies);
                }
            }
        }
        foreach (GameObject enemy in activeEnemies)
        {
            CombatantBasis cb = enemy.GetComponent<CombatantBasis>();
            if (cb.isSlain == false)
            {
                if (cb.target != null && cb.target.GetComponent<CombatantBasis>().isSlain)
                {
                    cb.SelectTarget(activePartyMembers);
                }
            }
        }
    }

    public void UpdateDropZones()
    {
        switch (currentPhase)
        {
            case CombatPhase.DrawPhase:
                foreach (GameObject card in Deck.instance.viewOrder)
                {
                    DragDrop dd = card.GetComponent<DragDrop>();
                    dd.allowedDropZones.Clear();
                }
                break;
            case CombatPhase.PlayPhase:
                foreach (GameObject card in Deck.instance.viewOrder)
                {
                    DragDrop dd = card.GetComponent<DragDrop>();
                    List<GameObject> allZones = new List<GameObject>();
                    allZones.AddRange(partyMembers);
                    allZones.AddRange(enemies);
                    dd.allowedDropZones.Clear();
                    dd.allowedDropZones = allZones;
                }
                break;
            case CombatPhase.DiscardPhase:
                foreach (GameObject card in Deck.instance.viewOrder)
                {
                    DragDrop dd = card.GetComponent<DragDrop>();
                    List<GameObject> allZones = new List<GameObject>();
                    allZones.Add(chc.discardPile);
                    dd.allowedDropZones.Clear();
                    dd.allowedDropZones = allZones;
                }
                break;
            case CombatPhase.ActionPhase:
                foreach (GameObject card in Deck.instance.viewOrder)
                {
                    DragDrop dd = card.GetComponent<DragDrop>();
                    dd.allowedDropZones.Clear();
                }
                break;
            default:
                foreach (GameObject card in Deck.instance.viewOrder)
                {
                    DragDrop dd = card.GetComponent<DragDrop>();
                    dd.allowedDropZones.Clear();
                }
                break;
        }
    }

    public void ToggleDrawButtons(bool state)
    {
        foreach (Button button in drawButtons)
        {
            button.interactable = state;
        }
    }

    public void NextPhase()
    {
        StopAllCoroutines();
        currentPhaseText.text = "";
        switch (currentPhase)
        {
            case CombatPhase.DrawPhase:
                currentPhase = CombatPhase.PlayPhase;

                currentPhaseText.text = "Play Phase";
                break;
            case CombatPhase.PlayPhase:
                currentPhase = CombatPhase.DiscardPhase;

                currentPhaseText.text = "Discard Phase";
                break;
            case CombatPhase.DiscardPhase:
                currentPhase = CombatPhase.ActionPhase;

                currentPhaseText.text = "Action Phase";
                break;
            case CombatPhase.ActionPhase:
                currentPhase = CombatPhase.DrawPhase;

                currentPhaseText.text = "Draw Phase";
                break;
        }
    }

    public void RemoveFallenCombatants()
    {
        for(int i = 0; i < activePartyMembers.Count; i++)
        {
            CombatantBasis cb = activePartyMembers[i].GetComponent<CombatantBasis>();
            if (cb.isSlain == true)
            {
                activePartyMembers.RemoveAt(i);
                i--;
            }
        }
        for (int i = 0; i < activeEnemies.Count; i++)
        {
            CombatantBasis cb = activeEnemies[i].GetComponent<CombatantBasis>();
            if (cb.isSlain == true)
            {
                activeEnemies.RemoveAt(i);
                i--;
            }
        }
    }

    public void CheckEnoughMana()
    {
        // Check if player has enough mana to play any cards in their hand, if not the action order can procede without input
        bool tempEnoughMana = false;
        foreach(GameObject card in chc.cardsInHand)
        {
            if(currentMana > card.GetComponent<Card>().manaCost)
            {
                tempEnoughMana = true;
                break;
            }
        }
        enoughMana = tempEnoughMana;
    }

    public void AddMana(int manaAmount)
    {
        currentMana += manaAmount;
        currentMana = Mathf.Clamp(currentMana, 0, maxMana);
        manaText.text = "Mana: " + currentMana + "/" + maxMana;
    }

    public void CreateActionQueue()
    {
        actionOrder.Clear();
        List<GameObject> allCombatants = new List<GameObject>();
        foreach(GameObject member in activePartyMembers)
        {
            allCombatants.Add(member);
        }
        foreach (GameObject enemy in activeEnemies)
        {
            allCombatants.Add(enemy);
        }

        while(allCombatants.Count != 0)
        {
            GameObject fastestCombatant = null;
            int maxSpeed = -1;
            foreach (GameObject combatant in allCombatants)
            {
                CombatantBasis cb = combatant.GetComponent<CombatantBasis>();
                if(cb.speed > maxSpeed)
                {
                    maxSpeed = cb.speed;
                    fastestCombatant = combatant;
                }
            }

            actionOrder.Add(fastestCombatant);
            allCombatants.Remove(fastestCombatant);
        }

    }

    public void CheckWinCondition()
    {
        if (activeEnemies.Count == 0)
        {
            Debug.Log("You Win!");
            StopAllCoroutines();
        }
        else if (activePartyMembers.Count == 0)
        {
            Debug.Log("You Lose...");
            StopAllCoroutines();
        }
    }
}
