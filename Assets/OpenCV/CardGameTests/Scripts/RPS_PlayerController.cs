using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RPS_PlayerController : MonoBehaviour
{
    private RPS_PlayManager manager;
    private Dictionary<RPS_Card.CardType, int> myCards;
    private CardParser cardParser;

    public TMP_Text playText;
    public Image progressIndicator;

    private bool debugMode;

    public void Init(RPS_PlayManager manager, Dictionary<RPS_Card.CardType, int> initCards)
    {
        this.manager = manager;
        myCards = new Dictionary<RPS_Card.CardType, int>(initCards);
        cardParser = FindObjectOfType<CardParser>(); // find the dontdestroyonload card parser...
        debugMode = cardParser == null;
        if (!debugMode)
            cardParser.UpdateMode(CardParser.ParseMode.RPS_Mode);
    }
    

    private bool ContinueButtonPressed()
    {
        return Input.GetKeyDown(KeyCode.Space);
    }
    private RPS_Card.CardType bestVisibleCard = RPS_Card.CardType.Unknown;

    private bool IsShowingValidCard()
    {
        if (debugMode)
        {
            bestVisibleCard = RPS_Card.CardType.Unknown;
            if (Input.GetKey(KeyCode.Alpha1))
                bestVisibleCard = RPS_Card.CardType.Water;
            else if (Input.GetKey(KeyCode.Alpha2))
                bestVisibleCard = RPS_Card.CardType.Fire;
            else if (Input.GetKey(KeyCode.Alpha3))
                bestVisibleCard = RPS_Card.CardType.Wind;
            else return false;

            return myCards[bestVisibleCard] > 0;
        }
        else return false; // TODO
    }

    private RPS_Card.CardType GetBestVisibleCard()
    {
        return bestVisibleCard;
    }

    private IEnumerator IHandleBid()
    {
        progressIndicator.fillAmount = 0.0f;
        playText.text = "Place your bid face down and press space to continue";
        // TODO : get input

        yield return new WaitUntil(ContinueButtonPressed);
        playText.text = "";

        RPS_Card card = new RPS_Card(RPS_Card.CardType.Unknown);
        manager.SendCardInContext(card);
    }


    public float timeToShowPlay = 0.7f;

    private IEnumerator IHandlePlay()
    {
        playText.text = "Show a card to play it";
        float t = 0;
        while (t < timeToShowPlay)
        {

            progressIndicator.fillAmount = t / timeToShowPlay;
            if (IsShowingValidCard())
            {
                t += Time.deltaTime;
                playText.text = "You are most likely showing " + GetBestVisibleCard() + " to play.";

            }
            else
            {
                t = Mathf.Max(t - Time.deltaTime * 3.0f, 0);
            }

            yield return null;
        }

        RPS_Card card = new RPS_Card(GetBestVisibleCard());
        print("Played " + card.type);
        progressIndicator.fillAmount = 0.0f;

        // TODO : polish anims and make a card
        manager.SendCardInContext(card);
    }

    public float timeToShowTrade = 0.7f;

    private IEnumerator IHandleTradeDecision()
    {
        playText.text = "Show your bid card to trade it, or keep it by pressing space";
        float t = 0;
        while (t < timeToShowTrade)
        {
            if (ContinueButtonPressed()) goto NoTrade;

            if (IsShowingValidCard())
            {
                t += Time.deltaTime;
                playText.text = "You are mostly showing " + GetBestVisibleCard() + " as you bid to trade.";
            }
            else
            {
                t = Mathf.Max(t - Time.deltaTime * 3.0f, 0);
                playText.text = "Show your bid card to trade it, or keep it by pressing space";
            }

            yield return null;
        }

        playText.text = "You are trading your " + GetBestVisibleCard() + " card";
        progressIndicator.fillAmount = 0.0f;
        yield return new WaitForSeconds(1.0f);

        manager.SendTradeCardsDecision(true, GetBestVisibleCard());
        yield break;
    NoTrade:
        print("NO TRADE");
        playText.text = "No trade";

        manager.SendTradeCardsDecision(false);
    }


    private IEnumerator IHandleRevealCardByEnemyDecision()
    {
        playText.text = "Your opponent has decided to take your bid, show your bid now...";
        float t = 0;

        while (t < timeToShowTrade)
        {
            progressIndicator.fillAmount = t / timeToShowTrade;

            if (IsShowingValidCard())
            {
                t += Time.deltaTime;
                playText.text = "You are mostly showing " + GetBestVisibleCard() + " as you bid to trade.";
            }
            else
            {
                t = Mathf.Max(t - Time.deltaTime * 3.0f, 0);
            }

            yield return null;
        }

        progressIndicator.fillAmount = 0.0f;
        manager.SendBidCardAfterDecision(GetBestVisibleCard());
    }

    public void RequestBidReveal()
    {
        StartCoroutine(IHandleRevealCardByEnemyDecision());
    }


    internal void GainCard(RPS_Card gained)
    {
        myCards[gained.type] += 1;
    }

    internal void RequestBid()
    {
        StartCoroutine(IHandleBid());
    }

    internal void RequestPlay()
    {
        StartCoroutine(IHandlePlay());
    }

    internal void RequestTradeDecision()
    {
        StartCoroutine(IHandleTradeDecision());
    }
}
