using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckViewer : MonoBehaviour
{
    public bool checkForUpdates = false;
    [SerializeField]
    public TMPro.TextMeshProUGUI deckSize;
    private int prevChildCount = 0;
    // Update is called once per frame
    void Update()
    {
        if (checkForUpdates)
        {
            if (transform.childCount != prevChildCount)
            {
                prevChildCount = transform.childCount;
                UpdateDeckSize();
            }
        }
    }

    public void UpdateDeckSize()
    {
        int size = transform.childCount;
        deckSize.text = size.ToString() + " / 30";
        if (size == 30)
        {
            transform.parent.parent.gameObject.GetComponent<BoxCollider2D>().enabled = false;
        }
        else
        {
            transform.parent.parent.gameObject.GetComponent<BoxCollider2D>().enabled = true;
        }
    }
}
