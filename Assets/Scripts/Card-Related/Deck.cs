using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Deck : MonoBehaviour
{
    [SerializeField]
    public GameObject offscreenPos;
    [SerializeField]
    public GameObject draggablePos;

    public static Deck instance;

    public List<GameObject> deck = new List<GameObject>();
    [SerializeField]
    public List<GameObject> starterDeck;
    [SerializeField]
    public List<GameObject> allCards;
    List<GameObject> discard = new List<GameObject>();
    public Dictionary<string, List<GameObject>> freeDraggables = new Dictionary<string, List<GameObject>>();
    [SerializeField]
    public List<GameObject> numMods;
    [SerializeField]
    public List<GameObject> elemMods;
    [SerializeField]
    public List<GameObject> utilMods;

    public string sceneToLoad = "CustomizedCardTestScene";

    //enforcing singleton of deck on game start
    void Awake()
    {
        if (instance != this && instance != null)
            Destroy(gameObject);
        else
            instance = this;

        DontDestroyOnLoad(gameObject);

        //handle initial deck - CHANGE THIS ONCE ALL CARDS IN
        GameObject[] cards = new GameObject[starterDeck.Count];
        starterDeck.CopyTo(cards);
        deck.AddRange(cards);

        ModifierLookup.LoadModifierTable();
        foreach (GameObject card in allCards)
        {
            card.GetComponent<Card>().InitializeCard();
        }

        //initializing free draggables to start
        freeDraggables = new Dictionary<string, List<GameObject>>();
        freeDraggables["num"] = numMods;
        freeDraggables["element"] = elemMods;
        freeDraggables["utility"] = utilMods;

        SceneManager.LoadScene(sceneToLoad);
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

    public void ToggleButtons(bool isActive)
    {
        foreach (GameObject card in allCards)
        {
            card.GetComponent<Button>().enabled = isActive;
        }
    }

    public void HideCards()
    {
        foreach (GameObject card in allCards)
        {
            RectTransform trans = card.GetComponent<RectTransform>();
            trans.SetParent(offscreenPos.transform);
            trans.anchorMax = new Vector2(0.5f, 0.5f);
            trans.anchorMin = new Vector2(0.5f, 0.5f);
            trans.anchoredPosition = new Vector2(0.5f, 0.5f);
            trans.localPosition = new Vector3(0, 0, 0);
        }
    }

    public void HideCard(GameObject card)
    {
        if (allCards.Contains(card))
        {
            RectTransform trans = card.GetComponent<RectTransform>();
            trans.SetParent(offscreenPos.transform);
            trans.anchorMax = new Vector2(0.5f, 0.5f);
            trans.anchorMin = new Vector2(0.5f, 0.5f);
            trans.anchoredPosition = new Vector2(0.5f, 0.5f);
            trans.localPosition = new Vector3(0, 0, 0);
        }
    }

    public void SetDragger(GameObject dragger, bool isCustomization)
    {
        foreach (GameObject card in allCards)
        {
            card.GetComponent<DragDrop>().dragger = dragger;
        }
        if (isCustomization)
        {
            foreach (KeyValuePair<string, List<GameObject>> list in freeDraggables)
            {
                foreach (GameObject drag in list.Value)
                {
                    drag.GetComponent<DragDrop>().dragger = dragger;
                }
            }
        }
    }
}
