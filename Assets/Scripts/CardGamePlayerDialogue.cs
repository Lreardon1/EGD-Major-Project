using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CardGamePlayerDialogue : MonoBehaviour
{
    public TMPro.TextMeshProUGUI uitext;
    public Image text_back;
    public TMPro.TextMeshProUGUI speaker;

    public string _name;
    public string cardsOpening, noCardsOpening;
    public string SceneToLoad;
    public bool inTrigger = false;


    private Vector3 playerPos;


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
        bool hasCards = PlayerPrefs.HasKey("hasCards");
        hasCards = true;
        if (hasCards && inTrigger && Input.GetKeyDown(KeyCode.Space))
        {
            MemorySceneLoader.LoadFromOverworld(SceneToLoad, playerPos);
        }
    }
    

    void OnTriggerEnter(Collider other)
    {
        //other.name should equal the root of your Player object
        if (other.name == "Player")
        {
            bool hasCards = PlayerPrefs.HasKey("hasCards");
            hasCards = true;
            playerPos = other.transform.position;
            uitext.text = hasCards ? cardsOpening : noCardsOpening;
            speaker.gameObject.SetActive(true);
            speaker.text = _name;
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
            speaker.gameObject.SetActive(false);
            inTrigger = false;
        }
    }
}
