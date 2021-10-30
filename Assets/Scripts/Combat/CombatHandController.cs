using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatHandController : MonoBehaviour
{
    public List<GameObject> cardsInHand = new List<GameObject>();
    public GameObject drawPile;
    public GameObject discardPile;
    public int startingHandSize = 4;
    public int maxHandSize = 7;
    public int cardsInDiscard = 0;

    public GameObject cardWorldColliderPrefab;
    public Transform cardWorldColliderParent;
    public CombatManager cm;

    Camera mainCam;

    // Start is called before the first frame update
    void Start()
    {
        mainCam = FindObjectOfType<Camera>();
    }

    private void Awake()
    {
        for(int i = 0; i < startingHandSize; i++)
        {
            GameObject card = Deck.instance.Draw();
            card.transform.localScale = transform.localScale;
            card.GetComponent<RectTransform>().SetParent(transform);
            card.GetComponent<CardEditHandler>().inCombat = true;

            UIToWorldCollider utwc = card.GetComponent<UIToWorldCollider>();
            utwc.mainCam = mainCam;
            utwc.inHand = true;
            GameObject cardWorldCollider = Instantiate(cardWorldColliderPrefab, Vector3.zero, Quaternion.identity ,cardWorldColliderParent);
            cardWorldCollider.GetComponent<CardWorldColliderDetection>().dragDrop = card.GetComponent<DragDrop>();
            utwc.colliderGO = cardWorldCollider;
            utwc.bc = cardWorldCollider.GetComponent<BoxCollider2D>();
            utwc.rb = cardWorldCollider.GetComponent<Rigidbody2D>();
            cardsInHand.Add(card);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DisableDrag()
    {
        foreach(GameObject card in cardsInHand)
        {
            card.GetComponent<DragDrop>().isDraggable = false;
        }
    }

    public void EnableDrag()
    {
        foreach (GameObject card in cardsInHand)
        {
            card.GetComponent<DragDrop>().isDraggable = true;
        }
    }

    public void DrawCards(int cardAmount)
    {
        if (transform.childCount + cardAmount > maxHandSize)
        {
            Debug.Log("You Can't Draw That Many Cards!");
            return;
        }
        if(Deck.instance.deck.Count < cardAmount)
        {
            Debug.Log("Not Enough Cards In Deck To Draw!");
            return;
        }

        for(int i = 0; i < cardAmount; i++)
        {
            GameObject card = Deck.instance.Draw();
            card.transform.localScale = transform.localScale;
            card.GetComponent<RectTransform>().SetParent(transform);
            card.GetComponent<CardEditHandler>().inCombat = true;

            UIToWorldCollider utwc = card.GetComponent<UIToWorldCollider>();
            utwc.mainCam = mainCam;
            utwc.inHand = true;
            GameObject cardWorldCollider = Instantiate(cardWorldColliderPrefab, Vector3.zero, Quaternion.identity, cardWorldColliderParent);
            cardWorldCollider.GetComponent<CardWorldColliderDetection>().dragDrop = card.GetComponent<DragDrop>();
            utwc.colliderGO = cardWorldCollider;
            utwc.bc = cardWorldCollider.GetComponent<BoxCollider2D>();
            utwc.rb = cardWorldCollider.GetComponent<Rigidbody2D>();
            cardsInHand.Add(card);
        }
        switch (cardAmount)
        {
            case 1:
                cm.AddMana(20);
                break;
            case 2:
                cm.AddMana(18);
                break;
            case 3:
                cm.AddMana(16);
                break;
            case 4:
                cm.AddMana(14);
                break;
        }
        cm.NextPhase();
    }

    public void DiscardCard(GameObject card)
    {
        cardsInDiscard++;
        Deck.instance.Discard(card);
    }

    public void ReShuffle()
    {
        cardsInDiscard = 0;
        Deck.instance.Shuffle();
    }
}
