using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
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


    public enum CombatPhase {DrawPhase, PlayPhase, DiscardPhase, ActionPhase, EndPhase, None };



    private UnityEvent<CombatPhase, CombatPhase> PhaseStepEvent = new UnityEvent<CombatPhase, CombatPhase>();
    private UnityEvent<CombatPhase> RequestInputForPhaseEvent = new UnityEvent<CombatPhase>();

    public CombatPhase currentPhase = CombatPhase.None;
    public CombatHandController chc;
    public List<Button> drawButtons = new List<Button>();
    public Button reshuffleButton;
    public Text currentPhaseText;
    public Text manaText;

    public List<GameObject> initialPartyMembers = new List<GameObject>();
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
    public Canvas canvas;

    public GameObject lastPlayedCard;

    public int maxMana = 30;
    public int currentMana = 20;
    public int discardCost = 1;


    public bool canDraw = false;
    public bool canPlay = true;
    private bool enoughMana = true;

    PauseManager pauseManager = null;

    public List<Transform> timeOutSpaces = new List<Transform>();
    private int timeOutIndex = 0;

    public GameObject tutorialUIParent;
    public List<GameObject> tutorialTexts = new List<GameObject>();


    // Start is called before the first frame update
    void Start()
    {
        if (pauseManager == null)
        {
            pauseManager = FindObjectOfType<PauseManager>();
        }

        
        initialPartyMembers.Add(partyMembers[0]);

        //pauseManager.AddPartyMember("priest", true);
        //PlayerPrefs.SetInt("hunter", 0);
        //PlayerPrefs.SetInt("warrior", 0);
        //PlayerPrefs.SetInt("mechanist", 0);

        if (PlayerPrefs.GetInt("hunter") == 0)
        {
            partyMembers[1].SetActive(false);
            partyMembers[0].transform.position += new Vector3(2, 0, -3);
        } else
        {
            initialPartyMembers.Add(partyMembers[1]);
        }
        if(PlayerPrefs.GetInt("warrior") == 0)
        {
            partyMembers[2].SetActive(false);
            if (PlayerPrefs.GetInt("hunter") == 1)
            {
                partyMembers[0].transform.position += new Vector3(2, 0, -1);
                partyMembers[1].transform.position += new Vector3(-1.75f, 0, -4);
            }
        } else
        {
            initialPartyMembers.Add(partyMembers[2]);
        }
        if (PlayerPrefs.GetInt("mechanist") == 0)
        {
            partyMembers[3].SetActive(false);
        } else
        {
            initialPartyMembers.Add(partyMembers[3]);
        }

        List<GameObject> allCombatants = new List<GameObject>();
        foreach (GameObject member in initialPartyMembers)
        {
            member.SetActive(true);
            activePartyMembers.Add(member);
            // This gameobject is used to detect collisions for dragging UI cards but also as a UI layer transform for cards to be placed to show a character has been played on
            GameObject uiCollider = Instantiate(uiColliderPrefab, Vector3.zero, Quaternion.identity ,uiColliderParent);
            WorldToUICollider wtuic = uiCollider.GetComponent<WorldToUICollider>();
            wtuic.combatant = member.transform;
            wtuic.worldSpaceBC = member.GetComponent<BoxCollider>();
            wtuic.canvas = canvas;
            member.GetComponent<CombatantBasis>().uiCollider = uiCollider;
            allCombatants.Add(member);

            pauseManager.LinkCombatant(member.GetComponent<CombatantBasis>().combatantName.ToLower(), member);
            member.GetComponent<CombatantBasis>().pauseManager = pauseManager;
        }

        if (PlayerPrefs.HasKey("priestCurrHealth"))
        {
            partyMembers[0].GetComponent<CombatantBasis>().currentHitPoints = PlayerPrefs.GetInt("priestCurrHealth");
            partyMembers[0].GetComponent<CombatantBasis>().healthBar.SetHealth(PlayerPrefs.GetInt("priestCurrHealth"));
        }
        if (PlayerPrefs.HasKey("hunterCurrHealth"))
        {
            partyMembers[1].GetComponent<CombatantBasis>().currentHitPoints = PlayerPrefs.GetInt("hunterCurrHealth");
            partyMembers[1].GetComponent<CombatantBasis>().healthBar.SetHealth(PlayerPrefs.GetInt("hunterCurrHealth"));
        }
        if (PlayerPrefs.HasKey("warriorCurrHealth"))
        {
            partyMembers[2].GetComponent<CombatantBasis>().currentHitPoints = PlayerPrefs.GetInt("warriorCurrHealth");
            partyMembers[2].GetComponent<CombatantBasis>().healthBar.SetHealth(PlayerPrefs.GetInt("warriorCurrHealth"));
        }
        if (PlayerPrefs.HasKey("mechanistCurrHealth"))
        {
            partyMembers[3].GetComponent<CombatantBasis>().currentHitPoints = PlayerPrefs.GetInt("mechanistCurrHealth");
            partyMembers[3].GetComponent<CombatantBasis>().healthBar.SetHealth(PlayerPrefs.GetInt("mechanistCurrHealth"));
        }

        GameObject encounter = GameObject.FindGameObjectWithTag("Encounter");
        Transform[] encounterCombatants = encounter.GetComponentsInChildren<Transform>();
        enemies.Clear();
        foreach (Transform enemy in encounterCombatants)
        {
            if(enemy.gameObject.GetComponent<CombatantBasis>() != null)
                enemies.Add(enemy.gameObject);
        }

        foreach (GameObject enemy in enemies)
        {
            activeEnemies.Add(enemy);

            // This gameobject is used to detect collisions for dragging UI cards but also as a UI layer transform for cards to be placed to show a character has been played on
            GameObject uiCollider = Instantiate(uiColliderPrefab, Vector3.zero, Quaternion.identity, uiColliderParent);
            WorldToUICollider wtuic = uiCollider.GetComponent<WorldToUICollider>();
            wtuic.combatant = enemy.transform;
            wtuic.worldSpaceBC = enemy.GetComponent<BoxCollider>();
            wtuic.canvas = canvas;
            enemy.GetComponent<CombatantBasis>().uiCollider = uiCollider;
            allCombatants.Add(enemy);

            enemy.GetComponent<CombatantBasis>().pauseManager = pauseManager;
        }
        // Populate Partymembers and enemies

        ToggleDrawButtons(false);
        reshuffleButton.interactable = false;

        foreach (GameObject member in initialPartyMembers)
        {
            CombatantBasis memberScript = member.GetComponent<CombatantBasis>();
            memberScript.SelectAction();
            if (memberScript.nextAction == CombatantBasis.Action.Block)
            {
                memberScript.text.text = "Block";
            }
        }

        foreach (GameObject enemy in enemies)
        {
            CombatantBasis enemyScript = enemy.GetComponent<CombatantBasis>();
            enemyScript.SelectAction();
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
        if (!IsInCVMode) {
            ActivatePlayPhase();
        }
        else  {
            ActivateDrawPhase();
        }
    }

    // Used by CV to have a better understanding of the manager's state and respond in real time
    public void SubscribeAsController(
        UnityAction<CombatPhase, CombatPhase> handleStepFunc,
        UnityAction<CombatPhase> handleInputRequestFunc)
    {
        PhaseStepEvent.AddListener(handleStepFunc);
        RequestInputForPhaseEvent.AddListener(handleInputRequestFunc);
    }

    public void ActivateDrawPhase()
    {
        PhaseStepEvent.Invoke(currentPhase, CombatPhase.DrawPhase);
        currentPhase = CombatPhase.DrawPhase;
        currentPhaseText.text = "Draw Phase";

        if (!IsInCVMode)
        {
            chc.UpdateDropZones();
            ToggleDrawButtons(true);
        }
        foreach (GameObject member in activePartyMembers)
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

        foreach (GameObject member in activePartyMembers)
        {
            CombatantBasis memberScript = member.GetComponent<CombatantBasis>();
            memberScript.SelectAction();
            if (memberScript.nextAction == CombatantBasis.Action.Block)
            {
                memberScript.text.text = "Block";
            }
        }

        foreach (GameObject enemy in activeEnemies)
        {
            CombatantBasis enemyScript = enemy.GetComponent<CombatantBasis>();
            enemyScript.SelectAction();
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
        RequestInputForPhaseEvent.Invoke(CombatPhase.DrawPhase);
    }

    public IEnumerator DrawPhaseCoroutine()
    {
        if (!IsInCVMode) // in CV mode, the CV manager will make this call
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
        }
        yield return null;
    }

    public void ActivatePlayPhase()
    {
        PhaseStepEvent.Invoke(currentPhase, CombatPhase.PlayPhase);
        currentPhase = CombatPhase.PlayPhase;
        if (!IsInCVMode)
            chc.UpdateDropZones(); // CV TODO : drop zomes could be a thing to disable in CV mode

        if (!IsInCVMode)
        {
            ToggleDrawButtons(false);
        }
        // Allow player to move cards to play on allies/enemies, update action order accordingly, ends when player clicks done or something, transition to Discard Phase
        StartCoroutine("PlayPhaseCoroutine");
        RequestInputForPhaseEvent.Invoke(CombatPhase.PlayPhase);
    }

    public IEnumerator PlayPhaseCoroutine()
    {
        if (!IsInCVMode) // if in CV mode, the CV controller will take care of switching
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
        }
        yield return null;
    }

    public void ActivateDiscardPhase()
    {
        PhaseStepEvent.Invoke(currentPhase, CombatPhase.DiscardPhase);
        currentPhase = CombatPhase.DiscardPhase;
        if (!IsInCVMode)
        {
            chc.UpdateDropZones();
        }
        // Player can drag cards to discard pile to discard them, ends when player clicks done or something, transition to Action Phase
        reshuffleButton.interactable = true;
        StartCoroutine("DiscardPhaseCoroutine");
        RequestInputForPhaseEvent.Invoke(CombatPhase.DiscardPhase);
    }

    public IEnumerator DiscardPhaseCoroutine()
    {
        if (!IsInCVMode) // if in CV mode, the CV controller will take care of switching
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
        }
        yield return null;
    }

    public void ActivateActionPhase()
    {
        PhaseStepEvent.Invoke(currentPhase, CombatPhase.ActionPhase);
        currentPhase = CombatPhase.ActionPhase;
        if (!IsInCVMode)
            chc.UpdateDropZones();
        reshuffleButton.interactable = false;
        currentPhaseText.text = "Action Phase";
        // Party members and enemies take turns attacking in action order, death prevents attacking, transition to Draw Phase
        StartCoroutine("StartActions");
    }

    private bool cvReadyForMoreActions = false;
    public void CVReadyToContinueActions()
    {
        cvReadyForMoreActions = true;
    }

    public bool IsReadyToContinueActions()
    {
        if (!IsInCVMode)
        {
            return Input.GetKeyDown(KeyCode.Space);
        } 
        else if (cvReadyForMoreActions)
        {
            cvReadyForMoreActions = false;
            return true;
        }
        else
            return false;
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

            // TODO : might be nice to overwite cards, which this does not allow
            // TODO : that or condition is like a bandaid on cancer
            
            Debug.Log("Play card on " + actionOrder[0].name);
            foreach (GameObject card in Deck.instance.allCards)
            {
                DragDrop dd = card.GetComponent<DragDrop>();
                List<GameObject> allZones = new List<GameObject>();
                allZones.Add(cb.uiCollider);
                dd.allowedDropZones.Clear();
                dd.allowedDropZones = allZones;
            }
            RequestInputForPhaseEvent.Invoke(CombatPhase.ActionPhase);
            yield return new WaitUntil(IsReadyToContinueActions);
                
            /*bool done = false;
            while (!done)
            {
                // skips when space is hit
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    done = true;
                }

                // Need code to detect if card has been applied

                // TODO : this would continue if the player has already played a card, giving them no opportunity to play another...
                // TODO : For many of these while (!done) loops I'd recommend making a request to the controller (CV or Hand) 
                //        which in turn calls back with a function when it's has an update, and then you continue
                if (cb.appliedCard != null)
                {
                    // Activate Card Effect
                    done = true;

                }
                yield return null;
            }*/
            

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
            // 

            if (actionOrder.Count > 0)
                PhaseStepEvent.Invoke(currentPhase, CombatPhase.ActionPhase);

        }

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
                    cb.SelectTarget();
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
                    cb.SelectTarget();
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

    // Note for Jay: Can be called to switch phase for all phases except ActionPhase, which is handled by CVReadyToContinueActions() instead
    public void NextPhase()
    {
        print("Current Phase " + currentPhase);
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

    public void PutPartyMemberInTimeOut(GameObject partyMember)
    {
        partyMember.transform.position = timeOutSpaces[timeOutIndex].position;
        timeOutIndex++;
    }

    public void CheckEnoughMana()
    {
        // TODO : yea I don't know what to do here, but I want my controller in charge of this, not the combatmanager.
        if (IsInCVMode)
        {
            enoughMana = true;
            return;
        }

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
            card.transform.localPosition = Vector3.zero;
            card.transform.localScale = new Vector3(1, 1, 1);
            cb.appliedCard = null;
            return false;
        }
        if (!IsInCVMode)
        {
            if (cb.appliedCard != null)
            {
                card.transform.SetParent(chc.gameObject.transform);
                card.transform.localScale = new Vector3(chc.cardLocalScale, chc.cardLocalScale, chc.cardLocalScale);
                card.transform.localPosition = Vector3.zero;
                Debug.Log("Card Already Played On This Combatant");
                return false;
            }
            if (currentMana - cardScript.manaCost < 0)
            {
                Debug.Log("Not Enough Mana To Play This Card");
                card.transform.SetParent(chc.gameObject.transform);
                card.transform.localScale = new Vector3(chc.cardLocalScale, chc.cardLocalScale, chc.cardLocalScale);
                card.transform.localPosition = Vector3.zero;
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
        manaText.text = "Mana: " + currentMana + "/" + maxMana;
        if (!cardScript.isWild)
        {
            lastPlayedCard = card;
        }
        card.transform.SetParent(combatant.GetComponent<CombatantBasis>().uiCollider.transform);
        card.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        // card.transform.SetParent(combatant.transform);
        // card.transform.position = Vector3.zero;
        card.transform.localPosition = Vector3.zero;
        card.GetComponent<DragDrop>().isDraggable = false;

        if(!cb.isEnemy)
        {
            cardScript.Play(combatant, activePartyMembers);
        } else
        {
            cardScript.Play(combatant, activeEnemies);
        }

        pauseManager.RefreshPartyView();

        // this works for CV but I really don't like it being here - JAY
        chc.DiscardCard(card);
        print(card.transform.lossyScale);
        //card.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        return true;
    }

    //specifically used to call a card play effect without handling certain checks and movement for the Echo card
    public bool ApplyCardEffect(GameObject card, GameObject combatant, GameObject wildCard)
    {
        CombatantBasis cb = combatant.GetComponent<CombatantBasis>();
        Card cardScript = card.GetComponent<Card>();
        
        if (!IsInCVMode)
        {
            if (currentMana - cardScript.manaCost < 0)
            {
                Debug.Log("Not Enough Mana To Play This Card");
                wildCard.transform.SetParent(chc.gameObject.transform);
                wildCard.transform.localScale = new Vector3(chc.cardLocalScale, chc.cardLocalScale, chc.cardLocalScale);
                wildCard.transform.localPosition = Vector3.zero;
                cb.appliedCard = null;
                return false;
            }
        }
        else
        {
            if (currentMana - cardScript.manaCost < 0)
            {
                Debug.Log("Not Enough Mana To Play This Card");
                return false;
            }
        }

        print("APPLIED " + card.name + " TO " + combatant);
        currentMana -= cardScript.manaCost;
        manaText.text = "Mana: " + currentMana + "/" + maxMana;
        manaText.text = "Mana: " + currentMana + "/" + maxMana;

        if (!cb.isEnemy)
        {
            cardScript.Play(combatant, partyMembers);
        }
        else
        {
            cardScript.Play(combatant, enemies);
        }

        pauseManager.RefreshPartyView();
        return true;
    }

    public void DrawCards(int cardsToDraw)
    {
        if (!IsInCVMode)
        {
            if (chc.DrawCards(cardsToDraw)) ;
                NextPhase();
        }
    }

    public void DiscardCard(GameObject card)
    {
        AddMana(discardCost);
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
            int index = initialPartyMembers.IndexOf(combatant);
            if (initialPartyMembers.Count >= 2 && index >= 1 && activePartyMembers.Contains(initialPartyMembers[index - 1]))
                if (index >= 1 && activePartyMembers.Contains(initialPartyMembers[index - 1]))
                {
                    adjacent.Add(initialPartyMembers[index - 1]);
                }
            if (initialPartyMembers.Count >= 2 && index <= initialPartyMembers.Count - 2 && activePartyMembers.Contains(initialPartyMembers[index + 1]))
                if (index <= initialPartyMembers.Count - 2 && activePartyMembers.Contains(enemies[index + 1]))
                {
                    adjacent.Add(initialPartyMembers[index + 1]);
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

    public void RemovePriority(GameObject combatant)
    {
        combatant.GetComponent<CombatantBasis>().hasPriority = false;
        //if combatant is still in queue, move them to top of the order
        if (actionOrder.Contains(combatant))
        {
            actionOrder.Remove(combatant);
            InsertCombatantToActionOrder(combatant);
        }
        actionOrderUI.UpdateOrder(actionOrder);
    }

    private void InsertCombatantToActionOrder(GameObject combatant)
    {
        CombatantBasis insertCB = combatant.GetComponent<CombatantBasis>();
        float freeCombatSpeed = insertCB.speed * insertCB.speedMultiplier;
        for (int i = 0; i < actionOrder.Count; i++)
        {
            CombatantBasis cb = actionOrder[i].GetComponent<CombatantBasis>();
            if (cb.hasPriority)
                continue;
            else
            {
                //insert position has been found
                if (cb.speed * cb.speedMultiplier < freeCombatSpeed)
                {
                    actionOrder.Insert(i, combatant);
                    return;
                }
            }
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
                cb.oldTarget = cb.target;
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
                cb.oldTarget = cb.target;
                cb.target = combatant;
            }
        }
    }

    public void DropFocus(GameObject combatant)
    {
        foreach (GameObject action in actionOrder)
        {
            CombatantBasis cb = action.GetComponent<CombatantBasis>();
            if (cb.target == combatant)
            {
                cb.target = cb.oldTarget;
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
                cb.oldTarget = combatant;
                cb.SelectTarget();
            }
        }
    }
    
    public void ReinstateTarget(GameObject combatant)
    {
        //focuses any remaining ally actions onto untarget this combatant
        foreach (GameObject action in actionOrder)
        {
            CombatantBasis cb = action.GetComponent<CombatantBasis>();
            if (cb.oldTarget == combatant)
            {
                cb.target = combatant;
                cb.UpdateTargetName();
            }
        }
    }

    public bool CheckWinCondition()
    {
        if (activeEnemies.Count == 0)
        {
            Debug.Log("You Win!");
            StopAllCoroutines();
            PlayerPrefs.SetInt("priestCurrHealth", partyMembers[0].GetComponent<CombatantBasis>().currentHitPoints);
            PlayerPrefs.SetInt("hunterCurrHealth", partyMembers[1].GetComponent<CombatantBasis>().currentHitPoints);
            PlayerPrefs.SetInt("warriorCurrHealth", partyMembers[2].GetComponent<CombatantBasis>().currentHitPoints);
            PlayerPrefs.SetInt("mechanistCurrHealth", partyMembers[3].GetComponent<CombatantBasis>().currentHitPoints);
            PlayerPrefs.Save();
            encounterScript.ScreenWipe();
            Invoke("ReturnToOverWorld", 0.5f);
            PhaseStepEvent.Invoke(currentPhase, CombatPhase.EndPhase);
            return true;
        }
        else if (activePartyMembers.Count == 0)
        {
            Debug.Log("You Lose...");
            StopAllCoroutines();
            PlayerPrefs.SetInt("priestCurrHealth", partyMembers[0].GetComponent<CombatantBasis>().totalHitPoints);
            PlayerPrefs.SetInt("hunterCurrHealth", partyMembers[1].GetComponent<CombatantBasis>().totalHitPoints);
            PlayerPrefs.SetInt("warriorCurrHealth", partyMembers[2].GetComponent<CombatantBasis>().totalHitPoints);
            PlayerPrefs.SetInt("mechanistCurrHealth", partyMembers[3].GetComponent<CombatantBasis>().totalHitPoints);
            PlayerPrefs.Save();
            encounterScript.ScreenWipe();
            Invoke("ReturnToOverWorld", 0.5f);
            PhaseStepEvent.Invoke(currentPhase, CombatPhase.EndPhase);
            return true;
        }
        return false;
    }

    public void ReturnToOverWorld()
    {
        pauseManager.SavePartyView();
        foreach (GameObject member in partyMembers)
        {
            pauseManager.RemoveCombatant(member.GetComponent<CombatantBasis>().combatantName.ToLower());
        }
        chc.ResetCardParents();
        chc.ReturnCardsInHand();
        chc.ReShuffle();
        encounterScript.ReturnToOverWorld();
    }

    public void ActivateTutorial()
    {
        StopAllCoroutines();
        tutorialUIParent.SetActive(true);
        chc.SetUpTutorial();
    }

    public IEnumerator Tutorial()
    {
        int i = 0;
        while(i < tutorialTexts.Count-1)
        {
            tutorialTexts[i].SetActive(false);
            i++;
            tutorialTexts[i].SetActive(true);
            bool done = false;
            while (!done)
            {
                // skips when space is hit
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    done = true;
                }
                
                yield return null;
            }
        }

        SkipTutorial(true);

        yield return null;
    }

    public void SkipTutorial(bool doSkip)
    {
        if(doSkip)
        {
            tutorialUIParent.SetActive(false);
            ActivatePlayPhase();
        } else
        {
            StartCoroutine(Tutorial());
        }
    }
}
