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
    public GameObject[] cardPrefabs; // TODO : construct these in advance or use Deck to manage valid cards

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

    public GameObject ConvertParseCardToCard(CardParser.CustomCard card)
    {
        GameObject cardObj = Instantiate(cardPrefabs[card.cardID]);
        // create Modifier() classes and ACTIVATE and attach them
        // TODO : ask ?Tyler? about this.
        //cardObj.GetComponent<Card>().modifiers[0].GetComponent<Modifier>().

        return cardObj;
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
        // figure out dropzone system and use it to select zones : DISCARD, APPLY
        // display the card and card data
        // flag disable everything
        // manage cards on your own, ignore the other guy, make your own deck and hand, manage the relations yourself
        // 
        // POSSIBLE BUG : CAN DRAG THE NEWLY CREATED CARDS BACK ONTO THE HAND THAT SHOULDN'T EXIST?? WE'LL SEE
        // 
}
