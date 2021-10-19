using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    public enum CardType { None, Attack, Block, Influence };
    public enum Element { None, Fire, Water, Lightning, Air };

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
    public List<int> intModifierVals;
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
    public Image cardText;

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
        int intModC = 0, spriteModC = 0;
        int i;
        for (i = 0; i < availableModifiers.Count; i++)
        {
            Modifier template = ModifierLookup.modifierLookupTable[availableModifiers[i]];
            //int detected, using next int modifier
            if (template.type == 0)
            {
                modifiers[i].GetComponent<Image>().sprite = template.icon;
                GameObject textMod = Instantiate(textComp, modifiers[i].transform.GetChild(0).transform);
                textMod.GetComponent<TMPro.TextMeshProUGUI>().text = intModifierVals[intModC].ToString();
                Modifier newMod = new Modifier(template.name, template.icon, template.type, intModifierVals[intModC], null);
                GetComponent<CardEditHandler>().activeModifiers.Add(modifiers[i], newMod);
                intModC++;
            }
            //sprite detected, using next sprite modifier
            else if (template.type == 1)
            {
                modifiers[i].GetComponent<Image>().sprite = template.icon;
                GameObject spriteMod = Instantiate(spriteComp, modifiers[i].transform.GetChild(0).transform);
                spriteMod.GetComponent<Image>().sprite = spriteModifierVals[spriteModC];
                Modifier newMod = new Modifier(template.name, template.icon, template.type, -1, spriteModifierVals[spriteModC]);
                GetComponent<CardEditHandler>().activeModifiers.Add(modifiers[i], newMod);
                spriteModC++;
            }
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

            case Element.Lightning:
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

            case Element.Lightning:
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
}
