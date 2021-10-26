using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    public enum CardType { None, Attack, Block, Influence };
    public enum Element { None, Fire, Water, Air, Earth, Light, Dark };

    [Header("Initial Settings")]
    [SerializeField]
    public int manaCost = 0;
    [SerializeField]
    public CardType type;
    [SerializeField]
    public Element element;
    [SerializeField]
    public List<Modifier.ModifierEnum> availableModifiers;
    [SerializeField]
    public List<Sprite> spriteModifierVals;

    [Header("Tunable Objects")]
    [SerializeField]
    public List<GameObject> modifiers;
    [SerializeField]
    public TMPro.TextMeshProUGUI manaText;
    [SerializeField]
    public GameObject elementIcon;
    [SerializeField]
    public GameObject typeIcon;

    [Header("OnPlay Script")]
    public CardActionTemplate onPlayScript;

    [Header("Quick Data References")]
    [SerializeField]
    public Image cardText;
    private Card copiedCard;

    [SerializeField]
    public Sprite transparentSprite;

    [Header("Prefab References")]
    [SerializeField]
    public List<GameObject> draggableElements;
    [SerializeField]
    public GameObject textComp;
    [SerializeField]
    public GameObject spriteComp;

    // Start is called before the first frame update
    public void InitializeCard()
    {
        //dynamically determining valid slots based on starting values
        int spriteModC = 0;
        int i;
        for (i = 0; i < availableModifiers.Count; i++)
        {
            Modifier template = ModifierLookup.modifierLookupTable[availableModifiers[i]];
            modifiers[i].GetComponent<Image>().sprite = template.icon;
            GameObject spriteMod = Instantiate(spriteComp, modifiers[i].transform.GetChild(0).transform);
            if (spriteModifierVals[spriteModC] != null)
            {
                spriteMod.GetComponent<Image>().sprite = spriteModifierVals[spriteModC];
            }
            else
            {
                spriteMod.GetComponent<Image>().sprite = transparentSprite;
            }
            Modifier newMod = new Modifier(template.name, template.icon, spriteModifierVals[spriteModC]);
            GetComponent<CardEditHandler>().activeModifiers.Add(modifiers[i], newMod);
            spriteModC++;
        }
        for (; i < modifiers.Count; i++)
        {
            modifiers[i].SetActive(false);
        }

        //updating element icon to match
        GameObject elemIcon = null;
        switch(element)
        {
            case Element.Fire:
                elemIcon = Instantiate(draggableElements[0], elementIcon.transform);
                break;

            case Element.Water:
                elemIcon = Instantiate(draggableElements[1], elementIcon.transform);
                break;

            case Element.Earth:
                elemIcon = Instantiate(draggableElements[2], elementIcon.transform);
                break;

            case Element.Air:
                elemIcon = Instantiate(draggableElements[3], elementIcon.transform);
                break;
        }
        if (elemIcon != null)
        {
            elemIcon.GetComponent<DragDrop>().isDraggable = false;
        }

        //setting mana text to the right cost
        manaText.text = manaCost.ToString();
    }

    public void Play()
    {
        onPlayScript.OnPlay();
    }

    public void UpdateManaCost(int newCost)
    {
        manaCost = newCost;
        manaText.text = manaCost.ToString();
    }

    public void CopyCardSprites(Card c)
    {
        //updating all default set sprites
        GetComponent<Image>().sprite = c.gameObject.GetComponent<Image>().sprite;
        GetComponent<Image>().color = c.gameObject.GetComponent<Image>().color;
        typeIcon.GetComponent<Image>().sprite = c.typeIcon.GetComponent<Image>().sprite;
        cardText.sprite = c.cardText.sprite;
        UpdateManaCost(c.manaCost);

        element = c.element;
        //updating element icon to match
        GameObject elemIcon = null;
        switch (element)
        {
            case Element.Fire:
                elemIcon = Instantiate(draggableElements[0], elementIcon.transform);
                break;

            case Element.Water:
                elemIcon = Instantiate(draggableElements[1], elementIcon.transform);
                break;

            case Element.Earth:
                elemIcon = Instantiate(draggableElements[2], elementIcon.transform);
                break;

            case Element.Air:
                elemIcon = Instantiate(draggableElements[3], elementIcon.transform);
                break;
        }
        if (elemIcon != null)
        {
            elemIcon.GetComponent<DragDrop>().isDraggable = false;
            c.GetComponent<CardEditHandler>().deckCustomizer.cardEditor.GetComponent<CardEditor>().primElementIcon = elemIcon;
        }
    }

    //Creates a visual only copy of a given Card script (MUST BE CALLED ON FULLY EQUIPPED CARD OR NULL REF OCCURS)
    public void VisualCopy(Card c)
    {
        //updating all default set sprites
        GetComponent<Image>().sprite = c.gameObject.GetComponent<Image>().sprite;
        GetComponent<Image>().color = c.gameObject.GetComponent<Image>().color;
        typeIcon.GetComponent<Image>().sprite = c.typeIcon.GetComponent<Image>().sprite;
        cardText.sprite = c.cardText.sprite;
        UpdateManaCost(c.manaCost);

        GameObject elemIcon = elementIcon.transform.GetChild(0).gameObject;
        if (c.element != Element.None) {
            
            elemIcon.GetComponent<Image>().sprite = c.elementIcon.transform.GetChild(0).gameObject.GetComponent<Image>().sprite;
        }
        else
        {
            elemIcon.GetComponent<Image>().sprite = transparentSprite;
        }

        for (int i = 0; i < modifiers.Count; i++)
        {
            if (c.modifiers[i].activeSelf)
            {
                modifiers[i].SetActive(true);
                modifiers[i].GetComponent<Image>().sprite = c.modifiers[i].GetComponent<Image>().sprite;
                modifiers[i].transform.GetChild(0).GetChild(0).gameObject.GetComponent<Image>().sprite = c.modifiers[i].transform.GetChild(0).GetChild(0).gameObject.GetComponent<Image>().sprite;
            }
            else
            {
                modifiers[i].SetActive(false);
            }
        }

        copiedCard = c;
    }

    public void HideDisplay()
    {
        transform.parent.gameObject.SetActive(false);
        copiedCard.gameObject.GetComponent<Button>().interactable = true;
    }

    public void Unequip()
    {
        //clearing modifier values and instancing modifiers to respective pools
        GetComponent<CardEditHandler>().Unequip();
        //clearing sprites
        for (int i = 0; i < modifiers.Count; i++)
        {
            if (modifiers[i].transform.GetChild(0).childCount != 0)
            {
                modifiers[i].transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = transparentSprite;
            }
        }
    }
}
