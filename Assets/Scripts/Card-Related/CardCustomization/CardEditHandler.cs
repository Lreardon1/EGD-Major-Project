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

    public bool isCustomizable;
    public bool displayOnClick = true;
    private float startHeldTime;
    private bool isHeld;
    private GameObject currentParent;

    public bool inCombat;

    [SerializeField]
    public GameObject spriteEditor;

    public void DontDisplay()
    {
        if (!isCustomizable)
        {
            startHeldTime = Time.time;
            isHeld = true;
            currentParent = transform.parent.gameObject;
        }
    }

    void Update()
    {
        if (isHeld)
        {
            //determine if clicking on card is a drag or a button click, don't display if not a button click
            if (Time.time - startHeldTime > 0.65 || currentParent != transform.parent.gameObject)
            {
                displayOnClick = false;
            }
        }
    }

    public void DisplayCard()
    {
        if (inCombat)
            return;
        if (deckCustomizer == null)
        {
            deckCustomizer = FindObjectOfType<DeckCustomizer>();
            cardEditor = deckCustomizer.cardEditor.GetComponent<CardEditor>();
        }
        if (displayOnClick)
        {
            if (isCustomizable)
            {
                gameObject.GetComponent<Button>().interactable = false;
                deckCustomizer.cardEditor.SetActive(true);
                //instantiate editable card UI, allowing for changes with more buttons 
                Card displayCard = cardEditor.editedCardRender.GetComponent<Card>();
                cardEditor.LoadCard(cardScript);
                formatModifierEditors(displayCard);
                cardEditor.checkForChanges = true;
            }
            else
            {
                gameObject.GetComponent<Button>().interactable = false;
                deckCustomizer.cardDisplay.SetActive(true);
                Card displayCard = deckCustomizer.cardDisplay.transform.GetChild(0).gameObject.GetComponent<Card>();
                displayCard.VisualCopy(cardScript);
            }
        }
        displayOnClick = true;
        isHeld = false;
    }

    private void formatModifierEditors(Card displayCard)
    {
        //updating modifiers on the editor to match card with interactable versions
        int i = 0;
        foreach (KeyValuePair<GameObject, Modifier> mod in activeModifiers)
        {
            displayCard.modifiers[i].SetActive(true);
            cardEditor.modifierTransforms[i].SetActive(true);
            cardEditor.modifierTransforms[i].GetComponent<DropZone>().slotType = mod.Value.name;
            displayCard.modifiers[i].GetComponent<Image>().sprite = mod.Key.GetComponent<Image>().sprite;
            cardEditor.previousChildrenNum[i] = 1;
            if (mod.Value.spriteVal != null)
            {
                GameObject spriteEdit = Instantiate(spriteEditor, cardEditor.modifierTransforms[i].transform);
                spriteEdit.GetComponent<Image>().sprite = mod.Value.spriteVal;
                spriteEdit.GetComponent<DragDrop>().dropType = mod.Value.name;
                if (mod.Value.name == Modifier.ModifierEnum.NumModifier)
                {
                    spriteEdit.GetComponent<DragDrop>().allowedDropZones.Add(deckCustomizer.numStorage);
                }
                else if (mod.Value.name == Modifier.ModifierEnum.SecondaryElement)
                {
                    spriteEdit.GetComponent<DragDrop>().allowedDropZones.Add(deckCustomizer.elementStorage);
                }
                else if (mod.Value.name == Modifier.ModifierEnum.Utility)
                {
                    spriteEdit.GetComponent<DragDrop>().allowedDropZones.Add(deckCustomizer.utilityStorage);
                }

                //all constantly available drop zones
                spriteEdit.GetComponent<DragDrop>().allowedDropZones.Add(deckCustomizer.modsDropZone);
                spriteEdit.GetComponent<DragDrop>().allowedDropZones.Add(deckCustomizer.editorDropZone);
                for (int j = 0; j < cardEditor.modifierTransforms.Count; j++)
                {
                    spriteEdit.GetComponent<DragDrop>().allowedDropZones.Add(cardEditor.modifierTransforms[j]);
                }
            }
            
            i++;
        }
        for (; i < displayCard.modifiers.Count; i++)
        {
            displayCard.modifiers[i].SetActive(false);
            cardEditor.modifierTransforms[i].SetActive(false);
        }
    }

    public void ShrinkCard()
    {
        cardEditor.checkForChanges = false;
        //save changes on editable card UI, and return
        gameObject.GetComponent<Button>().interactable = true;
        deckCustomizer.cardEditor.SetActive(false);
    }

    public void Unequip()
    {
        if (deckCustomizer == null)
        {
            deckCustomizer = FindObjectOfType<DeckCustomizer>();
            cardEditor = deckCustomizer.cardEditor.GetComponent<CardEditor>();
        }

        foreach (KeyValuePair<GameObject, Modifier> mod in activeModifiers)
        {
            if (mod.Value.spriteVal != null)
            {
                GameObject spriteEdit = null;

                if (mod.Value.name == Modifier.ModifierEnum.NumModifier)
                {
                    spriteEdit = Instantiate(spriteEditor, deckCustomizer.numStorage.transform);
                    spriteEdit.GetComponent<DragDrop>().allowedDropZones.Add(deckCustomizer.numStorage);
                }
                else if (mod.Value.name == Modifier.ModifierEnum.SecondaryElement)
                {
                    spriteEdit = Instantiate(spriteEditor, deckCustomizer.elementStorage.transform);
                    spriteEdit.GetComponent<DragDrop>().allowedDropZones.Add(deckCustomizer.elementStorage);
                }
                else if (mod.Value.name == Modifier.ModifierEnum.Utility)
                {
                    spriteEdit = Instantiate(spriteEditor, deckCustomizer.utilityStorage.transform);
                    spriteEdit.GetComponent<DragDrop>().allowedDropZones.Add(deckCustomizer.utilityStorage);
                }

                if (spriteEdit != null)
                {
                    spriteEdit.GetComponent<Image>().sprite = mod.Value.spriteVal;
                    spriteEdit.GetComponent<DragDrop>().dropType = mod.Value.name;

                    //all constantly available drop zones
                    spriteEdit.GetComponent<DragDrop>().allowedDropZones.Add(deckCustomizer.modsDropZone);
                    spriteEdit.GetComponent<DragDrop>().allowedDropZones.Add(deckCustomizer.editorDropZone);
                    for (int j = 0; j < cardEditor.modifierTransforms.Count; j++)
                    {
                        spriteEdit.GetComponent<DragDrop>().allowedDropZones.Add(cardEditor.modifierTransforms[j]);
                    }
                }
                mod.Value.DeactivateModifier(cardScript);
                mod.Value.setSpriteMod(null);
            }
        }
    }
}
