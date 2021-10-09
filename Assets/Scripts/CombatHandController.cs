using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatHandController : MonoBehaviour
{
    public List<GameObject> cardsInHand = new List<GameObject>();
    public GameObject drawPile;
    public GameObject discardPile;
    public int startingHandSize = 4;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Awake()
    {
        for(int i = 0; i < startingHandSize; i++)
        {
            GameObject card = Deck.instance.Draw();
            card.transform.localScale = transform.localScale;
            card.GetComponent<RectTransform>().SetParent(transform);
            cardsInHand.Add(card);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DrawCard()
    {
        GameObject card = Deck.instance.Draw();
        card.transform.localScale = transform.localScale;
        card.GetComponent<RectTransform>().SetParent(transform);
        cardsInHand.Add(card);
    }

    public void DiscardCard(GameObject card)
    {
        Deck.instance.Discard(card);
    }

    public void ReShuffle()
    {
        Deck.instance.Shuffle();
    }
}
