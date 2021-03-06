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
    public RawImage planeImage;
    public RawImage goodSeeImage;
    public RawImage stickerImage1;
    public RawImage stickerImage2;
    public RawImage stickerImage3;

    private TMP_Text playText;
    private TMP_Text cardText;
    private TMP_Text phaseInfoText;

    private bool activeController;


    private CombatManager.CombatPhase lastPhase = CombatManager.CombatPhase.None;
    private CombatManager.CombatPhase currentPhase = CombatManager.CombatPhase.None;


    public void HandlePhaseStep(CombatManager.CombatPhase lastPhase, CombatManager.CombatPhase newPhase)
    {
        if (this.lastPhase != lastPhase)
            Debug.Log("Phase Mismatch! " + "Expected Last Phase: " + this.lastPhase + ", this actual last: " + lastPhase + ". Current: " + newPhase);

        this.lastPhase = lastPhase;
        this.currentPhase = newPhase;
    }

    private float timeToCompleteDraw = 1.2f; // TODO : serial later
    float timeSpentWithCard = 0.0f;
    public int maxCardsInHand = 7;

    // private List<GameObject> hand = new List<GameObject>();
    private int handCount = 0;
    private int discardCount = 0;

    private float drawFinishWaitTime = 0.7f;
    private float playFinishWaitTime = 0.7f;
    private float discardFinishWaitTime = 0.7f;


    private bool StartAdviseEnd()
    {
        return Input.GetKey(KeyCode.Space);
    }

    IEnumerator RunInitDrawPhase(int numberToDraw)
    {
        phaseInfoText.text = "Shuffle Deck";
        playText.text = "Shuffle your whole deck together then press SPACE.";
        yield return new WaitUntil(StartAdviseEnd);
        phaseInfoText.text = "Play in a Bright Environment";
        playText.text = "Please, play in a bright environment.";
        yield return new WaitForSeconds(2.5f);
        phaseInfoText.text = "You may cheat";
        playText.text = "Cheating is expressly allowed.";
        yield return new WaitForSeconds(0.3f);
        playText.text = "";

        handCount = 0;
        phaseInfoText.text = "Initial Draw Phase";
        cardParser.UpdateMode(CardParser.ParseMode.Disabled);


        while (numberToDraw > 0)
        {
            // ATTEMPT TO WAIT FOR CONSISTENT ENOUGH INPUT TO GET A CARD READ, MAY BE TOO MUCH FOR CURRENT ITERATION
            timeSpentWithCard = 0;
            
            while (!Input.GetKeyDown(KeyCode.Space))
            {
                /*if (numCardsSeen > 0)
                {
                    // update and progress
                    playText.text = "Drawing 1 Card...";
                    progressIndicator.fillAmount = Mathf.Min(1.0f, timeSpentWithCard / timeToCompleteDraw);
                    timeSpentWithCard += Time.deltaTime;

                    if (Input.GetKeyDown(KeyCode.Space))
                        break;
                } else
                {*/
                playText.text = "You must draw " + numberToDraw + " more cards, draw a card and press SPACE with it.";
                // timeSpentWithCard = Mathf.Max(0, timeSpentWithCard - (3 * Time.deltaTime));
                progressIndicator.fillAmount = 0; // Mathf.Min(1.0f, timeSpentWithCard / timeToCompleteDraw);
                // }

                yield return null;
            }
            
            // DRAW CARD FOR REAL
            playText.text = "Drew 1 card.";
            yield return new WaitForSeconds(0.6f);
            handCount += 1;

            numberToDraw -= 1;
            yield return null;
        }

        // finish drawing and continue
        playText.text = "";
        phaseInfoText.text = "";
        for (float t = 0; t < drawFinishWaitTime; t += Time.deltaTime)
        {
            progressIndicator.fillAmount = (t / drawFinishWaitTime);
            yield return null;
        }
        timeSpentWithCard = 0.0f;
        progressIndicator.fillAmount = 0.0f;
        currentInputHandler = null;

        cm.NextPhase();
    }

    public void HandleNewImage(WebCamTexture webCamTexture)
    {
        if (cardParser.GetMode() != CardParser.ParseMode.Disabled)
            cardParser.ProcessTexture(webCamTexture);
    }

    int[] manaToDrawCounts = new int[] { 18, 16, 14, 12, 10 }; 
    private int ShuffleCost()
    {
        return Math.Max(15 - (Deck.instance.discard.Count + discardCount), 0);
    }

    IEnumerator RunDrawPhase()
    {
        int totalCardsDrawn = 0;
        cardParser.UpdateMode(CardParser.ParseMode.Disabled);
        phaseInfoText.text = "Draw Phase. Press Enter to stop drawing. " 
            + ((cm.currentMana >= ShuffleCost() && (Deck.instance.discard.Count + discardCount) > 0) ? "R to spend {ShuffleCost()} mana to shuffle discards into your deck." : "");

        playText.text = "Draw Cards up to 4 cards, max hand size of " + maxCardsInHand + ". Press SPACE with a drawn card."
            + "\nYou will gain " + manaToDrawCounts[totalCardsDrawn] + " mana if you stop drawing now.\n";
        while (maxCardsInHand > handCount && totalCardsDrawn < 4)
        {
            // ATTEMPT TO WAIT FOR CONSISTENT ENOUGH INPUT TO GET A CARD READ, MAY BE TOO MUCH FOR CURRENT ITERATION
            while (Input.GetKeyDown(KeyCode.Space))
            {

                playText.text = "Draw Cards up to 4 cards, max hand size of " + maxCardsInHand + ". Press SPACE with a drawn card."
                    + "\nYou will gain " + manaToDrawCounts[totalCardsDrawn] + " mana if you stop drawing now.\n";

                timeSpentWithCard = 0;
                progressIndicator.fillAmount = 0.0f;
                // FINISH
                if (Input.GetKeyDown(KeyCode.KeypadEnter)) goto FinishDraw;
                // SHUFFLE
                if (Input.GetKey(KeyCode.R) && cm.currentMana >= ShuffleCost())
                {
                    cm.AddMana(-ShuffleCost());
                    Deck.instance.Shuffle();
                }
                yield return null;
            }
            
            totalCardsDrawn++;
            // DRAW CARD FOR REAL
            bool canDrawMore = Math.Min(maxCardsInHand - handCount, 4 - totalCardsDrawn) > 0;
            playText.text = "Drew a card." + (canDrawMore ? ("\nYou MAY draw " + Math.Min(maxCardsInHand - handCount, 4 - totalCardsDrawn) + " more cards.") : "");
            yield return new WaitForSeconds(0.8f);
            if (!canDrawMore) goto FinishDraw; // end

            handCount += 1;

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
    private float timeToCompletePlay = 1.4f;

    private void UpdatePlayActionUI(bool validTarget, bool hasCardAttached, 
        GameObject currentCard, GameObject currentTarget, float fillMeter)
    {
        if (!validTarget)
            playText.text = "Apply cards to allies and enemies...";
        else if (hasCardAttached)
            playText.text = "You focus on " + currentTarget.name + "... but they already have a card.";
        else if (currentCard == null || Deck.instance.discard.Contains(currentCard))
            playText.text = "You focus on " + currentTarget.name + "... What card will you play?";
        else if (currentCard != null)
        {
            playText.text = "Playing " + currentCard.GetComponent<Card>().cardName + " on " + currentTarget.name + "...";
            progressIndicator.fillAmount = fillMeter;
        }
    }

    private bool CanPlayMore()
    {
        if (handCount <= 0) return false;

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
        cardParser.UpdateMode(CardParser.ParseMode.GetCardFromAll);
        currentCard = null;
        phaseInfoText.text = "Play Phase: Play cards. Press SPACE to continue.";

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
                    print("Raycasting to " + hitInfo.collider.gameObject);
                }
                else { timeSpentWithCard = 0.0f; currentTarget = null; }

                bool hasCardAttached = currentTarget != null && currentTarget.GetComponent<CombatantBasis>().appliedCard != null;
                validTarget = currentTarget != null;
                timeSpentWithCard = validTarget && !hasCardAttached ? timeSpentWithCard : 0.0f;
                timeSpentWithCard = currentCard != null ? timeSpentWithCard : 0.0f;
                timeSpentWithCard = currentCard != null && !Deck.instance.discard.Contains(currentCard) ? timeSpentWithCard : 0.0f; 

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
                timeSpentWithCard = 0.0f;
                progressIndicator.fillAmount = 0.0f;
                handCount -= 1;
                playText.text = "Played " + currentCard.GetComponent<Card>().cardName + ". Put the card in your Discard pile.";
                for (float t = 0; t < 1.0f; t += Time.deltaTime) { yield return null; if (Input.GetKeyDown(KeyCode.Space)) break; }
            } else
            {
                timeSpentWithCard = 0.0f;
                progressIndicator.fillAmount = 0.0f;
                playText.text = "You do not have enough mana to play " + currentCard.GetComponent<Card>().cardName + ".";
                for (float t = 0; t < 1.0f; t += Time.deltaTime) { yield return null; if (Input.GetKeyDown(KeyCode.Space)) break; }
            }
            yield return null;
        }

    FinishPlay:
        cardParser.UpdateMode(CardParser.ParseMode.Disabled);
        // visual of progressing and then
        phaseInfoText.text = "";
        playText.text = "Cards played...";
        for (float t = 0; t < playFinishWaitTime; t += Time.deltaTime)
        {
            progressIndicator.fillAmount = (t / playFinishWaitTime);
            yield return null;
        }
        progressIndicator.fillAmount = 0.0f;
        currentInputHandler = null;

        cm.NextPhase();
    }

    void OnDestroy()
    {
        instance = null;
        print("DISABLE");
    }



    IEnumerator RunDiscardPhase()
    {
        phaseInfoText.text = "Discard Phase. Press ENTER to continue.";
        cardParser.UpdateMode(CardParser.ParseMode.Disabled);
        int manaUp = 0;

        while (handCount > 0)
        {
            while (!Input.GetKeyDown(KeyCode.Space))
            {               
                playText.text = "Press SPACE using a card in your hand to discard it." + ". (Mana Gain per Card: " + 1 + ")";
                if (Input.GetKeyDown(KeyCode.KeypadEnter)) goto FinishDiscard;
                yield return null;
            }

            // APPLY THE CARD TO A VALID TARGET
            manaUp += 1;
            handCount -= 1;
            discardCount += 1;
            cm.AddMana(1);
            progressIndicator.fillAmount = 0.0f;
            playText.text = "Discarded card. Put the card in the discard pile.";
            timeSpentWithCard = 0.0f;
            yield return new WaitForSeconds(0.7f);
        }


    FinishDiscard:
        // visual of progressing and then
        playText.text = "Cards discarded, " + manaUp + " mana awarded.";
        phaseInfoText.text = "";
        for (float t = 0; t < discardFinishWaitTime; t += Time.deltaTime)
        {
            progressIndicator.fillAmount = (t / discardFinishWaitTime);
            yield return null;
        }
        progressIndicator.fillAmount = 0.0f;
        timeSpentWithCard = 0;
        playText.text = "";
        currentInputHandler = null;
        cm.NextPhase();
    }


    IEnumerator RunActionPhase()
    {
        cardParser.UpdateMode(CardParser.ParseMode.GetCardFromAll);

        CombatantBasis cb = cm.actionOrder[0].GetComponent<CombatantBasis>();

        phaseInfoText.text = "Action Phase for " + cb.name + ". ";
        if (cb.appliedCard != null)
        {
            playText.text = "Cannot play a card because " + cb.combatantName + " already has card applied.";
            yield return new WaitForSeconds(1.1f);
            goto FinishAction;
        } else if (CanPlayMore())
        {
            // TRACK IN A LOOP TO NET A CARD
            timeSpentWithCard = 0.0f;
            while (timeSpentWithCard < timeToCompletePlay)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    playText.text = "Skipping play on " + cb.combatantName + "...";
                    yield return new WaitForSeconds(0.7f);
                    goto FinishAction;
                }


                if (currentCard == null || Deck.instance.discard.Contains(currentCard))
                {
                    playText.text = "Play Card on " + cb.combatantName + "? (Or skip with SPACE.)";
                    timeSpentWithCard = Mathf.Max(0, timeSpentWithCard - (Time.deltaTime * 3));
                    progressIndicator.fillAmount = timeSpentWithCard / timeToCompletePlay;
                }
                else
                {
                    playText.text = "Playing " + currentCard.GetComponent<Card>().cardName + " on " + cb.combatantName + "...";
                    timeSpentWithCard += Time.deltaTime;
                    progressIndicator.fillAmount = timeSpentWithCard / timeToCompletePlay;
                }
                yield return null;
            }

            // apply card should add the card back to the discard pile of the deck
            cm.ApplyCard(currentCard, cm.actionOrder[0]);
            handCount -= 1;
            goto FinishAction;
        }

        if (!CanPlayMore())
        {
            playText.text = "Cannot play card. Your hand is empty.";
            yield return new WaitForSeconds(0.7f);
        }

    FinishAction:

        cardParser.UpdateMode(CardParser.ParseMode.Disabled);
        progressIndicator.fillAmount = 0.0f;
        phaseInfoText.text = "";
        playText.text = "";
        currentInputHandler = null;
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
                Deck.instance.Shuffle();
                activeController = false;
                handCount = 0;
                discardCount = 0;
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

        activeController = CombatManager.IsInCVMode();
        DisplayCardData(null, null);
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
        cardParser.NumberCardsSeenEvent.AddListener(HandleNumberSeenUpdate);

        cardParser.UpdateMode(CardParser.ParseMode.Disabled);

        SetUpOrderedCards(Deck.instance.allCards);
    }

    public void DisplayCardData(GameObject card, Mat goodPlaneImage)
    {
        planeImage.enabled = (card != null && goodPlaneImage != null);
        if (planeImage.texture != null)
            Destroy(planeImage.texture);

        if (card != null && goodPlaneImage != null)
        {
            planeImage.texture = OpenCvSharp.Unity.MatToTexture(goodPlaneImage);
            cardText.text = card.GetComponent<Card>().cardName;
        }
        else
        {
            cardText.text = "No Card Detected";
        }
    }

    public void HideCardData()
    {
        planeImage.enabled = false;
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

    private int numCardsSeen = 0;
    public void HandleNumberSeenUpdate(int num)
    {
        numCardsSeen = num;
        HideCardData();
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
        foreach (GameObject c in Deck.instance.deck)
        {
            if (c.GetComponent<Card>().cardName == name)
                cardList.Add(c);
        }
        if (cardList.Count > 0)
            return cardList;


        // If we got no cards from phase specific, get from all
        foreach (GameObject c in Deck.instance.allCards)
        {
            if (c.GetComponent<Card>().cardName == name)
                cardList.Add(c);
        }
        return cardList;
    }
    
    public void UpdateStickerDebugs(int i, Mat sticker)
    {
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
        if (stickerImage == null) return;

        if (stickerImage.texture != null)
            Destroy(stickerImage.texture);

        stickerImage.texture = OpenCvSharp.Unity.MatToTexture(sticker);
    }

    public void UpdateSeenImage(Mat blackout)
    {
        if (goodSeeImage == null) return;

        if (goodSeeImage.texture != null)
            Destroy(goodSeeImage.texture);
        goodSeeImage.texture = OpenCvSharp.Unity.MatToTexture(blackout);
        // throw new NotImplementedException();
    }

    // TODO 
    // POSSIBLE BUG : CAN DRAG THE NEWLY CREATED CARDS BACK ONTO THE HAND THAT SHOULDN'T EXIST?? WE'LL SEE
    // 
}
