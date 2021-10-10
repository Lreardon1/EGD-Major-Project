using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardEditHandler : MonoBehaviour
{

    public Card cardScript;
    private DeckCustomizer deckCustomizer;

    public void DisplayCard()
    {
        if (deckCustomizer == null)
        {
            deckCustomizer = FindObjectOfType<DeckCustomizer>();
        }

        gameObject.GetComponent<Button>().interactable = false;
        deckCustomizer.gameObject.SetActive(true);
        //instantiate editable card UI, allowing for changes with more buttons 
    }

    public void ShrinkCard()
    {
        //save changes on editable card UI, and return
        gameObject.GetComponent<Button>().interactable = true;
        deckCustomizer.gameObject.SetActive(false);
    }
}
