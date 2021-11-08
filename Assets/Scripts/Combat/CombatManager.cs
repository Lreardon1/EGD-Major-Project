using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Jay NOTE : this class should not control the deck or the hand AT ALL, not movement nor state.
//  This abstraction would have made my life so much easier.
//

// TODO : I would like to use UnityEvents, to subscribe to phase changes
    // https://docs.unity3d.com/ScriptReference/Events.UnityEvent.html
    // and maybe even requests from the combat manager to active control scheme
public class CombatManager : MonoBehaviour
{
    public static bool IsInCVMode = false;

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
    //public List<GameObject> encounterSets;

    public Transform enemySetLocation;

    public GameObject uiColliderPrefab;
    public Transform uiColliderParent;
    public Vector3 cameraPosition;
    public Quaternion cameraRotation;

    public LoadEncounter encounterScript;

    public GameObject pointer;
    public ActionOrderUI actionOrderUI;

    public GameObject lastPlayedCard;

    public int maxMana = 30;
    public int currentMana = 20;
    public int discardCost = 1;


    public bool canDraw = false;
    public bool canPlay = true;
    private bool enoughMana = true;


    // Start is called before the first frame update
    void Start()
    {
        List<GameObject> allCombatants = new List<GameObject>();
        foreach (GameObject member in partyMembers)
        {
            activePartyMembers.Add(member);
            // This gameobject is used to detect collisions for dragging UI cards but also as a UI layer transform for cards to be placed to show a character has been played on
            GameObject uiCollider = Instantiate(uiColliderPrefab, Vector3.zero, Quaternion.identity ,uiColliderParent);
            WorldToUICollider wtuic = uiCollider.GetComponent<WorldToUICollider>();
            wtuic.combatant = member.transform;
            wtuic.worldSpaceBC = member.GetComponent<BoxCollider>();
            member.GetComponent<CombatantBasis>().uiCollider = uiCollider;
            allCombatants.Add(member);
        }
        //int random = Random.Range(0, encounterSets.Count);
        //GameObject set = Instantiate(encounterSets[random]);

        //Game

        foreach (GameObject enemy in enemies)
        {
            activeEnemies.Add(enemy);

            // This gameobject is used to detect collisions for dragging UI cards but also as a UI layer transform for cards to be placed to show a character has been played on
            GameObject uiCollider = Instantiate(uiColliderPrefab, Vector3.zero, Quaternion.identity, uiColliderParent);
            WorldToUICollider wtuic = uiCollider.GetComponent<WorldToUICollider>();
            wtuic.combatant = enemy.transform;
            wtuic.worldSpaceBC = enemy.GetComponent<BoxCollider>();
            enemy.GetComponent<CombatantBasis>().uiCollider = uiCollider;
            allCombatants.Add(enemy);
        }
        // Populate Partymembers and enemies


        ToggleDrawButtons(false);

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


        actionOrderUI.Init(allCombatants, this);
        actionOrderUI.UpdateOrder(actionOrder);

        // TODO : for CV I need an initial draw phase before play to allow the player to tell me their cards 
        // (or we can honestly just forego trying to track that)
        ActivatePlayPhase();
    }

    public void ActivateDrawPhase()
    {
        currentPhase = CombatPhase.DrawPhase;
        currentPhaseText.text = "Draw Phase";

        if (!IsInCVMode)
        {
            chc.UpdateDropZones();
            ToggleDrawButtons(true);
        }
        foreach (GameObject member in partyMembers)
        {
            CombatantBasis cb = member.GetComponent<CombatantBasis>();
            if (cb.appliedCard != null && !cb.isChanneling) // Check to see if card is delay turn card in which case to not set to null
            {
                cb.appliedCard.transform.SetParent(chc.discardPile.transform);
                cb.appliedCard.transform.position = chc.discardPile.transform.position;
                cb.appliedCard.transform.localScale = new Vector3(1,1,1);

                cb.appliedCard = null;
            }
        }
        foreach (GameObject enemy in enemies)
        {
            CombatantBasis cb = enemy.GetComponent<CombatantBasis>();
            if (cb.appliedCard != null && !cb.isChanneling) // Check to see if card is delay turn card in which case to not set to null
            {
                cb.appliedCard.transform.SetParent(chc.discardPile.transform);
                cb.appliedCard.transform.position = chc.discardPile.transform.position;
                cb.appliedCard.transform.localScale = new Vector3(1, 1, 1);
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
        actionOrderUI.SetAllActive(actionOrder);
        actionOrderUI.UpdateOrder(actionOrder);

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
        if (!IsInCVMode)
        {
            chc.UpdateDropZones();

            ToggleDrawButtons(false);
        }
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
        if (!IsInCVMode)
        {
            chc.UpdateDropZones();
        }
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
        if (!IsInCVMode)
            chc.UpdateDropZones();

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

            pointer.SetActive(true);
            Vector3 pointerPos = cb.uiCollider.transform.position;
            if(cb.isEnemy)
            {
                pointerPos.x += cb.uiCollider.GetComponent<WorldToUICollider>().cardLocationOffset * -2;
                pointer.transform.rotation = Quaternion.Euler(0, 0, 90);
            } else
            {
                pointerPos.x += cb.uiCollider.GetComponent<WorldToUICollider>().cardLocationOffset * 2;
                pointer.transform.rotation = Quaternion.Euler(0, 0, -90);
            }
            pointer.transform.position = pointerPos;

            bool cardAlreadyPlayed = false;

            if (cb.appliedCard != null)
                cardAlreadyPlayed = true;

            if (enoughMana && !cardAlreadyPlayed && chc.transform.childCount != 0)
            {
                Debug.Log("Play card on " + actionOrder[0].name);
                foreach (GameObject card in Deck.instance.allCards)
                {
                    DragDrop dd = card.GetComponent<DragDrop>();
                    List<GameObject> allZones = new List<GameObject>();
                    allZones.Add(cb.uiCollider);
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

                    // TODO : this would continue if the player has already played a card, giving them no opportunity to play another...
                    // TODO : For many of these while (!done) loops I'd recommend making a request to the controller (CV or Hand) which in turn calls back with a function when it's has an update, and then you continue
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
            actionOrderUI.UpdateOrder(actionOrder);
            RemoveFallenCombatants();
            if(CheckWinCondition())
            {
                yield break;
            }
            UpdateTargets();
            CheckEnoughMana();
            pointer.SetActive(false);
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
                if(cb.appliedCard != null)
                {
                    cb.appliedCard.transform.SetParent(chc.discardPile.transform);
                    cb.appliedCard.transform.position = chc.discardPile.transform.position;
                    cb.appliedCard.transform.localScale = new Vector3(1, 1, 1);
                }
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
                if(cb.appliedCard != null)
                {
                    cb.appliedCard.transform.SetParent(chc.discardPile.transform);
                    cb.appliedCard.transform.position = chc.discardPile.transform.position;
                    cb.appliedCard.transform.localScale = new Vector3(1, 1, 1);
                }
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

    public bool ApplyCard(GameObject card, GameObject combatant)
    {
        CombatantBasis cb = combatant.GetComponent<CombatantBasis>();
        Card cardScript = card.GetComponent<Card>();
        if (cardScript.isWild && lastPlayedCard == null)
        {
            Debug.Log("cannot play Wild Card without a previously played card");
            card.transform.SetParent(chc.gameObject.transform);
            card.transform.localScale = new Vector3(1, 1, 1);
            cb.appliedCard = null;
            return false;
        }
        if (!IsInCVMode)
        {
            if (cb.appliedCard != null)
            {
                card.transform.SetParent(chc.gameObject.transform);
                card.transform.localScale = new Vector3(1, 1, 1);
                Debug.Log("Card Already Played On This Combatant");
                return false;
            }
            if (currentMana - cardScript.manaCost < 0)
            {
                Debug.Log("Not Enough Mana To Play This Card");
                card.transform.SetParent(chc.gameObject.transform);
                card.transform.localScale = new Vector3(1, 1, 1);
                cb.appliedCard = null;
                return false;
            }
        }
        else
        {
            if (cb.appliedCard != null)
            {
                Debug.Log("Card Already Played On This Combatant");
                return false;
            }
            if (currentMana - cardScript.manaCost < 0)
            {
                Debug.Log("Not Enough Mana To Play This Card");
                return false;
            }
        }
        print("APPLIED " + card.name + " TO " + combatant);
        cb.appliedCard = card;
        currentMana -= cardScript.manaCost;
        manaText.text = "Mana: " + currentMana + "/" + maxMana;
        if (!cardScript.isWild)
        {
            lastPlayedCard = card;
        }
        card.transform.SetParent(combatant.GetComponent<CombatantBasis>().uiCollider.transform);
        card.transform.localScale = new Vector3(1, 1, 1);
        card.GetComponent<DragDrop>().isDraggable = false;

        if(!cb.isEnemy)
        {
            cardScript.Play(combatant, partyMembers);
        } else
        {
            cardScript.Play(combatant, enemies);
        }

        Deck.instance.Discard(card);
        print(card.transform.lossyScale);
        //card.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        return true;
    }

    public void DrawCards(int cardsToDraw)
    {
        if (!IsInCVMode)
            chc.DrawCards(cardsToDraw);
    }

    public void DiscardCard(GameObject card)
    {
        currentMana -= discardCost;
        manaText.text = "Mana: " + currentMana + "/" + maxMana;
        if (!IsInCVMode)
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
        actionOrderUI.UpdateOrder(actionOrder);
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

    public bool CheckWinCondition()
    {
        if (activeEnemies.Count == 0)
        {
            Debug.Log("You Win!");
            StopAllCoroutines();
            chc.ResetCardParents();
            encounterScript.ReturnToOverWorld();
            return true;
        }
        else if (activePartyMembers.Count == 0)
        {
            Debug.Log("You Lose...");
            StopAllCoroutines();
            chc.ResetCardParents();
            encounterScript.ReturnToOverWorld();
            return true;
        }
        return false;
    }
}
