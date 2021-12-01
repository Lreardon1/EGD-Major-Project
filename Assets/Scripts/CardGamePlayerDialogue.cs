using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CardGamePlayerDialogue : MonoBehaviour
{
    public TMPro.TextMeshProUGUI uitext;
    public RawImage text_back;
    public string cardsOpening, noCardsOpening;
    public string SceneToLoad;
    public bool inTrigger = false;

    public bool hasCards = true;

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
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (hasCards && inTrigger && Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene(SceneToLoad);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        //other.name should equal the root of your Player object
        if (other.name == "Player")
        {
            print("START UP");
            uitext.text = hasCards ? cardsOpening : noCardsOpening;
            text_back.gameObject.SetActive(true);
            inTrigger = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        //other.name should equal the root of your Player object
        if (other.name == "Player")
        {
            uitext.text = "";
            text_back.gameObject.SetActive(false);
            inTrigger = false;
        }
    }
}
