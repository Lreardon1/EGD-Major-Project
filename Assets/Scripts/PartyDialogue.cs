using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyDialogue : MonoBehaviour
{
    public GameObject player;
    bool dialogue;
    bool dialogue_done;
    string[] lines;
    public TMPro.TextMeshProUGUI uitext;
    public Image text_back;
    public Image speaker_image;
    public TMPro.TextMeshProUGUI speaker_name;
    public List<Sprite> speakers;
    public TextAsset rawlines;
    int i;

    IEnumerator waiter()
    {
        yield return new WaitForSeconds(.5f);
        dialogue_done = true;
    }

    void parseText()
    {
        string text = rawlines.text;
        char[] separators = { '|', '\n' };
        lines = text.Split(separators);
    }

    void setSpeaker(string name)
    {
        switch (name)
        {
            case "MC":
                speaker_image.sprite = speakers[0];
                speaker_name.text = "MC";
                i++;
                break;
            case "Serena":
                speaker_image.sprite = speakers[0];
                speaker_name.text = "Serena";
                i++;
                break;
            case "Godfather":
                speaker_image.sprite = speakers[1];
                speaker_name.text = "Godfather";
                i++;
                break;
            case "Hunter":
                speaker_image.sprite = speakers[2];
                speaker_name.text = "Hunter";
                i++;
                break;
            case "Warrior":
                speaker_image.sprite = speakers[3];
                speaker_name.text = "Warrior";
                i++;
                break;
            case "Mechanist":
                speaker_image.sprite = speakers[4];
                speaker_name.text = "Mechanist";
                i++;
                break;
            default:
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //check if player is the collision
        if (other.gameObject.name == "Player" && !dialogue)
        {
            dialogue = true;
            i = 0;
            dialogue_done = false;
            nextLine();
            text_back.gameObject.SetActive(true);
            player.GetComponent<OverworldMovement>().SetCanMove(false);
        }
    }

    void nextLine()
    {
        //update picture if new speaker indicated by |
        setSpeaker(lines[i]);
        //update text
        uitext.text = lines[i];
        i++;
        //if no more lines enable movement and turn off ui
        if (i == lines.Length)
        {
            dialogue = false;
            StartCoroutine(waiter());
        }
    }

    void Start()
    {
        parseText();
        dialogue = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && dialogue)
        {
            nextLine();
        }
        if (dialogue_done && Input.GetKeyDown(KeyCode.Space))
        {
            uitext.text = "";
            speaker_name.text = "";
            text_back.gameObject.SetActive(false);
            player.GetComponent<OverworldMovement>().SetCanMove(true);
            Destroy(this.gameObject);
        }
    }
}
