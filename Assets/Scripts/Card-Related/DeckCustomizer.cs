using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckCustomizer : MonoBehaviour
{
    [SerializeField]
    public GameObject cardRenderer;
    [SerializeField]
    public GameObject elementDraggable;
    [SerializeField]
    public GameObject elementStorage;
    [SerializeField]
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
        Vector2 elementSize = elementDraggable.GetComponent<RectTransform>().sizeDelta;
        elementGrid.cellSize = new Vector2(elementSize.x, elementSize.y);

        List<GameObject> deck = Deck.instance.viewOrder;
        foreach (GameObject c in deck)
        {
            c.GetComponent<DragDrop>().isDraggable = false;
            c.GetComponent<RectTransform>().SetParent(transform);
        }

        deck[0].GetComponent<Card>().Play();
    }
}
