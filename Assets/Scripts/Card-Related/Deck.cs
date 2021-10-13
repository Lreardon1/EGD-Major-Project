using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Deck : MonoBehaviour
{
    [SerializeField]
    public GameObject offscreenPos;

    public static Deck instance;

    List<GameObject> deck = new List<GameObject>();
    public List<GameObject> viewOrder;
    List<GameObject> discard = new List<GameObject>();

    //enforcing singleton of deck on game start
    void Awake()
    {
        if (instance != this && instance != null)
            Destroy(gameObject);
        else
            instance = this;

        DontDestroyOnLoad(gameObject);

        GameObject[] cards = new GameObject[viewOrder.Count];
        viewOrder.CopyTo(cards);
        deck.AddRange(cards);

        ModifierLookup.LoadModifierTable();
        foreach (GameObject card in viewOrder)
        {
            card.GetComponent<Card>().InitializeCard();
        }

        SceneManager.LoadScene("CustomizedCardTestScene");
    }

    public GameObject Draw()
    {
        if (deck.Count == 0)
            return null;
        else
        {
            GameObject topDeck = deck[0];
            deck.RemoveAt(0);
            return topDeck;
        }
    }

    public void Discard(GameObject c)
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
            GameObject a = deck[index];
            deck[index] = deck[i];
            deck[i] = a;
        }

        discard.Clear();
    }

    public void ToggleButtons()
    {
        foreach (GameObject card in viewOrder)
        {
            card.GetComponent<Button>().enabled = !card.GetComponent<Button>().enabled;
        }
    }

    public void HideCards()
    {
        foreach (GameObject card in viewOrder)
        {
            card.GetComponent<RectTransform>().SetParent(offscreenPos.transform);
        }
    }

    public void HideCard(GameObject card)
    {
        if (viewOrder.Contains(card))
        {
            card.GetComponent<RectTransform>().SetParent(offscreenPos.transform);
        }
    }
}
