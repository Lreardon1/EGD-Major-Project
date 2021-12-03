using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPCDialogue : MonoBehaviour
{
    public TMPro.TextMeshProUGUI uitext;
    public TMPro.TextMeshProUGUI speaker;
    public Image text_back;
    public string opening;
    public string name;

    // Start is called before the first frame update
    void Start()
    {
        if (uitext != null)
        {
            uitext.text = "";
        }
        if (text_back != null)
        {
            text_back.gameObject.SetActive(false);
            speaker.gameObject.SetActive(false);
        }
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
            speaker.gameObject.SetActive(true);
            speaker.text = name;
            text_back.gameObject.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        //other.name should equal the root of your Player object
        if (other.name == "Player")
        {
            uitext.text = "";
            text_back.gameObject.SetActive(false);
            speaker.gameObject.SetActive(false);
        }
    }
}
