using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadableDeck : MonoBehaviour
{
    public DeckLoader dl;
    public string deckName;

    public void Selected()
    {
        if (dl.selectedDeck != null)
        {
            dl.selectedDeck.Unselect();
        }
        dl.selectedDeck = this;
        dl.loadButton.GetComponent<Button>().interactable = true;
        GetComponent<Button>().interactable = false;
    }

    public void Unselect()
    {
        GetComponent<Button>().interactable = true;
    }
}
