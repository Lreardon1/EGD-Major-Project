using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatHandController : MonoBehaviour
{
    public List<GameObject> cardsInHand = new List<GameObject>();
    public GameObject drawPile;
    public GameObject discardPile;
    public int startingHandSize = 4;

    public GameObject cardWorldColliderPrefab;
    public Transform cardWorldColliderParent;

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

    public void DrawCard()
    {
        GameObject card = Deck.instance.Draw();
        card.transform.localScale = transform.localScale;
        card.GetComponent<RectTransform>().SetParent(transform);
        card.GetComponent<CardEditHandler>().inCombat = true;
        card.GetComponent<UIToWorldCollider>().mainCam = mainCam;
        card.GetComponent<UIToWorldCollider>().inHand = true;
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
