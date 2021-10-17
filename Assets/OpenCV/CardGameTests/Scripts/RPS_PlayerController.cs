using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RPS_PlayerController : MonoBehaviour
{
    private RPS_PlayManager manager;
    private Dictionary<RPS_Card.CardType, int> myCards;

    public void Init(RPS_PlayManager manager, Dictionary<RPS_Card.CardType, int> initCards)
    {
        this.manager = manager;
        myCards = new Dictionary<RPS_Card.CardType, int>(initCards);
    }

    internal void AskForBid()
    {
        throw new NotImplementedException();
    }

    internal void AskForPlay()
    {
        throw new NotImplementedException();
    }

    internal void GainCard(RPS_Card opponentBid)
    {
        throw new NotImplementedException();
    }

    internal void RequestBid()
    {
        throw new NotImplementedException();
    }

    internal void RequestPlay()
    {
        throw new NotImplementedException();
    }

    internal void RequestTradeDecision()
    {
        throw new NotImplementedException();
    }
}
