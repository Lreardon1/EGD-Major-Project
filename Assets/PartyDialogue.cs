using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyDialogue : MonoBehaviour
{
    public GameObject player;
    bool dialogue;
    List<string> lines;
    public TMPro.TextMeshProUGUI uitext;
    public RawImage text_back;
    string speaker;
    public RawImage speaker_image;
    public List<Sprite> speakers;

    private void OnTriggerEnter(Collider other)
    {
        //check if player is the collision
        if (other.gameObject.name == "Player")
        {
            dialogue = true;
            //start first dialogue line
            //set who is speaking
            uitext.text = "yo";
            text_back.gameObject.SetActive(true);
            speaker_image.gameObject.SetActive(true);
            player.GetComponent<OverworldMovement>().SetCanMove(false);
        }
    }

    void nextLine()
    {
        //update text
        //update picture if new speaker indicated by |
        //if no more lines enable movement and turn off ui
        dialogue = false;
        uitext.text = "";
        text_back.gameObject.SetActive(false);
        speaker_image.gameObject.SetActive(false);
        player.GetComponent<OverworldMovement>().SetCanMove(true);
    }

    void Start()
    {
        dialogue = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && dialogue)
        {
            nextLine();
        }
    }
}
