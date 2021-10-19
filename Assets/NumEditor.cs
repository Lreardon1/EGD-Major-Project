using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class NumEditor : MonoBehaviour
{
    [SerializeField]
    public TMPro.TextMeshProUGUI displayNum;
    public int numValue;
    [SerializeField]
    public GameObject leftButton;
    [SerializeField]
    public GameObject rightButton;
    [SerializeField]
    public int minValue;
    [SerializeField]
    public int maxValue;

    private KeyValuePair<GameObject, Modifier> modPairing;

    public void ToggleButtons()
    {
        leftButton.SetActive(!leftButton.activeSelf);
        rightButton.SetActive(!rightButton.activeSelf);
    }

    public void SetUp(KeyValuePair<GameObject, Modifier> mod, int val, int minVal, int maxVal)
    {
        numValue = val;
        modPairing = mod;
        minValue = minVal;
        maxValue = maxVal;
        displayNum.text = val.ToString();

        if (numValue == minValue)
        {
            leftButton.GetComponent<Button>().interactable = false;
        }

        if (numValue == maxValue)
        {
            rightButton.GetComponent<Button>().interactable = false;
        }
    }

    public void Increase()
    {
        leftButton.GetComponent<Button>().interactable = true;
        numValue++;
        displayNum.text = numValue.ToString();
        UpdateModifier();

        if (numValue == maxValue)
        {
            rightButton.GetComponent<Button>().interactable = false;
        }
    }

    public void Decrease()
    {
        rightButton.GetComponent<Button>().interactable = true;
        numValue--;
        displayNum.text = numValue.ToString();
        UpdateModifier();

        if (numValue == minValue)
        {
            leftButton.GetComponent<Button>().interactable = false;
        }
    }

    private void UpdateModifier()
    {
        //mana editor
        if (modPairing.Value == null)
        {
            modPairing.Key.GetComponent<Card>().UpdateManaCost(numValue);
        }
        else
        {
            modPairing.Key.transform.GetChild(0).GetChild(0).gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = numValue.ToString();
            modPairing.Value.intVal = numValue;
        }
    }
}
