using OpenCvSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CardParser))]
public class CardParserManager : MonoBehaviour
{
    public static CardParserManager instance;
    public GameObject currentCard;
    public CardParser cardParser;
    private GameObject currentTarget = null;
    public CVControllerBackLoader cInterface;

    private CombatManager cm;
    private Image progressIndicator;
    private RawImage planeImage;
    private RawImage goodSeeImage;
    private RawImage stickerImage1;
    private RawImage stickerImage2;
    private RawImage stickerImage3;

    private TMP_Text playText;
    private TMP_Text cardText;
    private TMP_Text phaseInfoText;

    private bool activeController;


    private CombatManager.CombatPhase lastPhase = CombatManager.CombatPhase.None;
    private CombatManager.CombatPhase currentPhase = CombatManager.CombatPhase.None;


    public void HandlePhaseStep(CombatManager.CombatPhase lastPhase, CombatManager.CombatPhase newPhase)
    {
        if (this.lastPhase != lastPhase)
            Debug.LogError("Phase Mismatch!" + "Expected Last Phase: " + this.lastPhase + ", this actual last: " + lastPhase + ". Current: " + newPhase);

        this.lastPhase = lastPhase;
        this.currentPhase = newPhase;
    }

    public float timeToCompleteDraw = 2.5f;
    float timeSpentWithCard = 0.0f;
    public int maxCardsInHand = 7;

    private List<GameObject> hand = new List<GameObject>();

    private float drawFinishWaitTime = 1.5f;
    private float playFinishWaitTime = 1.5f;
    private float discardFinishWaitTime = 1.5f;


    

    IEnumerator RunInitDrawPhase(int numberToDraw)
    {
        hand.Clear();
        phaseInfoText.text = "Initial Draw Phase";
        cardParser.UpdateMode(CardParser.ParseMode.HandMode);

        while (numberToDraw > 0)
        {
            // ATTEMPT TO WAIT FOR CONSISTENT ENOUGH INPUT TO GET A CARD READ, MAY BE TOO MUCH FOR CURRENT ITERATION
            timeSpentWithCard = 0;
            while (timeSpentWithCard < timeToCompleteDraw)
            {
                if (currentCard != null && Deck.instance.deck.Contains(currentCard))
                {
                    // update and progress
                    playText.text = "Drawing Card " + currentCard.GetComponent<Card>().cardName;
                    progressIndicator.fillAmount = Mathf.Min(1.0f, timeSpentWithCard / timeToCompleteDraw);
                    timeSpentWithCard += Time.deltaTime;

                    if (Input.GetKeyDown(KeyCode.Space))
                        break;
                } else
                {
                    playText.text = "You must draw " + numberToDraw + " more cards, show them to the portal.";
                    progressIndicator.fillAmount = 0.0f;
                }

                yield return null;
            }

            if (currentCard == null)
                Debug.LogError("ERROR: Bad current card");

            // DRAW CARD FOR REAL
            playText.text = "Drew " + currentCard.GetComponent<Card>().cardName;
            yield return new WaitForSeconds(1.0f);
            Deck.instance.deck.Remove(currentCard);
            hand.Add(currentCard);

            numberToDraw -= 1;
            yield return null;
        }

        // finish drawing and continue
        playText.text = "Initial Cards Drawn, progressing...";
        phaseInfoText.text = "";
        for (float t = 0; t < drawFinishWaitTime; t += Time.deltaTime)
        {
            progressIndicator.fillAmount = (t / drawFinishWaitTime);
            yield return null;
        }
        timeSpentWithCard = 0.0f;
        playText.text = "";
        progressIndicator.fillAmount = 0.0f;
        currentInputHandler = null;
        // TODO : next phase
        cm.NextPhase();
    }

    public void HandleNewImage(WebCamTexture webCamTexture)
    {
        // goodSeeImage.texture = webCamTexture;
        cardParser.ProcessTexture(webCamTexture);
    }

    IEnumerator RunDrawPhase()
    {
        cardParser.UpdateMode(CardParser.ParseMode.HandMode);
        phaseInfoText.text = "Draw Cards up to 4 cards, max hand size of " + maxCardsInHand 
            + ". press Space to stop drawing.\n1 Draw: 22 Mana, 2 Draw: 16 Mana, 3 Draw: 14 Mana, 4 Draw: 12 Mana.";

        int totalCardsDrawn = 0;
        while (maxCardsInHand > hand.Count && totalCardsDrawn < 4)
        {
            // ATTEMPT TO WAIT FOR CONSISTENT ENOUGH INPUT TO GET A CARD READ, MAY BE TOO MUCH FOR CURRENT ITERATION
            timeSpentWithCard = 0;
            while (timeSpentWithCard < timeToCompleteDraw)
            {
                if (currentCard != null && Deck.instance.deck.Contains(currentCard))
                {
                    // update and progress
                    playText.text = "Drawing Card " + currentCard.GetComponent<Card>().cardName;
                    progressIndicator.fillAmount = Mathf.Max(1.0f, timeSpentWithCard / timeToCompleteDraw);
                    timeSpentWithCard += Time.deltaTime;
                }
                else
                {
                    playText.text = "Drawing cards, max hand size of " + maxCardsInHand + ".";
                    progressIndicator.fillAmount = 0.0f;
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        goto FinishDraw;
                    }
                }

                yield return null;
            }

            // We gotten a card that passed, so add it to your hand
            if (currentCard == null)
                Debug.LogError("ERROR: Bad current card");
            totalCardsDrawn++;
            // DRAW CARD FOR REAL
            playText.text = "Drew " + currentCard.GetComponent<Card>().cardName + "."
                + "\nYou MAY draw " + Math.Min(maxCardsInHand - hand.Count, 4 - totalCardsDrawn) + " more cards.";
            yield return new WaitForSeconds(1.0f);
            Deck.instance.deck.Remove(currentCard);
            hand.Add(currentCard);

            yield return null;
        }

    FinishDraw:
        // visual of progressing and then
        timeSpentWithCard = 0.0f;
        phaseInfoText.text = "";
        playText.text = "Cards Drawn, " + (14 + (2 * (4 - totalCardsDrawn))) + " Mana awarded, Progressing...";
        cm.AddMana((14 + (2 * (4 - totalCardsDrawn))));

        for (float t = 0; t < drawFinishWaitTime; t += Time.deltaTime)
        {
            progressIndicator.fillAmount = (t / drawFinishWaitTime);
            yield return null;
        }
        progressIndicator.fillAmount = 0.0f;
        currentInputHandler = null;
        cm.NextPhase();
    }








    // TODO : make public later
    private float timeToCompletePlay = 3.0f;

    private void UpdatePlayActionUI(bool validTarget, bool hasCardAttached, 
        GameObject currentCard, GameObject currentTarget, float fillMeter)
    {
        progressIndicator.fillAmount = 0.0f;

        if (!validTarget || hasCardAttached)
            playText.text = "Apply cards to combatants...";
        else if (!hand.Contains(currentCard))
            playText.text = "You focus on " + currentTarget.name + "... What card will you play?";
        else if (currentCard != null)
        {
            playText.text = "Playing " + currentCard.GetComponent<Card>().cardName + " on " + currentTarget.name + "...";
            progressIndicator.fillAmount = fillMeter;
        }
    }

    private bool CanPlayMore()
    {
        foreach (GameObject c in cm.activeEnemies)
            if (c.GetComponent<CombatantBasis>().appliedCard == null)
                return true;
        foreach (GameObject c in cm.activePartyMembers)
            if (c.GetComponent<CombatantBasis>().appliedCard == null)
                return true;
        return false;
    }

    IEnumerator RunPlayPhase()
    {
        cardParser.UpdateMode(CardParser.ParseMode.HandMode);
        phaseInfoText.text = "Play Phase: Play cards on any combatant. Press Space to continue.";

        while (CanPlayMore())
        {
            timeSpentWithCard = 0.0f;
            currentTarget = null;
            validTarget = false;

            while (timeSpentWithCard < timeToCompletePlay) {
                // cast for and detect combatants
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo, 100.0f, LayerMask.GetMask("Combatant")))
                {
                    timeSpentWithCard = (currentTarget == hitInfo.collider.gameObject) ? timeSpentWithCard : 0.0f;
                    currentTarget = hitInfo.collider.gameObject;
                }
                else timeSpentWithCard = 0.0f;
                bool hasCardAttached = currentTarget != null && currentTarget.GetComponent<CombatantBasis>().appliedCard != null;
                validTarget = currentTarget != null && !hasCardAttached;
                timeSpentWithCard = validTarget ? timeSpentWithCard : 0.0f;
                timeSpentWithCard = currentCard != null ? timeSpentWithCard : 0.0f;
                timeSpentWithCard = hand.Contains(currentCard) ? timeSpentWithCard : 0.0f; 

                UpdatePlayActionUI(validTarget, hasCardAttached, currentCard, currentTarget, timeSpentWithCard / timeToCompletePlay);

                if (Input.GetKey(KeyCode.Space)) goto FinishPlay;

                timeSpentWithCard += Time.deltaTime;
                yield return null;
            }

            // APPLY THE CARD TO A VALID TARGET
            if (cm.currentMana >= currentCard.GetComponent<Card>().manaCost)
            {
                // TODO : this function discards the card but might not go well natively
                cm.ApplyCard(currentCard, currentTarget);
                hand.Remove(currentCard);
                timeSpentWithCard = 0.0f;
                progressIndicator.fillAmount = 0.0f;
                playText.text = "Played " + currentCard.GetComponent<Card>().cardName + " on " + currentTarget.name + ".";
                yield return new WaitForSeconds(0.5f);
            } else
            {
                timeSpentWithCard = 0.0f;
                progressIndicator.fillAmount = 0.0f;
                playText.text = "You do not have enough mana to play " + currentCard.GetComponent<Card>().cardName + ".";
                yield return new WaitForSeconds(0.5f);
            }
            yield return null;
        }

    FinishPlay:
        // visual of progressing and then
        phaseInfoText.text = "";
        playText.text = "Cards played, Progressing...";
        for (float t = 0; t < playFinishWaitTime; t += Time.deltaTime)
        {
            progressIndicator.fillAmount = (t / playFinishWaitTime);
            yield return null;
        }
        progressIndicator.fillAmount = 0.0f;
        currentInputHandler = null;
        print("MOVING ON TO NEXT PHASE FROM PLAY");
        cm.NextPhase();
    }

    void OnDestroy()
    {
        instance = null;
        print("DISABLE");
    }

    private float timeToCompleteDiscard = 2.5f;

    IEnumerator RunDiscardPhase()
    {
        phaseInfoText.text = "Discard Phase: Show cards in hand to discard them, press Space to continue.";
        cardParser.UpdateMode(CardParser.ParseMode.HandMode);
        timeSpentWithCard = 0.0f;
        int manaUp = 0;

        while (hand.Count > 0)
        {
            while (timeSpentWithCard < timeToCompletePlay)
            {
                if (hand.Contains(currentCard))
                {
                    playText.text = "Discarding " + currentCard.GetComponent<Card>().cardName + "... (Mana Increase: " + 1 + ")"; // TODO :
                    progressIndicator.fillAmount = timeSpentWithCard / timeToCompleteDiscard;
                } else
                {
                    playText.text = "Show cards from your hand to discard for mana " + ". (Total Mana Reup: " + manaUp + ")"; // TODO :
                    progressIndicator.fillAmount = 0.0f;
                }
                if (Input.GetKeyDown(KeyCode.Space)) goto FinishDiscard;
                yield return null;
            }

            // APPLY THE CARD TO A VALID TARGET
            // TODO : this function discards the card but might not go well natively
            manaUp += 1;
            hand.Remove(currentCard);
            Deck.instance.Discard(currentCard);
            timeSpentWithCard = 0.0f;
            yield return null;
        }


    FinishDiscard:
        // visual of progressing and then
        playText.text = "Cards discarded, " + manaUp + " mana awarded progressing...";
        phaseInfoText.text = "";
        for (float t = 0; t < discardFinishWaitTime; t += Time.deltaTime)
        {
            progressIndicator.fillAmount = (t / discardFinishWaitTime);
            yield return null;
        }
        progressIndicator.fillAmount = 0.0f;
        playText.text = "";
        currentInputHandler = null;
        cm.AddMana(manaUp);
        cm.NextPhase();
    }


    IEnumerator RunActionPhase()
    {
        // TODO : how do I know the guy? that's the only hard part here...
        CombatantBasis cb = cm.actionOrder[0].GetComponent<CombatantBasis>();
        phaseInfoText.text = "Action Phase for " + cb.name + ". You can play a card on them or skip with Space.";
        if (cb.appliedCard != null)
        {
            playText.text = "Currently, cannot play because " + cb.combatantName + " already has played card.";
            yield return new WaitForSeconds(0.6f);
            goto FinishAction;
        } else
        {

            // TRACK IN A LOOP TO NET A CARD
            timeSpentWithCard = 0.0f;
            while (timeSpentWithCard < timeToCompletePlay)
            {
                if (!hand.Contains(currentCard))
                {
                    playText.text = "Play Card on " + cb.combatantName + "?";
                    timeSpentWithCard = 0.0f;
                }
                else
                {
                    playText.text = "Playing " + currentCard.GetComponent<Card>().cardName + " on " + cb.combatantName + "...";
                    progressIndicator.fillAmount = timeSpentWithCard / timeToCompletePlay;

                    if (Input.GetKeyDown(KeyCode.Space)) break;
                    if (Input.GetKeyDown(KeyCode.Q)) goto FinishAction;

                    timeSpentWithCard += Time.deltaTime;
                }
                yield return null;
            }
            // TODO : applycard should add the card back to the discard pile of the deck
            cm.ApplyCard(currentCard, cm.actionOrder[0]);
            hand.Remove(currentCard);
        }

    FinishAction:
        progressIndicator.fillAmount = 0.0f;
        playText.text = "";
        currentInputHandler = null;
        // TODO : next phase
        cm.CVReadyToContinueActions();
        yield return null;
    }

    private Coroutine currentInputHandler = null;

    public void HandleRequestForInput(CombatManager.CombatPhase phase)
    {
        if (currentPhase != phase || currentInputHandler != null)
        {
            Debug.LogError("ERROR: Incorrect Data or Phase Provided to CV Controller, in " + currentPhase);
            return;
        }

        switch (phase)
        {
            case CombatManager.CombatPhase.DrawPhase:
                currentInputHandler = StartCoroutine(RunDrawPhase());
                break;
            case CombatManager.CombatPhase.PlayPhase:
                currentInputHandler = StartCoroutine(RunPlayPhase());
                break;
            case CombatManager.CombatPhase.DiscardPhase:
                currentInputHandler = StartCoroutine(RunDiscardPhase());
                break;
            case CombatManager.CombatPhase.ActionPhase:
                currentInputHandler = StartCoroutine(RunActionPhase());
                break;
            case CombatManager.CombatPhase.EndPhase:
                Deck.instance.discard.AddRange(hand); // reset deck on end
                hand.Clear();
                Deck.instance.Shuffle();
                activeController = false;
                // cardParser.SetLookForInput(false);
                break;
            case CombatManager.CombatPhase.None:
                break;
        }
    }


    public void ActivateCVForCombat(CVControllerBackLoader backLoader)
    {
        cInterface = backLoader;
        cm = backLoader.cm;
        goodSeeImage = backLoader.goodSeeImage;
        planeImage = backLoader.planeImage;
        stickerImage1 = backLoader.stickerImage1;
        stickerImage2 = backLoader.stickerImage2;
        stickerImage3 = backLoader.stickerImage3;
        progressIndicator = backLoader.progressIndicator;
        playText = backLoader.playText;
        cardText = backLoader.cardText;
        phaseInfoText = backLoader.phaseInfoText;
        cm.SubscribeAsController(HandlePhaseStep, HandleRequestForInput);

        // TODO : sanity check but might just muddle things
        HandlePhaseStep(CombatManager.CombatPhase.None, CombatManager.CombatPhase.DrawPhase);
        currentInputHandler = StartCoroutine(RunInitDrawPhase(4));

        activeController = CombatManager.IsInCVMode;
        // DisplayCardData(null, null);
    }

    private int currentID = -1;
    private bool validTarget = true;

    public Dictionary<string, List<GameObject>> orderedCards = new Dictionary<string, List<GameObject>>();

    private void SetUpOrderedCards(List<GameObject> cards)
    {
        foreach(GameObject c in cards)
        {
            Card card = c.GetComponent<Card>();
            if (!orderedCards.ContainsKey(card.cardName))
                orderedCards.Add(card.cardName, new List<GameObject>());

            orderedCards[card.cardName].Add(c);
            print("CV: Adding " + card.cardName + " which is " + c);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // singleton pattern
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        // card parser true init
        cardParser = GetComponent<CardParser>();
        cardParser.StableUpdateEvent.AddListener(HandleStableUpdate);
        cardParser.ToNullUpdateEvent.AddListener(HandleNullUpdate);
        cardParser.ToNewUpdateEvent.AddListener(HandleNewUpdate);
        cardParser.UpdateMode(CardParser.ParseMode.AllMode);

        SetUpOrderedCards(Deck.instance.allCards);
    }

    public void DisplayCardData(GameObject card, Mat goodPlaneImage)
    {
        bool inHand = hand.Contains(card);
        if (planeImage.texture != null)
            Destroy(planeImage.texture);

        if (card != null && goodPlaneImage != null)
        {
            planeImage.texture = OpenCvSharp.Unity.MatToTexture(goodPlaneImage);
            cardText.text = "Card " + card.GetComponent<Card>().cardName + (inHand ? ", in HAND" : " not in HAND");
        }
        else
        {
            cardText.text = "No Card Detected";
        }
    }

    public void HandleStableUpdate(GameObject card, int id)
    {
        currentCard = card;
        currentID = id;
        DisplayCardData(card, cardParser.GetLastGoodReplane());
    }

    public void HandleNullUpdate(GameObject card, int id)
    {
        currentCard = card;
        currentID = id;
        timeSpentWithCard = 0;
        DisplayCardData(card, null);
    }

    public void HandleNewUpdate(GameObject card, int id)
    {
        currentCard = card;
        currentID = id;
        timeSpentWithCard = 0;
        DisplayCardData(card, cardParser.GetLastGoodReplane());
    }


    public List<GameObject> GetCardsOfName(string name)
    {
        List<GameObject> cardList = new List<GameObject>();

        if (currentPhase == CombatManager.CombatPhase.DrawPhase)
        {
            foreach (GameObject c in Deck.instance.deck)
            {
                if (c.GetComponent<Card>().cardName == name)
                    cardList.Add(c);
            }
        } else
        {
            foreach (GameObject c in hand)
            {
                if (c.GetComponent<Card>().cardName == name)
                    cardList.Add(c);
            }
        }
        return cardList;
        /*
        // TODO : dependnig
        if (orderedCards.TryGetValue(name, out List<GameObject> lis))
            return lis;
        else
            return new List<GameObject>();
            */
    }
    
    public void UpdateStickerDebugs(int i, Mat sticker)
    {
        stickerImage1.enabled = stickerImage2.enabled = stickerImage3.enabled = true;
        RawImage stickerImage = null;
        switch(i)
        {
            case 0:
                stickerImage = stickerImage1;
                break;
            case 1:
                stickerImage = stickerImage2;
                break;
            case 2:
                stickerImage = stickerImage3;
                break;
        }
        if (stickerImage.texture != null)
            Destroy(stickerImage.texture);

        stickerImage.texture = OpenCvSharp.Unity.MatToTexture(sticker);
    }

    public void UpdateSeenImage(Mat blackout)
    {
        if (goodSeeImage.texture != null)
            Destroy(goodSeeImage.texture);
        goodSeeImage.texture = OpenCvSharp.Unity.MatToTexture(blackout);
        // throw new NotImplementedException();
    }

    // TODO 
    // POSSIBLE BUG : CAN DRAG THE NEWLY CREATED CARDS BACK ONTO THE HAND THAT SHOULDN'T EXIST?? WE'LL SEE
    // 
}
