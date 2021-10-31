using OpenCvSharp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CardParser))]
public class CardParserManager : MonoBehaviour
{
    public CardParser.CustomCard currentCard;
    public CardParser cardParser;
    public RawImage goodSeeImage;

    public Dictionary<int, List<GameObject>> orderedCards = new Dictionary<int, List<GameObject>>();

    private void SetUpOrderedCards(List<GameObject> cards)
    {
        foreach(GameObject c in cards)
        {
            Card card = c.GetComponent<Card>();
            //card.type
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        cardParser = GetComponent<CardParser>();
        cardParser.StableUpdateEvent.AddListener(HandleStableUpdate);
        cardParser.ToNullUpdateEvent.AddListener(HandleNullUpdate);
        cardParser.ToNewUpdateEvent.AddListener(HandleNewUpdate);

        cardParser.SetLookForInput(false);

    }

    public void DisplayCardData(CardParser.CustomCard card, Mat goodImage)
    {
        if (goodSeeImage.texture)
            Destroy(goodSeeImage.texture);
        goodSeeImage.texture = OpenCvSharp.Unity.MatToTexture(goodImage);
        // TODO : text data on card?
    }

    public GameObject HashLookUpFromList(List<GameObject> cardList, CardParser.CustomCard card)
    {
        return cardList[card.cardID]; // BIG TODO
    }

    public GameObject ConvertParseCardToCard(CardParser.CustomCard card)
    {
        return HashLookUpFromList(Deck.instance.allCards, card);
    }

    public void HandleStableUpdate(CardParser.CustomCard card)
    {

        currentCard = card;
    }

    public void HandleNullUpdate(CardParser.CustomCard card)
    {
        if (goodSeeImage.texture)
            Destroy(goodSeeImage.texture);

        currentCard = card;
    }

    public void HandleNewUpdate(CardParser.CustomCard card)
    {
        Mat seenCard = cardParser.GetLastGoodReplane();
        if (goodSeeImage.texture)
            Destroy(goodSeeImage.texture);
        goodSeeImage.texture = OpenCvSharp.Unity.MatToTexture(seenCard);

        currentCard = card;
    }

    // TODO 
        // ask the combat manager to update me on state : have to wait on half of this
        // Disable functionality based on STATIC bool flag in combatManager

        // figure out dropzone system and use it to select zones : DISCARD, APPLY
        // display the card and card data
        // flag disable everything
        // manage cards on your own, ignore the other guy, make your own deck and hand, manage the relations yourself
        // 
        // POSSIBLE BUG : CAN DRAG THE NEWLY CREATED CARDS BACK ONTO THE HAND THAT SHOULDN'T EXIST?? WE'LL SEE
        // 
}
