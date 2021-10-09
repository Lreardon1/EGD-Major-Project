using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckCustomizer : MonoBehaviour
{
    [SerializeField]
    public GameObject cardRenderer;

    void Start()
    {
        SetUp();
    }

    public void SetUp()
    {
        List<GameObject> deck = Deck.instance.viewOrder;
        foreach (GameObject c in deck)
        {
            c.GetComponent<RectTransform>().SetParent(transform);
        }

        deck[0].GetComponent<Card>().Play();
    }
}
