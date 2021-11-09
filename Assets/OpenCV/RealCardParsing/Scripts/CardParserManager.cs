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
    public RawImage planeImage;
    public RawImage goodSeeImage;
    public RawImage stickerImage1;
    public RawImage stickerImage2;
    public RawImage stickerImage3;

    public TMP_Text playText;
    public TMP_Text cardText;

    private bool activeController;

    // CVReadyToContinueActions
    // IsReadyToContinueActions

    public void HandlePhaseStep(CombatManager.CombatPhase lastPhase, CombatManager.CombatPhase newPhase)
    {
        // TODO
    }

    public void HandlePhaseInitDone(CombatManager.CombatPhase currentPhase)
    {
        // TODO : try not to use this if you can help it
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
        cm.SubscribeAsController(HandlePhaseStep, HandlePhaseInitDone);

        // TODO : sanity check but might just muddle things
        HandlePhaseStep(CombatManager.CombatPhase.None, CombatManager.CombatPhase.DrawPhase);

        activeController = CombatManager.IsInCVMode;
        cardParser.SetLookForInput(CombatManager.IsInCVMode);

        DisplayCardData(null, null);
    }

    private int currentID = -1;
    private bool validTarget = true;

    public Dictionary<string, List<GameObject>> orderedCards = new Dictionary<string, List<GameObject>>();

    public List<GameObject> handCards = new List<GameObject>();

    private void SetUpOrderedCards(List<GameObject> cards)
    {
        foreach(GameObject c in cards)
        {
            Card card = c.GetComponent<Card>();
            if (!orderedCards.ContainsKey(card.cardName))
                orderedCards.Add(card.cardName, new List<GameObject>());

            orderedCards[card.cardName].Add(c);
            print("Adding " + card.cardName + " which is " + c);

            // TODO : current used to add all element cards to the hand, WILL IMMEDIATELY BREAK UPON NEW NAMES
            if (card.cardName != "")
                handCards.Add(c);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        cardParser = GetComponent<CardParser>();
        cardParser.StableUpdateEvent.AddListener(HandleStableUpdate);
        cardParser.ToNullUpdateEvent.AddListener(HandleNullUpdate);
        cardParser.ToNewUpdateEvent.AddListener(HandleNewUpdate);

        SetUpOrderedCards(Deck.instance.allCards);

    }

    private void Update()
    {
        // TODO : starting phase: ask to draw cards and show them, can't be done right now...

        currentTarget = null;
        validTarget = false;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo, 100.0f, LayerMask.GetMask("Combatant"))) {
            currentTarget = hitInfo.collider.gameObject;
            print(hitInfo.collider.gameObject.name);
        }

        if (cm.currentPhase == CombatManager.CombatPhase.ActionPhase)
        {
            validTarget = currentTarget == cm.currentCB;
            print(validTarget);
            if (validTarget)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    print("PLAYING CARD");
                    if (true || handCards.Contains(currentCard) || currentCard == null)
                    {
                        // TODO : manage card hand and deck here, make the apply card return a boolean of valid, or even a reason?
                        cm.ApplyCard(currentCard, currentTarget);
                        //handCards.Remove(currentCard);
                    }
                    else
                    {
                        Debug.LogError("CV: Card played is not in HAND" + currentCard);
                    }
                }
            }
            UpdateUIPlay();
        }
        else if (cm.currentPhase == CombatManager.CombatPhase.PlayPhase)
        {
            validTarget = currentTarget != null;
            print(validTarget);
            if (validTarget)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (true || handCards.Contains(currentCard) || currentCard == null)
                    {
                        // TODO : manage card hand and deck here, make the apply card return a boolean of valid, or even a reason?
                        cm.ApplyCard(currentCard, currentTarget);
                        //handCards.Remove(currentCard);
                    } else
                    {
                        Debug.LogError("CV: Card played is not in HAND" + currentCard);
                    }

                }
            }
            UpdateUIPlay();
        }
        else if (cm.currentPhase == CombatManager.CombatPhase.DrawPhase)
        {
            if (Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (Deck.instance.deck.Contains(currentCard))
                {
                    handCards.Add(currentCard);
                    Deck.instance.deck.Remove(currentCard);
                } else
                {
                    Debug.LogError("Shown card not in DECK");
                }
            }
            //Deck.instance.deck.Contains();

            UpdateUIDraw();
        }
        else
        {
            UpdateUINoAction();
        }
    }

    private void UpdateUIPlay()
    {
        playText.text = "Current Target: " + ((!validTarget) ? "None" : currentTarget.name);
    }

    private void UpdateUIDraw()
    {

    }

    private void UpdateUINoAction()
    {

    }

    public void DisplayCardData(GameObject card, Mat goodImage)
    {
        bool inHand = handCards.Contains(card);
        if (goodSeeImage.texture)
            Destroy(goodSeeImage.texture);
        if (card != null) 
            goodSeeImage.texture = OpenCvSharp.Unity.MatToTexture(goodImage);
        if (card != null)
            cardText.text = card.GetComponent<Card>().cardName + (inHand ? "" : " which is not in your HAND!");
        else
            cardText.text = "No Card Found";
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
        DisplayCardData(card, null);
    }

    public void HandleNewUpdate(GameObject card, int id)
    {
        currentCard = card;
        currentID = id;
        DisplayCardData(card, cardParser.GetLastGoodReplane());
    }


    public List<GameObject> GetCardsOfName(string name)
    {
        if (orderedCards.TryGetValue(name, out List<GameObject> lis))
            return lis;
        else
            return new List<GameObject>();
    }

    // TODO : delete this on clear
    public void RequestCardAction(OneOnCombatManager oneOnCombatManager)
    {
    }

    internal void UpdateSeenImage(WebCamTexture webCamTexture)
    {
        throw new NotImplementedException();
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
