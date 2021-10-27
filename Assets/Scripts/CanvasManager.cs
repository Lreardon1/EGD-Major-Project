using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    [SerializeField]
    public GameObject customizationCanvas;
    [SerializeField]
    public GameObject editOptionPopup;

    public GameObject player;

    public void ToggleEditOption(bool state)
    {
        editOptionPopup.SetActive(state);
    }

    public void OpenCustomization()
    {
        LockPlayer();
        customizationCanvas.GetComponent<DeckCustomizer>().SetUp();
    }

    public void LockPlayer()
    {
        player.GetComponent<OverworldMovement>().canMove = false;
    }

    public void UnlockPlayer()
    {
        player.GetComponent<OverworldMovement>().canMove = true;
    }
}
