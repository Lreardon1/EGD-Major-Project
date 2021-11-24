using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    public enum CardType { None, Attack, Block, Influence };
    public enum Element { None, Fire, Water, Air, Earth, Light, Dark };
    public enum AoE { Single, Adjascent, All };

    [Header("Initial Settings")]
    [SerializeField]
    public string cardName;
    [SerializeField]
    public int manaCost = 0;
    private int baseManaCost = 0;
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
    [SerializeField]
    public Image cardArt;
    [SerializeField]
    public Image cardBase;
    [SerializeField]
    public Image bannerArt;
    [SerializeField]
    public Image constantArt;

    [Header("OnPlay Script")]
    [SerializeField]
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
    public List<Color> elementColors;
    [SerializeField]
    public GameObject textComp;
    [SerializeField]
    public GameObject spriteComp;

    [Header("Combat Affected Stats")]
    public int baseNum = 0;
    public int numMod = 0;
    public List<Buff.Stat> buffedStats = new List<Buff.Stat>();
    public bool shieldWithThorns = false;
    public Element secondaryElem = Element.None;
    public AoE targetting = AoE.Single;
    public bool givePrio = false;
    public bool isWild = false;

    void Awake()
    {
        baseManaCost = manaCost;
    }

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
            if (spriteModifierVals.Count < spriteModC && spriteModifierVals[spriteModC] != null)
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
        Color cardColor = Color.white;
        switch(element)
        {
            case Element.Fire:
                elemIcon = Instantiate(draggableElements[0], elementIcon.transform);
                cardColor = elementColors[0];
                break;

            case Element.Water:
                elemIcon = Instantiate(draggableElements[1], elementIcon.transform);
                cardColor = elementColors[1];
                break;

            case Element.Earth:
                elemIcon = Instantiate(draggableElements[2], elementIcon.transform);
                cardColor = elementColors[2];
                break;

            case Element.Air:
                elemIcon = Instantiate(draggableElements[3], elementIcon.transform);
                cardColor = elementColors[3];
                break;

            case Element.Light:
                elemIcon = Instantiate(draggableElements[4], elementIcon.transform);
                cardColor = elementColors[4];
                break;

            case Element.Dark:
                elemIcon = Instantiate(draggableElements[5], elementIcon.transform);
                cardColor = elementColors[5];
                break;
        }
        if (elemIcon != null)
        {
            elemIcon.GetComponent<DragDrop>().isDraggable = false;
            bannerArt.color = cardColor;
            constantArt.color = cardColor;
        }

        //setting mana text to the right cost
        if (isWild)
        {
            manaText.text = "?";
        }
        else
        {
            manaText.text = manaCost.ToString();
        }
    }

    public void Play(GameObject combatant, List<GameObject> otherCombatants)
    {
        onPlayScript.OnPlay(this, combatant, otherCombatants);
    }

    public void UpdateManaCost(int newCost)
    {
        if (newCost > manaCost)
        {
            manaCost = baseManaCost;
        }
        else
        {
            manaCost = Mathf.Max(1, newCost);
        }
        manaText.text = manaCost.ToString();
    }

    public void CopyCardSprites(Card c)
    {
        //updating all default set sprites
        cardArt.sprite = c.cardArt.sprite;
        cardBase.sprite = c.cardBase.sprite;
        bannerArt.sprite = c.bannerArt.sprite;
        bannerArt.color = c.bannerArt.color;
        constantArt.color = c.constantArt.color;
        typeIcon.GetComponent<Image>().sprite = c.typeIcon.GetComponent<Image>().sprite;
        cardText.sprite = c.cardText.sprite;
        manaCost = c.manaCost;
        baseManaCost = c.baseManaCost;
        if (c.isWild)
        {
            manaText.text = "?";
        }
        else
        {
            manaText.text = manaCost.ToString();
        }

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

            case Element.Light:
                elemIcon = Instantiate(draggableElements[4], elementIcon.transform);
                break;

            case Element.Dark:
                elemIcon = Instantiate(draggableElements[5], elementIcon.transform);
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
        cardArt.sprite = c.cardArt.sprite;
        cardBase.sprite = c.cardBase.sprite;
        bannerArt.sprite = c.bannerArt.sprite;
        bannerArt.color = c.bannerArt.color;
        constantArt.color = c.constantArt.color;
        typeIcon.GetComponent<Image>().sprite = c.typeIcon.GetComponent<Image>().sprite;
        cardText.sprite = c.cardText.sprite;
        manaCost = c.manaCost;
        baseManaCost = c.baseManaCost;
        if (c.isWild)
        {
            manaText.text = "?";
        }
        else
        {
            manaText.text = manaCost.ToString();
        }

        GameObject elemIcon = elementIcon;
        if (c.element != Element.None)
        {
            elemIcon.GetComponent<Image>().enabled = true;
            elemIcon.GetComponent<Image>().sprite = c.elementIcon.transform.GetChild(0).gameObject.GetComponent<Image>().sprite;
        }
        else
        {
            elemIcon.GetComponent<Image>().enabled = false;
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
