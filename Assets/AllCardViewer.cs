using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllCardViewer : MonoBehaviour
{
    public bool checkForUpdates = false;
    private int prevChildCount = 0;
    // Update is called once per frame
    void Update()
    {
        if (checkForUpdates)
        {
            if (transform.childCount != prevChildCount)
            {
                if (transform.childCount > prevChildCount)
                {
                    GameObject lastCard = transform.GetChild(prevChildCount).gameObject;
                    lastCard.GetComponent<Card>().Unequip();
                }
                prevChildCount = transform.childCount;
            }
        }
    }
}
