using OpenCvSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(CardParser))]
public class CardParserManager : MonoBehaviour
{
    public static CardParserManager instance;
    public GameObject currentCard;
    public CardParser cardParser;
    private GameObject currentTarget = null;
    public CVControllerBackLoader cInterface;

    public CombatManager cm;
    public Image progressWheelImage;
    public RawImage planeImage;
    public RawImage goodSeeImage;
    public RawImage stickerImage1;
    public RawImage stickerImage2;
    public RawImage stickerImage3;

    public TMP_Text playText;
    public TMP_Text cardText;

    private bool activeController;


    private CombatManager.CombatPhase lastPhase = CombatManager.CombatPhase.None;
    private CombatManager.CombatPhase currentPhase = CombatManager.CombatPhase.None;

    // CVReadyToContinueActions
    // IsReadyToContinueActions

    public void HandlePhaseStep(CombatManager.CombatPhase lastPhase, CombatManager.CombatPhase newPhase)
    {
        if (this.lastPhase != lastPhase)
            Debug.LogError("Phase Mismatch!");

        this.lastPhase = lastPhase;
        this.currentPhase = newPhase;
    }

    public float timeToCompleteDraw = 3.0f;
    float timeSpentWithCard = 0.0f;
    public int maxCardsInHand = 7;

    private List<GameObject> hand = new List<GameObject>();

    private float drawFinishWaitTime = 0.5f;
    private float playFinishWaitTime = 0.5f;
    private float discardFinishWaitTime = 0.5f;




    IEnumerator RunInitDrawPhase(int numberToDraw)
    {
        hand.Clear();
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
                    progressWheelImage.fillAmount = Mathf.Max(1.0f, timeSpentWithCard / timeToCompleteDraw);
                    timeSpentWithCard += Time.deltaTime;

                    if (Input.GetKeyDown(KeyCode.Space))
                        break;
                } else
                {
                    playText.text = "You must draw " + numberToDraw + " more cards, show them to the portal.";
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
        for (float t = 0; t < drawFinishWaitTime; t += Time.deltaTime)
        {
            progressWheelImage.fillAmount = (t / drawFinishWaitTime);
            yield return null;
        }
        progressWheelImage.fillAmount = 0.0f;
        currentInputHandler = null;
        // TODO : next phase
        cm.NextPhase();
    }




    IEnumerator RunDrawPhase()
    {
        cardParser.UpdateMode(CardParser.ParseMode.HandMode);

        while (maxCardsInHand > hand.Count)
        {
            // ATTEMPT TO WAIT FOR CONSISTENT ENOUGH INPUT TO GET A CARD READ, MAY BE TOO MUCH FOR CURRENT ITERATION
            timeSpentWithCard = 0;
            while (timeSpentWithCard < timeToCompleteDraw)
            {
                if (currentCard != null && Deck.instance.deck.Contains(currentCard))
                {
                    // update and progress
                    playText.text = "Drawing Card " + currentCard.GetComponent<Card>().cardName;
                    progressWheelImage.fillAmount = Mathf.Max(1.0f, timeSpentWithCard / timeToCompleteDraw);
                    timeSpentWithCard += Time.deltaTime;

                    if (Input.GetKeyDown(KeyCode.Space))
                        break; // Spacebar bypass of wait
                }
                else
                {
                    playText.text = "Drawing cards, max of " + maxCardsInHand + ". Press space to stop.";
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        goto FinishDraw; // yea, I'm not sure about this one either...
                    }
                }

                yield return null;
            }

            // We gotten a card that passed, so add it to your hand
            if (currentCard == null)
                Debug.LogError("ERROR: Bad current card");

            // DRAW CARD FOR REAL
            playText.text = "Drew " + currentCard.GetComponent<Card>().cardName;
            yield return new WaitForSeconds(1.0f);
            Deck.instance.deck.Remove(currentCard);
            hand.Add(currentCard);

            yield return null;
        }

    FinishDraw:
        // visual of progressing and then
        playText.text = "Cards Drawn, progressing...";
        for (float t = 0; t < drawFinishWaitTime; t += Time.deltaTime)
        {
            progressWheelImage.fillAmount = (t / drawFinishWaitTime);
            yield return null;
        }
        progressWheelImage.fillAmount = 0.0f;
        currentInputHandler = null;
        // TODO : next phase
        cm.NextPhase();
    }








    public float timeToCompletePlay = 2.5f;

    private void UpdatePlayActionUI(bool validTarget, bool hasCardAttached, 
        GameObject currentCard, GameObject currentTarget, float fillMeter)
    {
        progressWheelImage.fillAmount = 0.0f;

        if (!validTarget || hasCardAttached)
            playText.text = "Apply cards to combatants...";
        else if (currentCard == null)
            playText.text = "You focus on " + currentTarget.name + "... What card will you play?";
        else
        {
            playText.text = "Playing " + currentCard.GetComponent<Card>().cardName + " on " + currentTarget.name + "...";
            progressWheelImage.fillAmount = fillMeter;
        }
    }

    private bool CanPlayMore()
    {
        foreach (GameObject c in cm.activeEnemies)
            if (c.GetComponent<CombatantBasis>().appliedCard != null)
                return true;
        foreach (GameObject c in cm.activePartyMembers)
            if (c.GetComponent<CombatantBasis>().appliedCard != null)
                return true;
        return false;
    }

    IEnumerator RunPlayPhase()
    {
        cardParser.UpdateMode(CardParser.ParseMode.HandMode);
        timeSpentWithCard = 0.0f;
        currentTarget = null;
        validTarget = false;

        while (CanPlayMore()) // TODO : also be done if 
        {
            while (timeSpentWithCard < timeToCompletePlay) {
                // cast for and detect combatants
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo, 100.0f, LayerMask.GetMask("Combatant")))
                {
                    timeSpentWithCard = (currentTarget == hitInfo.collider.gameObject) ? timeSpentWithCard : 0.0f;
                    currentTarget = hitInfo.collider.gameObject;
                }
                else timeSpentWithCard = 0.0f;
                bool hasCardAttached = currentTarget.GetComponent<CombatantBasis>().appliedCard != null;
                validTarget = currentTarget != null && !hasCardAttached;
                timeSpentWithCard = validTarget ? timeSpentWithCard : 0.0f;

                UpdatePlayActionUI(validTarget, hasCardAttached, currentCard, currentTarget, timeSpentWithCard / timeToCompletePlay);

                if (Input.GetKey(KeyCode.Space)) break;
                if (Input.GetKey(KeyCode.Q)) goto FinishPlay;

                timeSpentWithCard += Time.deltaTime;
                yield return null;
            }

            // APPLY THE CARD TO A VALID TARGET
            // TODO : this function discards the card but might not go well natively
            cm.ApplyCard(currentCard, currentTarget);
            hand.Remove(currentCard);
            timeSpentWithCard = 0.0f;
            yield return null;
        }

    FinishPlay:
        // visual of progressing and then
        playText.text = "Cards played, progressing...";
        for (float t = 0; t < playFinishWaitTime; t += Time.deltaTime)
        {
            progressWheelImage.fillAmount = (t / playFinishWaitTime);
            yield return null;
        }
        progressWheelImage.fillAmount = 0.0f;
        currentInputHandler = null;
        cm.NextPhase();
        // TODO : next phase
    }

    private float timeToCompleteDiscard;

    IEnumerator RunDiscardPhase()
    {
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
                    progressWheelImage.fillAmount = timeSpentWithCard / timeToCompleteDiscard;
                } else
                {
                    playText.text = "Show cards from your hand to discard for mana " + ". (Total Mana Reup: " + manaUp + ")"; // TODO :
                    progressWheelImage.fillAmount = 0.0f;
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
        playText.text = "Cards discarded, mana awarded progressing...";
        for (float t = 0; t < discardFinishWaitTime; t += Time.deltaTime)
        {
            progressWheelImage.fillAmount = (t / discardFinishWaitTime);
            yield return null;
        }
        progressWheelImage.fillAmount = 0.0f;
        playText.text = "";
        currentInputHandler = null;
        cm.AddMana(manaUp);
        // TODO : next phase
        cm.NextPhase();
    }


    IEnumerator RunActionPhase()
    {
        // TODO : how do I know the guy? that's the only hard part here...
        CombatantBasis cb = cm.actionOrder[0].GetComponent<CombatantBasis>();
        if (cb.appliedCard != null)
        {
            playText.text = "Currently cannot play because " + cb.combatantName + " already has played card.";
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
                    progressWheelImage.fillAmount = timeSpentWithCard / timeToCompletePlay;

                    if (Input.GetKeyDown(KeyCode.Space)) break;
                    if (Input.GetKeyDown(KeyCode.Q)) goto FinishAction;

                    timeSpentWithCard += Time.deltaTime;
                }
                yield return null;
            }

            cm.ApplyCard(currentCard, cm.actionOrder[0]);
            hand.Remove(currentCard);
        }

    FinishAction:
        progressWheelImage.fillAmount = 0.0f;
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
            case CombatManager.CombatPhase.None:
                break;
        }
    }
    

    public void ActivateCVForCombat(CVControllerBackLoader backLoader)
    {
        cInterface = backLoader;
        cm = backLoader.cm;
        planeImage = backLoader.planeImage;
        stickerImage1 = backLoader.stickerImage1;
        stickerImage2 = backLoader.stickerImage2;
        stickerImage3 = backLoader.stickerImage3;
        playText = backLoader.playText;
        cardText = backLoader.cardText;
        cm.SubscribeAsController(HandlePhaseStep, HandleRequestForInput);

        // TODO : sanity check but might just muddle things
        HandlePhaseStep(CombatManager.CombatPhase.None, CombatManager.CombatPhase.DrawPhase);

        activeController = CombatManager.IsInCVMode;
        cardParser.SetLookForInput(CombatManager.IsInCVMode);

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
            print("Adding " + card.cardName + " which is " + c);
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

        // card parser true init
        cardParser = GetComponent<CardParser>();
        cardParser.StableUpdateEvent.AddListener(HandleStableUpdate);
        cardParser.ToNullUpdateEvent.AddListener(HandleNullUpdate);
        cardParser.ToNewUpdateEvent.AddListener(HandleNewUpdate);
        cardParser.UpdateMode(CardParser.ParseMode.AllMode);

        SetUpOrderedCards(Deck.instance.allCards);
    }
    
    public void UpdateSeenImage(WebCamTexture webCamTexture)
    {
        goodSeeImage.texture = webCamTexture;
    }

    public void DisplayCardData(GameObject card, Mat goodImage)
    {
        /*
        --Card X in HAND.
       -- Card X, not in hand.
       -- No card detected.
       -- No card detected but one was suspected.
       -- TODO TODO : TODO
       */
        bool inHand = hand.Contains(card);
        if (goodSeeImage.texture)
            Destroy(goodSeeImage.texture);

        if (card != null)
        {
            goodSeeImage.texture = OpenCvSharp.Unity.MatToTexture(goodImage);
            cardText.text = card.GetComponent<Card>().cardName + (inHand ? "" : " which is not in your HAND!");
        }
        else
        {
            cardText.text = "No Card Found";
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
        if (orderedCards.TryGetValue(name, out List<GameObject> lis))
            return lis;
        else
            return new List<GameObject>();
    }
    
    public void UpdateStickerDebugs(Mat sticker1, Mat sticker2, Mat sticker3)
    {
        stickerImage1.enabled = stickerImage2.enabled = stickerImage3.enabled = true;

        if (stickerImage1.texture != null)
            Destroy(stickerImage1.texture);
        if (stickerImage2.texture != null)
            Destroy(stickerImage2.texture);
        if (stickerImage3.texture != null)
            Destroy(stickerImage3.texture);

        stickerImage1.texture = OpenCvSharp.Unity.MatToTexture(sticker1);
        stickerImage2.texture = OpenCvSharp.Unity.MatToTexture(sticker2);
        stickerImage3.texture = OpenCvSharp.Unity.MatToTexture(sticker3);
    }

    // TODO 
    // POSSIBLE BUG : CAN DRAG THE NEWLY CREATED CARDS BACK ONTO THE HAND THAT SHOULDN'T EXIST?? WE'LL SEE
    // 
}
