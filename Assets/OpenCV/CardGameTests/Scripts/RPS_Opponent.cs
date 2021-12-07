using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class RPS_Opponent : MonoBehaviour
{

    private RPS_PlayManager manager;
    private Dictionary<RPS_Card.CardType, int> myCards;
    private List<RPS_Card.CardType> myCardsArr = new List<RPS_Card.CardType>();

    public TMP_Text talkText;
    public GameObject interactiveTalkArrow;
    public string returnScene;

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

    private void WriteOutTalk(string text, bool interactive = false)
    {
        talkText.text = text;
        interactiveTalkArrow.SetActive(interactive);
    }


    private bool SpacePressed()
    {
        return Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0);
    }

    private IEnumerator IHandleStateChangeComment(RPS_PlayManager.PlayState state, RPS_PlayManager.PlayState nextState)
    {
        if (nextState == RPS_PlayManager.PlayState.End)
        {
            if (manager.playerPoints > manager.opponentPoints)
                WriteOutTalk("Brilliant work, kid. You've got a thing for cards!");
            else if (manager.playerPoints < manager.opponentPoints)
                WriteOutTalk("Better luck next time, kid. Feel free to play again!");
            else if (manager.playerPoints == manager.opponentPoints)
                WriteOutTalk("Dang! Sorry, we couldn't have a more exciting end, kid.");
            yield return new WaitForSeconds(2.7f);
            SceneManager.LoadScene(returnScene);
            yield break;
        }
        // TODO : we could do a dynamic system on this...
        string[] bidPhrases = new string[] { "Now, it's your turn to bid, kid.", "Your card bid, kid.", "You feeling like bidding a Fire for me, kid?", "You're gonna bid a Water, aren't you?" };
        string[] playPhrases = new string[] { "Now, it's your turn to bid, kid.", "Your card bid, kid.", "You feeling like bidding a Fire for me, kid?", "You're gonna bid a Water, aren't you?" };
        switch (state)
        {
            case RPS_PlayManager.PlayState.Start:
                WriteOutTalk("You ever played Rips before, kid? [Y/N]");
                while (true)
                {
                    if (Input.GetKeyDown(KeyCode.N))
                    {
                        WriteOutTalk("That's fine, kid. I'll give ya a quick little run-down.", true);
                        yield return new WaitUntil(SpacePressed); yield return new WaitForSeconds(0.1f);
                        WriteOutTalk("Now, I know you've played Rock-Paper-Scissors. Keep that game in mind, kid.", true);
                        yield return new WaitUntil(SpacePressed); yield return new WaitForSeconds(0.1f);
                        WriteOutTalk("We each get 6 cards, 2 Fire, 2 Water, 2 Wind.", true);
                        yield return new WaitUntil(SpacePressed); yield return new WaitForSeconds(0.1f);
                        WriteOutTalk("Water beats Fire, Fire beats Wind, and Wind beats Water...", true);
                        yield return new WaitUntil(SpacePressed); yield return new WaitForSeconds(0.1f);
                        WriteOutTalk("Don't look at me like that, I don't understand the match-ups either...", true);
                        yield return new WaitUntil(SpacePressed); yield return new WaitForSeconds(0.1f);
                        WriteOutTalk("Uh, let's say: Water puts out Fire, Fire becomes unstoppable in the Wind, and... uh... the Wind kicks up tsunamis?", true);
                        yield return new WaitUntil(SpacePressed); yield return new WaitForSeconds(0.1f);
                        WriteOutTalk("Anyway, kid, each round, we bid a card face-down and then play a card at once face-up.", true);
                        yield return new WaitUntil(SpacePressed); yield return new WaitForSeconds(0.1f);
                        WriteOutTalk("The winner of the face-up gets a point and gets to decide if the bids are traded or kept.", true);
                        yield return new WaitUntil(SpacePressed); yield return new WaitForSeconds(0.1f);
                        WriteOutTalk("The proper winner is the one with the most points at the end.", true);
                        yield return new WaitUntil(SpacePressed); yield return new WaitForSeconds(0.1f);
                        WriteOutTalk("Alright, you only look slightly confused, kid. Let's start playing!", true);
                        yield return new WaitUntil(SpacePressed); yield return new WaitForSeconds(0.1f);

                        break;
                    } else if (Input.GetKeyDown(KeyCode.Y))
                    {
                        WriteOutTalk("Perfect! Let's get playing then, kid!", false);
                        yield return new WaitForSeconds(1.7f);
                        break;
                    }
                    yield return null;
                }
                yield return new WaitForSeconds(0.2f);
                break;

            case RPS_PlayManager.PlayState.OpponentBid:
                if (manager.player.HasCard(RPS_Card.CardType.Fire) && Random.value < 0.2f) WriteOutTalk("You feeling like bidding a Fire for me, kid?");
                else if (manager.player.HasCard(RPS_Card.CardType.Water) && Random.value < 0.2f) WriteOutTalk("You're gonna bid a Water, aren't you ?");
                else WriteOutTalk(bidPhrases[Random.Range(0, bidPhrases.Length)]);
                yield return new WaitForSeconds(1.9f);
                break;
            case RPS_PlayManager.PlayState.PlayerBid:
                WriteOutTalk("What DID you put down, kid?");
                yield return new WaitForSeconds(2.0f);
                break;
            case RPS_PlayManager.PlayState.OpponentPlay:
                WriteOutTalk("Onto you!");
                yield return new WaitForSeconds(1.9f);
                break;
            case RPS_PlayManager.PlayState.PlayerPlay:
                WriteOutTalk(Random.value> 0.5f ? "Time to reveal! I've got a good feeling about this!" : "Time to reveal! How you feeling, kid?");
                yield return new WaitForSeconds(1.9f);
                break;
            case RPS_PlayManager.PlayState.Reveal:
                RPS_Card.CardType myPlay = manager.opponentPlay.type;
                if (myPlay == manager.playerPlay.type)
                {
                    WriteOutTalk($"A TIE on {myPlay}, nobody's gonna win like this...");
                    yield return new WaitForSeconds(2.2f);
                    break;
                }
                switch (manager.playerPlay.type)
                {
                    case RPS_Card.CardType.Water:
                        if (myPlay == RPS_Card.CardType.Wind)
                            WriteOutTalk($"You WON with WATER! So much steam now, huh, kid.");
                        else
                            WriteOutTalk($"You LOST to WIND. Not even the ocean can stop a hurricane!");
                        break;
                    case RPS_Card.CardType.Fire:
                        if (myPlay == RPS_Card.CardType.Fire)
                            WriteOutTalk($"You WON with FIRE, huh? Guess I should have expected given from your spirit!");
                        else
                            WriteOutTalk($"You LOST to WATER? Never underestimate the TIDE!");
                        break;
                    case RPS_Card.CardType.Wind:
                        if (myPlay == RPS_Card.CardType.Water)
                            WriteOutTalk($"You WON with WIND. Sometimes, I still forget that beats WATER.");
                        else
                            WriteOutTalk($"You LOST to FIRE. Your wind just made me stronger!");
                        break;
                }
                yield return new WaitForSeconds(2.2f);
                break;
            case RPS_PlayManager.PlayState.Trade:
                WriteOutTalk(manager.bLastBidsTraded ? "Now, what can I do with this card?" : "Nothing ventured, nothing gained.");
                yield return new WaitForSeconds(1.9f);
                break;
            case RPS_PlayManager.PlayState.End:
                WriteOutTalk("THIS IS A BUG, YOU SHOULD NOT BE HERE!.");
                yield return new WaitForSeconds(1.9f);
                break;
        }
        manager.ProgressFromComment(nextState);
    }


    private IEnumerator IHandleBid()
    {
        WriteOutTalk("Hmmm, let's see...");
        yield return new WaitForSeconds(0.6f);
        WriteOutTalk("I'll bid this one...");
        yield return new WaitForSeconds(0.9f);
        print(myCardsArr.Count + " Left in opponent's hand : for bid");
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
        if (myCardsArr.Count > 1)
            WriteOutTalk(Random.value > 0.5f ? "Hmmm, let's see..." : "What card, what card...?");
        else WriteOutTalk("Only one left to play.");
        yield return new WaitForSeconds(1.0f);
        if (myCardsArr.Count > 1)
            WriteOutTalk(Random.value > 0.5f ? "I'll play this one..." : "I'm sure the anticipation is killing you.");
        else
            WriteOutTalk("Do you think you've got me, kid?");

        yield return new WaitForSeconds(0.9f);
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
        WriteOutTalk(trade ? "Hmm... I think I'll trade..." : "Hmm, no, I think I'll keep my card...");
        yield return new WaitForSeconds(1.8f);
        manager.SendTradeCardsDecision(trade);
        WriteOutTalk("");
    }

    public void RequestCommentOnStateChange(RPS_PlayManager.PlayState state, RPS_PlayManager.PlayState nextState)
    {
        StartCoroutine(IHandleStateChangeComment(state, nextState));
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

    public float GetResponseToReveal(RPS_Card.Result result)
    {
        switch (result)
        {
            case RPS_Card.Result.Win:
                WriteOutTalk("Good play, kid.");
                return 1.0f;
            case RPS_Card.Result.Loss:
                WriteOutTalk("Gotta be smarter, kid.");
                return 1.0f;
            case RPS_Card.Result.Tie:
                WriteOutTalk("That was super boring though...");
                return 1.0f;
        }
        return 0;
    }

    public int GetNumberCards()
    {
        return myCardsArr.Count;
    }
}
