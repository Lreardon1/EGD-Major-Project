using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Jay NOTE : this class should not control the deck or the hand AT ALL, not movement nor state.
//  This abstraction would have made my life so much easier.
//
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

    public List<GameObject> activePartyMembers = new List<GameObject>();
    public List<GameObject> activeEnemies = new List<GameObject>();

    public List<GameObject> actionOrder = new List<GameObject>();

    public int maxMana = 30;
    public int currentMana = 20;
    public int discardCost = 1;


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

        ToggleDrawButtons(false);
        foreach (GameObject member in partyMembers)
        {
            CombatantBasis cb = member.GetComponent<CombatantBasis>();
            if (cb.appliedCard != null)
            {
                Deck.instance.Discard(cb.appliedCard);
                cb.appliedCard = null;
            }
        }
        foreach (GameObject enemy in enemies)
        {
            CombatantBasis cb = enemy.GetComponent<CombatantBasis>();
            if (cb.appliedCard != null)
            {
                Deck.instance.Discard(cb.appliedCard);
                cb.appliedCard = null;
            }
        }

        foreach (GameObject member in partyMembers)
        {
            CombatantBasis memberScript = member.GetComponent<CombatantBasis>();
            memberScript.SelectAction();
            memberScript.SelectTarget(activeEnemies);
            if (memberScript.nextAction == CombatantBasis.Action.Block)
            {
                memberScript.text.text = "Block";
            }
        }

        foreach (GameObject enemy in enemies)
        {
            CombatantBasis enemyScript = enemy.GetComponent<CombatantBasis>();
            enemyScript.SelectAction();
            enemyScript.SelectTarget(activePartyMembers);
            if (enemyScript.nextAction == CombatantBasis.Action.Block)
            {
                enemyScript.text.text = "Block";
            }

        }

        CreateActionQueue();
        foreach (GameObject combatant in actionOrder)
        {
            Debug.Log(combatant.name);
        }

        ActivatePlayPhase();
    }

    public void ActivateDrawPhase()
    {
        currentPhase = CombatPhase.DrawPhase;
        UpdateDropZones();
        ToggleDrawButtons(true);
        foreach (GameObject member in partyMembers)
        {
            CombatantBasis cb = member.GetComponent<CombatantBasis>();
            if (cb.appliedCard != null) // Check to see if card is delay turn card in which case to not set to null
            {
                cb.appliedCard = null;
            }
        }
        foreach (GameObject enemy in enemies)
        {
            CombatantBasis cb = enemy.GetComponent<CombatantBasis>();
            if (cb.appliedCard != null) // Check to see if card is delay turn card in which case to not set to null
            {
                cb.appliedCard = null;
            }
        }

        foreach (GameObject member in partyMembers)
        {
            CombatantBasis memberScript = member.GetComponent<CombatantBasis>();
            memberScript.SelectAction();
            memberScript.SelectTarget(activeEnemies);
            if (memberScript.nextAction == CombatantBasis.Action.Block)
            {
                memberScript.text.text = "Block";
            }
        }

        foreach (GameObject enemy in enemies)
        {
            CombatantBasis enemyScript = enemy.GetComponent<CombatantBasis>();
            enemyScript.SelectAction();
            enemyScript.SelectTarget(activePartyMembers);
            if (enemyScript.nextAction == CombatantBasis.Action.Block)
            {
                enemyScript.text.text = "Block";
            }

        }

        CreateActionQueue();
        foreach(GameObject combatant in actionOrder)
        {
            Debug.Log(combatant.name);
        }

        StartCoroutine("DrawPhaseCoroutine");
        // Allies and enemies select actions to perform, Player selects number of cards to draw, transition to Play Phase
    }

    public IEnumerator DrawPhaseCoroutine()
    {
        bool done = false;
        while (!done)
        {
            // skips when space is hit
            if (Input.GetKeyDown(KeyCode.Space))
            {
                done = true;
            }

            // This code waits until either a card amount to draw has been selected or for cv, a number of cards is shown to the camera

            // Need code to detect if card has been applied
            yield return null;
        }
        NextPhase();
        yield return null;
    }

    public void ActivatePlayPhase()
    {
        currentPhase = CombatPhase.PlayPhase;
        UpdateDropZones();

        ToggleDrawButtons(false);
        // Allow player to move cards to play on allies/enemies, update action order accordingly, ends when player clicks done or something, transition to Discard Phase
        StartCoroutine("PlayPhaseCoroutine");
    }

    public IEnumerator PlayPhaseCoroutine()
    {
        bool done = false;
        while (!done)
        {
            // skips when space is hit
            if (Input.GetKeyDown(KeyCode.Space))
            {
                done = true;
            }

            // Need code to detect if card has been applied
            yield return null;
        }
        NextPhase();
        yield return null;
    }

    public void ActivateDiscardPhase()
    {
        currentPhase = CombatPhase.DiscardPhase;
        UpdateDropZones();
        // Player can drag cards to discard pile to discard them, ends when player clicks done or something, transition to Action Phase
        StartCoroutine("DiscardPhaseCoroutine");
    }

    public IEnumerator DiscardPhaseCoroutine()
    {
        bool done = false;
        while (!done)
        {
            // skips when space is hit
            if (Input.GetKeyDown(KeyCode.Space))
            {
                done = true;
            }

            // Need code to detect if card has been applied
            yield return null;
        }
        NextPhase();
        yield return null;
    }

    public void ActivateActionPhase()
    {
        currentPhase = CombatPhase.ActionPhase;
        UpdateDropZones();

        currentPhaseText.text = "Action Phase";
        // Party members and enemies take turns attacking in action order, death prevents attacking, transition to Draw Phase
        StartCoroutine("StartActions");

    }

    public GameObject currentCB = null;
    public IEnumerator StartActions()
    {
        while (actionOrder.Count > 0)
        {
            CombatantBasis cb = actionOrder[0].GetComponent<CombatantBasis>();
            currentCB = actionOrder[0];
            bool cardAlreadyPlayed = false;

            if (cb.appliedCard != null)
                cardAlreadyPlayed = true;

            if (enoughMana && !cardAlreadyPlayed && chc.transform.childCount != 0)
            {
                Debug.Log("Play card on " + actionOrder[0].name);
                foreach (GameObject card in Deck.instance.viewOrder)
                {
                    DragDrop dd = card.GetComponent<DragDrop>();
                    List<GameObject> allZones = new List<GameObject>();
                    allZones.Add(actionOrder[0]);
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
                        // Activate Card Effect
                        done = true;

                    }
                    yield return null;
                }
            }

            cb.ExecuteAction();
            actionOrder.RemoveAt(0);
            RemoveFallenCombatants();
            CheckWinCondition();
            UpdateTargets();
            CheckEnoughMana();
            yield return new WaitForSeconds(1f);
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
                    dd.isDraggable = false;
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
                    dd.isDraggable = true;
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
                    dd.isDraggable = true;
                    dd.allowedDropZones.Clear();
                    dd.allowedDropZones = allZones;
                }
                break;
            case CombatPhase.ActionPhase:
                foreach (GameObject card in Deck.instance.viewOrder)
                {
                    DragDrop dd = card.GetComponent<DragDrop>();
                    dd.isDraggable = true;
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
        if (currentPhase == CombatPhase.ActionPhase)
            return;
        StopAllCoroutines();
        switch (currentPhase)
        {
            case CombatPhase.DrawPhase:
                currentPhase = CombatPhase.PlayPhase;
                currentPhaseText.text = "Play Phase";
                ActivatePlayPhase();
                break;
            case CombatPhase.PlayPhase:
                currentPhase = CombatPhase.DiscardPhase;
                currentPhaseText.text = "Discard Phase";
                ActivateDiscardPhase();
                break;
            case CombatPhase.DiscardPhase:
                currentPhase = CombatPhase.ActionPhase;
                currentPhaseText.text = "Action Phase";
                ActivateActionPhase();
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
                actionOrder.Remove(activePartyMembers[i]);
                activePartyMembers.RemoveAt(i);
                i--;
            }
        }
        for (int i = 0; i < activeEnemies.Count; i++)
        {
            CombatantBasis cb = activeEnemies[i].GetComponent<CombatantBasis>();
            if (cb.isSlain == true)
            {
                actionOrder.Remove(activeEnemies[i]);
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

    public void ApplyCard(GameObject card, GameObject combatant)
    {
        CombatantBasis cb = combatant.GetComponent<CombatantBasis>();
        Card cardScript = card.GetComponent<Card>();
        if(cb.appliedCard != null)
        {
            card.transform.SetParent(chc.gameObject.transform);
            card.transform.localScale = new Vector3(1, 1, 1);
            Debug.Log("Card Already Played On This Combatant");
            return;
        }
        if(currentMana - cardScript.manaCost < 0)
        {
            Debug.Log("Not Enough Mana To Play This Card");
            card.transform.SetParent(chc.gameObject.transform);
            card.transform.localScale = new Vector3(1, 1, 1);
            cb.appliedCard = null;
            return;
        }
        cb.appliedCard = card;
        currentMana -= cardScript.manaCost;
        manaText.text = "Mana: " + currentMana + "/" + maxMana;

        if(!cb.isEnemy)
        {
            cardScript.Play(combatant, partyMembers);
        } else
        {
            cardScript.Play(combatant, enemies);
        }
        Deck.instance.Discard(card);
    }

    public void DrawCards(int cardsToDraw)
    {
        chc.DrawCards(cardsToDraw);
    }

    public void DiscardCard(GameObject card)
    {
        currentMana -= discardCost;
        manaText.text = "Mana: " + currentMana + "/" + maxMana;
        chc.DiscardCard(card);
    }

    public void AddMana(int manaAmount)
    {
        currentMana += manaAmount;
        currentMana = Mathf.Clamp(currentMana, 0, maxMana);
        manaText.text = "Mana: " + currentMana + "/" + maxMana;
    }

    public void CreateActionQueue()
    {
        foreach(GameObject member in activePartyMembers)
        {
            actionOrder.Add(member);
        }
        foreach (GameObject enemy in activeEnemies)
        {
            actionOrder.Add(enemy);
        }

        UpdateActionQueue();
    }

    public void UpdateActionQueue()
    {
        List<GameObject> newOrder = new List<GameObject>();

        //first, maintain order of combatants with priority
        while (actionOrder.Count > 0 && actionOrder[0].GetComponent<CombatantBasis>().hasPriority)
        {
            newOrder.Add(actionOrder[0]);
            actionOrder.RemoveAt(0);
        }

        //then recalculate the order based on speed
        while (actionOrder.Count != 0)
        {
            GameObject fastestCombatant = null;
            float maxSpeed = -1;
            foreach (GameObject combatant in actionOrder)
            {
                CombatantBasis cb = combatant.GetComponent<CombatantBasis>();
                float spd = cb.speed * cb.speedMultiplier;
                if (spd > maxSpeed)
                {
                    maxSpeed = spd;
                    fastestCombatant = combatant;
                }
            }

            newOrder.Add(fastestCombatant);
            actionOrder.Remove(fastestCombatant);
        }

        actionOrder = newOrder;
    }

    public List<GameObject> GetAdjacentCombatants(GameObject combatant)
    {
        List<GameObject> adjacent = new List<GameObject>();

        CombatantBasis cb = combatant.GetComponent<CombatantBasis>();
        if(cb.isEnemy)
        {
            int index = enemies.IndexOf(combatant);
            if(index >= 1 && activeEnemies.Contains(enemies[index-1]))
            {
                adjacent.Add(enemies[index - 1]);
            }
            if (index <= enemies.Count - 2 && activeEnemies.Contains(enemies[index + 1]))
            {
                adjacent.Add(enemies[index + 1]);
            }

        } else
        {
            int index = partyMembers.IndexOf(combatant);
            if (index >= 1 && activePartyMembers.Contains(partyMembers[index - 1]))
            {
                adjacent.Add(partyMembers[index - 1]);
            }
            if (index <= partyMembers.Count - 2 && activePartyMembers.Contains(enemies[index + 1]))
            {
                adjacent.Add(partyMembers[index + 1]);
            }
        }

        return adjacent;
    }

    public void GivePriority(GameObject combatant)
    {
        combatant.GetComponent<CombatantBasis>().hasPriority = true;
        //if combatant is still in queue, move them to top of the order
        if (actionOrder.Contains(combatant))
        {
            actionOrder.Remove(combatant);
            actionOrder.Insert(0, combatant);
        }
    }

    public void FocusOnEnemy(GameObject combatant)
    {
        //focuses any remaining ally actions onto this enemy
        foreach (GameObject action in actionOrder)
        {
            CombatantBasis cb = action.GetComponent<CombatantBasis>();
            if (!cb.isEnemy && cb.target != null)
            {
                cb.target = combatant;
            }
        }
    }

    public void FocusOnAlly(GameObject combatant)
    {
        //focuses any remaining ally actions onto this enemy
        foreach (GameObject action in actionOrder)
        {
            CombatantBasis cb = action.GetComponent<CombatantBasis>();
            if (cb.isEnemy && cb.target != null)
            {
                cb.target = combatant;
            }
        }
    }

    public void ClearTarget(GameObject combatant)
    {
        //focuses any remaining ally actions onto untarget this combatant
        foreach (GameObject action in actionOrder)
        {
            CombatantBasis cb = action.GetComponent<CombatantBasis>();
            if (cb.target == combatant)
            {
                if (cb.isEnemy)
                {
                    cb.SelectTarget(activePartyMembers);
                }
                else
                {
                    cb.SelectTarget(activeEnemies);
                }
            }
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
