using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Campfire : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        //check if player is the collision
        if (other.gameObject.name == "Player")
        {
            if (PlayerPrefs.HasKey("hasCards") && PlayerPrefs.GetInt("hasCards") == 1) {
                CanvasManager cm = other.gameObject.GetComponent<PlayerInteraction>().cm;
                cm.ToggleEditOption(true);
                other.gameObject.GetComponent<PlayerInteraction>().canEditCards = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //check if player is the collision
        if (other.gameObject.name == "Player")
        {
            if (PlayerPrefs.HasKey("hasCards") && PlayerPrefs.GetInt("hasCards") == 1)
            {
                CanvasManager cm = other.gameObject.GetComponent<PlayerInteraction>().cm;
                cm.ToggleEditOption(false);
                other.gameObject.GetComponent<PlayerInteraction>().canEditCards = false;
            }
        }
    }
}
