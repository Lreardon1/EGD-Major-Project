using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardEditor : MonoBehaviour
{
    [SerializeField]
    public GameObject editedCardRender;

    public Card currentCard;
    public bool checkForChanges = false;
    public List<GameObject> modifierTransforms;
    public List<int> previousChildrenNum = new List<int>();

    public GameObject primElementIcon;

    void ResetChildrenNum()
    {
        previousChildrenNum.Clear();
        for (int i = 0; i < modifierTransforms.Count; i++)
        {
            previousChildrenNum.Add(0);
        }
    }

    public void LoadCard(Card card)
    {
        currentCard = card;
        ResetChildrenNum();
        editedCardRender.GetComponent<Card>().CopyCardSprites(card);
    }

    void Update()
    {
        //checking for any changes to draggable modifiers
        if (checkForChanges)
        {
            for (int i = 0; i < modifierTransforms.Count; i++)
            {
                if (modifierTransforms[i].transform.childCount != previousChildrenNum[i])
                {
                    previousChildrenNum[i] = modifierTransforms[i].transform.childCount;
                    if (previousChildrenNum[i] == 0)
                    {
                        currentCard.modifiers[i].transform.GetChild(0).GetChild(0).gameObject.GetComponent<Image>().sprite = currentCard.transparentSprite;
                        currentCard.gameObject.GetComponent<CardEditHandler>().activeModifiers[currentCard.modifiers[i]].DeactivateModifier(currentCard);
                        currentCard.gameObject.GetComponent<CardEditHandler>().activeModifiers[currentCard.modifiers[i]].DeactivateModifier(editedCardRender.GetComponent<Card>());
                        currentCard.gameObject.GetComponent<CardEditHandler>().activeModifiers[currentCard.modifiers[i]].setSpriteMod(null);
                    }
                    else if (previousChildrenNum[i] == 1)
                    {
                        GameObject newChild = modifierTransforms[i].transform.GetChild(0).gameObject;
                        currentCard.modifiers[i].transform.GetChild(0).GetChild(0).gameObject.GetComponent<Image>().sprite = newChild.GetComponent<Image>().sprite;
                        currentCard.gameObject.GetComponent<CardEditHandler>().activeModifiers[currentCard.modifiers[i]].setSpriteMod(newChild.GetComponent<Image>().sprite);
                        currentCard.gameObject.GetComponent<CardEditHandler>().activeModifiers[currentCard.modifiers[i]].ActivateModifier(currentCard);
                        currentCard.gameObject.GetComponent<CardEditHandler>().activeModifiers[currentCard.modifiers[i]].ActivateModifier(editedCardRender.GetComponent<Card>());
                    }
                }
            }
        }
    }

    public void SaveCard()
    {
        foreach (GameObject g in modifierTransforms)
        {
            if (g.transform.childCount == 1)
            {
                Destroy(g.transform.GetChild(0).gameObject);
            }
        }
        if (primElementIcon != null)
        {
            Destroy(primElementIcon);
            primElementIcon = null;
        }
        ResetChildrenNum();
        currentCard.gameObject.GetComponent<CardEditHandler>().ShrinkCard();
        currentCard = null;
    }
}
