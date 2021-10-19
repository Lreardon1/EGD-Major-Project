using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardEditHandler : MonoBehaviour
{

    public Card cardScript;
    public DeckCustomizer deckCustomizer;
    private CardEditor cardEditor;
    public Dictionary<GameObject, Modifier> activeModifiers = new Dictionary<GameObject, Modifier>();

    [SerializeField]
    public GameObject textEditor;
    [SerializeField]
    public GameObject spriteEditor;
    [SerializeField]
    public int numEditorMin;
    [SerializeField]
    public int numEditorMax;
    [SerializeField]
    public int manaCostMin;
    [SerializeField]
    public int manaCostMax;

    public void DisplayCard()
    {
        if (deckCustomizer == null)
        {
            deckCustomizer = FindObjectOfType<DeckCustomizer>();
            cardEditor = deckCustomizer.cardEditor.GetComponent<CardEditor>();
        }

        gameObject.GetComponent<Button>().interactable = false;
        deckCustomizer.cardEditor.SetActive(true);
        //instantiate editable card UI, allowing for changes with more buttons 
        Card displayCard = cardEditor.editedCardRender.GetComponent<Card>();
        cardEditor.LoadCard(cardScript);
        formatModifierEditors(displayCard);
        cardEditor.checkForChanges = true;
    }

    private void formatModifierEditors(Card displayCard)
    {
        //updating modifiers on the editor to match card with interactable versions
        int i = 0;
        foreach (KeyValuePair<GameObject, Modifier> mod in activeModifiers)
        {
            displayCard.modifiers[i].SetActive(true);
            cardEditor.modifierTransforms[i].SetActive(true);
            displayCard.modifiers[i].GetComponent<Image>().sprite = mod.Key.GetComponent<Image>().sprite;
            cardEditor.previousChildrenNum[i] = 1;
            if (mod.Value.type == 0) //text editor needed, so spawn a button that will create a text editor and match curr value
            {
                GameObject textEdit = Instantiate(textEditor, cardEditor.modifierTransforms[i].transform);
                textEdit.GetComponent<NumEditor>().SetUp(mod, mod.Value.intVal, numEditorMin, numEditorMax);
            }
            else if (mod.Value.type == 1) //draggable sprite needed, so spawn a draggable sprite matching the curr value
            {
                if (mod.Value.spriteVal != null)
                {
                    GameObject spriteEdit = Instantiate(spriteEditor, cardEditor.modifierTransforms[i].transform);
                    spriteEdit.GetComponent<Image>().sprite = mod.Value.spriteVal;
                    cardEditor.modifierTransforms[i].GetComponent<BoxCollider2D>().enabled = true;
                    if (mod.Value.name == Modifier.ModifierEnum.SecondaryElement)
                    {
                        spriteEdit.GetComponent<DragDrop>().allowedDropZones.Add(deckCustomizer.elementStorage.transform.parent.parent.gameObject);
                        spriteEdit.GetComponent<DragDrop>().allowedDropZones.Add(cardEditor.modifierTransforms[i]);
                    }
                }
            }
            
            i++;
        }
        for (; i < displayCard.modifiers.Count; i++)
        {
            displayCard.modifiers[i].SetActive(false);
            cardEditor.modifierTransforms[i].SetActive(false);
        }

        //spawning a mana editor
        cardEditor.manaEditor = Instantiate(textEditor, cardEditor.manaModifierTransform.transform);
        cardEditor.manaEditor.GetComponent<NumEditor>().SetUp(new KeyValuePair<GameObject, Modifier>(cardEditor.currentCard.gameObject, null), cardEditor.currentCard.manaCost, manaCostMin, manaCostMax);
    }

    public void ShrinkCard()
    {
        cardEditor.checkForChanges = false;
        //save changes on editable card UI, and return
        gameObject.GetComponent<Button>().interactable = true;
        deckCustomizer.cardEditor.SetActive(false);
    }
}
