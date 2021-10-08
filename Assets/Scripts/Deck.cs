using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{

    List<Card> deck = new List<Card>();
    List<Card> discard = new List<Card>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public Card Draw()
    {
        if (deck.Count == 0)
            return null;
        else
        {
            Card topDeck = deck[0];
            deck.RemoveAt(0);
            return topDeck;
        }
    }

    public void Discard(Card c)
    {
        discard.Add(c);
    }

    public void Shuffle()
    {
        deck.AddRange(discard);

        //shuffling based on Knuth shuffle algorithm
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int index = Random.Range(0, i);
            Card a = deck[index];
            deck[index] = deck[i];
            deck[i] = a;
        }

        discard.Clear();
    }
}
