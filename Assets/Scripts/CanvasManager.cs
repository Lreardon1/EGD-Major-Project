using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    [SerializeField]
    public GameObject customizationCanvas;
    [SerializeField]
    public GameObject editOptionPopup;
    public PauseManager pauseManager;

    private PlayerInteraction playerInteraction;

    public GameObject player;

    public void ToggleEditOption(bool state)
    {
        editOptionPopup.SetActive(state);
    }

    public void OpenCustomization(PlayerInteraction pi)
    {
        pauseManager.PartyHeal();

        playerInteraction = pi;
        editOptionPopup.SetActive(false);
        LockPlayer();
        customizationCanvas.SetActive(true);
        customizationCanvas.GetComponent<DeckCustomizer>().SetUp();
    }

    public void CloseCustomization()
    {
        UnlockPlayer();
        customizationCanvas.SetActive(false);
        editOptionPopup.SetActive(true);
        playerInteraction.canEditCards = true;
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
