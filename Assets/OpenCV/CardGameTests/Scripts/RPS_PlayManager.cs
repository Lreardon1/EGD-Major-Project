using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// controls all management actions BUT DOES NOT CONTROL ANY PLAYER ACTIONS, OF EITHER
public class RPS_PlayManager : MonoBehaviour
{
    public RPS_PlayerController player;
    public RPS_Opponent opponent;
    public int opponentPoints;
    public int playerPoints;

    public RPS_Card opponentBid;
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

    [Header("Physics Cards")]
    public GameObject physicsCard;
    public Transform physicsSpawnPoint;
    public float sphereRadius;
    public Vector3 rotRanges;

    [Header("Card Anim Points")]
    public Transform playerBidAnimPoint;
    public Transform opponentBidAnimPoint;
    public Transform playerPlayAnimPoint;
    public Transform opponentPlayAnimPoint;


    IEnumerator StartFromAndLerpInDirection(GameObject go, Vector3 start, Vector3 dir, float time, float dist)
    {
        float t = 0;
        while (t < time)
        {
            go.transform.position = start + (dir * (t / time) * dist);
            t += Time.deltaTime;
            yield return null;
        }
        go.transform.position = start + (dir * dist);
    }

    IEnumerator LerpRot(GameObject go, Quaternion start, Quaternion end, float time)
    {
        float t = 0;
        while (t < time)
        {
            go.transform.rotation = Quaternion.Slerp(start, end, t / time);
            t += Time.deltaTime;
            yield return null;
        }
        go.transform.rotation = end;
    }


    IEnumerator ReturnBidsAnim()
    {
        StartCoroutine(StartFromAndLerpInDirection(playerBidObj.gameObject, playerBidObj.transform.position, -playerBidAnimPoint.forward, 0.7f, 4));
        StartCoroutine(StartFromAndLerpInDirection(opponentBidObj.gameObject, opponentBidObj.transform.position, -opponentBidAnimPoint.forward, 0.7f, 4));
        yield return new WaitForSeconds(0.5f);
        playerBidObj.SetEnabled(false);
        opponentBidObj.SetEnabled(false);
    }

    IEnumerator TradeBidsAnim()
    {
        StartCoroutine(StartFromAndLerpInDirection(playerBidObj.gameObject, playerBidObj.transform.position, playerBidAnimPoint.forward, 1.0f, 19));
        StartCoroutine(StartFromAndLerpInDirection(opponentBidObj.gameObject, opponentBidObj.transform.position, opponentBidAnimPoint.forward, 1.0f, 19));
        yield return new WaitForSeconds(1.0f);
        playerBidObj.SetEnabled(false);
        opponentBidObj.SetEnabled(false);

    }

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
        winText.text = "";
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
                playerPlayObj.SetCard(new RPS_Card(RPS_Card.CardType.Unknown));
                playerPlayObj.SetEnabled(true);
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
                opponentBidObj.SetCard(card);
                opponentBidObj.transform.rotation = Quaternion.Euler(-90, 0, 0);
                StartCoroutine(StartFromAndLerpInDirection(
                    opponentBidObj.gameObject, opponentBidAnimPoint.position, opponentBidAnimPoint.forward, 0.7f, 4));
                SendStateChangeForComment(PlayState.PlayerBid);
                break;
            case PlayState.PlayerBid:
                playerBidObj.SetEnabled(true);
                playerBidObj.SetCard(card);
                playerBidObj.transform.rotation = Quaternion.Euler(90, 0, 0);
                StartCoroutine(StartFromAndLerpInDirection(
                    playerBidObj.gameObject, playerBidAnimPoint.position, playerBidAnimPoint.forward, 0.7f, 4));
                SendStateChangeForComment(PlayState.OpponentPlay);
                break;
            case PlayState.OpponentPlay:
                opponentPlay = card;
                opponentPlayObj.SetEnabled(true);
                opponentPlayObj.SetCard(card);
                opponentPlayObj.transform.rotation = Quaternion.Euler(-90, 0, 0);
                StartCoroutine(StartFromAndLerpInDirection(
                    opponentPlayObj.gameObject, opponentPlayAnimPoint.position, opponentPlayAnimPoint.forward, 0.7f, 8));
                SendStateChangeForComment(PlayState.PlayerPlay);
                break;
            case PlayState.PlayerPlay:
                playerPlay = card;
                playerPlayObj.SetEnabled(true);
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

    public void RevealCards()
    {
        StartCoroutine(IRevealCards());
    }

    IEnumerator IRevealCards()
    {
        playerPlayObj.AnimatePlayReveal(false);
        yield return new WaitForSeconds(0.1f);
        float revealTime = opponentPlayObj.AnimatePlayReveal(true);
        yield return new WaitForSeconds(revealTime);
        yield return new WaitForSeconds(0.4f);


        roundResult = RPS_Card.GetPlayResult(playerPlay, opponentPlay);
        string resStr = "Tie : Bids Returned";
        resStr = roundResult == RPS_Card.Result.Loss ? "Loss" : resStr;
        resStr = roundResult == RPS_Card.Result.Win ? "Win" : resStr;
        winText.text = resStr;
        yield return new WaitForSeconds(1.2f);
        if (roundResult == RPS_Card.Result.Tie) {
            StartCoroutine(ReturnBidsAnim());
        }
        yield return new WaitForSeconds(0.5f);
        playerPoints += roundResult == RPS_Card.Result.Win ? 1 : 0;
        opponentPoints += roundResult == RPS_Card.Result.Loss ? 1 : 0;

        yield return new WaitForSeconds(opponent.GetResponseToReveal(roundResult));

        playerPlayObj.SetEnabled(false);
        opponentPlayObj.SetEnabled(false);

        yield return new WaitForSeconds(0.3f);
        GameObject card1 = Instantiate(physicsCard, physicsSpawnPoint.position + Random.insideUnitSphere * sphereRadius, Quaternion.Euler(90, 0, 0));
        card1.transform.Rotate((new Vector3(rotRanges.x * Random.value, rotRanges.y * Random.value, rotRanges.z * Random.value) * 2.0f) - rotRanges);
        card1.GetComponent<RPS_CardObject>().SetCard(playerPlay);
        yield return new WaitForSeconds(0.2f);
        GameObject card2 = Instantiate(physicsCard, physicsSpawnPoint.position + Random.insideUnitSphere * sphereRadius, Quaternion.Euler(90, 0, 0));
        card2.transform.Rotate((new Vector3(rotRanges.x * Random.value, rotRanges.y * Random.value, rotRanges.z * Random.value) * 2.0f) - rotRanges);
        card2.GetComponent<RPS_CardObject>().SetCard(opponentPlay);

        yield return new WaitForSeconds(0.3f);

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






    public void SendTradeCardsDecision(bool tradeCards, RPS_Card.CardType playerCard = RPS_Card.CardType.Unknown)
    {
        // TODO : swing this
        bLastBidsTraded = tradeCards;
        print("The player card for trading is: " + playerCard);

        if (playerCard == RPS_Card.CardType.Unknown && tradeCards)
        {
            print("Branching to get bid");
            RequestBidCardAfterDecision();
        }
        else
        {
            print("Branching to trade");
            StartCoroutine(ITradeCards(tradeCards, playerCard));
        }
    }
    // boomerang X???? Boring??
    private void RequestBidCardAfterDecision()
    {
        player.RequestBidReveal();
    }

    public void SendBidCardAfterDecision(RPS_Card.CardType card)
    {
        StartCoroutine(ITradeCards(true, card));
    }

    IEnumerator ITradeCards(bool tradeCards, RPS_Card.CardType playerCard)
    {
        if (tradeCards)
        {
            tradeText.text = "Cards Traded : Place your bid aside and take a " + opponentBid.type;
            float t = opponentBidObj.AnimateBidReveal(true);
            playerBidObj.SetCard(new RPS_Card(playerCard));
            // t = playerBidObj.AnimateBidReveal(true);

            yield return new WaitForSeconds(t);
            yield return new WaitForSeconds(1.2f); // TODO : additional wait???


            opponent.GainCard(new RPS_Card(playerCard));
            player.GainCard(opponentBid);
            player.LoseCard(new RPS_Card(playerCard));
            StartCoroutine(TradeBidsAnim());

        }
        else
        {
            tradeText.text = "Cards NOT Traded. You retain your card, pick it up.";
            StartCoroutine(ReturnBidsAnim());
            opponent.GainCard(opponentBid);
            // player.GainCard(new RPS_Card(playerCard));
        }

        yield return new WaitForSeconds(2.2f);
        tradeText.text = "";
        SendStateChangeForComment(PlayState.OpponentBid);
    }
}
