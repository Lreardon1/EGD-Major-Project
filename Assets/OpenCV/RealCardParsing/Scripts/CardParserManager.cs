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
    public GameObject currentCard;
    public CardParser cardParser;
    public CombatManager cm;

    [Header("UI")]
    public RawImage goodSeeImage;
    public TMP_Text cardText;
    public TMP_Text playText;

    private GameObject currentTarget = null;
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

        cardParser.SetLookForInput(CombatManager.IsInCVMode);

        DisplayCardData(null, null);
    }

    private void Update()
    {
        
        currentTarget = null;
        validTarget = false;
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
            UpdateUIPlay();
        }
        else if (cm.currentPhase == CombatManager.CombatPhase.PlayPhase)
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
            UpdateUIPlay();
        }
        else if (cm.currentPhase == CombatManager.CombatPhase.DrawPhase)
        {
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
        if (goodSeeImage.texture)
            Destroy(goodSeeImage.texture);
        if (card != null) 
            goodSeeImage.texture = OpenCvSharp.Unity.MatToTexture(goodImage);
        if (card != null)
            cardText.text = card.GetComponent<Card>().cardName;
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
    // TODO 
    // Disable functionality based on STATIC bool flag in combatManager : TODO : wait
    // 
    // POSSIBLE BUG : CAN DRAG THE NEWLY CREATED CARDS BACK ONTO THE HAND THAT SHOULDN'T EXIST?? WE'LL SEE
    // 
}
