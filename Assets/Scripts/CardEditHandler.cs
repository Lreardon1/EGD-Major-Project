using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardEditHandler : MonoBehaviour
{

    public Card cardScript;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DisplayCard()
    {
        gameObject.GetComponent<Button>().interactable = false;
        //instantiate editable card UI, allowing for changes with more buttons 
    }

    public void ShrinkCard()
    {
        //save changes on editable card UI, and return
        gameObject.GetComponent<Button>().interactable = true;
    }
}
