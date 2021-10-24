using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckCustomizer : MonoBehaviour
{
    [SerializeField]
    public GameObject cardRenderer;
    [SerializeField]
    public GameObject draggableSprite;
    [SerializeField]
    public GameObject numStorage;
    [SerializeField]
    public GameObject elementStorage;
    [SerializeField]
    public GameObject utilityStorage;
    [SerializeField]
    public GameObject editorDropZone;
    [SerializeField]
    public GameObject modsDropZone;
    [SerializeField]
    public GameObject cantAcceptWindow;
    [SerializeField]
    public GameObject customizationWindow;

    public GameObject cardEditor;

    void Start()
    {
        cardEditor.SetActive(false);
        SetUp();
    }

    public void SetUp()
    {
        GridLayoutGroup cardGrid = gameObject.GetComponent<GridLayoutGroup>();
        Vector2 cardSize = cardRenderer.GetComponent<RectTransform>().sizeDelta;
        cardGrid.cellSize = new Vector2(cardSize.x, cardSize.y);

        GridLayoutGroup numGrid = numStorage.GetComponent<GridLayoutGroup>();
        GridLayoutGroup elementGrid = elementStorage.GetComponent<GridLayoutGroup>();
        GridLayoutGroup utilGrid = utilityStorage.GetComponent<GridLayoutGroup>();
        Vector2 elementSize = draggableSprite.GetComponent<RectTransform>().sizeDelta;
        numGrid.cellSize = new Vector2(elementSize.x, elementSize.y);
        elementGrid.cellSize = new Vector2(elementSize.x, elementSize.y);
        utilGrid.cellSize = new Vector2(elementSize.x, elementSize.y);

        List<GameObject> deck = Deck.instance.viewOrder;
        foreach (GameObject c in deck)
        {
            c.GetComponent<DragDrop>().isDraggable = false;
            c.GetComponent<RectTransform>().SetParent(transform);
            c.GetComponent<BoxCollider2D>().enabled = false;
        }

        Dictionary<string, List<GameObject>> freeDraggables = Deck.instance.freeDraggables;
        if (freeDraggables.ContainsKey("num"))
        {
            List<GameObject> currList = freeDraggables["num"];
            foreach (GameObject mod in currList)
            {
                mod.GetComponent<RectTransform>().SetParent(numStorage.transform);
            }
        }

        if (freeDraggables.ContainsKey("element"))
        {
            List<GameObject> currList = freeDraggables["element"];
            foreach (GameObject mod in currList)
            {
                mod.GetComponent<RectTransform>().SetParent(elementStorage.transform);
            }
        }

        if (freeDraggables.ContainsKey("utility"))
        {
            List<GameObject> currList = freeDraggables["utility"];
            foreach (GameObject mod in currList)
            {
                mod.GetComponent<RectTransform>().SetParent(utilityStorage.transform);
            }
        }
    }

    public void AttemptAccept()
    {   
        //checking for balanced banks
        if (true)
        {
            AcceptAndStore();
        }
        else
        {
            ShowUnableToAccept();
        }
    }

    public void ShowUnableToAccept()
    {
        cantAcceptWindow.SetActive(true);
    }

    public void HideUnableToAccept()
    {
        cantAcceptWindow.SetActive(false);
    }

    private void AcceptAndStore()
    {
        //need to store any remaining modifiers and move cards out of scene, then close customization canvas
        List<GameObject> storedDrags;
        if (Deck.instance.freeDraggables.ContainsKey("num"))
        {
            storedDrags = Deck.instance.freeDraggables["num"];
            storedDrags.Clear();
        }
        else
        {
            storedDrags = new List<GameObject>();
            Deck.instance.freeDraggables["num"] = storedDrags;
        }
        int childCount = numStorage.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            GameObject child = numStorage.transform.GetChild(0).gameObject;
            storedDrags.Add(child);
            RectTransform trans = child.GetComponent<RectTransform>();
            trans.SetParent(Deck.instance.draggablePos.transform);
            trans.anchorMax = new Vector2(0.5f, 0.5f);
            trans.anchorMin = new Vector2(0.5f, 0.5f);
            trans.anchoredPosition = new Vector2(0.5f, 0.5f);
            trans.localPosition = new Vector3(0, 0, 0);
        }

        if (Deck.instance.freeDraggables.ContainsKey("element"))
        {
            storedDrags = Deck.instance.freeDraggables["element"];
            storedDrags.Clear();
        }
        else
        {
            storedDrags = new List<GameObject>();
            Deck.instance.freeDraggables["element"] = storedDrags;
        }
        childCount = elementStorage.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            GameObject child = elementStorage.transform.GetChild(0).gameObject;
            storedDrags.Add(child);
            RectTransform trans = child.GetComponent<RectTransform>();
            trans.SetParent(Deck.instance.draggablePos.transform);
            trans.anchorMax = new Vector2(0.5f, 0.5f);
            trans.anchorMin = new Vector2(0.5f, 0.5f);
            trans.anchoredPosition = new Vector2(0.5f, 0.5f);
            trans.localPosition = new Vector3(0, 0, 0);
        }

        if (Deck.instance.freeDraggables.ContainsKey("utility"))
        {
            storedDrags = Deck.instance.freeDraggables["utility"];
            storedDrags.Clear();
        }
        else
        {
            storedDrags = new List<GameObject>();
            Deck.instance.freeDraggables["utility"] = storedDrags;
        }
        childCount = utilityStorage.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            GameObject child = utilityStorage.transform.GetChild(0).gameObject;
            storedDrags.Add(child);
            RectTransform trans = child.GetComponent<RectTransform>();
            trans.SetParent(Deck.instance.draggablePos.transform);
            trans.anchorMax = new Vector2(0.5f, 0.5f);
            trans.anchorMin = new Vector2(0.5f, 0.5f);
            trans.anchoredPosition = new Vector2(0.5f, 0.5f);
            trans.localPosition = new Vector3(0, 0, 0);
        }

        Deck.instance.HideCards();

        customizationWindow.SetActive(false);
    }
}
