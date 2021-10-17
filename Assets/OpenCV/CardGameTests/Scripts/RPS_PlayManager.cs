using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

// controls all management actions BUT DOES NOT CONTROL ANY PLAYER ACTIONS, OF EITHER
public class RPS_PlayManager : MonoBehaviour
{
    public RPS_PlayerController player;
    public RPS_Opponent opponent;
    public int opponentPoints;
    public int playerPoints;

    public RPS_Card opponentBid;
    public RPS_Card playerBid;
    public RPS_Card opponentPlay;
    public RPS_Card playerPlay;

    public RPS_CardObject opponentBidObj;
    public RPS_CardObject playerBidObj;
    public RPS_CardObject opponentPlayObj;
    public RPS_CardObject playerPlayObj;

    public PlayState state = PlayState.Start;
    public RPS_Card.Result roundResult = RPS_Card.Result.Tie;
    public bool bLastBidsTraded = false;

    public TMP_Text winText;
    public TMP_Text tradeText;


    public int currentRound;
    public int totalRounds;
    public int waterCount;
    public int fireCount;
    public int natureCount;

    public enum PlayState
    {
        Start,
        OpponentBid,
        PlayerBid,
        OpponentPlay,
        PlayerPlay,
        Reveal,
        Trade,
        End
    }

    // Start is called before the first frame update
    void Start()
    {
        currentRound = 1;
        totalRounds = waterCount + fireCount + natureCount - 1;
        state = PlayState.Start;

        player.Init(this, RPS_Card.CreateDeck(fireCount, waterCount, natureCount));
        opponent.Init(this, RPS_Card.CreateDeck(fireCount, waterCount, natureCount));
        SendStateChangeForComment(PlayState.OpponentBid);
    }

    public void ProgressFromComment(PlayState nextState)
    {
        state = nextState;
        switch (state)
        {
            case PlayState.OpponentBid:
                currentRound += 1;
                opponent.RequestBid();
                break;
            case PlayState.PlayerBid:
                player.RequestBid();
                break;
            case PlayState.OpponentPlay:
                opponent.RequestPlay();
                break;
            case PlayState.PlayerPlay:
                player.RequestPlay();
                break;
            case PlayState.Reveal:
                RevealCards();
                break;
            case PlayState.Trade:
                if (roundResult == RPS_Card.Result.Win)
                    player.RequestTradeDecision();
                else
                    opponent.RequestTradeDecision();
                break;
            case PlayState.End:
                print("FIN");
                break;
        }
    }

    private void SendStateChangeForComment(PlayState nextState)
    {
        opponent.RequestCommentOnStateChange(state, nextState);
    }

    public void SendCardInContext(RPS_Card card)
    {
        switch (state)
        {
            case PlayState.OpponentBid:
                opponentBid = card;
                opponentBidObj.SetEnabled(true);
                opponentBidObj.SetRevealed(false);
                opponentBidObj.SetCard(card);
                SendStateChangeForComment(PlayState.PlayerBid);
                break;
            case PlayState.PlayerBid:
                playerBid = card;
                playerBidObj.SetEnabled(true);
                playerBidObj.SetRevealed(true);
                playerBidObj.SetCard(card);
                SendStateChangeForComment(PlayState.OpponentPlay);
                break;
            case PlayState.OpponentPlay:
                opponentPlay = card;
                opponentPlayObj.SetEnabled(true);
                opponentPlayObj.SetRevealed(false);
                opponentPlayObj.SetCard(card);
                SendStateChangeForComment(PlayState.PlayerPlay);
                break;
            case PlayState.PlayerPlay:
                playerPlay = card;
                playerPlayObj.SetEnabled(true);
                playerPlayObj.SetRevealed(true);
                playerPlayObj.SetCard(card);
                SendStateChangeForComment(PlayState.Reveal);
                break;
            case PlayState.Reveal:
            case PlayState.Start:
            case PlayState.Trade:
            case PlayState.End:
                print("What the fuck");
                break;
        }
    }

    public void AskForOpponentBid()
    {
        opponent.AskForBid();
    }

    public void AskForPlayerBid()
    {
        player.AskForBid();
    }


    public void AskForOpponentPlay()
    {
        opponent.AskForPlay();
    }


    public void AskForPlayerPlay()
    {
        player.AskForPlay();
    }

    public void RevealCards()
    {
        StartCoroutine(IRevealCards());
    }

    IEnumerator IRevealCards()
    {
        opponentBidObj.SetRevealed(true);

        yield return new WaitForSeconds(0.4f);

        roundResult = RPS_Card.GetPlayResult(playerPlay, opponentPlay);
        string resStr = "Tie : Bids Returned";
        resStr = roundResult == RPS_Card.Result.Loss ? "Loss" : resStr;
        resStr = roundResult == RPS_Card.Result.Win ? "Win" : resStr;
        winText.text = resStr;

        yield return new WaitForSeconds(0.8f);
        playerPoints += roundResult == RPS_Card.Result.Win ? 1 : 0;
        opponentPoints += roundResult == RPS_Card.Result.Loss ? 1 : 0;

        // SPECIAL LOGIC TO ESCAPE AFTER LAST ROUND AND TO SKIP BIDS ON LAST ROUND
        if (currentRound == totalRounds)
        {
            SendStateChangeForComment(PlayState.End);
            yield return null;
        } else if (currentRound == totalRounds - 1)
        {
            SendStateChangeForComment(PlayState.OpponentPlay);
            yield return null;
        }

        if (roundResult == RPS_Card.Result.Tie) {
            SendStateChangeForComment(PlayState.OpponentBid);
        } else {
            SendStateChangeForComment(PlayState.Trade);
        }
    }

    public void SendTradeCardsDecision(bool tradeCards)
    {
        bLastBidsTraded = tradeCards;
        StartCoroutine(ITradeCards(tradeCards));
    }

    IEnumerator ITradeCards(bool tradeCards)
    {
        if (tradeCards)
        {
            tradeText.text = "Cards Traded : Place your bid aside and take a " + opponentBid.type;
            opponentBidObj.SetRevealed(true);
            opponent.GainCard(playerBid);
            player.GainCard(opponentBid);

        } else
        {
            tradeText.text = "Cards NOT Traded. You retain a " + playerBid.type;
            opponent.GainCard(opponentBid);
            player.GainCard(playerBid);
        }
        yield return new WaitForSeconds(1.2f);
        SendStateChangeForComment(PlayState.OpponentBid);
    }
}
