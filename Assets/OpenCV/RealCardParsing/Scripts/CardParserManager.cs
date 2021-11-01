using OpenCvSharp;
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
    public GameObject currentTarget = null;
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
        currentTarget = null;
        Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(r, out RaycastHit hitInfo, 1000.0f, LayerMask.GetMask("Combatant")))
        {
            currentTarget = hitInfo.collider.gameObject;
        }
        if (cm)


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

    public void HandleStableUpdate(GameObject card)
    {
        currentCard = card;
        DisplayCardData(card, cardParser.GetLastGoodReplane());
    }

    public void HandleNullUpdate(GameObject card)
    {
        currentCard = card;
        DisplayCardData(card, cardParser.GetLastGoodReplane());
    }

    public void HandleNewUpdate(GameObject card)
    {
        currentCard = card;
        DisplayCardData(card, cardParser.GetLastGoodReplane());
    }


    public List<GameObject> GetCardsOfName(string name)
    {
        if (orderedCards.TryGetValue(name, out List<GameObject> lis))
            return lis;
        else
            return new List<GameObject>();
    }
    // TODO 
    // ask the combat manager to update me on state : TODO : have to wait on half of this
    // Disable functionality based on STATIC bool flag in combatManager : TODO : wait
    // display the card and card data
    // 
    // POSSIBLE BUG : CAN DRAG THE NEWLY CREATED CARDS BACK ONTO THE HAND THAT SHOULDN'T EXIST?? WE'LL SEE
    // 
}
