using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPCDialogue : MonoBehaviour
{
    public Text uitext;
    public string opening;

    // Start is called before the first frame update
    void Start()
    {
        uitext.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        //other.name should equal the root of your Player object
        if (other.name == "Player")
        {
            uitext.text = opening;
        }
    }

    void OnTriggerExit(Collider other)
    {
        //other.name should equal the root of your Player object
        if (other.name == "Player")
        {
            uitext.text = "";
        }
    }
}
