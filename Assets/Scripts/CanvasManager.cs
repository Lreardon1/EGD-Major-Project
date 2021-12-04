using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    [SerializeField]
    public GameObject customizationCanvas;
    [SerializeField]
    public GameObject editOptionPopup;
    [SerializeField]
    public GameObject newModPopup;
    [SerializeField]
    public MinimapManager minimap;
    [SerializeField]
    public PauseManager pauseManager;

    private PlayerInteraction playerInteraction;

    public GameObject player;

    void Start()
    {
        //PopUpNewModifiers();
    }

    public void ToggleEditOption(bool state)
    {
        if (state)
        {
            editOptionPopup.GetComponent<Animator>().SetBool("close", false);
            editOptionPopup.GetComponent<Animator>().SetBool("open", true);
        }
        else
        {
            editOptionPopup.GetComponent<Animator>().SetBool("close", true);
            editOptionPopup.GetComponent<Animator>().SetBool("open", false);
        }
    }

    public void PopUpNewModifiers()
    {
        newModPopup.GetComponent<Animator>().SetBool("open", true);
        StartCoroutine(CloseNewModPopUp());
    }

    IEnumerator CloseNewModPopUp()
    {
        yield return new WaitForSeconds(5);
        newModPopup.GetComponent<Animator>().SetBool("open", false);
    }

    public void OpenCustomization(PlayerInteraction pi)
    {
        pauseManager.PartyHeal();
        minimap.SetVisualActive(false);

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

        minimap.SetVisualActive(true);
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
