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

    public static void ConvertParseCardToCard(CardParser.CustomCard card)
    {

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
}
