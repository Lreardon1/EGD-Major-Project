using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RPS_Opponent : MonoBehaviour
{

    private RPS_PlayManager manager;
    private Dictionary<RPS_Card.CardType, int> myCards;
    private List<RPS_Card.CardType> myCardsArr = new List<RPS_Card.CardType>();

    public TMP_Text talkText;

    public void Init(RPS_PlayManager manager, Dictionary<RPS_Card.CardType, int> initCards)
    {
        this.manager = manager;
        myCards = new Dictionary<RPS_Card.CardType, int>(initCards);
        foreach (RPS_Card.CardType ct in myCards.Keys)
        {
            for (int i = 0; i < myCards[ct]; ++i)
                myCardsArr.Add(ct);
        }
    }

    private void WriteOutTalk(string text)
    {
        talkText.text = text;
    }

    private IEnumerator IHandleStateChangeComment(RPS_PlayManager.PlayState nextState)
    {
        WriteOutTalk("Onto the next stage");
        yield return new WaitForSeconds(0.2f);
        manager.ProgressFromComment(nextState);
    }


    private IEnumerator IHandleBid()
    {
        WriteOutTalk("Hmmm, let's see...");
        yield return new WaitForSeconds(1.0f);
        WriteOutTalk("I'll bid this one...");
        yield return new WaitForSeconds(0.6f);
        int r = Random.Range(0, myCardsArr.Count);
        RPS_Card.CardType ct = myCardsArr[r];
        myCardsArr.RemoveAt(r);
        myCards[ct] -= 1;

        RPS_Card card = new RPS_Card(ct);
        // TODO : polish anims and make a card
        manager.SendCardInContext(card);
    }

    private IEnumerator IHandlePlay()
    {
        WriteOutTalk("Hmmm, let's see...");
        yield return new WaitForSeconds(1.0f);
        WriteOutTalk("I'll play this one...");
        yield return new WaitForSeconds(0.6f);
        int r = Random.Range(0, myCardsArr.Count);
        RPS_Card.CardType ct = myCardsArr[r];
        myCardsArr.RemoveAt(r);
        myCards[ct] -= 1;


        RPS_Card card = new RPS_Card(ct);
        // TODO : polish anims and make a card
        manager.SendCardInContext(card);
    }

    private IEnumerator IHandleTradeDecision()
    {
        bool trade = Random.value > 0.5f;
        WriteOutTalk(trade ? "Hmm... I think I'll trade..." : "Hmm, no, I think I'll keep this fella...");
        yield return new WaitForSeconds(0.7f);
        manager.SendTradeCardsDecision(Random.value > 0.5f);
        WriteOutTalk("");
    }

    public void RequestCommentOnStateChange(RPS_PlayManager.PlayState state, RPS_PlayManager.PlayState nextState)
    {
        StartCoroutine(IHandleStateChangeComment(nextState));
    }


    public void AskForPlay()
    {
        StartCoroutine(IHandlePlay());
    }

    public void GainCard(RPS_Card gained)
    {
        myCards[gained.type] += 1;
        myCardsArr.Add(gained.type);
    }

    public void RequestBid()
    {
        StartCoroutine(IHandleBid());
    }

    public void RequestPlay()
    {
        StartCoroutine(IHandlePlay());
    }

    public void RequestTradeDecision()
    {
        StartCoroutine(IHandleTradeDecision());
    }
}
