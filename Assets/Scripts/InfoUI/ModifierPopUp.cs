using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModifierPopUp : MonoBehaviour
{
    [SerializeField]
    public Image modifierImage;
    [SerializeField]
    public InfoPopUp popup;

    // Start is called before the first frame update
    void Start()
    {
        LookUpText();
    }

    public void LookUpText()
    {
        popup.title = ModifierLookup.titleLookup[modifierImage.sprite];
        popup.description = ModifierLookup.descLookup[modifierImage.sprite];
    }
}
