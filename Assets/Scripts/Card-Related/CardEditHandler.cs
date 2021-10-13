using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardEditHandler : MonoBehaviour
{

    public Card cardScript;
    private DeckCustomizer deckCustomizer;
    public Dictionary<GameObject, Modifier> activeModifiers = new Dictionary<GameObject, Modifier>();

    public void DisplayCard()
    {
        if (deckCustomizer == null)
        {
            deckCustomizer = FindObjectOfType<DeckCustomizer>();
        }

        gameObject.GetComponent<Button>().interactable = false;
        deckCustomizer.cardEditor.SetActive(true);
        //instantiate editable card UI, allowing for changes with more buttons 
        deckCustomizer.cardEditor.GetComponent<CardEditor>().LoadCard(cardScript);
    }

    public void ShrinkCard()
    {
        //save changes on editable card UI, and return
        gameObject.GetComponent<Button>().interactable = true;
        deckCustomizer.cardEditor.SetActive(false);
    }
}
