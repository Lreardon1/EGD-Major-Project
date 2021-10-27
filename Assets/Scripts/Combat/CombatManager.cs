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
        int currentCardsInDiscard = chc.discardPile.transform.childCount;
        while (!done)
        {
            // skips when space is hit
            if (Input.GetKeyDown(KeyCode.Space))
            {
                done = true;
            }
            if(currentCardsInDiscard != chc.discardPile.transform.childCount)
            {
                currentMana -= Mathf.Abs(chc.discardPile.transform.childCount - currentCardsInDiscard);
                currentCardsInDiscard = chc.discardPile.transform.childCount;
                manaText.text = "Mana: " + currentMana + "/" + maxMana;
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

    public IEnumerator StartActions()
    {
        for(int i = 0; i < actionOrder.Count; i++)
        {
            CombatantBasis cb = actionOrder[i].GetComponent<CombatantBasis>();
            bool cardAlreadyPlayed = false;

            if (cb.appliedCard != null)
                cardAlreadyPlayed = true;

            if (enoughMana && !cardAlreadyPlayed && chc.transform.childCount != 0)
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

            cb.appliedCard.transform.SetParent(chc.gameObject.transform);
            cb.appliedCard.transform.localScale = new Vector3(1, 1, 1);
            cb.appliedCard = null;
            return;
        }
        cb.appliedCard = card;
        currentMana -= cardScript.manaCost;
        manaText.text = "Mana: " + currentMana + "/" + maxMana;
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
