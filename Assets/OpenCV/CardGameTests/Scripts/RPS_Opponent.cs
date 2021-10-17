using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RPS_Opponent : MonoBehaviour
{
    private RPS_PlayManager manager;
    private Dictionary<RPS_Card.CardType, int> myCards;

    public void Init(RPS_PlayManager manager, Dictionary<RPS_Card.CardType, int> initCards)
    {
        this.manager = manager;
        myCards = new Dictionary<RPS_Card.CardType, int>(initCards);
    }

    public void RequestCommentOnStateChange(RPS_PlayManager.PlayState state, RPS_PlayManager.PlayState nextState)
    {
        throw new NotImplementedException();
    }

    internal void AskForBid()
    {
        throw new NotImplementedException();
    }

    internal void AskForPlay()
    {
        throw new NotImplementedException();
    }

    internal void GainCard(RPS_Card playerBid)
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
