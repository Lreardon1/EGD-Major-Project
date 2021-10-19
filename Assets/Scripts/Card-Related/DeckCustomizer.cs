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
    public GameObject elementStorage;
    [SerializeField]
    public GameObject otherStorage;
    [SerializeField]
    public TMPro.TextMeshProUGUI manaText;
    [SerializeField]
    public TMPro.TextMeshProUGUI attackBlockText;

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

        GridLayoutGroup elementGrid = elementStorage.GetComponent<GridLayoutGroup>();
        GridLayoutGroup otherGrid = otherStorage.GetComponent<GridLayoutGroup>();
        Vector2 elementSize = draggableSprite.GetComponent<RectTransform>().sizeDelta;
        elementGrid.cellSize = new Vector2(elementSize.x, elementSize.y);
        otherGrid.cellSize = new Vector2(elementSize.x, elementSize.y);

        List<GameObject> deck = Deck.instance.viewOrder;
        foreach (GameObject c in deck)
        {
            c.GetComponent<DragDrop>().isDraggable = false;
            c.GetComponent<RectTransform>().SetParent(transform);
            c.GetComponent<BoxCollider2D>().enabled = false;
        }

        Dictionary<string, List<GameObject>> freeDraggables = Deck.instance.freeDraggables;
        if (freeDraggables.ContainsKey("element"))
        {
            List<GameObject> currList = freeDraggables["element"];
            foreach (GameObject mod in currList)
            {
                mod.GetComponent<RectTransform>().SetParent(elementStorage.transform);
            }
        }
    }
}
