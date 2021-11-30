using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Jay note : this class should have been the one to control ALL movement of cards, 
public class CombatHandController : MonoBehaviour
{
    public bool isActiveControlScheme = true;
    public List<GameObject> cardsInHand = new List<GameObject>();
    public GameObject drawPile;
    public GameObject discardPile;
    public Text reshuffleText;
    public int startingHandSize = 4;
    public int maxHandSize = 7;
    public int cardsInDiscard = 0;

    public float cardLocalScale = 0.8f;

    public GameObject cardWorldColliderPrefab;
    public Transform cardWorldColliderParent;
    public CombatManager cm;
    public GameObject dragger;

    public Transform originalCardTransform;

    private int reShuffleCost = 15;

    Camera mainCam;

    // Start is called before the first frame update
    void Start()
    {
        if (!isActiveControlScheme || CombatManager.IsInCVMode)
            return;

        mainCam = FindObjectOfType<Camera>();
    }

    private void Awake()
    {
        if (!isActiveControlScheme || CombatManager.IsInCVMode)
            return;

        Invoke("DrawStartingHand", 0.1f);
    }

    public void DrawStartingHand()
    {
        Deck.instance.SetDragger(dragger, false);
        Deck.instance.Shuffle();
        for (int i = 0; i < startingHandSize; i++)
        {
            GameObject card = Deck.instance.Draw();
            Debug.Log(Deck.instance.deck.Count);
            originalCardTransform = card.transform.parent;
            card.GetComponent<RectTransform>().SetParent(transform);
            card.transform.localScale = new Vector3(cardLocalScale, cardLocalScale, cardLocalScale);
            card.GetComponent<CardEditHandler>().inCombat = true;
            
            cardsInHand.Add(card);
        }
        UpdateReshuffleCost();
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

    public void UpdateDropZones()
    {
        switch (cm.currentPhase)
        {
            case CombatManager.CombatPhase.DrawPhase:
                foreach (GameObject card in Deck.instance.allCards)
                {
                    DragDrop dd = card.GetComponent<DragDrop>();
                    dd.isDraggable = false;
                    dd.allowedDropZones.Clear();
                }
                break;
            case CombatManager.CombatPhase.PlayPhase:
                foreach (GameObject card in Deck.instance.allCards)
                {
                    DragDrop dd = card.GetComponent<DragDrop>();
                    List<GameObject> allZones = new List<GameObject>();
                    foreach(GameObject member in cm.activePartyMembers)
                    {
                        allZones.Add(member.GetComponent<CombatantBasis>().uiCollider);
                    }
                    foreach (GameObject enemy in cm.activeEnemies)
                    {
                        allZones.Add(enemy.GetComponent<CombatantBasis>().uiCollider);
                    }
                    dd.isDraggable = true;
                    dd.allowedDropZones.Clear();
                    dd.allowedDropZones = allZones;
                }
                break;
            case CombatManager.CombatPhase.DiscardPhase:
                foreach (GameObject card in Deck.instance.allCards)
                {
                    DragDrop dd = card.GetComponent<DragDrop>();
                    List<GameObject> allZones = new List<GameObject>();
                    allZones.Add(discardPile);
                    dd.isDraggable = true;
                    dd.allowedDropZones.Clear();
                    dd.allowedDropZones = allZones;
                }
                break;
            case CombatManager.CombatPhase.ActionPhase:
                foreach (GameObject card in Deck.instance.allCards)
                {
                    DragDrop dd = card.GetComponent<DragDrop>();
                    dd.isDraggable = true;
                    dd.allowedDropZones.Clear();
                }
                break;
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
            card.GetComponent<RectTransform>().SetParent(transform);
            card.transform.localScale = new Vector3(cardLocalScale, cardLocalScale, cardLocalScale);
            card.GetComponent<CardEditHandler>().inCombat = true;
            
            cardsInHand.Add(card);
        }
        switch (cardAmount)
        {
            case 0:
                cm.AddMana(18);
                break;
            case 1:
                cm.AddMana(16);
                break;
            case 2:
                cm.AddMana(14);
                break;
            case 3:
                cm.AddMana(12);
                break;
            case 4:
                cm.AddMana(10);
                break;
        }
    }

    public void DiscardCard(GameObject card)
    {
        cardsInDiscard++;
        cardsInHand.Remove(card);
        Deck.instance.Discard(card);
        UpdateReshuffleCost();
    }

    public void ReShuffle()
    {
        if(cm.currentMana < reShuffleCost)
        {
            Debug.Log("Not Enough Mana To Reshuffle Discard Pile!");
            return;
        }
        Deck.instance.Shuffle();
        UpdateReshuffleCost();
        cm.reshuffleButton.interactable = false;
    }

    public void ReturnCardsInHand()
    {
        foreach(GameObject card in cardsInHand)
        {
            Deck.instance.Discard(card);
        }
        Deck.instance.Shuffle();
    }

    public void UpdateReshuffleCost()
    {
        int manaCost =  Mathf.Clamp(15 - Deck.instance.discard.Count, 0, 15);
        reshuffleText.text = "Reshuffle: " + manaCost + " Mana";
        reShuffleCost = manaCost;
    }

    public void ResetCardParents()
    {
        Deck.instance.HideCards();
        foreach(GameObject card in Deck.instance.allCards)
        {
            DragDrop dd = card.GetComponent<DragDrop>();
            dd.allowedDropZones.Clear();
            card.transform.localScale = new Vector3(1, 1, 1);
        }
    }
}
