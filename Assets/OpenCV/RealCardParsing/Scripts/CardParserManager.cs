using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CardParser))]
public class CardParserManager : MonoBehaviour
{
    public CardParser cardParser;
    // Start is called before the first frame update
    void Start()
    {
        cardParser = GetComponent<CardParser>();
        cardParser.StableUpdateEvent.AddListener(HandleStableUpdate);
        cardParser.ToNullUpdateEvent.AddListener(HandleNullUpdate);
        cardParser.ToNewUpdateEvent.AddListener(HandleNewUpdate);

        cardParser.SetLookForInput(false);
    }

    public static void ConvertParseCardToCard(CardParser.CustomCard card)
    {
       // TODO
    }

    public void HandleStableUpdate(CardParser.CustomCard card)
    {
        // TODO:::: call combat manager...
        cardParser.GetLastGoodReplane();
    }

    public void HandleNullUpdate(CardParser.CustomCard card)
    {

    }

    public void HandleNewUpdate(CardParser.CustomCard card)
    {

    }
}
