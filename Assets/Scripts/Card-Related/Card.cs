using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    public enum Element { None, Fire, Water, Lightning, Air };

    [Header("Initial Settings")]
    [SerializeField]
    public int manaCost = 0;
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

    [Header("OnPlay Script")]
    public CardActionTemplate onPlayScript;

    [Header("References")]
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
                GetComponent<CardEditHandler>().activeModifiers[modifiers[i]] = newMod;
                intModC++;
            }
            //sprite detected, using next sprite modifier
            else if (template.type == 1)
            {
                modifiers[i].GetComponent<Image>().sprite = template.icon;
                GameObject spriteMod = Instantiate(spriteComp, modifiers[i].transform.GetChild(0).transform);
                spriteMod.GetComponent<Image>().sprite = spriteModifierVals[spriteModC];
                Modifier newMod = new Modifier(template.name, template.icon, template.type, intModifierVals[spriteModC], null);
                GetComponent<CardEditHandler>().activeModifiers[modifiers[i]] = newMod;
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
            elemIcon.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        }

        //setting mana text to the right cost
        manaText.text = manaCost.ToString();
    }

    public void Play()
    {
        onPlayScript.OnPlay();
    }

    public void Refresh()
    {

    }
}
