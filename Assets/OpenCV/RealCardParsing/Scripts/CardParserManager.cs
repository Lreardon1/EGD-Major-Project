using OpenCvSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CardParser))]
public class CardParserManager : MonoBehaviour
{
    public GameObject currentCard;
    public CardParser cardParser;
    public CombatManager cm;
    public RawImage goodSeeImage;
    private GameObject currentTarget = null;
    private int currentID = -1;
    private bool validTarget = true;

    public Dictionary<string, List<GameObject>> orderedCards = new Dictionary<string, List<GameObject>>();

    private void SetUpOrderedCards(List<GameObject> cards)
    {
        foreach(GameObject c in cards)
        {
            Card card = c.GetComponent<Card>();
            if (orderedCards.ContainsKey(card.cardName))
                orderedCards.Add(card.cardName, new List<GameObject>());

            orderedCards[card.cardName].Add(c);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        cardParser = GetComponent<CardParser>();
        cardParser.StableUpdateEvent.AddListener(HandleStableUpdate);
        cardParser.ToNullUpdateEvent.AddListener(HandleNullUpdate);
        cardParser.ToNewUpdateEvent.AddListener(HandleNewUpdate);

        SetUpOrderedCards(Deck.instance.allCards);

        cardParser.SetLookForInput(false);
    }

    private void Update()
    {
        /*
        currentTarget = null;
        Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(r, out RaycastHit hitInfo, 1000.0f, LayerMask.GetMask("Combatant")))
        {
            currentTarget = hitInfo.collider.gameObject;
        }
        if (cm.currentPhase == CombatManager.CombatPhase.ActionPhase)
        {
            validTarget = currentTarget == cm.currentCB;
            if (validTarget)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    // TODO : manage card hand and deck here, make the apply card return a boolean of valid, or even a reason?
                    cm.ApplyCard(currentCard, currentTarget);
                }
            }
        } 
        if (cm.currentPhase == CombatManager.CombatPhase.PlayPhase)
        {
            validTarget = currentTarget != null;
            if (validTarget)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    // TODO : manage card hand and deck here, make the apply card return a boolean of valid, or even a reason?
                    cm.ApplyCard(currentCard, currentTarget);
                }
            }
        }
        */


        UpdateUI();
    }

    private void UpdateUI()
    {

    }

    public void DisplayCardData(GameObject card, Mat goodImage)
    {
        if (goodSeeImage.texture)
            Destroy(goodSeeImage.texture);
        goodSeeImage.texture = OpenCvSharp.Unity.MatToTexture(goodImage);


        // TODO : text data on card?
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
        DisplayCardData(card, cardParser.GetLastGoodReplane());
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

    // TODO : this is a test class using my system cause I can't hit Tyler's yet...
    OneOnCombatManager manager;
    bool inAction;
    public void RequestCardAction(OneOnCombatManager oneOnCombatManager)
    {
        manager = oneOnCombatManager;
        inAction = true;
    }
    // TODO 
    // Disable functionality based on STATIC bool flag in combatManager : TODO : wait
    // display the card and card data
    // 
    // POSSIBLE BUG : CAN DRAG THE NEWLY CREATED CARDS BACK ONTO THE HAND THAT SHOULDN'T EXIST?? WE'LL SEE
    // 
}
